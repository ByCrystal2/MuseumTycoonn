using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WorkerPurchasePanelController : MonoBehaviour
{
    [SerializeField] Button yesButton;
    [SerializeField] Button noButton;
    WorkerInfoUIs workersToBuy;
    private void Awake()
    {
        yesButton.onClick.AddListener(WhenPurchased);
        noButton.onClick.AddListener(CancelPurchase);
    }
    public void SetWorkersToBuy(WorkerInfoUIs _workersToBuy)
    {
        workersToBuy = _workersToBuy;
        if (workersToBuy.GetMyPrice() > MuseumManager.instance.GetCurrentGold())
            yesButton.GetComponent<ButtonSoundHandler>().enabled = false;
    }
    private void OnEnable()
    {
        if (workersToBuy == null)
        {
            Debug.Log("Panele isci atanmali.");
            gameObject.SetActive(false);
        }
    }
    private void OnDisable()
    {
        yesButton.GetComponent<ButtonSoundHandler>().enabled = true;
    }
    public void WhenPurchased() // Additionally, it is used in the tutorial.
    {
        Hiring();
        workersToBuy = null;
        gameObject.SetActive(false);
    }
    void CancelPurchase()
    {
        Debug.Log("Isci satin alma islemi iptal edildi.");
        workersToBuy = null;
        gameObject.SetActive(false);
    }
    void Hiring()
    {
        if(workersToBuy == null)
        {
            Debug.LogError("Panele isci atanmali.");
            return;
        }
        if (workersToBuy.GetMyPrice() > MuseumManager.instance.GetCurrentGold())
        {
            UIController.instance.InsufficientGoldEffect();
            Debug.Log(workersToBuy.txtFullName.text + " Adli npcyi ise alacak yeterli para bulunmamaktadir. (Extra gerekli Para => " + (workersToBuy.GetMyPrice() - MuseumManager.instance.GetCurrentGold()).ToString());
            return;
        }
        else
            MuseumManager.instance.SpendingGold(workersToBuy.GetMyPrice());
        WorkerBehaviour wb = WorkerManager.instance.GetAllWorkers().Where(x => x.ID == workersToBuy.workerID).SingleOrDefault();
        //GPGamesManager.instance.achievementController.IncreaseWorkerHiringCount(wb.workerType);
        int MyWorkerID = workersToBuy.workerID;
        int length = WorkerManager.instance.GetWorkersInMarket().Count;
        for (int i = 0; i < length; i++)
        {
            if (WorkerManager.instance.GetWorkersInMarket()[i].ID == MyWorkerID)
            {
                WorkerManager.instance.TransferMarketWorkerToInventory(MyWorkerID);
                Destroy(workersToBuy.gameObject);
                UIController.instance.GetDesiredWorkersInContent(MuseumManager.instance.WorkersInInventory.Where(x => x.ID == MyWorkerID).SingleOrDefault().workerType);
                GameManager.instance.Save();
                break;
            }
        }
        //GPGamesManager.instance.achievementController.WorkerHiringControl(wb.workerType);
    }
}
