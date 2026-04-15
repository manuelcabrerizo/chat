using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatView : MonoBehaviour
{
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private GameObject content;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button button;
    [SerializeField] private ScrollRect scrollRect;

    private List<MessageBlock> messages;

    private void Start()
    {
        messages = new List<MessageBlock>();
        button.onClick.AddListener(OnSendButtonClick);
        EventBus.Instance.Subscribe<ClientReciveDataEvent>(OnReciveData);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
        EventBus.Instance.Unsubscribe<ClientReciveDataEvent>(OnReciveData);
    }

    private void OnSendButtonClick()
    {
        if (inputField.text.Length > 0)
        {
            byte[] data = Encoding.UTF8.GetBytes(inputField.text);
            EventBus.Instance.Raise<ClientSendDataEvent>(data);
            inputField.text = "";
        }
    }

    private void OnReciveData(in ClientReciveDataEvent reciveDataEvent)
    {
        string message = Encoding.UTF8.GetString(reciveDataEvent.Data);
        
        GameObject go = Instantiate(messagePrefab, content.transform);

        MessageBlock messageBlock = go.GetComponent<MessageBlock>();
        messageBlock.SetName("Manu");
        messageBlock.SetMessage(message);
        messages.Add(messageBlock);

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0.0f;
    }
}