using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;


class UDPClientConnection : Connection
{
    private UdpClient udpClient;
    private Thread clientThread;

    private bool isConnected;
    private Dictionary<long, byte[]> toSendMessages;

    private long lastMessageReciveId;
    private Queue<byte[]> dataRecive = new Queue<byte[]>();

    private int connectionId;
    private long idGenerator;

    private float TARGET_TICK_RATE = 100.0f / 1000.0f;
    private float timer = 0;

    private object readHandle = new object();
    private object lockHandle = new object();

    public UDPClientConnection(string address, int port,
        Action<Connection> onConnected, Action<Connection> onDisconnected)
        : base(onConnected, onDisconnected)
    {
        connectionId = 0;
        idGenerator = 0;

        isConnected = false;
        
        toSendMessages = new Dictionary<long, byte[]>();

        lastMessageReciveId = 0;

        udpClient = new UdpClient();
        udpClient.Connect(address, port);

        clientThread = new Thread(ClienThread);
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    public override bool IsConnected => isConnected;

    public override void Close()
    {
        clientThread.Abort();
        udpClient.Close();
    }

    public override void SendData(byte[] data)
    {
        long id = ((long)connectionId << 32) | ++idGenerator;
        lock (lockHandle)
        {
            toSendMessages.Add(id, data);
        }
    }

    private void ClienThread()
    {
        IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 0);
        bool isRunning = true;
        while (isRunning)
        {
            try
            {
                byte[] bytes = udpClient.Receive(ref groupEP);
                if (bytes.Length < sizeof(int)) return;
                MemoryStream stream = new MemoryStream(bytes);
                BinaryReader reader = new BinaryReader(stream);
                long readStartPosition = reader.BaseStream.Position;
                UDPHeader header = (UDPHeader)reader.ReadInt32();
                switch (header)
                {
                    case UDPHeader.ConnectionAccepted:
                        OnConnectionAccepted(reader, readStartPosition);
                        break;
                    case UDPHeader.ServerSendMessage:
                        OnServerSendMessage(reader, readStartPosition, bytes);
                        break;
                    case UDPHeader.ServerRegisterMessage:
                        OnServerRegisterMessage(reader, readStartPosition);
                        break;
                    default:
                        break;
                }
            }
            catch (SocketException)
            {
                Debug.Log("The server is offline");
                isRunning = false;
            }
            catch (ObjectDisposedException e)
            {
                Debug.Log("Disposed: " + e);
                isRunning = false;
            }
        }
    }

    private void OnConnectionAccepted(BinaryReader reader, long readStartPosition)
    {
        connectionId = reader.ReadInt32();
        isConnected = true;
        onConnected?.Invoke(this);
    }

    private void OnServerSendMessage(BinaryReader reader, long readStartPosition, byte[] bytes)
    {
        long id = reader.ReadInt64();
        int bytesRead = (int)(reader.BaseStream.Position - readStartPosition);
        byte[] message = reader.ReadBytes(bytes.Length - bytesRead);
        if (id != lastMessageReciveId)
        {
            dataRecive.Enqueue(message);
            lastMessageReciveId = id;
        }
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        UDPHeader header = UDPHeader.ClientRegisterMessage;
        writer.Write((int)header);
        writer.Write(id);
        byte[] data = stream.ToArray();
        udpClient.Send(data, data.Length);
    }

    private void OnServerRegisterMessage(BinaryReader reader, long readStartPosition)
    {
        long id = reader.ReadInt64();
        lock (lockHandle)
        {
            toSendMessages.Remove(id);
        }
    }

    private void TryToConnect()
    {
        UDPHeader header = UDPHeader.ConnectionRequest;
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)header);
        byte[] data = stream.ToArray();
        udpClient.Send(data, data.Length);
    }

    private void TryToSendMessages()
    {
        lock (lockHandle)
        {
            foreach (KeyValuePair<long, byte[]> toSend in toSendMessages)
            {
                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);
                UDPHeader header = UDPHeader.ClientSendMessage;
                writer.Write((int)header);
                writer.Write(toSend.Key);
                writer.Write(toSend.Value);
                byte[] dataToSend = stream.ToArray();
                udpClient.Send(dataToSend, dataToSend.Length);
            }
        }
    }

    public override void Tick<EventType>(float deltaTime)
    {
        if (timer < TARGET_TICK_RATE)
        {
            timer += deltaTime;
            return;
        }
        timer -= TARGET_TICK_RATE;
        

        if (!IsConnected)
        {
            TryToConnect();
        }
        TryToSendMessages();

        lock (readHandle)
        {
            while (dataRecive.Count > 0)
            {
                byte[] data = dataRecive.Dequeue();
                EventBus.Instance.Raise<EventType>(data);
            }
        }
    }
}