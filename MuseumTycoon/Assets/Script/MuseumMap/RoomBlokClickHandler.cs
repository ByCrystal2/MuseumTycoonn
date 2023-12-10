using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RoomBlokClickHandler : MonoBehaviour
{
    private void OnMouseDown()
    {
        RoomData ClickedRoom = this.GetComponentInParent<RoomData>();

        if (!EventSystem.current.IsPointerOverGameObject() && ClickedRoom.isActive && ClickedRoom.isLock)
        {
            RoomManager.instance.BuyTheRoom(ClickedRoom);
        }
    }
}
