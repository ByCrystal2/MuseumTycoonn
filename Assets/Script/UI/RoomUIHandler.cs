using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomUIHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] public RoomCell MyRoomCellCode;

    [SerializeField] Text RoomCellCodeText;
    [SerializeField] GameObject RoomCloud;

    private RoomData MyTargetRoom;
    private void OnEnable()
    {
        MyTargetRoom = RoomManager.instance.GetRoomWithRoomCell(MyRoomCellCode);
        RoomCellCodeText.text = MyRoomCellCode.CellLetter.ToString() + MyRoomCellCode.CellNumber.ToString();
        UpdateMyUI();
    }
    public void UpdateMyUI()
    {
        if (MyTargetRoom.isActive)
        {
            SetRoomCloudActivation(false);
        }
        if (!MyTargetRoom.isLock && MyTargetRoom.isActive)
        {
            GetComponent<Image>().color = Color.green;            
        }
        else
        {
            GetComponent<Image>().color = Color.black;
        }
        
    }

    IEnumerator WaitingForIsPointerOver()
    {
        for (int i = 0; i < 3; i++)
            yield return new WaitForEndOfFrame();

        if (MyTargetRoom.isActive && MyTargetRoom.isLock)
        {
            Debug.Log("Oda Aktif Ve Kilitli!");
            Debug.Log("Tiklanan Obje => " + EventSystem.current.currentSelectedGameObject);

            if (!UIController.instance.IsPointerOverAnyUI())
            {
                RoomManager.instance.BuyTheRoom(MyTargetRoom);
            }

        }
        else if (MyTargetRoom.isActive && !MyTargetRoom.isLock && GameManager.instance.GetCurrentGameMode() == GameMode.MuseumEditing)
        {
            if (!UIController.instance.IsPointerOverAnyUI())
            {
                Debug.Log("Oda Aktif Ve Kilitli Degil!");
                Debug.Log("Oda Duzenleme Moduna Giris Yapildi. Oda Hucre No =>" + MyTargetRoom.availableRoomCell.CellLetter + MyTargetRoom.availableRoomCell.CellNumber);
                RightUIPanelController.instance.EditModeObj.SetActive(false);
                MyTargetRoom.SetActivationMyRoomEditingCamera(true);
                GameManager.instance.SetCurrenGameMode(GameMode.RoomEditing);
                RoomManager.instance.CurrentEditedRoom = MyTargetRoom;
                UIController.instance.CloseEditModeCanvas(true);
                //RoomManager.instance.CurrentEditedRoom.SetMyStatue()
                //RoomManager.instance.CurrentEditedRoom.GetMyStatueInTheMyRoom()._currentRoom = ClickedRoom;
                //GetComponent<BoxCollider>().enabled = false;
            }
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(WaitingForIsPointerOver());
    }
    public void SetRoomCloudActivation(bool _active)
    {
        RoomCloud.SetActive(_active);
    }
}
