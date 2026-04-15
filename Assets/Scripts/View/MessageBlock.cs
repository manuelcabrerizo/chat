using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageBlock : MonoBehaviour
{
    [SerializeField] protected TMP_Text nameText;
    [SerializeField] protected TMP_Text messageText;
    [SerializeField] protected Button responseButton;

    public string Name => nameText.text;
    public string Message => messageText.text;

    private void Awake()
    {
        responseButton.onClick.AddListener(OnResponseButtonClick);
    }

    private void OnDestroy()
    {
        responseButton.onClick.RemoveAllListeners();
    }

    private void OnResponseButtonClick()
    {
        EventBus.Instance.Raise<ResponseMessageEvent>(this);
    }

    public void SetName(string name)
    {
        nameText.text = name;
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
