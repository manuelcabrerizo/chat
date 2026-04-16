using System;

public abstract class Listener
{
    protected Action<Connection> onConnectionAccepted;
    protected Action<Connection> onConnectionDisconnected;

    public static Listener Create(NetworkProtocolType protocol, int port,
        Action<Connection> onConnectionAccepted, Action<Connection> onConnectionDisconnected)
    {
        switch (protocol)
        {
            case NetworkProtocolType.TCP:
                return new TCPListener(port, onConnectionAccepted, onConnectionDisconnected);
            case NetworkProtocolType.UDP:
                return new UDPListener(port, onConnectionAccepted, onConnectionDisconnected);
            default:
                return null;
        }
    }

    public Listener(
        Action<Connection> onConnectionAccepted,
        Action<Connection> onConnectionDisconnected)
    {
        this.onConnectionAccepted = onConnectionAccepted;
        this.onConnectionDisconnected = onConnectionDisconnected;
    }

    public abstract void Tick();
    public abstract void Stop();
}