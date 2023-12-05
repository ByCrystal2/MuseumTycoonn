using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 2.0f;

    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    private Vector2 touchDelta;

    private Vector3 initialRotation;

    private void Start()
    {
        initialRotation = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                touchEndPos = touch.position;
                touchDelta = touchEndPos - touchStartPos;

                float rotationX = -touchDelta.y * rotationSpeed * Time.deltaTime;
                float rotationY = touchDelta.x * rotationSpeed * Time.deltaTime;

                Vector3 newRotation = initialRotation + new Vector3(rotationX, rotationY, 0f);
                newRotation.x = Mathf.Clamp(newRotation.x, -80f, 80f);

                transform.rotation = Quaternion.Euler(newRotation);

                touchStartPos = touch.position;
                initialRotation = newRotation;
            }
        }
    }
}

