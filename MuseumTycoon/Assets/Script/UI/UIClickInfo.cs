using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIClickInfo : MonoBehaviour
{
    // Ana kamera referansý
    Camera mainCamera;

    void Start()
    {
        // Ana kamerayý bulma
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Fare pozisyonu alýnýyor
        Vector3 mousePosition = Input.mousePosition;

        // Fare pozisyonundan bir ýþýn oluþturuluyor
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        // Iþýna çarpan tüm UI objeleri alýnýyor
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = mousePosition;

        // Iþýna çarpan objelerin listesi
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        // Iþýna çarpan objelerin kontrolü
        foreach (RaycastResult result in results)
        {
            // Eðer ýþýn gönderilen obje buton ise engelleyici deðilse butona týklanabilir
            if (result.gameObject.GetComponent<Button>() != null)
            {
                Debug.Log("Butona ulaþýldý, engelleyici deðil!");
                // Butona týklama iþlemleri burada gerçekleþtirilebilir
                break;
            }
            else
            {
                // Butona engelleyici obje var
                Debug.Log("Butonun önünde engelleyici bir obje var: " + result.gameObject.name);
                // Engelleyici obje ile ilgili gerekli iþlemler burada yapýlabilir
                // Örneðin: Engelleyici objeyi devre dýþý býrakma veya görünmez yapma
            }
        }
    }
}
