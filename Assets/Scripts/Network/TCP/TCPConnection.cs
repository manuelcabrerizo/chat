using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;

class TCPConnection : Connection
{
    private TcpClient tcpClient = null;
    private Queue<byte[]> dataRecive = new Queue<byte[]>();
    private byte[] readBuffer = new byte[1024];
    private object readHandler = new object();

    public override bool IsConnected => tcpClient.Connected;

    public TCPConnection(string address, int port,
        Action<Connection> onConnected, Action<Connection> onDisconnected)
        : base(onConnected, onDisconnected)
    {
        tcpClient = new TcpClient();
        tcpClient.BeginConnect(address, port, OnConnectClient, null);
    }

    public TCPConnection(TcpClient client, 
        Action<Connection> onConnected, Action<Connection> onDisconnected)
        : base(onConnected, onDisconnected)
    {
        tcpClient = client;
        NetworkStream stream = tcpClient.GetStream();
        stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);
    }

    private void OnConnectClient(IAsyncResult asyncResult)
    {
        try
        {
            tcpClient.EndConnect(asyncResult);
            NetworkStream stream = tcpClient.GetStream();
            stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);
            onConnected?.Invoke(this);
        }
        catch (SocketException)
        {
            Debug.Log("The server is offline");
        }
        catch (ObjectDisposedException e)
        {
            Debug.Log("Disposed: " + e);
        }
    }

    public override void Close()
    {
        tcpClient.Close();
    }

    private void OnRead(IAsyncResult asyncResult)
    {
        try
        {
            NetworkStream stream = tcpClient.GetStream();
            if (stream.EndRead(asyncResult) == 0)
            {
                onDisconnected?.Invoke(this);
                return;
            }

            lock (readHandler)
            {
                byte[] data = readBuffer.TakeWhile(b => (char)b != '\0').ToArray();
                dataRecive.Enqueue(data);
            }

            Array.Clear(readBuffer, 0, readBuffer.Length);
            stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e);
        }
        catch (ObjectDisposedException)
        {
            Debug.Log("The server is offline");
        }
    }

    public override void SendData(byte[] data)
    {
        NetworkStream stream = tcpClient.GetStream();
        stream.Write(data, 0, data.Length);
    }

    public override void FlushReciveData<EventType>()
    {
        lock (readHandler)
        {
            while (dataRecive.Count > 0)
            {
                byte[] data = dataRecive.Dequeue();
                EventBus.Instance.Raise<EventType>(data);
            }
        }
    }
}

