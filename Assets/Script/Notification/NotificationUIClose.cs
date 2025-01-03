using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationUIClose : MonoBehaviour
{
    NotificationHandler notificationHandler;
    UIFade uiFade;
    float totalTime;
    private void Awake()
    {
        notificationHandler = GetComponent<NotificationHandler>();
        uiFade = GetComponent<UIFade>();
    }
    private void Start()
    {
        StartCoroutine(OnNotificationAssigned());
    }
    private void Update()
    {
        if (totalTime > 0)
        {
            totalTime -= Time.deltaTime;
            if (totalTime <= 0)
            {
                Debug.Log("Notification Delay Time Ended.");
                uiFade.FadeOut(true);
                notificationHandler.GetNotification().StartComplateFunction();
                NotificationManager.instance.NotificationContentEmptyControl();
            }
        }
    }
    IEnumerator OnNotificationAssigned()
    {
        yield return new WaitUntil(() => notificationHandler.GetNotification() != null);
        totalTime = notificationHandler.GetNotification().DelayTime;
    }
}
