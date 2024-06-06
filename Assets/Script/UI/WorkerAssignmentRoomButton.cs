using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorkerAssignmentRoomButton : MonoBehaviour,IPointerClickHandler
{
    public int CurrentWorkerID;
    public int CurrentRoomID;
    [SerializeField] private Image imgFrame;
    [SerializeField] private TextMeshProUGUI txtCell_Number;
    bool Interactable = false;   
    public void AssignmentRoomButton(int _currentRoomID, int _currentWorkerID,Color _color, string _cellNumber, bool _interectable)
    {
        CurrentWorkerID = _currentWorkerID;
        CurrentRoomID = _currentRoomID;
        imgFrame.color = _color;
        txtCell_Number.text = _cellNumber;
        Interactable = _interectable;
        GetComponent<Button>().enabled = Interactable;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Interactable)
        {
            WorkerBehaviour currentBehaviour = WorkerManager.instance.GetAllWorkers().Where(x => x.ID == CurrentWorkerID).SingleOrDefault();
            Worker currentWorker = currentBehaviour.MyScript;
            GPGamesManager.instance.achievementController.IncreaseWorkerAssignCount(currentWorker.WorkerType);
            WorkerData currentWorkerData = currentBehaviour.MyDatas;
            currentWorkerData.WorkRoomsIDs.Add(CurrentRoomID);
            currentWorker.IWorkRoomsIDs.Add(CurrentRoomID);

            RoomData currentRoom = RoomManager.instance.RoomDatas.Where(x=> x.ID == CurrentRoomID).SingleOrDefault();
            currentRoom.MyRoomWorkersIDs.Add(CurrentWorkerID);
            Debug.Log("Interectable => " + Interactable + " Current Worker is => " + currentWorker.Name);
            WorkerManager.instance.GetWorkersInInventory().Remove(WorkerManager.instance.GetAllWorkers().Where(x => x.ID == currentWorker.ID).SingleOrDefault());
            WorkerManager.instance.GetCurrentWorkers().Add(WorkerManager.instance.GetAllWorkers().Where(x=> x.ID == currentWorker.ID).SingleOrDefault());
            UIController.instance.ClearAssignmentRoomsButtonContent();
            UIController.instance.GetDesiredInventoryWorkersInContent(currentWorker.WorkerType);

            currentBehaviour.gameObject.SetActive(true);
            GPGamesManager.instance.achievementController.WorkerAssignControl(currentWorker.WorkerType);
            GameManager.instance.Save();
        }
    }
}
