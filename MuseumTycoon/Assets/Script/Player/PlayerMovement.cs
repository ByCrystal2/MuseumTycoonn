using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    private CharacterController controller;

    PlayerMotor pm;
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        pm = GetComponent<PlayerMotor>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move()
    {
        // Mobil dokunmatik giri�i kontrol etme
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                // Dokunmatik ekran� s�r�kleyerek karakteri hareket ettirme
                Vector2 touchDeltaPosition = touch.deltaPosition;
                Vector3 moveDirection = new Vector3(touchDeltaPosition.x, 0, touchDeltaPosition.y);

                // Hareketi d�nya koordinatlar�na d�n��t�rme
                moveDirection = Camera.main.transform.TransformDirection(moveDirection);

                // Yatay d�zlemde hareket etme
                moveDirection.y = 0;


                controller.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);
                pm.currentPlayerStat = PlayerStats.Move;
            }
        }
    }
}
