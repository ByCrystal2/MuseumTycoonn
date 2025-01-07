using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class NotificationUIHandler : MonoBehaviour
{
    [SerializeField] GameObject imgTick;
    [SerializeField] Text txtStack;
    [SerializeField] Button takeRewardButton;
    [SerializeField] Button takeMissionButton;
    [SerializeField] Button markButton;
    [SerializeField] GameObject ShortTimeObj;
    [Header("Mission Time UIs")]
    [SerializeField] Text txtTime;
    public bool IsActive = false;
    SenderHelper Sender;
    NotificationHandler notificationHandler;
    float _validatyPeriodTime;
    private void Awake()
    {
        notificationHandler = GetComponent<NotificationHandler>();
        OnNotificationAssigned();
        takeRewardButton.onClick.AddListener(WinReward);
        takeMissionButton.onClick.AddListener(TakeMission);
        markButton.onClick.AddListener(OnClickNotification);
        _validatyPeriodTime = 0;
    }
    public void OnNotificationAssigned() => StartCoroutine(IEOnNotificationAssigned());
    IEnumerator IEOnNotificationAssigned()
    {
        yield return new WaitUntil(()=>notificationHandler.GetNotification() != null);
        CloseAllUIs();
        if (notificationHandler.GetNotification().NotificationType == NotificationType.Reward)
        {
            takeRewardButton.gameObject.SetActive(true);
        }
        else if (notificationHandler.GetNotification().NotificationType == NotificationType.Mission)
        {
            yield return new WaitUntil(() => notificationHandler.GetMission() != null);
            takeMissionButton.gameObject.SetActive(true);
            GameMission gameMission = notificationHandler.GetMission();
            if (gameMission.ValidityPeriodMission > 0)
            {
                ShortTimeObj.SetActive(true);
                _validatyPeriodTime = gameMission.ValidityPeriodMission;
                StartMissionCountDown();
            }           
        }
        else
        {
            markButton.gameObject.SetActive(true);
            txtStack.gameObject.SetActive(true);
        }
    }
    void CloseAllUIs()
    {
        takeRewardButton.gameObject.SetActive(false);
        takeMissionButton.gameObject.SetActive(false);
        markButton.gameObject.SetActive(false);
        txtStack.gameObject.SetActive(false);
        ShortTimeObj.SetActive(false);
    }
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
    void WinReward()
    {
        if (notificationHandler.GetNotification().NotificationType == NotificationType.Reward)
        {
            NotificationRewardHandler rewardHandler = NotificationManager.instance.GetNotificationRewardHandler(notificationHandler.GetNotification().ID);
            if (rewardHandler.ActionToBeWinReward == null)
            {
                Debug.LogError("Win reward action null!");
            }
            rewardHandler.ActionToBeWinReward?.Invoke();
            AfterNotificationActionInvoke();
        }
    }
    void TakeMission()
    {
        if (notificationHandler.GetNotification().NotificationType == NotificationType.Mission)
        {
            if (MissionManager.instance.AnyActiveMission())
            {
                NotificationManager.instance.SendNotification(NotificationManager.instance.GetNotificationWithID(5), new SenderHelper(WhoSends.System, 9999), 2);
                Debug.Log("Su anda zaten aktif bir gorev bulunmaktadir!");
                return;
            }

            GameMission currentGameMission = MissionManager.instance.GetMissionWithTargetId(notificationHandler.GetNotification().ID);
            currentGameMission.isActive = true;
            NotificationMissionHandler missionHandler = NotificationManager.instance.GetNotificationMissionHandler(currentGameMission.TargetNotificationID);
            bool nullable = missionHandler == null;
            Debug.Log("Mission Handler is null = " +  nullable + " and notification ID: " + notificationHandler.GetNotification().ID);
            missionHandler.ActionToBeMission?.Invoke();
            AfterNotificationActionInvoke();
        }
    }
    void AfterNotificationActionInvoke()
    {
        NotificationManager.instance.ActiveNotifications.Remove(NotificationManager.instance.GetNotificationHelperWithID(notificationHandler.GetNotification().ID));
        if (notificationHandler.GetNotification().NotificationType == NotificationType.Reward)
        NotificationManager.instance.notificationRewardHandlers.Remove(NotificationManager.instance.GetNotificationRewardHandler(notificationHandler.GetNotification().ID));
        else if (notificationHandler.GetNotification().NotificationType == NotificationType.Mission)
            NotificationManager.instance.notificationMissionHandlers.Remove(NotificationManager.instance.GetNotificationMissionHandler(notificationHandler.GetNotification().ID));
        gameObject.SetActive(false);        
        Destroy(gameObject, 0.2f);
    }
    Coroutine MissionCountDownCoroutine;
    void StartMissionCountDown()
    {
        if (MissionCountDownCoroutine != null)
            StopCoroutine(MissionCountDownCoroutine);

        MissionCountDownCoroutine = CoroutineHelper.Instance.RunCoroutine(IEStartMissionCountDown());
    }
    IEnumerator IEStartMissionCountDown()
    {
        Debug.Log("Start Mission Count Down Method is starting...");
        if (notificationHandler.GetNotification().NotificationType != NotificationType.Mission) yield break;
        while (_validatyPeriodTime > 0)
        {
            int minutes = Mathf.FloorToInt(_validatyPeriodTime / 60f);
            int seconds = Mathf.FloorToInt(_validatyPeriodTime % 60f);

            txtTime.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            _validatyPeriodTime -= 1f;
            yield return new WaitForSeconds(1f);
        }
        txtTime.text = "00:00";
        Debug.Log("Start Mission Count Down Method is ending...");
        AfterNotificationActionInvoke();
    }

}

