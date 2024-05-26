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
    [SerializeField] GameObject PlayerEditModeCanvas;

    [SerializeField] public GameObject EditObj;
    [SerializeField] GameObject VisibleUIObj;

    [SerializeField] Button EditModeButton;
    [SerializeField] Button UIVisibleButton;
    [SerializeField] GameObject UINotVisibleObj;

    [SerializeField] GameObject btnDailyRewardObj;
    private Vector3 defaultDailyRewardPos;
    [SerializeField] Transform fpsModeDailyRewardTransform;
    public static RightUIPanelController instance { get; private set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        defaultDailyRewardPos = btnDailyRewardObj.transform.position;
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
                //UIController.instance.CloseEditModeCanvas(true);
                FPSModeObj.SetActive(true);
                VisibleUIObj.SetActive(false);
                GameManager.instance.SetCurrenGameMode(GameMode.FPS);
                PicturesMenuController.instance.ExitPicturePanel();
                PlayerEditModeCanvas.SetActive(false);
                btnDailyRewardObj.transform.position = fpsModeDailyRewardTransform.position;
                break;
            case GameMode.FPS:
                // Ghost Moduna Gecis.
                //UIController.instance.CloseEditModeCanvas(false);
                EditModeObj.SetActive(true);
                VisibleUIObj.SetActive(true);
                UINotVisibleObj.SetActive(false);
                GameManager.instance.SetCurrenGameMode(GameMode.MuseumEditing);
                Vector3 _playerCurrentPos = PlayerManager.instance.GetPlayer().transform.position;
                PlayerEditModeCanvas.transform.position = new Vector3(_playerCurrentPos.x,PlayerEditModeCanvas.transform.position.y,_playerCurrentPos.z);
                PlayerEditModeCanvas.SetActive(true);
                PicturesMenuController.instance.ExitPicturePanel();
                btnDailyRewardObj.transform.position = defaultDailyRewardPos;
                //GhostModeObj.SetActive(true);
                //GameManager.instance.SetCurrenGameMode(GameMode.Ghost);
                break;
            case GameMode.Ghost: // simdilik devre disi. Ileride acilabilir.
                // Muze Edit Moduna Gecis.
                //UIController.instance.CloseEditModeCanvas(false);
                EditModeObj.SetActive(true);
                VisibleUIObj.SetActive(true);
                UINotVisibleObj.SetActive(false);
                GameManager.instance.SetCurrenGameMode(GameMode.MuseumEditing);
                btnDailyRewardObj.transform.position = defaultDailyRewardPos;
                break;
            case GameMode.RoomEditing:
                // Muze Edit Moduna Gecis.
                //UIController.instance.CloseEditModeCanvas(false);
                RoomManager.instance.CurrentEditedRoom.SetActivationMyRoomEditingCamera(false);
                UIController.instance.SetActivationRoomEditingPanel(false);
                EditModeObj.SetActive(true);
                VisibleUIObj.SetActive(true);
                UINotVisibleObj.SetActive(false);
                btnDailyRewardObj.transform.position = defaultDailyRewardPos;
                GameManager.instance.SetCurrenGameMode(GameMode.MuseumEditing);
                RoomManager.instance.CurrentEditedRoom.SetRoomBlockPanelActive(true);
                PicturesMenuController.instance.ExitPicturePanel();
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
                btnDailyRewardObj.SetActive(false);
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
                UIController.instance.SetActivationRoomEditingPanel(false);
                btnDailyRewardObj.SetActive(true);
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
            btnDailyRewardObj.SetActive(false);
            _uIVisible = false;
        }
        else
        {
            UINotVisibleObj.SetActive(false);
            EditObj.SetActive(true);
            UIController.instance.CloseCultureExpObj(false);
            UIController.instance.CloseLeftUIsPanel(false);
            UIController.instance.CloseMoneysObj(false);
            btnDailyRewardObj.SetActive(true);
            _uIVisible = true;
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
