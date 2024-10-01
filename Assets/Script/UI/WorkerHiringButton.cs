using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorkerHiringButton : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        UIController.instance.WorkerPurchasePanelController.SetWorkersToBuy(GetComponentInParent<WorkerInfoUIs>());
        UIController.instance.WorkerPurchasePanelController.gameObject.SetActive(true);
    }
    
}