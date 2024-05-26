using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomUIHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] public RoomCell MyRoomCellCode;

    [SerializeField] TextMeshProUGUI RoomCellCodeText;
    [SerializeField] Image RoomIUImage;
    [SerializeField] GameObject RoomCloud;

    private RoomData MyTargetRoom;    
    private void Awake()
    {
        MyTargetRoom = GetComponentInParent<RoomData>();
        MyRoomCellCode = MyTargetRoom.availableRoomCell;
        RoomCellCodeText.text = MyRoomCellCode.CellLetter.ToString() + MyRoomCellCode.CellNumber.ToString();
    }
    private void Start()
    {
        
    }
    public void UpdateMyUI()
    {
        Debug.Log("MyTargetRoom Infos => " + "Active => " + MyTargetRoom.isActive + " Room Cell => " + MyTargetRoom.availableRoomCell.CellLetter.ToString() + MyTargetRoom.availableRoomCell.CellNumber.ToString() + " / " + RoomCellCodeText);
        if (MyTargetRoom.isActive)
        {
            SetRoomCloudActivation(false);
        }
        if (!MyTargetRoom.isLock && MyTargetRoom.isActive)
        {
            RoomIUImage.color = Color.green;            
        }
        else
        {
            RoomIUImage.color = Color.black;
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
