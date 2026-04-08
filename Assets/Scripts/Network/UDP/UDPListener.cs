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
    private bool listenerThreadIsRunning;
    private int listenPort;
    private List<IPEndPoint> acceptedEndpoints;

    public UDPListener(int port,
        Action<Connection> onConnectionAccepted,
        Action<Connection> onConnectionDisconnected) 
        : base(onConnectionAccepted, onConnectionDisconnected)
    {
        listenPort = port;
        udpListener = new UdpClient(listenPort);
        acceptedEndpoints = new List<IPEndPoint>();

        listenerThread = new Thread(ListenerThread);
        listenerThread.IsBackground = true;
        listenerThread.Start();
    }

    private void OnClientAccepted(UdpClient client)
    {
    }

    public override void Stop()
    {
        listenerThreadIsRunning = false;
        listenerThread.Join();
        udpListener.Close();
    }

    private void AcceptConnection(IPEndPoint endPoint)
    { 
        if (!acceptedEndpoints.Contains(endPoint))
        {
            Connection connection = new UDPServerConnection(
                udpListener, endPoint, null, onConnectionDisconnected);
            onConnectionAccepted?.Invoke(connection);
            acceptedEndpoints.Add(endPoint);
        }

        UDPHeader header = UDPHeader.ConnectionAccepted;
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)header);
        byte[] data = stream.ToArray();
        udpListener.Send(data, data.Length, endPoint);
    }

    private void ListenerThread()
    {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, listenPort);
        listenerThreadIsRunning = true;
        while (listenerThreadIsRunning)
        {
            byte[] bytes = udpListener.Receive(ref endPoint);
            if (bytes.Length < sizeof(int)) return;

            MemoryStream stream = new MemoryStream(bytes);
            BinaryReader reader = new BinaryReader(stream);
            UDPHeader header = (UDPHeader)reader.ReadInt32();
            switch (header)
            {
                case UDPHeader.ConnectionRequest:
                    AcceptConnection(endPoint);
                    break;
                case UDPHeader.Message:
                {
                    byte[] data = reader.ReadBytes(bytes.Length - sizeof(int));
                } break;
                default:
                    break;
            }
        }
    }

}