using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;

public class RightUIPanelController : MonoBehaviour
{
    [SerializeField] public GameObject EditModeObj;
    [SerializeField] GameObject FPSModeObj;
    [SerializeField] GameObject GhostModeObj;

    [SerializeField] public GameObject EditObj;
    [SerializeField] GameObject VisibleUIObj;

    [SerializeField] Button EditModeButton;
    [SerializeField] Button UIVisibleButton;
    [SerializeField] GameObject UINotVisibleObj;
    public static RightUIPanelController instance { get; private set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;        
    }
    private void Start()
    {
        GameManager.instance.SetCurrenGameMode(GameMode.Ghost);

        EditModeButton.onClick.RemoveAllListeners();
        UIVisibleButton.onClick.RemoveAllListeners();
        EditModeButton.onClick.AddListener(EditMode);
        UIVisibleButton.onClick.AddListener(UIVisibleClose);
        EditMode();
    }
    bool _uIVisible = true;

    public void EditMode() // Edit Mode Button AddListener.
    {
        CloseAllMods();
        GameMode _gameMode = GameManager.instance.GetCurrentGameMode();
        switch (_gameMode)
        {
            case GameMode.MuseumEditing:
                // FPS Moduna gecis.
                FPSModeObj.SetActive(true);
                VisibleUIObj.SetActive(false);
                GameManager.instance.SetCurrenGameMode(GameMode.FPS);
                break;
            case GameMode.FPS:
                // Ghost Moduna Gecis.
                GhostModeObj.SetActive(true);
                GameManager.instance.SetCurrenGameMode(GameMode.Ghost);
                break;
            case GameMode.Ghost:
                // Muze Edit Moduna Gecis.
                EditModeObj.SetActive(true);
                VisibleUIObj.SetActive(true);
                UINotVisibleObj.SetActive(false);
                GameManager.instance.SetCurrenGameMode(GameMode.MuseumEditing);
                break;
            case GameMode.RoomEditing:
                // Muze Edit Moduna Gecis.
                RoomManager.instance.CurrentEditedRoom.SetActivationMyRoomEditingCamera(false);
                UIController.instance.SetActivationRoomEditingPanel(false);
                EditModeObj.SetActive(true);
                VisibleUIObj.SetActive(true);
                UINotVisibleObj.SetActive(false);
                GameManager.instance.SetCurrenGameMode(GameMode.MuseumEditing);
                RoomManager.instance.CurrentEditedRoom.SetRoomBlockPanelActive(true);
                Debug.Log("RoomEditing mode Debug.");
                break;
            default:
                break;
        }
    }
    public void UIVisibleClose()
    {
        if (GameManager.instance.GetCurrentGameMode() == GameMode.MuseumEditing || GameManager.instance.GetCurrentGameMode() == GameMode.RoomEditing)
        {
            if (_uIVisible)
            {
                UINotVisibleObj.SetActive(true);
                EditObj.SetActive(false);
                PicturesMenuController.instance.ExitPicturePanel();
                //if (GameManager.instance.GetCurrentGameMode() == GameMode.RoomEditing)
                //    UIController.instance.SetActivationRoomEditingPanel(true);

                UIController.instance.CloseNPCInformationPanel();
                UIController.instance.CloseWorkerShopPanel(true);
                UIController.instance.CloseWorkerAssignmentPanel(true);
                UIController.instance.CloseDailyRewardPanel(true);
                UIController.instance.CloseMuseumStatsPanel(true);
                UIController.instance.CloseCultureExpObj(true);
                UIController.instance.CloseLeftUIsPanel(true);
                UIController.instance.CloseMoneysObj(true);
                _uIVisible = false;
            }
            else
            {
                UINotVisibleObj.SetActive(false);
                //if (GameManager.instance.GetCurrentGameMode() == GameMode.RoomEditing)
                //    UIController.instance.SetActivationRoomEditingPanel(false);
                EditObj.SetActive(true);
                UIController.instance.CloseCultureExpObj(false);
                UIController.instance.CloseLeftUIsPanel(false);
                UIController.instance.CloseMoneysObj(false);
                _uIVisible = true;
            }
        }
    }
    public void UIVisibleClose(bool _forceVisible)
    {
        if (_forceVisible)
        {
            UINotVisibleObj.SetActive(true);
            EditObj.SetActive(false);
            PicturesMenuController.instance.ExitPicturePanel();

            UIController.instance.CloseNPCInformationPanel();
            UIController.instance.CloseWorkerShopPanel(true);
            UIController.instance.CloseWorkerAssignmentPanel(true);
            UIController.instance.CloseDailyRewardPanel(true);
            UIController.instance.CloseMuseumStatsPanel(true);
            UIController.instance.CloseCultureExpObj(true);
            UIController.instance.CloseLeftUIsPanel(true);
            UIController.instance.CloseMoneysObj(true);
        }
        else
        {
            UINotVisibleObj.SetActive(false);
            EditObj.SetActive(true);
            UIController.instance.CloseCultureExpObj(false);
            UIController.instance.CloseLeftUIsPanel(false);
            UIController.instance.CloseMoneysObj(false);
        }
    }
    public void CloseEditObj(bool _close)
    {
        EditObj.SetActive(!_close);
        VisibleUIObj.SetActive(false);
        if (GameManager.instance.GetCurrentGameMode() == GameMode.MuseumEditing)
        {
            VisibleUIObj.SetActive(!_close);
        }
    }
    private void CloseAllMods()
    {
        EditModeObj.SetActive(false);
        FPSModeObj.SetActive(false);
        GhostModeObj.SetActive(false);
    }
}
