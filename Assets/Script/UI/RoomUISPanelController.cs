using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomUISPanelController : MonoBehaviour
{
    private List<RoomUIHandler> RoomUIS = new List<RoomUIHandler>();
    public void InitializeRoomUIS()
    {
        RoomUIS = FindObjectsOfType<RoomUIHandler>().ToList();
    }
    public List<RoomUIHandler> GetRoomUIS()
    {
        return RoomUIS;
    }
    public RoomUIHandler GetRoomUI(RoomCell _cell)
    {
        RoomUIHandler roomUIHandler = RoomUIS.Where(x => x.MyRoomCellCode.CellLetter.ToString() == _cell.CellLetter.ToString() && x.MyRoomCellCode.CellNumber.ToString() == _cell.CellNumber.ToString()).SingleOrDefault();
        Debug.Log("roomUIHandler => " + roomUIHandler.MyRoomCellCode.CellLetter.ToString() + roomUIHandler.MyRoomCellCode.CellNumber.ToString());
        return roomUIHandler;
    }
}
