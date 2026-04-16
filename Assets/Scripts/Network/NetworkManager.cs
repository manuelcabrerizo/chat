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
#if UNITY_SERVER
        string[] args = Environment.GetCommandLineArgs();

        int port = 0;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-port" && (i + 1) < args.Length)
            {
                port = int.Parse(args[i + 1]);
            }
        }
        if (port == 0)
        {
            Debug.Log("Usage: Chat.exe -port 3000");
            Application.Quit();
        }

        server = port != 0 ? new Server(protocol, port) : null;
#else
        EventBus.Instance.Subscribe<NetworkLoginRequestEvent>(OnLoginRequest);
#endif
    }

    private void OnDestroy()
    {
        client?.Shutdown();
        server?.Shutdown();
#if UNITY_SERVER
#else
        EventBus.Instance.Unsubscribe<NetworkLoginRequestEvent>(OnLoginRequest);
#endif
    }

    private void Update()
    {
        server?.Tick(Time.deltaTime);
        client?.Tick(Time.deltaTime);
    }

    private void OnLoginRequest(in NetworkLoginRequestEvent networkLoginRequestEvent)
    {
        string address = networkLoginRequestEvent.Address;
        int port = networkLoginRequestEvent.Port;
        bool isServer = networkLoginRequestEvent.IsServer;
        server = isServer ? new Server(protocol, port) : null;
        client = new Client(protocol, address, port);
        EventBus.Instance.Raise<NetworkLoginAcceptedEvent>();
    }
}