using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatView : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button button;

    private void Start()
    {
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
        byte[] data = Encoding.UTF8.GetBytes(inputField.text);
        EventBus.Instance.Raise<ClientSendDataEvent>(data);
        inputField.text = "";
    }

    private void OnReciveData(in ClientReciveDataEvent reciveDataEvent)
    {
        string message = Encoding.UTF8.GetString(reciveDataEvent.Data) + "\n";
        text.text += message;
    }
}