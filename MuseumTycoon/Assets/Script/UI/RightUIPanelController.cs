using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;

public class RightUIPanelController : MonoBehaviour
{
    [SerializeField] GameObject EditModeObj;
    [SerializeField] GameObject FPSModeObj;
    [SerializeField] GameObject GhostModeObj;

    [SerializeField] GameObject EditObj;
    [SerializeField] GameObject VisibleUIObj;

    [SerializeField] Button EditModeButton;
    [SerializeField] Button UIVisibleButton;
    [SerializeField] GameObject UINotVisibleObj;

    [HideInInspector] public GameModes CurrentGameMode;
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
        EditModeButton.onClick.RemoveAllListeners();
        UIVisibleButton.onClick.RemoveAllListeners();
        EditModeButton.onClick.AddListener(EditMode);
        UIVisibleButton.onClick.AddListener(UIVisible);
        CurrentGameMode = GameModes.Ghost;
        EditMode();
    }
    bool _uIVisible = true;

    public void EditMode() // Edit Mode Button AddListener.
    {
        CloseAllMods();
        switch (CurrentGameMode)
        {
            case GameModes.Edit:
                // FPS Moduna gecis.
                FPSModeObj.SetActive(true);
                VisibleUIObj.SetActive(false);
                CurrentGameMode = GameModes.FPS;
                break;
            case GameModes.FPS:
                // Ghost Moduna Gecis.
                GhostModeObj.SetActive(true);
                CurrentGameMode = GameModes.Ghost;
                break;
            case GameModes.Ghost:
                // Edit Moduna Gecis.
                EditModeObj.SetActive(true);
                VisibleUIObj.SetActive(true);
                UINotVisibleObj.SetActive(false);
                CurrentGameMode = GameModes.Edit;
                break;
            default:
                break;
        }
    }
    public void UIVisible()
    {
        if (CurrentGameMode == GameModes.Edit)
        {
            if (_uIVisible)
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
                _uIVisible = false;
            }
            else
            {
                UINotVisibleObj.SetActive(false);
                EditObj.SetActive(true);
                UIController.instance.CloseCultureExpObj(false);
                UIController.instance.CloseLeftUIsPanel(false);
                UIController.instance.CloseMoneysObj(false);
                _uIVisible = true;
            }
        }
    }
    public void CloseEditObj(bool _close)
    {
        EditObj.SetActive(!_close);
        VisibleUIObj.SetActive(false);
        if (CurrentGameMode == GameModes.Edit)
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
public enum GameModes
{
    Edit,
    FPS,
    Ghost
}
