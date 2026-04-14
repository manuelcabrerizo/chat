using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPListener : Listener
{
    private UdpClient udpListener;
    private Thread listenerThread;
    private int listenPort;
    private int connectionIdGenerator;

    private Dictionary<IPEndPoint, UDPServerConnection> connections;

    public UDPListener(int port,
        Action<Connection> onConnectionAccepted,
        Action<Connection> onConnectionDisconnected) 
        : base(onConnectionAccepted, onConnectionDisconnected)
    {
        listenPort = port;
        connectionIdGenerator = 0;
        udpListener = new UdpClient(listenPort);
        connections = new Dictionary<IPEndPoint, UDPServerConnection>();
        
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
        if(!connections.ContainsKey(endPoint))
        {
            UDPServerConnection connection = new UDPServerConnection(
                udpListener, endPoint, null, onConnectionDisconnected);
            onConnectionAccepted?.Invoke(connection);
            connections.Add(endPoint, connection);
        }

        UDPHeader header = UDPHeader.ConnectionAccepted;
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)header);
        writer.Write(++connectionIdGenerator);
        byte[] data = stream.ToArray();
        udpListener.Send(data, data.Length, endPoint);
    }

    private void OnClientSendMessage(IPEndPoint endPoint, long messageId, byte[] message)
    {
        if (connections.ContainsKey(endPoint))
        {
            UDPServerConnection connection = connections[endPoint];
            if(!connection.HasMessage(messageId))
            {
                connection.EnqueueMessage(messageId, message);
            }
        }
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
        if (connections.ContainsKey(endPoint))
        {
            UDPServerConnection connection = connections[endPoint];
            if (connection.HasMessage(messageId))
            {
                connection.DequeueMessage();
            }
        }
    }

    private void ListenerThread()
    {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, listenPort);
        while (true)
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
                } break;
                case UDPHeader.ClientRegisterMessage:
                {
                    long id = reader.ReadInt64();
                    bytesReaded += sizeof(long);
                    OnClientRegisterMessage(endPoint, id);
                } break;
                default:
                    break;
            }
        }
    }
}