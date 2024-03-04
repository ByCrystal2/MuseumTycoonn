using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditModeCameraController : MonoBehaviour
{
    // Hareket s�n�rlar�
    public float minX = -158f;
    public float maxX = -75f;
    public float sensitivity = 0.1f; // Hareket hassasiyeti
    [SerializeField] bool isPC;
    void Update()
    {
        if (isPC)
        {
            // Edit�rde dokunmatik giri�ini sim�le etmek i�in
            if (Input.GetMouseButton(0))
            {
                // �lk parma��n hareket vekt�r�n� al
                Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;

                // Hareket vekt�r�n� d���k hassasiyetle �arp ve x de�erini s�n�rla
                float deltaX = Mathf.Clamp(touchDeltaPosition.y * sensitivity, minX, maxX);

                // Yeni kamera pozisyonunu hesapla ve s�n�rla
                float newX = Mathf.Clamp(transform.position.x + deltaX, minX, maxX);

                // Kameran�n pozisyonunu g�ncelle
                transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            }
                return;
        }
        if (Input.touchCount > 0)
        {
            // �lk parma��n pozisyonunu al
            Touch touch = Input.GetTouch(0);

            // Ekran�n ortas�na g�re pozisyonu normalle�tir
            float normalizedX = (touch.position.x / Screen.width) * 2 - 1;

            // S�n�rlar� kontrol et
            float targetX = Mathf.Clamp(normalizedX * 5, minX, maxX);
            Debug.Log(targetX);
            // Hareketi uygula sadece s�n�rlar i�indeyse
            if (targetX >= minX && targetX <= maxX)
            {
                transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
            }
        }
    }
}
