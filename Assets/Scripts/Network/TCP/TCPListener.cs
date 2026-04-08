using System;
using System.Net;
using System.Net.Sockets;

public class TCPListener : Listener
{
    private bool isStopped = false;
    private TcpListener tcpListener = null;

    public TCPListener(int port,
        Action<Connection> onConnectionAccepted,
        Action<Connection> onConnectionDisconnected)
        : base(onConnectionAccepted, onConnectionDisconnected)
    {
        isStopped = false;
        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(OnClientAccepted, null);
    }

    private void OnClientAccepted(IAsyncResult ar)
    {
        //if (isStopped) return;

        TcpClient client = tcpListener.EndAcceptTcpClient(ar);
        Connection connection = new TCPConnection(client, null, onConnectionDisconnected);
        onConnectionAccepted?.Invoke(connection);
        tcpListener.BeginAcceptTcpClient(OnClientAccepted, null);
    }

    public override void Stop()
    {
        if (isStopped) return;
        isStopped = true;

        tcpListener.Stop();
    }
}