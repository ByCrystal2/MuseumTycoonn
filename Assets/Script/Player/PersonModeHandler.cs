using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PersonModeHandler : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] CinemachineFreeLook tpsCamera;
    [SerializeField] CinemachineVirtualCamera flcCamera;
    [Header("Buttons")]
    [SerializeField] Button flcButton;
    [SerializeField] Button tpsButton;
    [Header("Others")]
    [SerializeField] float defaultFLCCameraYPosition = 1.5f;
    bool _tpsModeOn = true;
    private void Awake()
    {
        flcButton.onClick.AddListener(OnSwitchCamera);
        tpsButton.onClick.AddListener(OnSwitchCamera);
    }
    public void OnSwitchCamera()
    {
        _tpsModeOn = !_tpsModeOn;
        tpsButton.gameObject.SetActive(!_tpsModeOn);
        flcButton.gameObject.SetActive(_tpsModeOn);

        tpsCamera.gameObject.SetActive(_tpsModeOn);

        if (!_tpsModeOn)
        {
            Transform player = PlayerManager.instance.GetPlayer().transform;
            if (player != null)
            {
                flcCamera.transform.SetPositionAndRotation(player.position, player.rotation);
                Vector3 newPos = flcCamera.transform.position;
                newPos = new Vector3(newPos.x, defaultFLCCameraYPosition, newPos.z);
                flcCamera.transform.position = newPos;
            }
        }

        flcCamera.gameObject.SetActive(!_tpsModeOn);
    }
}
