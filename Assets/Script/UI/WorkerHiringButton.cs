using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorkerHiringButton : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        int MyWorkerID = GetComponentInParent<WorkerInfoUIs>().workerID;
        int length = WorkerManager.instance.GetWorkersInMarket().Count;
        for (int i = 0; i < length; i++)
        {
            if (WorkerManager.instance.GetWorkersInMarket()[i].ID == MyWorkerID)
            {
                WorkerManager.instance.GetWorkersInMarket().Remove(WorkerManager.instance.GetWorkersInMarket().Where(x => x.ID == MyWorkerID).SingleOrDefault());
                WorkerManager.instance.AddWorkerToInventory(WorkerManager.instance.GetAllWorkers().Where(x => x.ID == MyWorkerID).SingleOrDefault());
                Destroy(GetComponentInParent<WorkerInfoUIs>().gameObject);
                UIController.instance.GetDesiredWorkersInContent(WorkerManager.instance.GetWorkersInInventory().Where(x => x.ID == MyWorkerID).SingleOrDefault().workerType);

                GameManager.instance.Save();
                break;
            }
        }
        
    }
}