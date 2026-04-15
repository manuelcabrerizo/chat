using UnityEngine;
public class ResponseMessageEvent : Event
{
    public MessageBlock MessageBlock;

    public override void Initialize(params object[] parameters)
    {
        MessageBlock = (MessageBlock)parameters[0];
    }
}