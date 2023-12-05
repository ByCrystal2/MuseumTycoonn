using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FloatingJoystick : Joystick
{
    
    protected override void Start()
    {
        base.Start();
        background.gameObject.SetActive(false);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        background.gameObject.SetActive(true);
        base.OnPointerDown(eventData);
        
    }
    private void Update()
    {
        if (background.gameObject.activeInHierarchy)        
            MoveCamera();        
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        background.gameObject.SetActive(false);
        base.OnPointerUp(eventData);
    }

    public void MoveCamera()
    {
        float cameraSpeed = 5.0f;
        Vector3 cameraMovement = new Vector3(0, 0, Vertical * cameraSpeed * Time.deltaTime);
        // cameraSpeed, kameranın ne kadar hızlı hareket edeceğini belirleyen bir değişken olmalıdır.
        // Vertical, FloatingJoystick'ten gelen veri ile belirlediğiniz yönü temsil eder.
        Debug.Log(cameraMovement);        
        // Kameranın pozisyonunu güncelleyin.
        Camera.main.transform.Translate(cameraMovement);
    }
}