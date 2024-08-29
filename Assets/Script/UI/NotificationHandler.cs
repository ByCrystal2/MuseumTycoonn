using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIFade))]
public class NotificationHandler : MonoBehaviour
{
    [SerializeField] private Transform iconContent;
    [SerializeField] private Text messageText;

    private Notification notification;
    public UIFade uiFade { get; private set; }    
    private void Awake()
    {
        uiFade = GetComponent<UIFade>();
    }

    public void SetNotification(Notification newNotification)
    {
        notification = new Notification(newNotification);
        ClearIcons();
        SetIcon();
        messageText.text = notification.Message;
    }

    public Notification GetNotification() { return notification; }


    private void ClearIcons()
    {
        foreach (Transform child in iconContent)
        {
            Destroy(child.gameObject);
        }
    }

    private void SetIcon()
    {
        var iconPrefab = NotificationManager.instance.GetIcon(notification.NotificationState);
        if (iconPrefab)
        {
            Instantiate(iconPrefab, iconContent);
        }
    }
}
