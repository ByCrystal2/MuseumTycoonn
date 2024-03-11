using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcMess : MonoBehaviour
{
    public RoomData inRoom;
    
    public void SetCurrentRoom(RoomData _room)
    {
        inRoom = _room;
    }
}
