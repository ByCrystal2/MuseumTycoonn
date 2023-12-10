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

    public float speed = 5.0f; // Hareket hýzý
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Karakterin rotasyonunu dondur
    }

    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); // Yatay giriþ (WSAD veya ok tuþlarý)
        float verticalInput = Input.GetAxis("Vertical"); // Dikey giriþ (WSAD veya ok tuþlarý)

        Vector3 movement = new Vector3(horizontalInput, 0.0f, verticalInput) * speed; // Hareket vektörü

        rb.velocity = transform.TransformDirection(movement); // Hareket vektörünü karakterin yönüne dönüþtürerek uygula
    }


}

public enum PlayerStats
{
    Idle,
    Move,
    Jump,
}
