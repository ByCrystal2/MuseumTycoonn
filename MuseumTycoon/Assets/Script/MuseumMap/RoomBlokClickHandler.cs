using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RoomBlokClickHandler : MonoBehaviour
{
    private void OnMouseDown()
    {
        RoomData ClickedRoom = this.GetComponentInParent<RoomData>();
        if (ClickedRoom.isActive && ClickedRoom.isLock)
        {
            Debug.Log("Oda Aktif Ve Kilitli!");
            Debug.Log("Tiklanan Obje => " + EventSystem.current.currentSelectedGameObject);
            
            if (!EventSystem.current.IsPointerOverGameObject() )
            {
                RoomManager.instance.BuyTheRoom(ClickedRoom);
            }

        }
        else if (ClickedRoom.isActive && !ClickedRoom.isLock && GameManager.instance.GetCurrentGameMode() == GameMode.MuseumEditing)
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("Oda Aktif Ve Kilitli Degil!");
                Debug.Log("Oda Duzenleme Moduna Giris Yapildi. Oda Hucre No =>" + ClickedRoom.availableRoomCell.CellLetter + ClickedRoom.availableRoomCell.CellNumber);
                RightUIPanelController.instance.EditModeObj.SetActive(false);
                ClickedRoom.SetActivationMyRoomEditingCamera(true);
                GameManager.instance.SetCurrenGameMode(GameMode.RoomEditing);
                RoomManager.instance.CurrentEditedRoom = ClickedRoom;
                GetComponent<BoxCollider>().enabled = false;
            }
        }
    }
}
