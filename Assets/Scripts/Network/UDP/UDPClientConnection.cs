using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

class UDPClientConnection : Connection
{
    private UdpClient udpClient;
    private Thread clientThread;
    private bool clientThreadIsRunning;

    private bool isConnected;

    public UDPClientConnection(string address, int port,
        Action<Connection> onConnected, Action<Connection> onDisconnected)
        : base(onConnected, onDisconnected)
    {
        isConnected = false;
        udpClient = new UdpClient();
        udpClient.Connect(address, port);

        clientThread = new Thread(ClienThread);
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    public override bool IsConnected => isConnected;

    public override void Close()
    {
        clientThreadIsRunning = false;
        clientThread.Join();
        udpClient.Close();
    }

    public override void SendData(byte[] data)
    {
        UDPHeader header = UDPHeader.Message;
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)header);
        writer.Write(data);
        byte[] dataToSend = stream.ToArray();
        udpClient.Send(dataToSend, dataToSend.Length);
    }

    private void ClienThread()
    {
        IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 0);
        clientThreadIsRunning = true;
        while (clientThreadIsRunning)
        {
            if (!IsConnected)
            {
                TryToConnect();
            }
            ProcessMessages(ref groupEP);
        }
    }

    private void TryToConnect()
    {
        UDPHeader header = UDPHeader.ConnectionRequest;
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)header);
        byte[] data = stream.ToArray();
        udpClient.Send(data, data.Length);
    }

    private void ProcessMessages(ref IPEndPoint groupEP)
    {
        byte[] bytes = udpClient.Receive(ref groupEP);
        if (bytes.Length < sizeof(int)) return;

        MemoryStream stream = new MemoryStream(bytes);
        BinaryReader reader = new BinaryReader(stream);
        UDPHeader header = (UDPHeader)reader.ReadInt32();
        if (IsConnected)
        {
            byte[] data = reader.ReadBytes(bytes.Length - sizeof(int));
            if (header == UDPHeader.Message)
            {
            }
        }
        else
        {
            if (header == UDPHeader.ConnectionAccepted)
            {
                isConnected = true;
                onConnected?.Invoke(this);
            }
        }
    }

    public override void FlushReciveData<EventType>()
    {
        throw new NotImplementedException();
    }
}