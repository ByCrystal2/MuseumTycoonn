using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatueSlotHandler : MonoBehaviour
{
    public EditObjData MyStatue;
    [SerializeField] public string MyRoomCode;
    public void SetMyStatue(EditObjData _myStatue)
    {
        MyStatue = _myStatue;
    }

    private void OnMouseDown()
    {
        if (!UIController.instance.AllUIPanelClosed())
            return;
        UIController.instance.SetActivationRoomEditingPanel(true);
        RoomManager.instance.CurrentEditedRoom = RoomManager.instance.RoomDatas.Where(x => (x.availableRoomCell.CellLetter.ToString() + x.availableRoomCell.CellNumber.ToString()) == MyRoomCode).SingleOrDefault();
        RightUIPanelController.instance.UIVisibleClose(true);
    }
}
