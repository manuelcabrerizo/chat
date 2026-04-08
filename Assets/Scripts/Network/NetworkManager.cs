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
    Message
}

public class NetworkManager : MonoBehaviour
{ 
    [SerializeField] private bool isServer = false;
    [SerializeField] private string address = "localhost";
    [SerializeField] private int port = 3000;
    [SerializeField] private NetworkProtocolType protocol = NetworkProtocolType.TCP;

    private Client client = null;
    private Server server = null;

    private void Awake()
    {
        server = isServer ? new Server(protocol, port) : null;
        client = !isServer ? new Client(protocol, address, port) : null;
    }

    private void OnDestroy()
    {
        client?.Shutdown();
        server?.Shutdown();
    }

    private void Update()
    {
        server?.Tick();
        client?.Tick();
    }
}

/*
TODO: ...
public enum NetworkRole
{
    Client,
    Server,
    Hybrid
}
*/