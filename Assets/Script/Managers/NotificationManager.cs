using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
public class NotificationManager : MonoBehaviour
{
    [Header("V1 Notification")]
    [SerializeField] public GameObject notificationsCanvas_V1;
    [SerializeField] Transform notificationContent_V1;
    [SerializeField] Transform notificationTabPanel_V1;
    [SerializeField] NotificationHandler notificationPrefab_V1;
    [SerializeField] Button MarkAllButton;
    [SerializeField] Button ReadAllButton;
    NotificationType currentNotificationType_V1;
    [Header("V2 Notification")]
    [SerializeField] GameObject notificationsCanvas_V2;
    [SerializeField] public Transform notificationContent_V2;
    [SerializeField] NotificationHandler notificationPrefab_V2;
    [SerializeField] private float notificationCheckInterval_V2 = 0.05f;
    private NotificationIconHandler[] icons;
    List<Notification> notifications = new List<Notification>();
    public List<Notification> Notifications => notifications;
    public TaskCompletionSource<DG.Tweening.Tween> emptyTween = new TaskCompletionSource<Tween>();
    public List<NotificationHelper> ActiveNotifications = new List<NotificationHelper>();
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
        currentNotificationType_V1 = NotificationType.All;
        int length = notificationTabPanel_V1.childCount;
        List<Button> tabButtons = new List<Button> ();

        for (int i = 0; i < length; i++)
            if (notificationTabPanel_V1.GetChild(i).TryGetComponent(out Button button))
                tabButtons.Add(button);

