using System;

public abstract class Connection
{
    protected Action<Connection> onConnected;
    protected Action<Connection> onDisconnected;

    public static Connection Create(NetworkProtocolType protocol, string address, int port,
        Action<Connection> onConnected, Action<Connection> onDisconnected)
    {
        switch (protocol)
        {
            case NetworkProtocolType.TCP:
                return new TCPConnection(address, port, onConnected, onDisconnected);
            case NetworkProtocolType.UDP:
                return new UDPClientConnection(address, port, onConnected, onDisconnected);
            default:
                return null;
        }
    }

    public Connection(Action<Connection> onConnected, Action<Connection> onDisconnected)
    {
        this.onConnected = onConnected;
        this.onDisconnected = onDisconnected;
    }

    public abstract bool IsConnected { get; }
    public abstract void FlushReciveData<EventType>() where EventType : Event, new();
    public abstract void SendData(byte[] data);
    public abstract void Close();
}