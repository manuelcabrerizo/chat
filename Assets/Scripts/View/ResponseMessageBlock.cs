using TMPro;
using UnityEngine;

public class ResponseMessageBlock : MessageBlock
{
    [SerializeField] private TMP_Text responseNameText;
    [SerializeField] private TMP_Text responseMessageText;
    public void SetResponseName(string name)
    {
        responseNameText.text = name;
    }
    public void SetResponseMessage(string message)
    {
        responseMessageText.text = message;
    }
}
