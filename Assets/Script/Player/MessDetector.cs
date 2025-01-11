using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class MessDetector : MonoBehaviour
{
    public ThirdPersonController Controller;

    public Button CleanMessButton;

    public GameObject CleanObject;

    public List<ParticleSystem> CleanEffect;

    private List<NpcMess> CurrentMesses;
    private NpcMess CleaningMess;

    private bool _Cleaning;
    private void Start()
    {
        CurrentMesses = new();
        CleanMessButton.gameObject.SetActive(false);
        CleanMessButton.onClick.AddListener(OnCleanMess);
    }

    private void FixedUpdate()
    {
        int length = CurrentMesses.Count;
        for (int i = length - 1; i >= 0; i--)
            if (CurrentMesses[i] == null)
                CurrentMesses.RemoveAt(i);

        CleanMessButton.gameObject.SetActive(CurrentMesses.Count > 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("NPC_Mess"))
        {
            OnFindMess(other.gameObject.GetComponent<NpcMess>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("NPC_Mess"))
        {
            OnMessFar(other.gameObject.GetComponent<NpcMess>());
        }
    }

    public void OnFindMess(NpcMess _mess)
    {
        if (!CurrentMesses.Contains(_mess) && !_Cleaning)
        {
            CurrentMesses.Insert(0, _mess);
            CleanMessButton.gameObject.SetActive(true);
        } 
    }

    public void OnCleanMess()
    {
        if (_Cleaning || CurrentMesses[0]._cleaningNow)
            return;

        if (CurrentMesses[0]._cleaningNow)
        {
            NotificationManager.instance.SendNotification(NotificationManager.instance.GetNotificationWithID(9), new SenderHelper(WhoSends.System, 9999), 2);
            return;
        }

        if (!Controller.IsReady(2))
            return;

        _Cleaning = true;

        CleaningMess = CurrentMesses[0];
        CleaningMess.SetCleaning();
        NpcManager.instance.SetMessCleaning(CleaningMess.transform);

        OnMessFar(CurrentMesses[0]);

        foreach (var item in CleanEffect)
            item.Play();
        CleanObject.SetActive(true);
        Controller.OnStartClean();
        Invoke(nameof(OnCleanMessEnd), 3);
    }

    public void OnCleanMessEnd()
    {
        _Cleaning = false;

        foreach (var item in CleanEffect)
            item.Stop();
        CleanObject.SetActive(false);
        Controller.OnEndClean();

        if (CleaningMess != null)
            NpcManager.instance.DestroyMess(CleaningMess.gameObject);
    }

    public void OnMessFar(NpcMess _mess)
    {
        if (CurrentMesses.Contains(_mess))
            CurrentMesses.Remove(_mess);

        CleanMessButton.gameObject.SetActive(CurrentMesses.Count > 0);
    }
}
