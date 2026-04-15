using System;
using UnityEngine;

public enum NetworkProtocolType
{
    TCP,
    UDP
}

public enum UDPHeader
{ 
    ConnectionRequest,
    ConnectionAccepted,
    ClientSendMessage,
    ServerRegisterMessage,
    ServerSendMessage,
    ClientRegisterMessage
}

public struct UDPMessage
{
    public long Id;
    public byte[] Data;
}

public class NetworkManager : MonoBehaviour
{ 
    [SerializeField] private NetworkProtocolType protocol = NetworkProtocolType.TCP;

    private Client client = null;
    private Server server = null;

    private void Awake()
    {
        EventBus.Instance.Subscribe<NetworkLoginRequestEvent>(OnLoginRequest);
    }

    private void OnDestroy()
    {
        client?.Shutdown();
        server?.Shutdown();

        EventBus.Instance.Unsubscribe<NetworkLoginRequestEvent>(OnLoginRequest);
    }

    private void Update()
    {
        server?.Tick();
        client?.Tick();
    }

    private void OnLoginRequest(in NetworkLoginRequestEvent networkLoginRequestEvent)
    {
        string username = networkLoginRequestEvent.Username;
        string address = networkLoginRequestEvent.Address;
        int port = networkLoginRequestEvent.Port;
        bool isServer = networkLoginRequestEvent.IsServer;
        server = isServer ? new Server(protocol, port) : null;
        client = new Client(protocol, address, port);
        EventBus.Instance.Raise<NetworkLoginAcceptedEvent>();
    }
}