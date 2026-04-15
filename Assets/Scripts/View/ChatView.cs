using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

enum MessageType
{ 
    Single,
    Response
}

public class ChatView : MonoBehaviour
{
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private GameObject responsePrefab;


    [SerializeField] private GameObject content;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button button;
    [SerializeField] private ScrollRect scrollRect;

    private string clientUserName;
    private MessageBlock responseMessage;

    private void Awake()
    {
        responseMessage = null;
        button.onClick.AddListener(OnSendButtonClick);
        EventBus.Instance.Subscribe<ClientReciveDataEvent>(OnReciveData);
        EventBus.Instance.Subscribe<ResponseMessageEvent>(OnResponseMessage);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
        EventBus.Instance.Unsubscribe<ResponseMessageEvent>(OnResponseMessage);
        EventBus.Instance.Unsubscribe<ClientReciveDataEvent>(OnReciveData);
    }

    public void SetUsername(string username)
    { 
        clientUserName = username;
    }

    private void OnSendButtonClick()
    {
        if (inputField.text.Length > 0)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            MessageType messageType = (responseMessage == null) ? MessageType.Single : MessageType.Response;
            writer.Write((int)messageType);
            byte[] usernameBytes = Encoding.UTF8.GetBytes(clientUserName);
            byte[] messageBytes = Encoding.UTF8.GetBytes(inputField.text);
            writer.Write(usernameBytes.Length);
            writer.Write(usernameBytes);
            writer.Write(messageBytes.Length);
            writer.Write(messageBytes);
            if (messageType == MessageType.Response)
            {
                byte[] responseUsernameBytes = Encoding.UTF8.GetBytes(responseMessage.Name);
                byte[] responsemMessageBytes = Encoding.UTF8.GetBytes(responseMessage.Message);
                writer.Write(responseUsernameBytes.Length);
                writer.Write(responseUsernameBytes);
                writer.Write(responsemMessageBytes.Length);
                writer.Write(responsemMessageBytes);
            }
            byte[] data = stream.ToArray();
            EventBus.Instance.Raise<ClientSendDataEvent>(data);

            inputField.text = "";
            responseMessage = null;
        }
    }

    private void OnReciveData(in ClientReciveDataEvent reciveDataEvent)
    {
        MemoryStream stream = new MemoryStream(reciveDataEvent.Data);
        BinaryReader reader = new BinaryReader(stream);

        MessageType messageType = (MessageType)reader.ReadInt32();
        int usernameLenght = reader.ReadInt32();
        byte[] usernameBytes = reader.ReadBytes(usernameLenght);
        int messageLenght = reader.ReadInt32();
        byte[] messageBytes = reader.ReadBytes(messageLenght);
        string username = Encoding.UTF8.GetString(usernameBytes);
        string message = Encoding.UTF8.GetString(messageBytes);
        if (messageType == MessageType.Single)
        {
            GameObject go = Instantiate(messagePrefab, content.transform);
            MessageBlock messageBlock = go.GetComponent<MessageBlock>();
            messageBlock.SetName(username);
            messageBlock.SetMessage(message);
        }
        else 
        {
            GameObject go = Instantiate(responsePrefab, content.transform);
            ResponseMessageBlock messageBlock = go.GetComponent<ResponseMessageBlock>();
            messageBlock.SetName(username);
            messageBlock.SetMessage(message);
            int responseUsernameLenght = reader.ReadInt32();
            byte[] responseUsernameBytes = reader.ReadBytes(responseUsernameLenght);
            int responseMessageLenght = reader.ReadInt32();
            byte[] responseMessageBytes = reader.ReadBytes(responseMessageLenght);
            string responseUsername = Encoding.UTF8.GetString(responseUsernameBytes);
            string responseMessage = Encoding.UTF8.GetString(responseMessageBytes);
            messageBlock.SetResponseName(responseUsername);
            messageBlock.SetResponseMessage(responseMessage);
        }
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0.0f;
    }

    private void OnResponseMessage(in ResponseMessageEvent responseEvent)
    {
        responseMessage = responseEvent.MessageBlock;
    }
}