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
    private UIFade uiFade;

    public bool IsProcessStarted;
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

    public async Task<Tween> StartNotificationProcess()
    {
        if (transform.GetSiblingIndex() > 4) return await NotificationManager.instance.emptyTween.Task;
        IsProcessStarted = true;
        var tcs = new TaskCompletionSource<Tween>();

        Tween fadeInTween = uiFade.Fade(1, 0.2f);
        await fadeInTween.AsyncWaitForCompletion();

        if (notification.IsDestroyable)
        {
            Tween fadeOutTween = uiFade.Fade(0.2f, notification.DelayTime);
            await fadeOutTween.AsyncWaitForCompletion();
            DestroyImmediate(gameObject);
            tcs.SetResult(fadeInTween);
        }
        else
        {
            
            notification.AlertCount++;            
            await notification.StartComplateFunction(); // Burada asenkron fonksiyonun tamamlanmasýný bekliyoruz
            notification.IsDestroyable = true;
            await StartNotificationProcess();
            Debug.Log("StartComplateFunction() await end.");
            tcs.SetResult(fadeInTween);
        }
        IsProcessStarted = false;
        return await tcs.Task;
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
