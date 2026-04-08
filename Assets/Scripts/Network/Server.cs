using System.Collections.Generic;

public class Server : ITickable
{
    Listener listener = null;
    List<Connection> connections;

    public Server(NetworkProtocolType protocol, int port)
    {
        listener = Listener.Create(protocol, port, OnConnectionAccepted, OnConnectionDisconnected);
        connections = new List<Connection>();
        EventBus.Instance.Subscribe<ServerReciveDataEvent>(OnReciveData);
    }

    public void Shutdown()
    {
        EventBus.Instance.Unsubscribe<ServerReciveDataEvent>(OnReciveData);
        listener.Stop();
    }

    public void Tick()
    {
        foreach (Connection connection in connections)
        {
            connection.FlushReciveData<ServerReciveDataEvent>();
        }
    }

    private void OnConnectionAccepted(Connection connection)
    {
        connections.Add(connection);
    }

    private void OnConnectionDisconnected(Connection connection)
    {
        connection.Close();
        connections.Remove(connection);
    }

    private void OnReciveData(in ServerReciveDataEvent reciveDataEvent)
    {
        foreach (Connection connection in connections)
        {
            connection.SendData(reciveDataEvent.Data);
        }
    }
}