        for (int i = 0; i < tabButtons.Count; i++)
        {
            int capturedIndex = i;
            tabButtons[capturedIndex].onClick.AddListener(() => FillNotificationPanelContent((NotificationType)System.Enum.ToObject(typeof(NotificationType), capturedIndex)));
        }
        MarkAllButton.onClick.AddListener(MarkAllNotifications);
        ReadAllButton.onClick.AddListener(ReadAllMarkedNotifications);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (sendExampleErrorNotification)
        {
            sendExampleErrorNotification = false;
            SendNotification(new Notification(9900, "This is an Error test message", 2f, NotificationType.System,NotificationState.Error,1),new SenderHelper(WhoSends.System,9999));
        }
        if (sendExampleWarningNotification)
        {
            sendExampleWarningNotification = false;
            SendNotification(new Notification(9901, "This is an Warning test message", 1f,NotificationType.Visitor, NotificationState.Warning,1), new SenderHelper(WhoSends.Visitor,9999));
        }
        if (sendExampleInfoNotification)
        {
            sendExampleInfoNotification = false;
            SendNotification(new Notification(9902, "This is an Information test message", 10f, NotificationType.Worker, NotificationState.Information,1), new SenderHelper(WhoSends.Worker,9999));
        }
#endif
    }

    public void NotificationInit()
    {
        Notification n1 = null, n2 = null, n3 = null, n4 = null;
        n1 = new Notification(1,
            "You have changed the language frequently. Please wait a while before changing it again.",10.0f, NotificationType.System,
            NotificationState.Error,
            async () =>
            {
                MainMenu.instance.AllLanguageButtonInteractable(false);
                await Task.Delay(((int)n1.DelayTime) * 1000);
                MainMenu.instance.ResetLanguageChangedValues();
                Debug.Log("Notification completed.");
                return true;
            },5);
        n2 = new Notification(2,
            "A worker didn't get his salary", 10.0f, NotificationType.System,
            NotificationState.Warning,
            async () =>
            {                
                return true;
            }, 3);
        n3 = new Notification(3,
            "Was transferred to inventory because a worker was not paid his salary.", 10.0f, NotificationType.System,
            NotificationState.Error,
            async () =>
            {
                
                return true;
            }, 3);
        n4 = new Notification(4, "Selected notifications read.", 1f, NotificationType.System, NotificationState.Information, 1);
        notifications.Add(n1);
        notifications.Add(n2);
        notifications.Add(n3);
        notifications.Add(n4);
        StartCoroutine(V2NotificationLoop());
    }
    public Notification GetNotificationWithID(int _id)
    {
        Notification result = Notifications.Where(x => x.ID == _id).SingleOrDefault();
        return result;
    }
    public NotificationHelper GetNotificationHelperWithID(int _id)
    {
        NotificationHelper result = ActiveNotifications.Where(x => x.Notification.ID == _id).SingleOrDefault();
        return result;
    }
    // Sýnýfýn içinde bir alan olarak tanýmlayýn
    private bool isNotificationLoopRunning = false;

    private IEnumerator V2NotificationLoop()
    {
        while (true)
        {
            if (!notificationsCanvas_V2.activeSelf && notificationContent_V2.childCount > 0)
            {
                notificationsCanvas_V2.SetActive(true);
            }

            for (int i = 0; i < notificationContent_V2.childCount; i++)
            {
                var notificationHandler = notificationContent_V2.GetChild(i).GetComponent<NotificationHandler>();

                if (notificationHandler != null)
                {
                    Notification currentNotification = notificationHandler.GetNotification();

                    // Bildirimi göster
                    if (!currentNotification.IsProcessStarted)
                    {
                        Debug.Log("Starting notification process...");
                        yield return currentNotification.StartNotificationProcess();
                        Debug.Log("Notification process complete.");
                    }

                    // Fade iþlemi ve bildirim yok etme
                    if (currentNotification.IsDestroyable)
                    {
                        Debug.Log("Fading out notification...");
                        Tween fadeOutTween = notificationHandler.uiFade.Fade(0.2f, currentNotification.DelayTime);
                        yield return fadeOutTween.AsyncWaitForCompletion();  // Fade-out iþlemi tamamlanana kadar bekle

                        // Tween tamamlandýktan sonra bildirimi yok et
                        Destroy(notificationHandler.gameObject);
                    }
                    else
                    {
                        Debug.Log("Fading in notification...");
                        Tween fadeInTween = notificationHandler.uiFade.Fade(1, 0.2f);
                        yield return fadeInTween.AsyncWaitForCompletion();  // Fade-in iþlemi tamamlanana kadar bekle
                    }
                }
            }

            // Eðer hiç bildirim yoksa canvas'ý kapat ve Coroutine'i durdur
            if (notificationContent_V2.childCount == 0 && notificationsCanvas_V2.activeSelf)
            {
                notificationsCanvas_V2.SetActive(false);
                isNotificationLoopRunning = false;  // Coroutine durdurulduðu için kontrolü sýfýrla
                yield break;  // Coroutine'i sonlandýr
            }

            yield return new WaitForSeconds(notificationCheckInterval_V2);
        }
    }

    public void SendNotification(Notification _notification, SenderHelper _sender, int _whichVersion = 1)
    {
        _notification.AlertCount++;

        if (_whichVersion == 1)
        {
            int index = ActiveNotifications.FindIndex(x => x.Notification.ID == _notification.ID &&
                                                           x.SenderHelper.SenderID == _sender.SenderID &&
                                                           x.SenderHelper.Sender == _sender.Sender);

            if (index != -1)
            {
                NotificationHelper TargetHelper = ActiveNotifications[index];
                TargetHelper.StackCount++;
                ActiveNotifications[index] = TargetHelper;
            }
            else
            {
                ActiveNotifications.Add(new NotificationHelper(_notification, _sender));
            }

            FillNotificationPanelContent(currentNotificationType_V1);
        }
        else if (_whichVersion == 2)
        {
            if (_notification.AlertCount >= _notification.TriggerAlertNumber)
            {
                CreateNotificationInContent(notificationPrefab_V2, _notification, notificationContent_V2);

                // Coroutine'i baþlatmak için kontrol ekleyin
                if (!isNotificationLoopRunning)
                {
                    StartCoroutine(V2NotificationLoop());
                    isNotificationLoopRunning = true;  // Coroutine baþlatýldýðýnda kontrolü iþaretle
                }
            }
        }
    }



    public void FillNotificationPanelContent(NotificationType _notificationType)
    {
        Debug.Log($"Notification Content Filling Progress is starting... Type:{_notificationType.ToString()} Active Notifications count:{ActiveNotifications.Count}");
        currentNotificationType_V1 = _notificationType;
        List<NotificationHelper> targetHelpers = new();
        if (_notificationType == NotificationType.All)
            targetHelpers = ActiveNotifications;
        else
            targetHelpers = ActiveNotifications.Where(x => x.Notification.NotificationType == _notificationType).ToList();
        ClearNotificationPanelContent();
        foreach (var helper in targetHelpers)
        {
            NotificationHandler createdHandler = CreateNotificationInContent(notificationPrefab_V1, helper.Notification, notificationContent_V1);
            if (createdHandler.TryGetComponent(out NotificationUIHandler uIHandler))
            {
                uIHandler.UpdateStackText(helper.StackCount);
            }
        }
        if (targetHelpers.Count > 0)
        {
            MarkAllButton.gameObject.SetActive(true);
            ReadAllButton.gameObject.SetActive(true);
        }
        else
        {
            MarkAllButton.gameObject.SetActive(false);
            ReadAllButton.gameObject.SetActive(false);
        }
    }
    public NotificationHandler CreateNotificationInContent(NotificationHandler notificationPrefab,Notification _notification, Transform _content)
    {
        var notificationHandler = Instantiate(notificationPrefab, _content);
        notificationHandler.SetNotification(_notification);
        if (notificationHandler.GetNotification().NotificationState == NotificationState.Error)
            notificationHandler.gameObject.transform.SetSiblingIndex(0);
        return notificationHandler;
    }
    void ClearNotificationPanelContent(int _whichVersion = 1)
    {
        Transform targetContent = null;
        if (_whichVersion == 1)
            targetContent = notificationContent_V1;
        else if (_whichVersion == 2)
            targetContent = notificationContent_V2;

        if (targetContent == null) return;

        for (int i = targetContent.childCount - 1; i >= 0; i--)
        {
            Transform child = targetContent.GetChild(i);
            if (child.TryGetComponent(out NotificationHandler handler))
            {
                Destroy(child.gameObject);
            }
        }
    }

    public GameObject GetIcon(NotificationState state)
    {
        return icons[(int)state].gameObject;
    }
    // UI Progresses
    bool allMarkOpen = false;
    void MarkAllNotifications() //Version 1 canvasina ozeldir.
    {
        int length = notificationContent_V1.childCount;
        if (length <= 0) return;
        MarkAllButton.transform.GetChild(0).gameObject.SetActive(!allMarkOpen);
        for (int i = 0; i < length; i++)
        {
            if (notificationContent_V1.GetChild(i).TryGetComponent(out NotificationUIHandler notificationUIHandler))
            {
                notificationUIHandler.Mark(!allMarkOpen);
            }
        }
        allMarkOpen = !allMarkOpen;
    }
    void ReadAllMarkedNotifications() //Version 1 canvasina ozeldir.
    {
        int length = notificationContent_V1.childCount;
        List<int> siblingIndexes = new List<int>();
        for (int i = 0; i < length; i++)
            if (notificationContent_V1.GetChild(i).TryGetComponent(out NotificationHandler handler) && handler.GetNotification().IsDestroyable)
                if (handler.TryGetComponent(out NotificationUIHandler uiHandler) && uiHandler.IsActive)
                    siblingIndexes.Add(handler.transform.GetSiblingIndex());

        int siblingsLength = siblingIndexes.Count;
        if (siblingsLength <= 0) return;
        for (int i = 0; i < length; i++)
        {
            if (notificationContent_V1.GetChild(i).TryGetComponent(out NotificationHandler notificationHandler))
            {
                if(siblingIndexes.Contains(notificationHandler.transform.GetSiblingIndex()))
                if (notificationHandler.GetNotification().IsDestroyable)
                {
                    ActiveNotifications.Remove(GetNotificationHelperWithID(notificationHandler.GetNotification().ID));
                    Destroy(notificationHandler.gameObject);
                }
            }
        }

        MarkAllButton.transform.GetChild(0).gameObject.SetActive(false);
        SendNotification(GetNotificationWithID(4), new SenderHelper(WhoSends.System, 9999),2);
        allMarkOpen = false;
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
    public int TriggerAlertNumber { get; set; }
    public int StackCount { get; set; } = 1;
    Notification defaultNotification;
    public bool IsProcessStarted;
    public Notification(int _id,string _message, float _delayTime, NotificationType _notificationType, NotificationState notificationState, int triggerAlertNumber)
    {
        ID = _id;
        Message = _message;
        DelayTime = _delayTime;
        IsDestroyable = true;
        NotificationType = _notificationType;
        NotificationState = notificationState;
        AlertCount = 0;
        TriggerAlertNumber = triggerAlertNumber;
        defaultNotification = new Notification(this);
        IsProcessStarted = false;
    }
    public Notification(int _id, string _message, float _delayTime, NotificationType _notificationType, NotificationState notificationState,System.Func<Task<bool>> _onComplate, int triggerAlertNumber)
    {
        ID = _id;
        Message = _message;
        DelayTime = _delayTime;
        IsDestroyable = false;
        NotificationType = _notificationType;
        NotificationState = notificationState;
        OnComplate = _onComplate;
        AlertCount = 0;
        TriggerAlertNumber = triggerAlertNumber;
        defaultNotification = new Notification(this);
        IsProcessStarted = false;
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
        TriggerAlertNumber = copy.TriggerAlertNumber;
        defaultNotification = copy;
        IsProcessStarted = false;
    }
    public async Task StartNotificationProcess()
    {
        //if (transform.GetSiblingIndex() > 4) return await NotificationManager.instance.emptyTween.Task;
        IsProcessStarted = true;
        if (IsDestroyable)
        {
            int length = NotificationManager.instance.notificationContent_V2.childCount;
            for (int i = 0; i < length; i++)
            {
                if (NotificationManager.instance.notificationContent_V2.GetChild(i).TryGetComponent(out NotificationHandler handler))
                {
                    Notification notification = handler.GetNotification();
                    if (notification.ID == this.ID)
                    {
                        
                    }
                }
            }
        }
        else
        {
            if (AlertCount >= TriggerAlertNumber)
            {
                await StartComplateFunction(); // Burada asenkron fonksiyonun tamamlanmasýný bekliyoruz
                IsDestroyable = true;
                Debug.Log("StartComplateFunction() await end.");
            }
        }
        IsProcessStarted = false;
    }
    public async Task<bool> StartComplateFunction()
    {
        Debug.Log($"Complate function of this notification (id:{ID}) has been initialized.");
        var onComplate = OnComplate;
        if (onComplate != null)
        {
            return await onComplate.Invoke();
        }
        return true;
    }
    public void ResetNotification()
    {
        ID = defaultNotification.ID;
        Message = defaultNotification.Message;
        //DelayTime = defaultNotification.DelayTime;
        IsDestroyable = defaultNotification.IsDestroyable;
        NotificationType = defaultNotification.NotificationType;
        NotificationState = defaultNotification.NotificationState;
        OnComplate = defaultNotification.OnComplate;
        AlertCount = defaultNotification.AlertCount;
        TriggerAlertNumber = defaultNotification.TriggerAlertNumber;
    }
}
public struct NotificationHelper
{
    public Notification Notification { get; private set; }
    public SenderHelper SenderHelper { get; private set; }
    public int StackCount { get; set; }
    public NotificationHelper(Notification _notification, SenderHelper _senderHelper)
    {
        Notification = _notification;
        SenderHelper = _senderHelper;
        StackCount = 1;
    }
}
public struct SenderHelper //System ID: 9999!!
{
    public WhoSends Sender { get; private set; }
    public int SenderID { get; private set; }
    public SenderHelper(WhoSends _sender, int _senderID)
    {
        Sender = _sender;
        SenderID = _senderID;
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
public enum WhoSends // bildirimi kim gonderdi.
{
    System,
    Visitor,
    Worker,
    King
}