using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UIFade))]
public class NotificationHandler : MonoBehaviour
{
    [SerializeField] private Transform iconContent;
    [SerializeField] private Text messageText;

    private Notification notification;
    NotificationUIHandler uIHandler;
    GameMission currentGameMission;
    public UIFade uiFade { get; private set; }    
    private void Awake()
    {
        uiFade = GetComponent<UIFade>();
        uIHandler = GetComponent<NotificationUIHandler>();
    }

    public void SetNotification(Notification newNotification)
    {
        notification = new Notification(newNotification);
        ClearIcons();
        SetIcon();
        messageText.text = notification.Message;
    }
    public void SetMission(GameMission gameMission)
    {
        Debug.Log("in SetMission method...");
        if (notification.NotificationType == NotificationType.Mission)
            currentGameMission = gameMission;
        else
            Debug.LogError("Gonderilen gorev, mevcut bildirimin bir gorev bildirimi olmamasindan dolayi kabul edilmemistir!");
    }
    public Notification GetNotification() { return notification; }
    public GameMission GetMission() { return currentGameMission; }


    private void ClearIcons()
    {
        foreach (Transform child in iconContent)
        {
            if (child.name.Contains("Icon"))
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
