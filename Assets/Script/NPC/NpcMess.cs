using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcMess : MonoBehaviour
{
    public RoomData inRoom;

    public bool _cleaningNow;
    public void SetCurrentRoom(RoomData _room)
    {
        inRoom = _room;
    }

    public void SetCleaning()
    {
        _cleaningNow = true;
    }
}
