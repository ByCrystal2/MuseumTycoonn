using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController1 : MonoBehaviour
{
    public float MoveSpeed = 0.025f; // Parmak hareketine g�re lens shift h�z�n� kontrol eder

    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    private Vector2 touchDelta;

    private Camera myCamera;
    private Vector2 targetLensShift;
    private bool isTouching = false;

    private float minLensShiftX;
    private float maxLensShiftX;
    private float minLensShiftY;
    private float maxLensShiftY;

    private void Awake()
    {
        myCamera = GetComponent<Camera>();
        AdjustCameraView(); // Ekran boyutlar�na g�re kamera g�r���n� ayarla
        targetLensShift = myCamera.lensShift; // Hedef lens shift ba�lang��ta mevcut lens shift
    }

    private void Update()
    {
        if (TutorialLevelManager.instance != null && !TutorialLevelManager.instance.IsWatchTutorial) return;
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
                isTouching = true;
            }
            else if (touch.phase == TouchPhase.Moved && isTouching)
            {
                touchEndPos = touch.position;
                touchDelta = touchEndPos - touchStartPos;

                float moveDeltaX = touchDelta.x * MoveSpeed * Time.deltaTime;
                float moveDeltaY = touchDelta.y * MoveSpeed * Time.deltaTime;
                targetLensShift.x -= moveDeltaX; // Parmak hareketine g�re hedef lens shift g�ncelleniyor
                targetLensShift.y -= moveDeltaY;

                // touchStartPos'u g�ncelle, b�ylece hareket devam ederken de�i�iklikler do�ru hesaplan�r
                touchStartPos = touchEndPos;

                // Lens shift'in x ve y de�erlerini belirlenen s�n�rlar aras�nda tut
                targetLensShift.x = Mathf.Clamp(targetLensShift.x, minLensShiftX, maxLensShiftX);
                targetLensShift.y = Mathf.Clamp(targetLensShift.y, minLensShiftY, maxLensShiftY);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isTouching = false; // Parma��n�z� �ekti�inizde hareket durur
            }
        }

        if (isTouching)
        {
            // Lens shift'i hedef lens shift'e do�rudan hareket ettir
            myCamera.lensShift = targetLensShift;
        }
    }

    private void AdjustCameraView()
    {
        float aspectRatio = (float)Screen.width / (float)Screen.height;

        // Perspektif kamera i�in g�r�� a��s�n� ve odak uzakl���n� ekran oran�na g�re ayarla
        myCamera.fieldOfView = CalculateFieldOfView(aspectRatio);
        myCamera.focalLength = CalculateFocalLength(aspectRatio);

        // Ekran boyutlar�na g�re lens shift s�n�rlar�n� hesapla
        CalculateLensShiftLimits(aspectRatio);
    }

    private float CalculateFieldOfView(float aspectRatio)
    {
        // Perspektif kameran�n g�r�� a��s�n� ekran oran�na g�re hesapla
        float baseFov = 60.0f; // Temel g�r�� a��s�
        return Mathf.Clamp(baseFov / aspectRatio, 40.0f, 100.0f);
    }

    private float CalculateFocalLength(float aspectRatio)
    {
        // Perspektif kameran�n odak uzakl���n� ekran oran�na g�re hesapla
        float baseFocalLength = 14.0f; // Temel odak uzakl���
        return baseFocalLength * aspectRatio;
    }

    private void CalculateLensShiftLimits(float aspectRatio)
    {
        // Ekran boyutlar�na g�re lens shift s�n�rlar�n� hesapla
        float baseShiftX = 0.34f; // Temel lens shift x s�n�r�
        float baseMaxShiftY = 0.28f; // Temel lens shift y s�n�r�
        float baseMinShiftY = 1.1f; // Temel lens shift y s�n�r�
        minLensShiftX = -baseShiftX * aspectRatio;
        maxLensShiftX = baseShiftX * aspectRatio;
        minLensShiftY = -baseMinShiftY * aspectRatio;
        maxLensShiftY = baseMaxShiftY * aspectRatio;

        myCamera.lensShift = new Vector2(0,maxLensShiftY);
    }
}
