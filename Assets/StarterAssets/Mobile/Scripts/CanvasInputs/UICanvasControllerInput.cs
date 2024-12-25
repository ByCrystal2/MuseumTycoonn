using UnityEngine;

namespace StarterAssets
{
    public class UICanvasControllerInput : MonoBehaviour
    {

        [Header("Output")]
        public StarterAssetsInputs starterAssetsInputs;

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
            virtualSprintState = !virtualSprintState;
            starterAssetsInputs.SprintInput(virtualSprintState);
        }
    }

}
