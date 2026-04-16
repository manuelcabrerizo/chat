using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class UDPListener : Listener
{
    const float DISCONNECTION_TIME_OUT = 60*5;

    private UdpClient udpListener;
    private Thread listenerThread;
    private int listenPort;
    private int connectionIdGenerator;

    object lockHandle = new object();
    private Dictionary<IPEndPoint, UDPServerConnection> connections;
    private Dictionary<IPEndPoint, long> lastSendMessageIds;
    private Dictionary<IPEndPoint, DateTime> lastSendMessageTime;

    
    public UDPListener(int port,
        Action<Connection> onConnectionAccepted,
        Action<Connection> onConnectionDisconnected) 
        : base(onConnectionAccepted, onConnectionDisconnected)
    {
        listenPort = port;
        connectionIdGenerator = 0;
        udpListener = new UdpClient(listenPort);
        connections = new Dictionary<IPEndPoint, UDPServerConnection>();
        lastSendMessageIds = new Dictionary<IPEndPoint, long>();
        lastSendMessageTime = new Dictionary<IPEndPoint, DateTime>();

        listenerThread = new Thread(ListenerThread);
        listenerThread.IsBackground = true;
        listenerThread.Start();
    }

    public override void Stop()
    {
        listenerThread.Abort();
        udpListener.Close();
    }

    private void OnConnectionRequest(IPEndPoint endPoint)
    {
        lock (lockHandle)
        {
            if (!connections.ContainsKey(endPoint))
            {
                UDPServerConnection connection = new UDPServerConnection(
                    udpListener, endPoint, null, onConnectionDisconnected);
                onConnectionAccepted?.Invoke(connection);
                connections.Add(endPoint, connection);
                lastSendMessageIds.Add(endPoint, 0);
            }
            UDPHeader header = UDPHeader.ConnectionAccepted;
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((int)header);
            writer.Write(++connectionIdGenerator);
            byte[] data = stream.ToArray();
            udpListener.Send(data, data.Length, endPoint);
        }
    }

    private void OnClientSendMessage(IPEndPoint endPoint, long messageId, byte[] message)
    {
        lock (lockHandle)
        {
            if (!lastSendMessageIds.ContainsKey(endPoint))
            {
                return;
            }
            if (messageId != lastSendMessageIds[endPoint])
            {
                lastSendMessageTime[endPoint] = DateTime.Now;
                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(messageId);
                writer.Write(message);
                byte[] data = stream.ToArray();
                EventBus.Instance.Raise<ServerReciveDataEvent>(data);
                lastSendMessageIds[endPoint] = messageId;
            }
            SendServerRegisterMessage(endPoint, messageId);
        }
    }

    private void SendServerRegisterMessage(IPEndPoint endPoint, long messageId)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        UDPHeader header = UDPHeader.ServerRegisterMessage;
        writer.Write((int)header);
        writer.Write(messageId);
        byte[] data = stream.ToArray();
        udpListener.Send(data, data.Length, endPoint);
    }

    private void OnClientRegisterMessage(IPEndPoint endPoint, long messageId)
    {
        lock (lockHandle)
        {
            if (connections.ContainsKey(endPoint))
            {
                UDPServerConnection connection = connections[endPoint];
                if (connection.HasMessage(messageId))
                {
                    connection.DequeueMessage();
                }
            }
        }
    }

    public override void Tick()
    {
        lock (lockHandle)
        {
            List<IPEndPoint> toRemove = new List<IPEndPoint>();
            foreach (KeyValuePair<IPEndPoint, DateTime> lastTime in lastSendMessageTime)
            {
                double secondsSinceLastSeen = (DateTime.Now - lastTime.Value).TotalSeconds;
                if (secondsSinceLastSeen > DISCONNECTION_TIME_OUT)
                {
                    DisconnectConnection(lastTime.Key);
                    toRemove.Add(lastTime.Key);
                }
            }
            foreach (IPEndPoint endPoint in toRemove)
            {
                lastSendMessageTime.Remove(endPoint);
            }
            toRemove.Clear();
        }
    }

    private void ListenerThread()
    {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, listenPort);
        bool isRunning = true;
        while (isRunning)
        {
            try
            {
                byte[] bytes = udpListener.Receive(ref endPoint);
                if (bytes.Length < sizeof(int)) return;

                MemoryStream stream = new MemoryStream(bytes);
                BinaryReader reader = new BinaryReader(stream);
                int bytesReaded = 0;
                UDPHeader header = (UDPHeader)reader.ReadInt32();
                bytesReaded += sizeof(int);
                switch (header)
                {
                    case UDPHeader.ConnectionRequest:
                        OnConnectionRequest(endPoint);
                        break;
                    case UDPHeader.ClientSendMessage:
                        {
                            long id = reader.ReadInt64();
                            bytesReaded += sizeof(long);
                            byte[] data = reader.ReadBytes(bytes.Length - bytesReaded);
                            OnClientSendMessage(endPoint, id, data);
                        }
                        break;
                    case UDPHeader.ClientRegisterMessage:
                        {
                            long id = reader.ReadInt64();
                            bytesReaded += sizeof(long);
                            OnClientRegisterMessage(endPoint, id);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (SocketException e)
            {
                Debug.Log("Disposed: " + e);
            }
            catch (ObjectDisposedException e)
            {
                Debug.Log("Disposed: " + e);
                isRunning = false;
            }
        }
    }

    private void DisconnectConnection(IPEndPoint endPoint)
    {
        if (connections.ContainsKey(endPoint))
        {
            UDPServerConnection connection = connections[endPoint];
            connection.Disconnect();
            connections.Remove(endPoint);
            lastSendMessageIds.Remove(endPoint);
        }
    }
}