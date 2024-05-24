using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomUISPanelController : MonoBehaviour
{
    private List<RoomUIHandler> RoomUIS = new List<RoomUIHandler>();
    private void Awake()
    {        
        RoomUIS = FindObjectsOfType<RoomUIHandler>().ToList();
    }
    public List<RoomUIHandler> GetRoomUIS()
    {
        return RoomUIS;
    }
    public RoomUIHandler GetRoomUI(RoomCell _cell)
    {
        return RoomUIS.Where(x => x.MyRoomCellCode.CellLetter == _cell.CellLetter && x.MyRoomCellCode.CellNumber == _cell.CellNumber).SingleOrDefault();
    }
}
