using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerMovement : MonoBehaviour
{
    public bool isGhostMode = false;

    public float movementSpeed = 5.0f;
    public float rotationSpeed = 3.0f;
    public float jumpForce = 10.0f;
    public LayerMask collisionLayers;
    public float maxVerticalRotation = 80.0f; // Maksimum yukarý dönüþ açýsý
    public float minVerticalRotation = -80.0f; // Maksimum aþaðý dönüþ açýsý
    public float ascendDescendSpeed = 5.0f;
    private Rigidbody rb;
    private float verticalRotation = 0.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        
    }

    void Update()
    {        
        if (Input.GetMouseButtonUp(1))
        {
            if (Cursor.lockState == CursorLockMode.Confined)
                Cursor.lockState = CursorLockMode.Locked;
            else
                Cursor.lockState = CursorLockMode.Confined;
        }
        if (isGhostMode)
        {
            rb.useGravity = false;
            GetComponent<CapsuleCollider>().enabled = false;
            if (Cursor.lockState == CursorLockMode.Locked)
                Move();
            else
            rb.velocity = Vector3.zero;
            // Yükselme ve alçalma iþlevselliði
            if (Input.GetKey(KeyCode.LeftShift))
            {
               Vector3 up = transform.position;
                up += Vector3.up * ascendDescendSpeed * Time.deltaTime;
                transform.position = up;
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                Vector3 up = transform.position;
                up -= Vector3.up * ascendDescendSpeed * Time.deltaTime;
                transform.position = up;
            }
        }
        else 
        {
            rb.useGravity = true;
            GetComponent<CapsuleCollider>().enabled = true;
            if (Cursor.lockState == CursorLockMode.Locked)
             Move();           
        }

    }
      
    public void Move()
    {
        float forwardMovement = Input.GetAxis("Vertical") * movementSpeed;
        float sidewaysMovement = Input.GetAxis("Horizontal") * movementSpeed;

        Vector3 moveDirection = (transform.forward * forwardMovement) + (transform.right * sidewaysMovement);

        rb.velocity = new Vector3(moveDirection.x, rb.velocity.y, moveDirection.z);

        float horizontalRotation = Input.GetAxis("Mouse X") * rotationSpeed;
        transform.Rotate(0, horizontalRotation, 0);


        float verticalRotationInput = -Input.GetAxis("Mouse Y") * rotationSpeed;
        verticalRotation += verticalRotationInput;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalRotation, maxVerticalRotation);

        transform.localEulerAngles = new Vector3(verticalRotation, transform.localEulerAngles.y, 0);
    }
}

