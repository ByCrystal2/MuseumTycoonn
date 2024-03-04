using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIClickInfo : MonoBehaviour
{
    // Ana kamera referans�
    Camera mainCamera;

    void Start()
    {
        // Ana kameray� bulma
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Fare pozisyonu al�n�yor
        Vector3 mousePosition = Input.mousePosition;

        // Fare pozisyonundan bir ���n olu�turuluyor
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        // I��na �arpan t�m UI objeleri al�n�yor
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = mousePosition;

        // I��na �arpan objelerin listesi
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        // I��na �arpan objelerin kontrol�
        foreach (RaycastResult result in results)
        {
            // E�er ���n g�nderilen obje buton ise engelleyici de�ilse butona t�klanabilir
            if (result.gameObject.GetComponent<Button>() != null)
            {
                Debug.Log("Butona ula��ld�, engelleyici de�il!");
                // Butona t�klama i�lemleri burada ger�ekle�tirilebilir
                break;
            }
            else
            {
                // Butona engelleyici obje var
                Debug.Log("Butonun �n�nde engelleyici bir obje var: " + result.gameObject.name);
                // Engelleyici obje ile ilgili gerekli i�lemler burada yap�labilir
                // �rne�in: Engelleyici objeyi devre d��� b�rakma veya g�r�nmez yapma
            }
        }
    }
}
