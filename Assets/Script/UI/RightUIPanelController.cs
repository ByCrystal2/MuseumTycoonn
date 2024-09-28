using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Entities.UniversalDelegates;
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
    [SerializeField] GameObject UINotVisibleObj;

    [SerializeField] Button EditModeButton;
    [SerializeField] Button UIVisibleButton;

    [SerializeField] GameObject btnDailyRewardObj;

    [SerializeField] GameObject notificationsObj;
    private Vector3 defaultDailyRewardPos;
    [SerializeField] Transform fpsModeDailyRewardTransform;

    [SerializeField] GameObject SelectionCamsPanel;
    [SerializeField] Button CamUISActivetonButton;
    [SerializeField] Button[] CamButtons;
    [SerializeField] Button DrawerButton;
    [SerializeField] DrawerController DrawerPanel;

    [SerializeField] Button CustomizePanelOnButton;
    
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
        DrawerPanel.gameObject.SetActive(false);
    }
    private void Start()
    {
        GameManager.instance.SetCurrenGameMode(GameMode.MuseumEditing);
        SetActivationSelectionCamsCanvas(false);
        CamUISActivetonButton.onClick.RemoveAllListeners();
        EditModeButton.onClick.RemoveAllListeners();
        UIVisibleButton.onClick.RemoveAllListeners();
        CustomizePanelOnButton.onClick.RemoveAllListeners();
        EditModeButton.onClick.AddListener(EditMode);
        UIVisibleButton.onClick.AddListener(UIVisibleClose);
        CamUISActivetonButton.onClick.AddListener(SetActivationCamUIS);
        CustomizePanelOnButton.onClick.AddListener(CustomizeHandler.instance.SwitchCustomizePanel);
        CustomizePanelOnButton.onClick.AddListener(() => UIVisibleClose(true));
        CustomizePanelOnButton.onClick.AddListener(PlayerManager.instance.LockPlayer);
        CustomizePanelOnButton.onClick.AddListener(() => UIController.instance.CloseJoystickObj(true));

        DrawerButton.onClick.AddListener(DrawerSetActiveConroller);

        int index = 0;
        foreach (Button camButton in CamButtons)
        {
            int capturedIndex = index; // Yerel deðiþken oluþturuyoruz
            camButton.onClick.AddListener(() => RoomManager.instance.CurrentEditedRoom.SetActivationMyRoomEditingCamera(capturedIndex, true));
            index++;
        }

    }
    public void DrawerSetActiveConroller()
    {
        if (DrawerController.TweenIsPlaying())
            return;

        bool drawerSetActiveControl = DrawerPanel.gameObject.activeSelf;
        if (!drawerSetActiveControl)
        {
            DrawerPanel.gameObject.SetActive(true);
        }
        DrawerPanel.ScaleMove(drawerSetActiveControl);

            
    }
    bool _uIVisible = true;

    public void EditMode() // Edit Mode Button AddListener.
    {
        CloseAllMods();
        GameMode _gameMode = GameManager.instance.GetCurrentGameMode();
        DrawerPanel.ScaleMove(true);
        switch (_gameMode)
        {
            case GameMode.MuseumEditing:
                // FPS Moduna gecis.
                //UIController.instance.CloseEditModeCanvas(true);
                FPSModeObj.SetActive(true);
                notificationsObj.SetActive(true);
                VisibleUIObj.SetActive(false);
                UIController.instance.CloseJoystickObj(false);
                PlayerManager.instance.UnLockPlayer();
                GameManager.instance.SetCurrenGameMode(GameMode.FPS);
                if (GameManager.instance.IsWatchTutorial)
                //PicturesMenuController.instance.ExitPicturePanel();
                PlayerEditModeCanvas.SetActive(false);
                btnDailyRewardObj.transform.position = fpsModeDailyRewardTransform.position;
                break;
            case GameMode.FPS:
                // Ghost Moduna Gecis.
                //UIController.instance.CloseEditModeCanvas(false);
                EditModeObj.SetActive(true);
                notificationsObj.SetActive(true);
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
                SetActivationSelectionCamsCanvas(false);
                RoomManager.instance.CurrentEditedRoom.CloseAllMyRoomEditingCameras();
                UIController.instance.SetActivationRoomEditingPanel(false);
                EditModeObj.SetActive(true);
                VisibleUIObj.SetActive(true);
                notificationsObj.SetActive(true);
                UINotVisibleObj.SetActive(false);
                UIController.instance.CloseJoystickObj(true);
                btnDailyRewardObj.transform.position = defaultDailyRewardPos;
                GameManager.instance.SetCurrenGameMode(GameMode.MuseumEditing);
                RoomManager.instance.CurrentEditedRoom.SetRoomBlockPanelActive(true);
                //PicturesMenuController.instance.ExitPicturePanel();
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
                DrawerActivation(false);
                EditObj.SetActive(false);
                SetActivationSelectionCamsCanvas(false);
                //PicturesMenuController.instance.ExitPicturePanel();
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
                //btnDailyRewardObj.SetActive(false);
                _uIVisible = false;
            }
            else
            {
                UINotVisibleObj.SetActive(false);
                //if (GameManager.instance.GetCurrentGameMode() == GameMode.RoomEditing)
                //    UIController.instance.SetActivationRoomEditingPanel(false);
                EditObj.SetActive(true);
                DrawerActivation(true);
                if (GameManager.instance.GetCurrentGameMode() == GameMode.RoomEditing)
                    SetActivationSelectionCamsCanvas(true);

                UIController.instance.CloseCultureExpObj(false);
                UIController.instance.CloseLeftUIsPanel(false);
                UIController.instance.CloseMoneysObj(false);
                UIController.instance.SetActivationRoomEditingPanel(false);
                ///*btnDailyRewardObj*/.SetActive(true);
                _uIVisible = true;
            }
        }
    }
    public void UIVisibleClose(bool _forceVisible)
    {
        Debug.Log("UI Visible: " + _forceVisible);
        if (_forceVisible)
        {
            //UINotVisibleObj.SetActive(true);
            EditObj.SetActive(false);
            DrawerActivation(false);
            SetActivationSelectionCamsCanvas(false);
            //PicturesMenuController.instance.ExitPicturePanel();

            UIController.instance.CloseNPCInformationPanel();
            UIController.instance.CloseWorkerShopPanel(true);
            UIController.instance.CloseWorkerAssignmentPanel(true);
            UIController.instance.CloseDailyRewardPanel(true);
            UIController.instance.CloseMuseumStatsPanel(true);
            UIController.instance.CloseCultureExpObj(true);
            UIController.instance.CloseLeftUIsPanel(true);
            UIController.instance.CloseMoneysObj(true);
            //btnDailyRewardObj.SetActive(false);
            _uIVisible = false;
        }
        else
        {
            //UINotVisibleObj.SetActive(false);
            EditObj.SetActive(true);
            DrawerActivation(true);
            if (GameManager.instance.GetCurrentGameMode() == GameMode.RoomEditing)
                SetActivationSelectionCamsCanvas(true);
            UIController.instance.CloseCultureExpObj(false);
            UIController.instance.CloseLeftUIsPanel(false);
            UIController.instance.CloseMoneysObj(false);
            //btnDailyRewardObj.SetActive(true);
            _uIVisible = true;
        }
    }
    public void CloseEditObj(bool _close)
    {
        EditObj.SetActive(!_close);
        VisibleUIObj.SetActive(false);
        if (GameManager.instance.GetCurrentGameMode() == GameMode.MuseumEditing || GameManager.instance.GetCurrentGameMode() == GameMode.RoomEditing)
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
    bool camUIActive = false;
    public void SetActivationCamUIS()
    {
        Debug.Log("SetActivationCamUIS method is starting..");
        StartCoroutine(IESetActivationCamUIS());
    }
    IEnumerator IESetActivationCamUIS()
    {
        int length = CamButtons.Length;
        for (int i = 0; i < length; i++)
        {
            Button currentButton = CamButtons[i];
            if (!camUIActive)
            {
                currentButton.gameObject.SetActive(true);
                currentButton.gameObject.transform.DOLocalRotate(new Vector3(0, 360, 0), 0.6f).SetEase(Ease.OutCirc);
                yield return new WaitForSeconds(0.08f);
            }
            else
            {
                currentButton.gameObject.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.3f).SetEase(Ease.InCirc);
                yield return new WaitForSeconds(0.04f);
                currentButton.gameObject.SetActive(false);
            }
        }
            camUIActive = !camUIActive;
    }
    public void SetActivationSelectionCamsCanvas(bool _active)
    {
        if (!_active)
        {
            int length = CamButtons.Length;
            for (int i = 0; i < length; i++)
            {
                Button currentButton = CamButtons[i];
                currentButton.gameObject.SetActive(false);
            }
        }
        SelectionCamsPanel.SetActive(_active);
    }
    void DrawerActivation(bool _active)
    {
        DrawerPanel.gameObject.SetActive(_active);
        DrawerButton.gameObject.SetActive(_active);
    }
}
