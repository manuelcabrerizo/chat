public class ServerReciveDataEvent : Event
{
    public byte[] Data;

    public override void Initialize(params object[] parameters)
    {
        Data = (byte[])parameters[0];
    }
}

public class NetworkLoginRequestEvent : Event
{
    public string Username;
    public string Address;
    public int Port;
    public bool IsServer;
    public override void Initialize(params object[] parameters)
    {
        Username = (string)parameters[0];
        Address = (string)parameters[1];
        Port = (int)parameters[2];
        IsServer = (bool)parameters[3];
    }
}

public class NetworkLoginAcceptedEvent : Event
{
    public override void Initialize(params object[] parameters)
    {
    }
}