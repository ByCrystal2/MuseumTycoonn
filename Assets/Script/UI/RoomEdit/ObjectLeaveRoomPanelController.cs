using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectLeaveRoomPanelController : MonoBehaviour
{
    [SerializeField] Image objProfile;
    [SerializeField] Text txtName;
    [SerializeField] Text txtPrice;
    [SerializeField] Text txtFocusedLevel;
    [SerializeField] Text txtBonusses;
    [SerializeField] Button btnLeaveObj;

    EditObjData _uiData;
    StatueSlotHandler _slotHandler;
    private void Awake()
    {
        btnLeaveObj.onClick.AddListener(OnClickedLeaveButton);
    }
    public void SetData(StatueSlotHandler _handler)
    {
        _slotHandler = _handler;
        _uiData = _handler.MyStatue;
        _uiData._currentRoomCell = _handler.MyStatue._currentRoomCell;
    }
    private void OnEnable()
    {
        if (_slotHandler != null && _uiData != null)
            SetUISWithObj();
        else
        {
            Debug.LogError($"Slot veya UIData objesi null oldugundan dolayi UI'lar gosterilemiyor! (SlotHandler Null =? {_slotHandler == null} and UIData Null =? {_uiData == null})");
            gameObject.SetActive(false);
        }
    }
    private void OnDisable()
    {
        _slotHandler = null;
        _uiData = null;
    }
    void SetUISWithObj()
    {
        objProfile.sprite = _uiData.ImageSprite;
        txtName.text = _uiData.Name;
        txtPrice.text = _uiData.Price.ToString();
        txtFocusedLevel.text = _uiData.FocusedLevel.ToString();
        txtBonusses.text = "";
        List<string> bonusses = new List<string>();
        if(_uiData.Bonusses != null && _uiData.Bonusses.Count > 0)
        {
            int length = _uiData.Bonusses.Count;
            for (int i = 0; i < length; i++)
            {
                Bonus bonus = _uiData.Bonusses[i];
                bonusses.Add(bonus.BonusType.ToString() + " => " + bonus.Value);
            }
        }
        int length1 = bonusses.Count;
        if (bonusses != null && bonusses.Count > 0)
            for (int i = 0; i < length1; i++)
                txtBonusses.text += bonusses[i] + "\n";
    }
    void OnClickedLeaveButton()
    {
        RoomManager.instance.statuesHandler.ObjAddToInventory(_uiData);
        EditObjData flagData = new EditObjData(RoomManager.instance.statuesHandler.GetStatueWithID(_uiData.ID));
        _slotHandler.MyStatue = null;
        Destroy(_slotHandler.gameObject.transform.GetChild(0).gameObject);
        UIController.instance.SetActivationRoomEditingPanel(false);
        RightUIPanelController.instance.UIVisibleClose(false);
        UIController.instance.CloseJoystickObj(false);
        FirestoreManager.instance.statueDatasHandler.AddOrUpdateStatueWithUserId(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID, flagData);
        gameObject.SetActive(false);
    }
}
