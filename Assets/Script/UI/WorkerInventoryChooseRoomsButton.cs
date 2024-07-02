using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorkerInventoryChooseRoomsButton : MonoBehaviour,IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Oda Sec Butonuna Tiklandi."); // Girdi.
        ChooseRoom();
    }
    public void ChooseRoom()
    {

        UIController.instance.ClearAssignmentRoomsButtonContent();
        Worker MyWorker = WorkerManager.instance.GetAllWorkers().Where(x => x.ID == GetComponentInParent<WorkerInfoUIs>().workerID).SingleOrDefault().MyScript; // 

        List<RoomData> AllRooms = RoomManager.instance.RoomDatas.OrderBy(x => x.availableRoomCell.CellLetter).ThenBy(x => x.availableRoomCell.CellNumber).ToList();
        List<int> MyWorkRoomsIDs = MyWorker.IWorkRoomsIDs;
        foreach (RoomData room in AllRooms)
        {
            Debug.Log("Iþlem yapýlan ODA => " + room.availableRoomCell.CellLetter + "" + room.availableRoomCell.CellNumber);
            if (!room.isLock && room.isActive)
            {
                bool isWorking = false;
                foreach (var myRoomID in MyWorkRoomsIDs)
                {
                    if (myRoomID == room.ID)
                    {
                        isWorking = true;
                        int length = room.MyRoomWorkersIDs.Count;
                        for (int i = 0; i < length; i++)
                        {

                            if (WorkerManager.instance.GetAllWorkers().Where(x => x.ID == room.MyRoomWorkersIDs[i]).SingleOrDefault().workerType == WorkerManager.instance.GetAllWorkers().Where(x => x.ID == MyWorker.ID).SingleOrDefault().workerType)
                            {
                                Debug.Log("Bu turde bir isci, bu odaya baglý. Oda Kodu => " + room.availableRoomCell.CellLetter + "" + room.availableRoomCell.CellNumber);
                                UIController.instance.AddDesiredChooseRoomsInContent(room.ID, MyWorker.ID, Color.red, room.availableRoomCell.CellLetter + "" + room.availableRoomCell.CellNumber, false);
                                break;
                            }

                        }
                    }
                }
                if (!isWorking)
                {
                    int length = room.MyRoomWorkersIDs.Count;
                    bool isOtherWorkerType = false;
                    for (int i = 0; i < length; i++)
                    {
                        if (WorkerManager.instance.GetAllWorkers().Where(x => x.ID == room.MyRoomWorkersIDs[i]).SingleOrDefault().workerType == WorkerManager.instance.GetAllWorkers().Where(x => x.ID == MyWorker.ID).SingleOrDefault().workerType)
                        {
                            Debug.Log("Bu turde bir isci, bu odaya baglý. Oda Kodu => " + room.availableRoomCell.CellLetter + "" + room.availableRoomCell.CellNumber);
                            UIController.instance.AddDesiredChooseRoomsInContent(room.ID, MyWorker.ID, Color.red, room.availableRoomCell.CellLetter + "" + room.availableRoomCell.CellNumber, false);
                            isOtherWorkerType = true;
                            break;
                        }
                    }
                    if (!isOtherWorkerType)
                    {
                        UIController.instance.AddDesiredChooseRoomsInContent(room.ID, MyWorker.ID, Color.green, room.availableRoomCell.CellLetter + "" + room.availableRoomCell.CellNumber, true);
                    }
                }
            }
            else
            {
                Debug.Log("Oda satin alinmamis. Oda Kodu => " + room.availableRoomCell.CellLetter + "" + room.availableRoomCell.CellNumber);
                UIController.instance.AddDesiredChooseRoomsInContent(room.ID, MyWorker.ID, Color.black, room.availableRoomCell.CellLetter + "" + room.availableRoomCell.CellNumber, false);
            }
        }
    }
}
