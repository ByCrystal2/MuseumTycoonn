using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RoomBlokClickHandler : MonoBehaviour
{
    RoomData MyParentRoomData;
    private void Awake()
    {
        MyParentRoomData = GetComponentInParent<RoomData>();
    }
    private void OnCollisionEnter(Collision _col) //_col is NPC
    {
        Debug.Log("NPC: <color=#4CC324>" + _col.gameObject.name.ToString() + "</color> entered the room ID: <color=#4CC324>" + MyParentRoomData.ID + "</color>");
        if (_col.gameObject.TryGetComponent(out NPCBehaviour _enteredNpc))
        {
            _enteredNpc.CurrentVisitedRoom = MyParentRoomData;
            RoomManager.instance.AddNpcInTheRoom(MyParentRoomData, _enteredNpc);
        }
    }
    private void OnCollisionExit(Collision _col)
    {
        Debug.Log("NPC: <color=#C3A624>" + _col.gameObject.name.ToString() + "</color> exitted the room ID: <color=#4CC324>" + MyParentRoomData.ID + "</color>");
        if (_col.gameObject.TryGetComponent(out NPCBehaviour _exitedNpc))
        {
            RoomManager.instance.RemoveNpcInTheRoom(MyParentRoomData, _exitedNpc);
        }
    }
    IEnumerator WaitingForIsPointerOver()
    {
        //for (int i = 0; i < 3; i++)
            yield return new WaitForEndOfFrame();

        RoomData ClickedRoom = this.GetComponentInParent<RoomData>();
        
        if (ClickedRoom.isActive && ClickedRoom.isLock)
        {
            Debug.Log("Oda Aktif Ve Kilitli!");
            Debug.Log("Tiklanan Obje => " + EventSystem.current.currentSelectedGameObject);
            if (!UIController.instance.IsPointerOverAnyUI())
            {
                if (GameManager.instance.GetCurrentGameMode() == GameMode.FPS)
                {
                    PlayerManager.instance.LockPlayer();
                }
                RoomManager.instance.BuyTheRoom(ClickedRoom);                
            }
        }
        else if (ClickedRoom.isActive && !ClickedRoom.isLock && GameManager.instance.GetCurrentGameMode() == GameMode.MuseumEditing)
        {
            if (!UIController.instance.IsPointerOverAnyUI())
            {
                Debug.Log("Oda Aktif Ve Kilitli Degil!");
                Debug.Log("Oda Duzenleme Moduna Giris Yapildi. Oda Hucre No =>" + ClickedRoom.availableRoomCell.CellLetter + ClickedRoom.availableRoomCell.CellNumber);
                RightUIPanelController.instance.EditModeObj.SetActive(false);
                ClickedRoom.SetActivationMyRoomEditingCamera(true);
                GameManager.instance.SetCurrenGameMode(GameMode.RoomEditing);
                RoomManager.instance.CurrentEditedRoom = ClickedRoom;
                //UIController.instance.CloseEditModeCanvas(true);
                //RoomManager.instance.CurrentEditedRoom.SetMyStatue()
                //RoomManager.instance.CurrentEditedRoom.GetMyStatueInTheMyRoom()._currentRoom = ClickedRoom;
                //GetComponent<BoxCollider>().enabled = false;
            }
        }
    }
    private void OnMouseDown()
    {
        StopAllCoroutines();
        StartCoroutine(WaitingForIsPointerOver());
        
    }
}
