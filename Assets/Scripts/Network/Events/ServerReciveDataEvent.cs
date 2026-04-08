public class ServerReciveDataEvent : Event
{
    public byte[] Data;

    public override void Initialize(params object[] parameters)
    {
        Data = (byte[])parameters[0];
    }
}