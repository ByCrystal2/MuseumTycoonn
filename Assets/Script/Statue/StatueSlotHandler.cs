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

    IEnumerator WaitingForIsPointer()
    {
        //for (int i = 0; i < 3; i++)
            yield return new WaitForEndOfFrame();

        if (UIController.instance.IsPointerOverAnyUI() || MyStatue.GetIsPurchased())
        {
            // satin alinmissa veya ui üzerindeyse.
        }
        else
        {
            if (GameManager.instance.GetCurrentGameMode() == GameMode.FPS)
            {
                PlayerManager.instance.LockPlayer();
            }
            UIController.instance.SetActivationRoomEditingPanel(true);
            RoomManager.instance.CurrentEditedRoom = RoomManager.instance.RoomDatas.Where(x => (x.availableRoomCell.CellLetter.ToString() + x.availableRoomCell.CellNumber.ToString()) == MyRoomCode).SingleOrDefault();
            RightUIPanelController.instance.UIVisibleClose(true);
            UIController.instance.CloseJoystickObj(true);
        }
        
    }
    private void OnMouseDown()
    {
        StopAllCoroutines();
        StartCoroutine(WaitingForIsPointer());
    }
}
