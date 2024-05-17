using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    

    public PlayerStats currentPlayerStat;
    PlayerMovement playerMove;

    private void Awake()
    {        
        currentPlayerStat = PlayerStats.Idle;
        playerMove = GetComponent<PlayerMovement>();
    }

    public float speed = 5.0f; // Hareket h�z�
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Karakterin rotasyonunu dondur
    }

    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); // Yatay giri� (WSAD veya ok tu�lar�)
        float verticalInput = Input.GetAxis("Vertical"); // Dikey giri� (WSAD veya ok tu�lar�)

        Vector3 movement = new Vector3(horizontalInput, 0.0f, verticalInput) * speed; // Hareket vekt�r�

        rb.linearVelocity = transform.TransformDirection(movement); // Hareket vekt�r�n� karakterin y�n�ne d�n��t�rerek uygula
    }


}

public enum PlayerStats
{
    Idle,
    Move,
    Jump,
}
