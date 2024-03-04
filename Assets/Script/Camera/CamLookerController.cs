using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CamLookerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public GameObject joystickPrefab;

    Vector2 joystickOutput = Vector2.zero;
    UIVirtualJoystick uIVirtualJoystick;

    private bool onExecuted;
    private void Awake()
    {
        joystickPrefab = GameObject.FindWithTag("Joystick");
        uIVirtualJoystick = FindObjectOfType<UIVirtualJoystick>();
    }
    void Start()
    {
        // UIVirtualJoystick nesnesine eriþim yok, bu yüzden çýktýyý event ile al
        uIVirtualJoystick.joystickOutputEvent.AddListener(OnJoystickOutput);
    }

    // Update is called once per frame
    void Update()
    {

        // Joystick çýktýsýný kullanarak karakteri hareket ettir
        if (Input.touchCount > 0 && !GameManager.instance.UIControl)
        {

            Vector2 touch = Input.GetTouch(0).position;
            if (!onExecuted)
            {
                joystickPrefab.transform.position = touch;
                JoystickSetActive(true);
                onExecuted = true;
            }
            Vector3 movement = new Vector3(joystickOutput.x, -joystickOutput.y, joystickOutput.y) * moveSpeed * Time.deltaTime;
            transform.Translate(movement);
        }
        else
        {
            onExecuted = false;
            joystickPrefab.transform.position = Vector2.zero;
            JoystickSetActive(false);
        }
    }
    public void JoystickSetActive(bool active)
    {
        if (joystickPrefab != null)
        {
            joystickPrefab.SetActive(active);
        }
    }
    private void OnJoystickOutput(Vector2 output)
    {
        joystickOutput = output;
    }

    void OnDestroy()
    {
        // Event dinlemeyi kaldýr
        uIVirtualJoystick.joystickOutputEvent.RemoveListener(OnJoystickOutput);
    }
}
