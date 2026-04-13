using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class TCPListener : Listener
{
    private TcpListener tcpListener = null;

    public TCPListener(int port,
        Action<Connection> onConnectionAccepted,
        Action<Connection> onConnectionDisconnected)
        : base(onConnectionAccepted, onConnectionDisconnected)
    {
        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(OnClientAccepted, null);
    }

    private void OnClientAccepted(IAsyncResult ar)
    {
        try
        {
            TcpClient client = tcpListener.EndAcceptTcpClient(ar);
            Connection connection = new TCPConnection(client, null, onConnectionDisconnected);
            onConnectionAccepted?.Invoke(connection);
            tcpListener.BeginAcceptTcpClient(OnClientAccepted, null);
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e);
        }
        catch (ObjectDisposedException e)
        {
            Debug.Log("Disposed: " + e);
        }
    }

    public override void Stop()
    {
        tcpListener.Stop();
    }
}