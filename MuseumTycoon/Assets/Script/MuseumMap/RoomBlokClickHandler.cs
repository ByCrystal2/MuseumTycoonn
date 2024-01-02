using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RoomBlokClickHandler : MonoBehaviour
{
    private void OnMouseDown()
    {
        RoomData ClickedRoom = this.GetComponentInParent<RoomData>();

        Debug.Log("Odaya Tiklandi!");
        if (ClickedRoom.isActive && ClickedRoom.isLock)
        {
            Debug.Log("Oda Aktif Ve Kilitli!");
            Debug.Log("Tiklanan Obje => " + EventSystem.current.currentSelectedGameObject);
            
            if (!EventSystem.current.IsPointerOverGameObject() )
            {
                RoomManager.instance.BuyTheRoom(ClickedRoom);
            }

        }
    }
}
