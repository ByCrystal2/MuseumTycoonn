using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    [SerializeField] GameObject notificationsCanvas;
    [SerializeField] Transform notificationContent;
    [SerializeField] Transform notificationTabPanel;
    [SerializeField] NotificationHandler notificationPrefab;
    [SerializeField] private float notificationCheckInterval = 0.05f;
    private NotificationIconHandler[] icons;
    List<Notification> notifications = new List<Notification>();
    public List<Notification> Notifications => notifications;
    public TaskCompletionSource<DG.Tweening.Tween> emptyTween = new TaskCompletionSource<DG.Tweening.Tween>();
    public List<Notification> ActiveNotificationHandlers = new List<Notification>();
    public static NotificationManager instance { get; private set; }

    [Header("Only Unity Editor")]
    [SerializeField] bool sendExampleErrorNotification = false;
    [SerializeField] bool sendExampleWarningNotification = false;
    [SerializeField] bool sendExampleInfoNotification = false;

    private void Awake()
    {
        if (instance && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        sendExampleErrorNotification = false;
        sendExampleWarningNotification = false;
        sendExampleInfoNotification = false;
    }

    private void Start()
    {
        icons = Resources.LoadAll<NotificationIconHandler>("NotificationIcons");
        int length = notificationTabPanel.childCount;
        List<UnityEngine.UI.Button> tabButtons = new List<UnityEngine.UI.Button> ();

        for (int i = 0; i < length; i++)
            if (notificationTabPanel.GetChild(i).TryGetComponent(out UnityEngine.UI.Button button))
                tabButtons.Add(button);

        for (int i = 0;i < tabButtons.Count; i++)
            tabButtons[i].onClick.AddListener(() => FillNotificationPanelContent((NotificationType)System.Enum.ToObject(typeof(NotificationType), i)));
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (sendExampleErrorNotification)
        {
            sendExampleErrorNotification = false;
            SendNotification(new Notification(9900, "This is an Error test message", 2f, NotificationType.System,NotificationState.Error));
        }
        if (sendExampleWarningNotification)
        {
            sendExampleWarningNotification = false;
            SendNotification(new Notification(9901, "This is an Warning test message", 1f,NotificationType.Visitor, NotificationState.Warning));
        }
        if (sendExampleInfoNotification)
        {
            sendExampleInfoNotification = false;
            SendNotification(new Notification(9902, "This is an Information test message", 10f, NotificationType.Worker, NotificationState.Information));
        }
#endif
    }

    public void NotificationInit()
    {
        Notification n1 = new Notification(1,
            "You have changed the language frequently. Please wait a while before changing it again.", NotificationType.System,
            NotificationState.Error,
            async () =>
            {
                await Task.Delay((2 * 5) * 1000);
                MainMenu.instance.ResetLanguageChangedValues();
                Debug.Log("Notification completed.");
                return true;
            });
        notifications.Add(n1);
        StartCoroutine(NotificationLoop());
    }

    private IEnumerator NotificationLoop()
    {
        while (true)
        {
            if (notificationsCanvas.activeSelf == false && notificationContent.childCount > 0)
            {
                notificationsCanvas.SetActive(true);
            }

            for (int i = 0; i < notificationContent.childCount; i++)
            {
                var notificationHandler = notificationContent.GetChild(i).GetComponent<NotificationHandler>();
                if (notificationHandler != null)
                {
                    if (!notificationHandler.IsProcessStarted)
                    {
                        yield return notificationHandler.StartNotificationProcess();
                        Debug.Log("yield return StartNotificationProcess");
                    }                    
                }
            }

            // Eðer hiç bildirim yoksa canvas'ý kapat
            if (notificationContent.childCount == 0 && notificationsCanvas.activeSelf)
            {
                notificationsCanvas.SetActive(false);
            }

            yield return new WaitForSeconds(notificationCheckInterval);
        }
    }

    public void SendNotification(Notification notification)
    {
        ActiveNotificationHandlers.Add(notification);
    }
    public void FillNotificationPanelContent(NotificationType _notificationType)
    {
        List<Notification> targetNotifications = ActiveNotificationHandlers.Where(x=> x.NotificationType == _notificationType).ToList();
        ClearNotificationPanelContent();
        foreach (var notification in targetNotifications)
        {
            var notificationHandler = Instantiate(notificationPrefab, notificationContent);
            notificationHandler.SetNotification(notification);
            if (notificationHandler.GetNotification().NotificationState == NotificationState.Error)
                notificationHandler.gameObject.transform.SetSiblingIndex(0);
        }
    }
    void ClearNotificationPanelContent()
    {
        int length = notificationContent.childCount;
        for (int i = 0; i < length; i++)
            if (notificationContent.GetChild(i).TryGetComponent(out NotificationHandler handler))
                DestroyImmediate(handler.gameObject);
    }
    public GameObject GetIcon(NotificationState state)
    {
        return icons[(int)state].gameObject;
    }
}

//[System.Serializable]
public class Notification
{
    public int ID;
    public string Message;
    public float DelayTime;
    public bool IsDestroyable;
    public NotificationState NotificationState;
    public NotificationType NotificationType;
    public System.Func<Task<bool>> OnComplate;
    public int AlertCount { get; set; }
    public Notification(int _id,string _message, float _delayTime, NotificationType _notificationType, NotificationState notificationState)
    {
        ID = _id;
        Message = _message;
        DelayTime = _delayTime;
        IsDestroyable = true;
        NotificationType = _notificationType;
        NotificationState = notificationState;
        AlertCount = 0;
    }
    public Notification(int _id, string _message, NotificationType _notificationType, NotificationState notificationState,System.Func<Task<bool>> _onComplate)
    {
        ID = _id;
        Message = _message;
        IsDestroyable = false;
        NotificationType = _notificationType;
        NotificationState = notificationState;
        OnComplate = _onComplate;
        AlertCount = 0;
    }
    public Notification(Notification copy)
    {
        ID = copy.ID;
        Message = copy.Message;
        DelayTime = copy.DelayTime;
        IsDestroyable = copy.IsDestroyable;
        NotificationType = copy.NotificationType;
        NotificationState = copy.NotificationState;
        OnComplate = copy.OnComplate;
        AlertCount = copy.AlertCount;
    }
    public async Task<bool> StartComplateFunction()
    {
        if (OnComplate != null)
        {
            return await OnComplate.Invoke();
        }
        return true;
    }
}
public enum NotificationState // => DIKKAT! Bu degerler Assets/Resources/NotificationIcons yolunda ki prefab siralamasini baz almaktadir.
{
    Error,
    Information,
    Warning,    
}
public enum NotificationType
{
    All,
    System,
    Visitor,
    Worker //not: isci maasi odenmemesi bir sistem bildirimidir,worker bildirimi degil. (ornek isci bildirimi: "cok fazla kavga oluyor yetisemiyorum.")
}