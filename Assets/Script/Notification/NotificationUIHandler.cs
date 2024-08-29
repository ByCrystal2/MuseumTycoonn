using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationUIHandler : MonoBehaviour
{
    [SerializeField] GameObject imgTick;
    [SerializeField] Text txtStack;
    public bool IsActive = false;
    SenderHelper Sender;
    public void SetSender(SenderHelper sender)
    {
        Sender = sender;
    }
    public SenderHelper GetSender()
    {
        return Sender;
    }
    public void UpdateStackText(int stack)
    {
        txtStack.text = stack.ToString();
    }
    public void OnClickNotification()
    {
        Mark(!IsActive);
    }
    public void Mark(bool _open)
    {
        imgTick.SetActive(_open);
        IsActive = _open;
    }
}
