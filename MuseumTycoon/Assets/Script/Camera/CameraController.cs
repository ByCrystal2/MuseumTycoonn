using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region Base Props
    public float zoomSpeed = 0.5f; // Zoom h�z�
    public float minZoom = 1.0f;   // Minimum yak�nla�t�rma d�zeyi
    public float maxZoom = 80.0f;   // Maksimum yak�nla�t�rma d�zeyi
    public float panSpeed = 1.0f;    // Hareket h�z�

    private Camera mainCamera;
    private float initialZoom;
    private Vector2 initialTouchPosition;

    public bool inPC = false;

    public Transform camLooker; // Kameran�n takip edece�i nesne
    public float cameraFollowSpeed = 5f; // Kamera takip h�z�
    public float cubeMoveSpeed = 6.6f; // K�p�n hareket h�z�
    public float maxCubeSpeed = 8.6f; // K�p�n hareket h�z�
    public float rotationSpeed = 2f; // Kamera'n�n d�n�� h�z�

    private Vector3 initialCamOffset;
    private Vector3 initialCamRotation;
    #endregion

    public Transform target; // Kameran�n takip edece�i nesne (k�p)
    public float RotationSpeed = 2f; // Kamera'n�n d�n�� h�z�
    public float minRotationX = -45f; // Kameran�n minimum yatay d�n�� a��s�
    public float maxRotationX = 45f; // Kameran�n maksimum yatay d�n�� a��s�

    
    
    public float cameraRotationSpeed = 0.5f; // Kameran�n d�n�� h�z�
    private Vector3 lastCubeRotation;

    private Vector3 InitialCamOffset;
    void Start()
    {
        /* Cam Start
        camLooker = GameObject.FindWithTag("CamLooker").transform;
        //mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        //initialZoom = mainCamera.orthographicSize;

        // Kameran�n ba�lang�� pozisyonundan hedefe olan mesafeyi hesapla        
        initialCamOffset = transform.position - camLooker.position;
        initialCamRotation = transform.rotation.eulerAngles;
        */
        target = GameObject.FindWithTag("CamLooker").transform;
        InitialCamOffset = transform.position - target.position;
        

    }

    void Update()
    {
        // K�p�n d�nme a��s�n� al
        Vector3 cubeRotation = target.rotation.eulerAngles;

        // Kameray� k�p�n etraf�nda d�nd�r
        transform.rotation = Quaternion.Euler(cubeRotation);

        // Kameran�n d�n�� h�z�n� ayarla
        float rotationSpeed = Input.touchCount > 0 ? -cameraRotationSpeed : 0f;

        // Dokunmatik giri�leri kontrol et
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            float rotationY = -touch.deltaPosition.x * rotationSpeed * Time.deltaTime;
            float rotationX = touch.deltaPosition.y * rotationSpeed * Time.deltaTime;

            // Kameray� d�nd�r
            target.Rotate(Vector3.up, rotationY);
            target.Rotate(Vector3.right, rotationX);
        }

        /* Kamera Movement
         * 
        // Kameray� hedef nesneyi takip edecek �ekilde g�ncelle
        Vector3 targetPosition = camLooker.position + initialCamOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, cameraFollowSpeed * Time.deltaTime);

        
        Vector3 targetRotation = camLooker.rotation.eulerAngles + initialCamRotation;
        transform.rotation = Quaternion.Euler(targetRotation);

        
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchDelta = touch.deltaPosition;

            // K�p� dokunmatik hareketlere g�re hareket ettir
            Vector3 cubeMovement = new Vector3(touchDelta.x, 0, touchDelta.y) * cubeMoveSpeed * Time.deltaTime;
            cubeMovement = Vector3.ClampMagnitude(cubeMovement, maxCubeSpeed * Time.deltaTime);
            camLooker.Translate(cubeMovement);

            // Parma��n hareket y�n�ne g�re kameray� d�nd�r
            float rotationY = touchDelta.x * rotationSpeed;
            float rotationX = touchDelta.y * rotationSpeed;
            
        }
        */
        #region Kamera Zoom
        //if (inPC)
        //{
        //    // Mouse tekerle�i ile yak�nla�t�rma ve uzakla�t�rma yapma
        //    float scroll = Input.GetAxis("Mouse ScrollWheel");
        //    Debug.Log(scroll);
        //    float newSize = mainCamera.orthographicSize - scroll * zoomSpeed;

        //    // �ki parma��n pozisyonlar�n� al
        //    Vector2 touch1 = Vector2.zero;
        //    Vector2 touch2 = Vector2.zero;

        //    if (Input.touchCount == 2)
        //    {
        //        touch1 = Input.GetTouch(0).position;
        //        touch2 = Input.GetTouch(1).position;
        //    }

        //    // �ki parmak aras�ndaki uzakl��� hesapla
        //    float touchDelta = Vector2.Distance(touch1, touch2);

        //    // �nceki kare ile kar��la�t�r
        //    float delta = touchDelta - initialZoom;

        //    // Yak�nla�t�rma s�n�rlar�n� kontrol etme
        //    float newSizeFromTouch = mainCamera.orthographicSize - delta * zoomSpeed;
        //    newSizeFromTouch = Mathf.Clamp(newSizeFromTouch, minZoom, maxZoom);

        //    // Kameran�n yak�nla�t�rma d�zeyini g�ncelleme
        //    mainCamera.orthographicSize = Mathf.Max(newSize, newSizeFromTouch);
        //    initialZoom = touchDelta;
        //}
        //else
        //{
        //    if (Input.touchCount == 2)
        //    {
        //        // �lk ve ikinci parma��n pozisyonlar�n� al
        //        Vector2 touch1 = Input.GetTouch(0).position;
        //        Vector2 touch2 = Input.GetTouch(1).position;

        //        // �ki parmak aras�ndaki uzakl��� hesapla
        //        float touchDelta = Vector2.Distance(touch1, touch2);

        //        // �nceki kare ile kar��la�t�r
        //        float delta = touchDelta - initialZoom;


        //        if (delta > 0)
        //        {
        //            // Kullan�c� yak�nla�t�rma yap�yor.
        //            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - delta * SetCamOrthographicSize(zoomSpeed, false), minZoom, maxZoom);
        //        }
        //        else if (delta < 0)
        //        {
        //            // Kullan�c� uzakla�t�rma yap�yor.
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

        //            // Kameray� sa�a veya sola kayd�rma
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

