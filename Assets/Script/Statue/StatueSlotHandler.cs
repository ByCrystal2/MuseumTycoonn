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

    IEnumerator IEWaitingForIsPointer()
    {
        yield return new WaitForEndOfFrame();
        if (!UIController.instance.IsPointerOverAnyUI())
        {
            if (MyStatue != null && MyStatue.OnSlot)
            {
                //slottaysa.
                UIController.instance.SetActivationRoomEditingPanel(true);
                RoomEditingPanelController.instance.BuyEditObjPanel.SetActive(false);
                RoomEditingPanelController.instance.EditObjPanel.SetActive(false);
                RoomEditingPanelController.instance.RoomObjsPanel.SetActive(false);

                RoomEditingPanelController.instance.objLeaveRoomController.SetData(this);
                RoomEditingPanelController.instance.objLeaveRoomController.gameObject.SetActive(true);

                RightUIPanelController.instance.UIVisibleClose(true);
                RightUIPanelController.instance.CloseVisibleUIObjs(true);
                UIController.instance.CloseJoystickObj(true);
            }
            else
            {
                if (GameManager.instance.GetCurrentGameMode() == GameMode.FPS)
                {
                    PlayerManager.instance.LockPlayer();
                }
                UIController.instance.SetActivationRoomEditingPanel(true);
                RoomEditingPanelController.instance.BuyEditObjPanel.SetActive(false);
                RoomEditingPanelController.instance.EditObjPanel.SetActive(false);
                RoomEditingPanelController.instance.RoomObjsPanel.SetActive(true);
                RoomManager.instance.CurrentEditedRoom = RoomManager.instance.RoomDatas.Where(x => (x.availableRoomCell.CellLetter.ToString() + x.availableRoomCell.CellNumber.ToString()) == MyRoomCode).SingleOrDefault();
                RightUIPanelController.instance.UIVisibleClose(true);
                RightUIPanelController.instance.CloseVisibleUIObjs(true);
                UIController.instance.CloseJoystickObj(true);
            }
        }

        
        
    }
    IEnumerator IEWaitingForIsPointerOverride()
    {
        yield return new WaitForEndOfFrame();

        UIController.instance.SetActivationRoomEditingPanel(true);
        RoomEditingPanelController.instance.BuyEditObjPanel.SetActive(false);
        RoomEditingPanelController.instance.EditObjPanel.SetActive(false);
        RoomEditingPanelController.instance.RoomObjsPanel.SetActive(true);
        RoomManager.instance.CurrentEditedRoom = RoomManager.instance.RoomDatas.Where(x => (x.availableRoomCell.CellLetter.ToString() + x.availableRoomCell.CellNumber.ToString()) == MyRoomCode).SingleOrDefault();
        RightUIPanelController.instance.UIVisibleClose(true);
        RightUIPanelController.instance.CloseVisibleUIObjs(true);
        UIController.instance.CloseJoystickObj(true);
    }
    public void WaitingForIsPointer() //Tutorial unity event
    {
        StartCoroutine(IEWaitingForIsPointerOverride());
    }
    private void OnMouseDown()
    {
        
        StopAllCoroutines();
        StartCoroutine(IEWaitingForIsPointer());
    }
}
