using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

public class UDPServerConnection : Connection
{
    private UdpClient udpClient = null;
    private IPEndPoint endPoint = null;
    public UDPServerConnection(UdpClient udpClient, IPEndPoint endPoint,
        Action<Connection> onConnected, Action<Connection> onDisconnected)
        : base(onConnected, onDisconnected)
    {
        this.udpClient = udpClient;
        this.endPoint = endPoint;
    }

    public override bool IsConnected => endPoint != null;

    public override void Close()
    {
        udpClient = null;
        endPoint = null;
    }

    public override void FlushReciveData<EventType>()
    {
        throw new NotImplementedException();
    }

    public override void SendData(byte[] bytes)
    {
        UDPHeader header = UDPHeader.Message;
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)header);
        writer.Write(bytes);
        byte[] data = stream.ToArray();
        udpClient.Send(data, data.Length, endPoint);
    }
}