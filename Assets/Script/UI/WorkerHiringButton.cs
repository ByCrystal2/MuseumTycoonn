using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorkerHiringButton : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        WorkerInfoUIs myWorkerInfoUIS = GetComponentInParent<WorkerInfoUIs>();
        if (myWorkerInfoUIS.GetMyPrice() > MuseumManager.instance.GetCurrentGold())
        {
            UIController.instance.InsufficientGoldEffect();
            Debug.Log(myWorkerInfoUIS.txtFullName.text + " Adli npcyi ise alacak yeterli para bulunmamaktadir. (Extra gerekli Para => " + (myWorkerInfoUIS.GetMyPrice() - MuseumManager.instance.GetCurrentGold()).ToString());
            return;
        }
        else
            MuseumManager.instance.SpendingGold(myWorkerInfoUIS.GetMyPrice());
        int MyWorkerID = myWorkerInfoUIS.workerID;
        int length = WorkerManager.instance.GetWorkersInMarket().Count;
        for (int i = 0; i < length; i++)
        {
            if (WorkerManager.instance.GetWorkersInMarket()[i].ID == MyWorkerID)
            {
                WorkerManager.instance.GetWorkersInMarket().Remove(WorkerManager.instance.GetWorkersInMarket().Where(x => x.ID == MyWorkerID).SingleOrDefault());
                WorkerManager.instance.AddWorkerToInventory(WorkerManager.instance.GetAllWorkers().Where(x => x.ID == MyWorkerID).SingleOrDefault());
                Destroy(myWorkerInfoUIS.gameObject);
                UIController.instance.GetDesiredWorkersInContent(WorkerManager.instance.GetWorkersInInventory().Where(x => x.ID == MyWorkerID).SingleOrDefault().workerType);

                GameManager.instance.Save();
                break;
            }
        }
        
    }
}