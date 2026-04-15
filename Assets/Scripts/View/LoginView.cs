using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginView : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private TMP_InputField addressInputField;
    [SerializeField] private TMP_InputField portInputField;
    [SerializeField] private Toggle isServerToggle;
    [SerializeField] private Button button;
    [SerializeField] private GameObject chatPanel;

    private void Awake()
    {
        button.onClick.AddListener(OnLoginButtonClick);
        EventBus.Instance.Subscribe<NetworkLoginAcceptedEvent>(OnLogin);
    }

    private void OnDestroy()
    {
        EventBus.Instance.Unsubscribe<NetworkLoginAcceptedEvent>(OnLogin);
        button.onClick.RemoveAllListeners();
    }

    private void OnLoginButtonClick()
    {
        if (InputIsValid())
        {
            string address = addressInputField.text;
            int port = int.Parse(portInputField.text);
            bool isServer = isServerToggle.isOn;
            EventBus.Instance.Raise<NetworkLoginRequestEvent>(address, port, isServer);
        }
    }

    private void OnLogin(in NetworkLoginAcceptedEvent networkLoginAcceptedEvent)
    {
        ChatView chatView = chatPanel.GetComponent<ChatView>();
        chatView.SetUsername(usernameInputField.text);
        chatPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    private bool InputIsValid()
    {
        if (usernameInputField.text.Length <= 0)
        {
            return false;
        }
        if (addressInputField.text.Length <= 0)
        {
            return false;
        }
        if (portInputField.text.Length <= 0)
        {
            return false;
        }
        return true;
    }
}
