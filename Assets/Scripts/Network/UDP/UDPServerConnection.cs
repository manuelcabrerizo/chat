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

    public override void SendData(byte[] data)
    {
        udpClient.Send(data, data.Length, endPoint);
    }

    public override void FlushReciveData<EventType>()
    {
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
        EventBus.Instance.Raise<EventType>(data);
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

    public void EnqueueMessage(long id, byte[] message)
    {
        UDPMessage udpMessage;
        udpMessage.Id = id;
        udpMessage.Data = message;
        toSendMessages.Enqueue(udpMessage);
    }

    public void DequeueMessage()
    { 
        toSendMessages.Dequeue();
    }
}