public class NetworkLoginRequestEvent : Event
{
    public string Address;
    public int Port;
    public bool IsServer;
    public override void Initialize(params object[] parameters)
    {
        Address = (string)parameters[0];
        Port = (int)parameters[1];
        IsServer = (bool)parameters[2];
    }
}
