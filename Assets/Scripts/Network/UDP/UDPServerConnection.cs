using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

public class UDPServerConnection : Connection
{
    private UdpClient udpClient = null;
    private IPEndPoint endPoint = null;
    private Queue<UDPMessage> toSendMessages;

    private float TIMER_PER_TICK = 16.0f / 1000.0f;
    private float timer = 0;

    public UDPServerConnection(UdpClient udpClient, IPEndPoint endPoint,
        Action<Connection> onConnected, Action<Connection> onDisconnected)
        : 
        
        base(onConnected, onDisconnected)
    {
        this.udpClient = udpClient;
        this.endPoint = endPoint;
        toSendMessages = new Queue<UDPMessage>();
    }

    public override bool IsConnected => endPoint != null;

    public override void Close()
    {
        udpClient = null;
        endPoint = null;
    }

    public override void SendData(byte[] bytes)
    {
        MemoryStream stream = new MemoryStream(bytes);
        BinaryReader reader = new BinaryReader(stream);
        UDPMessage udpMessage = new UDPMessage();
        udpMessage.Id = (long)reader.ReadUInt64();
        udpMessage.Data = reader.ReadBytes(bytes.Length - sizeof(long));
        toSendMessages.Enqueue(udpMessage);
    }

    public override void Tick<EventType>(float deltaTime)
    {
        if (timer < TIMER_PER_TICK)
        {
            timer += deltaTime;
            return;
        }
        timer -= TIMER_PER_TICK;

        if (toSendMessages.Count <= 0)
        {
            return;
        }
        UDPMessage message = toSendMessages.Peek();
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        UDPHeader header = UDPHeader.ServerSendMessage;
        writer.Write((int)header);
        writer.Write(message.Id);
        writer.Write(message.Data);
        byte[] data = stream.ToArray();
        udpClient.Send(data, data.Length, endPoint);
    }

    public bool HasMessage(long id)
    {
        foreach (UDPMessage message in toSendMessages)
        {
            if (message.Id == id)
            {
                return true;
            }
        }
        return false;
    }

    public void DequeueMessage()
    { 
        toSendMessages.Dequeue();
    }

    public void Disconnect()
    {
        onDisconnected?.Invoke(this);
    }
}