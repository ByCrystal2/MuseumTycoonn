using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RoomEditingPanelController : MonoBehaviour
{
    
    [SerializeField] Transform EditObjsContent;
    [SerializeField] GameObject EditObj_StatueVariant;
    [SerializeField] GameObject EditObj_DecorationVariant;    
    //UI
    [SerializeField] public GameObject BuyEditObjPanel;
    [SerializeField] public Image ClickedEditObjImage;
    public StatueSlotHandler CurrentStatueSlot;
    public EditObjBehaviour ClickedEditObjBehaviour;
    
    [SerializeField] GameObject LockedPanel;
    public static RoomEditingPanelController instance { get; private set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;        
    }
    private void OnEnable()
    {
        
    }
    
    
    private void ClearEditObjContent()
    {
        int length = EditObjsContent.childCount;
        if (length <= 0) return;
        for (int i = 0; i < length; i++)
            Destroy(EditObjsContent.GetChild(i).gameObject);
    }
    
    
    
    public void AddStatuesInContent() // pnlRoomEditing / pnlRoomObjs / btnStatue
    {
        BuyEditObjPanel.SetActive(false);
        ClearEditObjContent();
        List<EditObjData> _statues = new List<EditObjData>();
        _statues = RoomManager.instance.statuesHandler.currentEditObjs.Where(x=> x.EditType == EditObjType.Statue).ToList();
        int length = _statues.Count;
        for (int i = 0; i < length; i++)
        {
            GameObject _newStatue = Instantiate(EditObj_StatueVariant, EditObjsContent);
            EditObjBehaviour MyEditObj = _newStatue.AddComponent<EditObjBehaviour>();
            EditObjData _newData = _statues[i];
            MyEditObj.data = new EditObjData();
            MyEditObj.data.ID = _newData.ID;
            MyEditObj.data.Name = _newData.Name;
            MyEditObj.data.Price = _newData.Price;
            MyEditObj.data.EditType = _newData.EditType;
            MyEditObj.data.ImageSprite = _newData.ImageSprite;
            MyEditObj.data._currentRoom = _newData._currentRoom;
            MyEditObj.data.BonusEnums = new List<(EditObjBonusType _bonusses, int _value)>();
            MyEditObj.data.BonusEnums.Clear();
            MyEditObj.data.IsPurchased = _newData.IsPurchased;
            MyEditObj.data.IsLocked = _newData.IsLocked;
            MyEditObj.data.FocusedLevel = _newData.FocusedLevel;

            if (MuseumManager.instance.GetCurrentCultureLevel() >= MyEditObj.data.FocusedLevel)
            {
                MyEditObj.data.UnLock();
                MyEditObj.data.isClickable = true;                
            }
            else
            {
                MyEditObj.data.IsLocked = true;
                MyEditObj.data.isClickable = false;
                Instantiate(LockedPanel, _newStatue.transform.GetChild(0));
            }
            int length1 = _newData.BonusEnums.Count;
            for (int j = 0; j < length1; j++)
            {
                MyEditObj.data.BonusEnums.Add(_newData.BonusEnums[j]);
            }
            _newStatue.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = _newData.ImageSprite;
        }
    }
    public void AddDecorationInContent() // pnlRoomEditing / pnlRoomObjs / btnDecoration
    {
        BuyEditObjPanel.SetActive(false);
        ClearEditObjContent();
        List<EditObjData> _decoration = new List<EditObjData>();
        _decoration = RoomManager.instance.statuesHandler.editObjs.Where(x => x.EditType == EditObjType.Decoration).ToList();
        int length = _decoration.Count;
        for (int i = 0; i < length; i++)
        {
            GameObject _newStatue = Instantiate(EditObj_DecorationVariant, EditObjsContent);
            EditObjBehaviour MyEditObj = _newStatue.AddComponent<EditObjBehaviour>();
            EditObjData _newData = _decoration[i];
            MyEditObj.data = new EditObjData();
            MyEditObj.data.ID = _newData.ID;
            MyEditObj.data.Name = _newData.Name;
            MyEditObj.data.Price = _newData.Price;
            MyEditObj.data.EditType = _newData.EditType;
            MyEditObj.data.ImageSprite = _newData.ImageSprite;
            MyEditObj.data._currentRoom = _newData._currentRoom;
            MyEditObj.data.IsPurchased = _newData.IsPurchased;
            MyEditObj.data.IsLocked = _newData.IsLocked;
            if (MuseumManager.instance.GetCurrentCultureLevel() >= MyEditObj.data.FocusedLevel)
            {
                MyEditObj.data.UnLock();
                MyEditObj.data.isClickable = true;
                Instantiate(LockedPanel, _newStatue.transform.GetChild(0));
            }
            else
            {
                MyEditObj.data.IsLocked = true;
                MyEditObj.data.isClickable = false;
            }
            MyEditObj.data.FocusedLevel = _newData.FocusedLevel;

            MyEditObj.data.BonusEnums = new List<(EditObjBonusType _bonusses, int _value)>();
            MyEditObj.data.BonusEnums.Clear();
            int length1 = _newData.BonusEnums.Count;
            for (int j = 0; j < length1; j++)
            {
                MyEditObj.data.BonusEnums.Add(_newData.BonusEnums[j]);
            }

            _newStatue.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = _newData.ImageSprite;            
        }
    }
    public void AddBonusActivation() // pnlRoomEditing / pnlBuyTheEditObj / btnBuyEditObj ~onClick
    {
        if (ClickedEditObjBehaviour.data.GetIsPurchased())
        {
            Debug.Log("Obje Satilmis. ");
            return;
        }
        if (ClickedEditObjBehaviour.data.Price > MuseumManager.instance.GetCurrentGold())
        {
            Debug.Log(ClickedEditObjBehaviour.data.Name + " Adli heykeli almaya paraniz yetmedi.");
            ClickedEditObjBehaviour = null;
            return;
        }
        else
            MuseumManager.instance.SpendingGold(ClickedEditObjBehaviour.data.Price);

        ClickedEditObjBehaviour.data._currentRoom = RoomManager.instance.CurrentEditedRoom;
        ClickedEditObjBehaviour.data.SetIsPurchased();
        StatueSlotHandler currentStateContent = FindObjectsOfType<StatueSlotHandler>().Where(x => x.MyRoomCode == (ClickedEditObjBehaviour.data._currentRoom.availableRoomCell.CellLetter.ToString() + ClickedEditObjBehaviour.data._currentRoom.availableRoomCell.CellNumber.ToString())).SingleOrDefault();
        Debug.Log("currentState.name => " + currentStateContent.name);
        Debug.Log("Statues[ClickedEditObjBehaviour.myStatueIndex].name => " + RoomManager.instance.statuesHandler.Statues[ClickedEditObjBehaviour.data.myStatueIndex].name);

        Vector3 anaScale = gameObject.transform.localScale;

        // Prefabýn scale deðerlerini hesapla ve ayarla
        Vector3 prefabScale = new Vector3(1.0f * anaScale.x, 1.0f * anaScale.y, 1.0f * anaScale.z);

        GameObject Statue = Instantiate(RoomManager.instance.statuesHandler.Statues[ClickedEditObjBehaviour.data.myStatueIndex], currentStateContent.transform);
        Statue.transform.localScale = prefabScale;
        EditObjBehaviour currentStatueData = Statue.AddComponent<EditObjBehaviour>();
        currentStatueData.data = new EditObjData(ClickedEditObjBehaviour.GetComponent<EditObjBehaviour>().data);
        currentStateContent.MyStatue = currentStatueData.data;
        RoomManager.instance.statuesHandler.currentEditObjs.Remove(RoomManager.instance.statuesHandler.currentEditObjs.Where(x => x.ID == ClickedEditObjBehaviour.data.ID).SingleOrDefault());
        ClickedEditObjBehaviour = null;
        //UIController.instance.SetActivationRoomEditingPanel(false);
        AddStatuesInContent();
        UIController.instance.SetActivationRoomEditingPanel(false);
        RightUIPanelController.instance.UIVisibleClose(false);
    }
}

public enum EditObjType
{
    None,
    Statue,
    Decoration
}