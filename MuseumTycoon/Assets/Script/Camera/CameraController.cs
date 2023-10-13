using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region Base Props
    public float zoomSpeed = 0.5f; // Zoom hýzý
    public float minZoom = 1.0f;   // Minimum yakýnlaþtýrma düzeyi
    public float maxZoom = 80.0f;   // Maksimum yakýnlaþtýrma düzeyi
    public float panSpeed = 1.0f;    // Hareket hýzý

    private Camera mainCamera;
    private float initialZoom;
    private Vector2 initialTouchPosition;

    public bool inPC = false;

    public Transform camLooker; // Kameranýn takip edeceði nesne
    public float cameraFollowSpeed = 5f; // Kamera takip hýzý
    public float cubeMoveSpeed = 6.6f; // Küpün hareket hýzý
    public float maxCubeSpeed = 8.6f; // Küpün hareket hýzý
    public float rotationSpeed = 2f; // Kamera'nýn dönüþ hýzý

    private Vector3 initialCamOffset;
    private Vector3 initialCamRotation;
    #endregion

    public Transform target; // Kameranýn takip edeceði nesne (küp)
    public float RotationSpeed = 2f; // Kamera'nýn dönüþ hýzý
    public float minRotationX = -45f; // Kameranýn minimum yatay dönüþ açýsý
    public float maxRotationX = 45f; // Kameranýn maksimum yatay dönüþ açýsý

    
    
    public float cameraRotationSpeed = 0.5f; // Kameranýn dönüþ hýzý
    private Vector3 lastCubeRotation;

    private Vector3 InitialCamOffset;
    void Start()
    {
        /* Cam Start
        camLooker = GameObject.FindWithTag("CamLooker").transform;
        //mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        //initialZoom = mainCamera.orthographicSize;

        // Kameranýn baþlangýç pozisyonundan hedefe olan mesafeyi hesapla        
        initialCamOffset = transform.position - camLooker.position;
        initialCamRotation = transform.rotation.eulerAngles;
        */
        target = GameObject.FindWithTag("CamLooker").transform;
        InitialCamOffset = transform.position - target.position;
        

    }

    void Update()
    {
        // Küpün dönme açýsýný al
        Vector3 cubeRotation = target.rotation.eulerAngles;

        // Kamerayý küpün etrafýnda döndür
        transform.rotation = Quaternion.Euler(cubeRotation);

        // Kameranýn dönüþ hýzýný ayarla
        float rotationSpeed = Input.touchCount > 0 ? -cameraRotationSpeed : 0f;

        // Dokunmatik giriþleri kontrol et
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            float rotationY = -touch.deltaPosition.x * rotationSpeed * Time.deltaTime;
            float rotationX = touch.deltaPosition.y * rotationSpeed * Time.deltaTime;

            // Kamerayý döndür
            target.Rotate(Vector3.up, rotationY);
            target.Rotate(Vector3.right, rotationX);
        }

        /* Kamera Movement
         * 
        // Kamerayý hedef nesneyi takip edecek þekilde güncelle
        Vector3 targetPosition = camLooker.position + initialCamOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, cameraFollowSpeed * Time.deltaTime);

        
        Vector3 targetRotation = camLooker.rotation.eulerAngles + initialCamRotation;
        transform.rotation = Quaternion.Euler(targetRotation);

        
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchDelta = touch.deltaPosition;

            // Küpü dokunmatik hareketlere göre hareket ettir
            Vector3 cubeMovement = new Vector3(touchDelta.x, 0, touchDelta.y) * cubeMoveSpeed * Time.deltaTime;
            cubeMovement = Vector3.ClampMagnitude(cubeMovement, maxCubeSpeed * Time.deltaTime);
            camLooker.Translate(cubeMovement);

            // Parmaðýn hareket yönüne göre kamerayý döndür
            float rotationY = touchDelta.x * rotationSpeed;
            float rotationX = touchDelta.y * rotationSpeed;
            
        }
        */
        #region Kamera Zoom
        //if (inPC)
        //{
        //    // Mouse tekerleði ile yakýnlaþtýrma ve uzaklaþtýrma yapma
        //    float scroll = Input.GetAxis("Mouse ScrollWheel");
        //    Debug.Log(scroll);
        //    float newSize = mainCamera.orthographicSize - scroll * zoomSpeed;

        //    // Ýki parmaðýn pozisyonlarýný al
        //    Vector2 touch1 = Vector2.zero;
        //    Vector2 touch2 = Vector2.zero;

        //    if (Input.touchCount == 2)
        //    {
        //        touch1 = Input.GetTouch(0).position;
        //        touch2 = Input.GetTouch(1).position;
        //    }

        //    // Ýki parmak arasýndaki uzaklýðý hesapla
        //    float touchDelta = Vector2.Distance(touch1, touch2);

        //    // Önceki kare ile karþýlaþtýr
        //    float delta = touchDelta - initialZoom;

        //    // Yakýnlaþtýrma sýnýrlarýný kontrol etme
        //    float newSizeFromTouch = mainCamera.orthographicSize - delta * zoomSpeed;
        //    newSizeFromTouch = Mathf.Clamp(newSizeFromTouch, minZoom, maxZoom);

        //    // Kameranýn yakýnlaþtýrma düzeyini güncelleme
        //    mainCamera.orthographicSize = Mathf.Max(newSize, newSizeFromTouch);
        //    initialZoom = touchDelta;
        //}
        //else
        //{
        //    if (Input.touchCount == 2)
        //    {
        //        // Ýlk ve ikinci parmaðýn pozisyonlarýný al
        //        Vector2 touch1 = Input.GetTouch(0).position;
        //        Vector2 touch2 = Input.GetTouch(1).position;

        //        // Ýki parmak arasýndaki uzaklýðý hesapla
        //        float touchDelta = Vector2.Distance(touch1, touch2);

        //        // Önceki kare ile karþýlaþtýr
        //        float delta = touchDelta - initialZoom;


        //        if (delta > 0)
        //        {
        //            // Kullanýcý yakýnlaþtýrma yapýyor.
        //            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - delta * SetCamOrthographicSize(zoomSpeed, false), minZoom, maxZoom);
        //        }
        //        else if (delta < 0)
        //        {
        //            // Kullanýcý uzaklaþtýrma yapýyor.
        //            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - delta * SetCamOrthographicSize(zoomSpeed, true), minZoom, maxZoom);
        //        }
        //        initialZoom = touchDelta;
        //    }
        //    else if (Input.touchCount == 1)
        //    {
        //        // Tek parmakla hareket ettirme
        //        if (Input.GetTouch(0).phase == TouchPhase.Began)
        //        {
        //            initialTouchPosition = Input.GetTouch(0).position;
        //        }
        //        else if (Input.GetTouch(0).phase == TouchPhase.Moved)
        //        {
        //            Vector2 currentPosition = Input.GetTouch(0).position;
        //            Vector2 deltaPosition = currentPosition - initialTouchPosition;

        //            // Kamerayý saða veya sola kaydýrma
        //            mainCamera.transform.Translate(-deltaPosition * panSpeed * Time.deltaTime);

        //            initialTouchPosition = currentPosition;
        //        }
        //    }
        //}
        #endregion
    }
    private float SetCamOrthographicSize(float zoomSpeed, bool increase)
    {
        float newZoomSpeed;
        if (increase)
        {
            newZoomSpeed = mainCamera.orthographicSize + (-zoomSpeed * Time.deltaTime);
        }
        else
        {
            newZoomSpeed = mainCamera.orthographicSize + (zoomSpeed * Time.deltaTime);
        }
        return newZoomSpeed;
    }
}

