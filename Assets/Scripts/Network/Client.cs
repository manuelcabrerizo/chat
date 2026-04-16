public class Client : ITickable
{
    private Connection connection = null;

    public Client(NetworkProtocolType protocol, string address, int port)
    {
        connection = Connection.Create(protocol, address, port, OnConnected, OnDisconnected);
    }

    public void Shutdown()
    {
        if (connection.IsConnected)
        {
            EventBus.Instance.Unsubscribe<ClientSendDataEvent>(SendData);
        }
        connection.Close();
    }

    public void Tick(float deltaTime)
    {
        connection.Tick<ClientReciveDataEvent>(deltaTime);
    }

    private void OnConnected(Connection connection)
    {
        EventBus.Instance.Subscribe<ClientSendDataEvent>(SendData);
    }

    private void OnDisconnected(Connection connection)
    {
        Shutdown();
    }

    private void SendData(in ClientSendDataEvent sendDataEvent)
    {
        connection.SendData(sendDataEvent.Data);
    }
}
