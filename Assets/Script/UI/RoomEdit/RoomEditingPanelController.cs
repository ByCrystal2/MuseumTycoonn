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
    [SerializeField] public Transform ClikedStatueBonusContentPanel;
    [SerializeField] public Image ClickedStatueImage;
    [SerializeField] public Text txtClickedStatuePrice;
    [SerializeField] public Text txtClickedStatueName;
    [SerializeField] public Text txtClickedStatueTargetLevel;
    [SerializeField] public GameObject InsufficientFundsText;
    [SerializeField] public GameObject BonusText; // Prefab
    [SerializeField] public Button BuyStatueButton;
    [SerializeField] public Button ExitButton;


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
        ExitButton.onClick.AddListener(ExitMyPanel);
    }
    private void OnEnable()
    {
        
    }
    
    private void ExitMyPanel()
    {
        if (GameManager.instance.GetCurrentGameMode() == GameMode.FPS)
        {
            PlayerManager.instance.UnLockPlayer();
        }
        UIController.instance.SetActivationRoomEditingPanel(false);
        RightUIPanelController.instance.UIVisibleClose(false);
        UIController.instance.CloseJoystickObj(false);
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
            MyEditObj.data._currentRoomCell = _newData._currentRoomCell;
            MyEditObj.data.Bonusses = new List<Bonus>();
            MyEditObj.data.Bonusses.Clear();
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
            int length1 = _newData.Bonusses.Count;
            for (int j = 0; j < length1; j++)
            {
                MyEditObj.data.Bonusses.Add(_newData.Bonusses[j]);
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
            MyEditObj.data._currentRoomCell = _newData._currentRoomCell;
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

            MyEditObj.data.Bonusses = new List<Bonus>();
            MyEditObj.data.Bonusses.Clear();
            int length1 = _newData.Bonusses.Count;
            for (int j = 0; j < length1; j++)
            {
                MyEditObj.data.Bonusses.Add(_newData.Bonusses[j]);
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

        ClickedEditObjBehaviour.data._currentRoomCell = RoomManager.instance.CurrentEditedRoom.availableRoomCell;
        ClickedEditObjBehaviour.data.SetIsPurchased();
        StatueSlotHandler currentStateContent = FindObjectsOfType<StatueSlotHandler>().Where(x => x.MyRoomCode == (ClickedEditObjBehaviour.data._currentRoomCell.CellLetter.ToString() + ClickedEditObjBehaviour.data._currentRoomCell.CellNumber.ToString())).SingleOrDefault();
        Debug.Log("currentState.name => " + currentStateContent.name);
        Debug.Log("Statues[ClickedEditObjBehaviour.myStatueIndex].name => " + RoomManager.instance.statuesHandler.Statues[ClickedEditObjBehaviour.data.myStatueIndex].name);

        Vector3 anaScale = currentStateContent.transform.localScale;

        // Prefabýn scale deðerlerini hesapla ve ayarla

        GameObject Statue = Instantiate(RoomManager.instance.statuesHandler.Statues[ClickedEditObjBehaviour.data.myStatueIndex], currentStateContent.transform);
        Vector3 prefabScale = new Vector3(Statue.transform.localScale.x / anaScale.x, Statue.transform.localScale.y / anaScale.y, Statue.transform.localScale.z / anaScale.z);
        Statue.transform.localScale = prefabScale;
        EditObjBehaviour currentStatueData = Statue.AddComponent<EditObjBehaviour>();
        currentStatueData.data = new EditObjData(ClickedEditObjBehaviour.GetComponent<EditObjBehaviour>().data);
        currentStateContent.MyStatue = currentStatueData.data;

        RoomManager.instance.statuesHandler.activeEditObjs.Add(ClickedEditObjBehaviour.data);

        RoomManager.instance.statuesHandler.currentEditObjs.Remove(ClickedEditObjBehaviour.data);


        ClickedEditObjBehaviour = null;
        //UIController.instance.SetActivationRoomEditingPanel(false);
        AddStatuesInContent();
        UIController.instance.SetActivationRoomEditingPanel(false);
        RightUIPanelController.instance.UIVisibleClose(false);
        UIController.instance.CloseJoystickObj(false);
        GameManager.instance.Save();
    }

    public void ClearClikedStatueBonusContent()
    {
        int length = ClikedStatueBonusContentPanel.childCount;
        if (length == 0) return;
        for (int i = 0; i < length; i++)
        {
            Destroy(ClikedStatueBonusContentPanel.GetChild(i).gameObject);
        }
    }
    public void SetStatueBuyingPanelUIs(EditObjData _data)
    {
        ClearClikedStatueBonusContent();
        ClickedStatueImage.sprite = _data.ImageSprite;
        float dataPrice = _data.Price;
        txtClickedStatuePrice.text = dataPrice.ToString();
        if (dataPrice > MuseumManager.instance.GetCurrentGold())
        {
            txtClickedStatuePrice.color = Color.red;
            InsufficientFundsText.SetActive(true);
            BuyStatueButton.interactable = false;
        }
        else
        {
            txtClickedStatuePrice.color = Color.green;
            InsufficientFundsText.SetActive(false);
            BuyStatueButton.interactable = true;
        }
        txtClickedStatueName.text = _data.Name;
        txtClickedStatueTargetLevel.text = "+" + _data.FocusedLevel.ToString() + " lvl";

        foreach (var bonus in _data.Bonusses)
        {
            GameObject newBonusObj = Instantiate(BonusText, ClikedStatueBonusContentPanel.transform);
            Text newBonusText = newBonusObj.GetComponent<Text>();
            newBonusText.text = $"{bonus.BonusType} => +{bonus.Value}";
        }
    }
}

public enum EditObjType
{
    None,
    Statue,
    Decoration
}