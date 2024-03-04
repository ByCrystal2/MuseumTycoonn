using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditModeCameraController : MonoBehaviour
{
    // Hareket sınırları
    public float minX = -158f;
    public float maxX = -75f;
    public float sensitivity = 0.1f; // Hareket hassasiyeti
    [SerializeField] bool isPC;
    void Update()
    {
        if (isPC)
        {
            // Editörde dokunmatik girişini simüle etmek için
            if (Input.GetMouseButton(0))
            {
                // İlk parmağın hareket vektörünü al
                Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;

                // Hareket vektörünü düşük hassasiyetle çarp ve x değerini sınırla
                float deltaX = Mathf.Clamp(touchDeltaPosition.y * sensitivity, minX, maxX);

                // Yeni kamera pozisyonunu hesapla ve sınırla
                float newX = Mathf.Clamp(transform.position.x + deltaX, minX, maxX);

                // Kameranın pozisyonunu güncelle
                transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            }
                return;
        }
        if (Input.touchCount > 0)
        {
            // İlk parmağın pozisyonunu al
            Touch touch = Input.GetTouch(0);

            // Ekranın ortasına göre pozisyonu normalleştir
            float normalizedX = (touch.position.x / Screen.width) * 2 - 1;

            // Sınırları kontrol et
            float targetX = Mathf.Clamp(normalizedX * 5, minX, maxX);
            Debug.Log(targetX);
            // Hareketi uygula sadece sınırlar içindeyse
            if (targetX >= minX && targetX <= maxX)
            {
                transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
            }
        }
    }
}
