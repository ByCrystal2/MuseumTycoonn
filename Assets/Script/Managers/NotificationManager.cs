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
    public Button exitButton;
    NotificationHeader currentNotificationType_V1;

    [Header("V2 Notification")]
    [SerializeField] GameObject notificationsCanvas_V2;
    [SerializeField] public Transform notificationContent_V2;
    [SerializeField] NotificationHandler notificationPrefab_V2;
    [SerializeField] private float notificationCheckInterval_V2 = 0.05f;
    private NotificationIconHandler[] icons;
    List<Notification> notifications = new List<Notification>();
    public List<Notification> Notifications => notifications;
    public List<NotificationRewardHandler> notificationRewardHandlers = new List<NotificationRewardHandler>();
    public List<NotificationMissionHandler> notificationMissionHandlers = new List<NotificationMissionHandler>();

    public TaskCompletionSource<DG.Tweening.Tween> emptyTween = new TaskCompletionSource<Tween>();
    public List<NotificationHelper> ActiveNotifications = new List<NotificationHelper>();
    public static NotificationManager instance { get; private set; }

    [Header("Only Unity Editor")]
    [SerializeField] bool sendExampleErrorNotification = false;
    [SerializeField] bool sendExampleWarningNotification = false;
    [SerializeField] bool sendExampleInfoNotification = false;
    [SerializeField] bool sendExampleRewardNotification = false;
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
        sendExampleRewardNotification = false;
    }

    private void Start()
    {
        icons = Resources.LoadAll<NotificationIconHandler>("NotificationIcons");
        currentNotificationType_V1 = NotificationHeader.All;
        int length = notificationTabPanel_V1.childCount;
        List<Button> tabButtons = new List<Button> ();

        for (int i = 0; i < length; i++)
            if (notificationTabPanel_V1.GetChild(i).TryGetComponent(out Button button))
                tabButtons.Add(button);

        for (int i = 0; i < tabButtons.Count; i++)
        {
            int capturedIndex = i;
            tabButtons[capturedIndex].onClick.AddListener(() => FillNotificationPanelContent((NotificationHeader)System.Enum.ToObject(typeof(NotificationHeader), capturedIndex)));
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
            SendNotification(new Notification(9900, "This is an Error test message", 2f, NotificationHeader.System,NotificationState.Error,NotificationType.Trivial,1),new SenderHelper(WhoSends.System,9999));
        }
        if (sendExampleWarningNotification)
        {
            sendExampleWarningNotification = false;
            SendNotification(new Notification(9901, "This is an Warning test message", 1f,NotificationHeader.Visitor, NotificationState.Warning, NotificationType.Trivial, 1), new SenderHelper(WhoSends.Visitor,9999));
        }
        if (sendExampleInfoNotification)
        {
            sendExampleInfoNotification = false;
            SendNotification(new Notification(9902, "This is an Information test message", 10f, NotificationHeader.Worker, NotificationState.Information, NotificationType.Trivial, 1), new SenderHelper(WhoSends.Worker,9999));
        }
        if (sendExampleRewardNotification)
        {
            sendExampleRewardNotification = false;
            NotificationHandler notificationHandler = SendNotification(new Notification(9903, "This is an Reward test message", 10f, NotificationHeader.System, NotificationState.Information, NotificationType.Reward, 1), new SenderHelper(WhoSends.System, 9999), 1, new NotificationRewardHandler(9903, () =>
            {
                MuseumManager.instance.AddGold(1000);
            }));
           
        }
#endif
    }
    public NotificationRewardHandler GetNotificationRewardHandler(int _notifiID)
    {
        return notificationRewardHandlers.Where(x => x.TargetNotificationID == _notifiID).FirstOrDefault();
    }
    public NotificationMissionHandler GetNotificationMissionHandler(int _notifiID)
    {
        return notificationMissionHandlers.Where(x=> x.TargetNotificationID ==_notifiID).FirstOrDefault();
    }
    public void NotificationInit()
    {
        Notification n1 = null, n2 = null, n3 = null, n4 = null, n5 = null, n6 = null, n7 = null, n8 = null, n9 = null;
        n1 = new Notification(1,
            "You have changed the language frequently. Please wait a while before changing it again.",10.0f, NotificationHeader.System,
            NotificationState.Error, NotificationType.Emergency,
            async () =>
            {
                MainMenu.instance.AllLanguageButtonInteractable(false);
                await Task.Delay(((int)n1.DelayTime) * 1000);
                MainMenu.instance.ResetLanguageChangedValues();
                Debug.Log("Notification completed.");
                return true;
            },5);
        n2 = new Notification(2,
            "A worker didn't get his salary", 10.0f, NotificationHeader.System,
            NotificationState.Warning, NotificationType.Emergency,
            async () =>
            {                
                return true;
            }, 3);
        n3 = new Notification(3,
            "Was transferred to inventory because a worker was not paid his salary.", 10.0f, NotificationHeader.System,
            NotificationState.Error, NotificationType.Emergency,
            async () =>
            {
                
                return true;
            }, 3);
        n4 = new Notification(4, "Selected notifications read.", 1f, NotificationHeader.System, NotificationState.Information, NotificationType.Trivial, 1);
        n5 = new Notification(5, "You already have a mission right now.", 2f, NotificationHeader.System, NotificationState.Warning, NotificationType.Trivial, 1);
        n6 = new Notification(6, "You have completed the mission! You can get the reward in the notifications.", 2f, NotificationHeader.System, NotificationState.Information, NotificationType.Trivial, 1);
        n7 = new Notification(7, "The current mandate is over! Mission failed.", 1.5f, NotificationHeader.System, NotificationState.Error, NotificationType.Trivial, 1);
        n8 = new Notification(8, "I have so many objects to clean. I can't keep up!", 1.5f, NotificationHeader.Worker, NotificationState.Warning, NotificationType.Trivial, 1);
        n9 = new Notification(9, "A cleaner already targetted this mess, she will clean it soon!", 2.5f, NotificationHeader.System, NotificationState.Warning, NotificationType.Trivial, 1);
        //  RewardNotifications
        Notification n9999 = new Notification(9999, "", 2f, NotificationHeader.System, NotificationState.Information, NotificationType.Reward, 1);
        Notification n10001 = new Notification(10001, "", 2f, NotificationHeader.System, NotificationState.Information, NotificationType.Reward, 1);
        Notification n10002 = new Notification(10002, "", 2f, NotificationHeader.System, NotificationState.Information, NotificationType.Reward, 1);
        // Achievement Notifications
        Notification n10000 = new Notification(10000, "", 2f, NotificationHeader.System, NotificationState.Information, NotificationType.Reward,1);
            //
        //

        // Game Mission Notifications
        Notification n100000 = new Notification(100000, "Müze yönetimi yeni görev seçti! Kabul etmek için bildirimler kýsmýna git.", 5f, NotificationHeader.System, NotificationState.Information, NotificationType.Mission, async() =>
        {
            //SendNotification()
            return true;
        },1);
        Notification n100001 = new Notification(100001, "", 3f, NotificationHeader.System, NotificationState.Information, NotificationType.Mission,1);
        Notification n100002 = new Notification(100002, "", 3f, NotificationHeader.System, NotificationState.Information, NotificationType.Mission,1);
        Notification n100003 = new Notification(100003, "", 3f, NotificationHeader.System, NotificationState.Information, NotificationType.Mission,1);
        //
        notifications.Add(n1);
        notifications.Add(n2);
        notifications.Add(n3);
        notifications.Add(n4);
        notifications.Add(n5);
        notifications.Add(n6);
        notifications.Add(n7);
        notifications.Add(n8);

        //Reward Notifications Adding...
        notifications.Add(n9999);
        notifications.Add(n10001);
        notifications.Add(n10002);

        //Acihevement Notifications Adding...
        notifications.Add(n10000);
        //Acihevement Notifications Adding...

        //Reward Notifications Adding...

        //Game Mission Notifications Adding...
        notifications.Add(n100000);
        notifications.Add(n100001);
        notifications.Add(n100002);
        notifications.Add(n100003);
        //Game Mission Notifications Adding...
        //StartCoroutine(V2NotificationLoop());
    }
    public Notification GetNotificationWithID(int _id)
    {
        Notification result = Notifications.Where(x => x.ID == _id).SingleOrDefault();
        return result;
    }
    public NotificationHelper GetNotificationHelperWithID(int _id)
    {
        NotificationHelper result = ActiveNotifications.Where(x => x.Notification.ID == _id).FirstOrDefault();
        return result;
    }
    // Sýnýfýn içinde bir alan olarak tanýmlayýn
    private bool isNotificationLoopRunning = false;



    public NotificationHandler SendNotification(Notification _notification, SenderHelper _sender, int _whichVersion = 1, NotificationRewardHandler _rewardHandler = null,NotificationMissionHandler _missionHandler = null,GameMission gameMission = null, string _overrideMessage = "")
    {
        if (_rewardHandler != null)
            notificationRewardHandlers.Add(_rewardHandler);
        if (_missionHandler != null)
            notificationMissionHandlers.Add(_missionHandler);
        if (!string.IsNullOrEmpty(_overrideMessage))
            _notification.OverrideMessage(_overrideMessage);

        _notification.AlertCount++;
        NotificationHandler targetHandler = null;
        if (_whichVersion == 1)
        {
            int index = ActiveNotifications.FindIndex(x => x.Notification.ID == _notification.ID &&
                                                           x.SenderHelper.SenderID == _sender.SenderID &&
                                                           x.SenderHelper.Sender == _sender.Sender);

            if (index != -1 && (_notification.NotificationType == NotificationType.Trivial || _notification.NotificationType == NotificationType.Emergency))
            {
                NotificationHelper TargetHelper = ActiveNotifications[index];
                TargetHelper.StackCount++;
                ActiveNotifications[index] = TargetHelper;
            }
            else
            {
                ActiveNotifications.Add(new NotificationHelper(_notification, _sender,gameMission));
            }

            targetHandler = FillNotificationPanelContent(currentNotificationType_V1);
            if (RightUIPanelController.instance != null && !RightUIPanelController.instance.DrawerPanel.gameObject.activeSelf)
                RedExclamationMarkManager.instance.CreateMark(RightUIPanelController.instance.DrawerButton.transform);

            if (UIController.instance != null && !notificationsCanvas_V1.gameObject.activeSelf)
                RedExclamationMarkManager.instance.CreateMark(UIController.instance.NotificationCanvasOnButton.transform);
        }
        else if (_whichVersion == 2)
        {
            if (_notification.AlertCount >= _notification.TriggerAlertNumber)
            {
                targetHandler = CreateNotificationInContent(notificationPrefab_V2, _notification, notificationContent_V2, null);
                notificationsCanvas_V2.gameObject.SetActive(true);
            }
        }
        
        return targetHandler;
    }



    public NotificationHandler FillNotificationPanelContent(NotificationHeader _notificationHeader)
    {
        Debug.Log($"Notification Content Filling Progress is starting... Type:{_notificationHeader.ToString()} Active Notifications count:{ActiveNotifications.Count}");
        currentNotificationType_V1 = _notificationHeader;
        List<NotificationHelper> targetHelpers = new();
        if (_notificationHeader == NotificationHeader.All)
            targetHelpers = ActiveNotifications;
        else
            targetHelpers = ActiveNotifications.Where(x => x.Notification.NotificationHeader == _notificationHeader).ToList();
        ClearNotificationPanelContent();
        NotificationHandler createdHandler = null;
        foreach (var helper in targetHelpers)
        {
            if (helper.Notification.NotificationType == NotificationType.Reward || helper.Notification.NotificationType == NotificationType.Mission)
                if (HasAnyNotificationInContent(helper.Notification.ID))
                    continue;

            createdHandler = CreateNotificationInContent(notificationPrefab_V1, helper.Notification,notificationContent_V1, helper.Mission);
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
        return createdHandler;
    }
    public NotificationHandler CreateNotificationInContent(NotificationHandler notificationPrefab,Notification _notification, Transform _content, GameMission gameMission = null)
    {
        var notificationHandler = Instantiate(notificationPrefab, _content);
        notificationHandler.SetNotification(_notification);
        notificationHandler.SetMission(gameMission);
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
                if (handler.GetNotification().NotificationType == NotificationType.Mission || handler.GetNotification().NotificationType == NotificationType.Reward)
                    continue;

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
                if (notificationHandler.GetNotification().NotificationType == NotificationType.Reward || notificationHandler.GetNotification().NotificationType == NotificationType.Mission) continue;
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
    bool HasAnyNotificationInContent(int _id) // For version 1
    {
        int length = notificationContent_V1.childCount;
        bool has = false;
        for (int i = 0; i < length; i++)
        {
            if (notificationContent_V1.GetChild(i).TryGetComponent(out NotificationHandler currentHandler))
            {
                if (currentHandler.GetNotification().ID == _id)
                    has = true;
            }            
        }
        return has;
    }
    Coroutine ContentEmptyControl;
    public void NotificationContentEmptyControl()
    {
        if (ContentEmptyControl != null)
            StopCoroutine(ContentEmptyControl);

        ContentEmptyControl = StartCoroutine(IENotificationContentEmptyControl());
    }
    IEnumerator IENotificationContentEmptyControl()
    {
        int countDown = 10;
        while (countDown > 0)
        {
            if (notificationContent_V2.childCount <= 0)
            {
                notificationsCanvas_V2.gameObject.SetActive(false);
                break;
            }
            countDown--;
            yield return new WaitForSeconds(0.5f);
        }
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
    public NotificationHeader NotificationHeader;
    public NotificationType NotificationType;
    public System.Func<Task<bool>> OnComplate;
    public int AlertCount { get; set; }
    public int TriggerAlertNumber { get; set; }
    public int StackCount { get; set; } = 1;
    Notification defaultNotification;
    public bool IsProcessStarted;
    public Notification(int _id,string _message, float _delayTime, NotificationHeader _notificationHeader, NotificationState notificationState,NotificationType _notificationType , int triggerAlertNumber)
    {
        ID = _id;
        Message = _message;
        DelayTime = _delayTime;
        IsDestroyable = true;
        NotificationHeader = _notificationHeader;
        NotificationState = notificationState;
        NotificationType = _notificationType;
        AlertCount = 0;
        TriggerAlertNumber = triggerAlertNumber;
        defaultNotification = new Notification(this);
        IsProcessStarted = false;
    }
    public Notification(int _id, string _message, float _delayTime, NotificationHeader _notificationHeader, NotificationState notificationState, NotificationType _notificationType, System.Func<Task<bool>> _onComplate, int triggerAlertNumber)
    {
        ID = _id;
        Message = _message;
        DelayTime = _delayTime;
        IsDestroyable = false;
        NotificationHeader = _notificationHeader;
        NotificationState = notificationState;
        NotificationType = _notificationType;
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
        NotificationHeader = copy.NotificationHeader;
        NotificationState = copy.NotificationState;
        NotificationType = copy.NotificationType;
        OnComplate = copy.OnComplate;
        AlertCount = copy.AlertCount;
        TriggerAlertNumber = copy.TriggerAlertNumber;
        defaultNotification = copy;
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
        NotificationHeader = defaultNotification.NotificationHeader;
        NotificationState = defaultNotification.NotificationState;
        OnComplate = defaultNotification.OnComplate;
        AlertCount = defaultNotification.AlertCount;
        TriggerAlertNumber = defaultNotification.TriggerAlertNumber;
    }
    public void OverrideMessage(string _text)
    {
        Message = _text;
    }
}
[System.Serializable]
public class NotificationRewardHandler
{
    public int TargetNotificationID;
    public System.Action ActionToBeWinReward;
    public NotificationRewardHandler(int _notificationID, System.Action _rewardMethod)
    {
        TargetNotificationID = _notificationID;
        ActionToBeWinReward = _rewardMethod;
    }
}
[System.Serializable]
public class NotificationMissionHandler
{
    public int TargetNotificationID;
    public System.Action ActionToBeMission;
    public NotificationMissionHandler(int _notificationID, System.Action _missionMethod)
    {
        TargetNotificationID = _notificationID;
        ActionToBeMission = _missionMethod;
    }
}
[System.Serializable]
public struct NotificationHelper
{
    public Notification Notification { get; private set; }
    public GameMission Mission { get; private set; }
    public SenderHelper SenderHelper { get; private set; }
    public int StackCount { get; set; }
    public NotificationHelper(Notification _notification, SenderHelper _senderHelper,GameMission _mission = null)
    {
        Notification = _notification;
        SenderHelper = _senderHelper;
        Mission = _mission;
        StackCount = 1;
    }
}
[System.Serializable]
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
public enum NotificationHeader
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
public enum NotificationType
{
    Reward,
    Mission,
    Emergency,
    Trivial
}