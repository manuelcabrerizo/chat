using TMPro;
using UnityEngine;

public class MessageBlock : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private RectTransform messageBlock;
    [SerializeField] private TMP_Text messageText;
    public void SetName(string name)
    {
        nameText.text = name;
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
        /*
        float currentWidth = (messageBlock.parent as RectTransform).rect.width;
        Vector2 adjustedBoxSize = messageText.GetPreferredValues(message, currentWidth, Mathf.Infinity);
        messageBlock.sizeDelta = new Vector2(messageBlock.sizeDelta.x, adjustedBoxSize.y);
        */
    }
}
