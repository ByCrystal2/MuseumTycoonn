using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
    public class UICanvasControllerInput : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] Button danceOnButton;
        [SerializeField] GameObject danceButtonsPanel;
        [Header("Output")]
        public StarterAssetsInputs starterAssetsInputs;

        private void Awake()
        {
            danceOnButton.onClick.AddListener(OnDanceOnButtonClick);
        }
        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            starterAssetsInputs.MoveInput(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            starterAssetsInputs.LookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            starterAssetsInputs.JumpInput(virtualJumpState);
        }

        //public void VirtualSprintInput(bool virtualSprintState) // default metot.
        //{
        //    starterAssetsInputs.SprintInput(virtualSprintState);
        //}
        bool virtualSprintState = false;
        public void VirtualSprintInput() // default metot.
        {
            virtualSprintState = virtualSprintState == false ? true : false;
            starterAssetsInputs.SprintInput(virtualSprintState);
        }

        void OnDanceOnButtonClick()
        {
            bool active = danceButtonsPanel.activeSelf == false ? true : false;
            Debug.Log("In OnDanceOnButtonClick metot: danceButtonsPanel.activeSelf: " + active);
            danceButtonsPanel.SetActive(active);
            PlayGuitarHandler guitarHandler = PlayerManager.instance.GetPlayer().GetComponent<PlayGuitarHandler>();
            if (guitarHandler != null)
            {
                guitarHandler.StopPlay();
            }
        }
    }
}
