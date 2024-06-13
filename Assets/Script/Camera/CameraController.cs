using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController1 : MonoBehaviour
{
    public float MoveSpeed = 0.025f; // Parmak hareketine göre lens shift hýzýný kontrol eder

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
        AdjustCameraView(); // Ekran boyutlarýna göre kamera görüþünü ayarla
        targetLensShift = myCamera.lensShift; // Hedef lens shift baþlangýçta mevcut lens shift
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
                targetLensShift.x -= moveDeltaX; // Parmak hareketine göre hedef lens shift güncelleniyor
                targetLensShift.y -= moveDeltaY;

                // touchStartPos'u güncelle, böylece hareket devam ederken deðiþiklikler doðru hesaplanýr
                touchStartPos = touchEndPos;

                // Lens shift'in x ve y deðerlerini belirlenen sýnýrlar arasýnda tut
                targetLensShift.x = Mathf.Clamp(targetLensShift.x, minLensShiftX, maxLensShiftX);
                targetLensShift.y = Mathf.Clamp(targetLensShift.y, minLensShiftY, maxLensShiftY);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isTouching = false; // Parmaðýnýzý çektiðinizde hareket durur
            }
        }

        if (isTouching)
        {
            // Lens shift'i hedef lens shift'e doðrudan hareket ettir
            myCamera.lensShift = targetLensShift;
        }
    }

    private void AdjustCameraView()
    {
        float aspectRatio = (float)Screen.width / (float)Screen.height;

        // Perspektif kamera için görüþ açýsýný ve odak uzaklýðýný ekran oranýna göre ayarla
        myCamera.fieldOfView = CalculateFieldOfView(aspectRatio);
        myCamera.focalLength = CalculateFocalLength(aspectRatio);

        // Ekran boyutlarýna göre lens shift sýnýrlarýný hesapla
        CalculateLensShiftLimits(aspectRatio);
    }

    private float CalculateFieldOfView(float aspectRatio)
    {
        // Perspektif kameranýn görüþ açýsýný ekran oranýna göre hesapla
        float baseFov = 60.0f; // Temel görüþ açýsý
        return Mathf.Clamp(baseFov / aspectRatio, 40.0f, 100.0f);
    }

    private float CalculateFocalLength(float aspectRatio)
    {
        // Perspektif kameranýn odak uzaklýðýný ekran oranýna göre hesapla
        float baseFocalLength = 14.0f; // Temel odak uzaklýðý
        return baseFocalLength * aspectRatio;
    }

    private void CalculateLensShiftLimits(float aspectRatio)
    {
        // Ekran boyutlarýna göre lens shift sýnýrlarýný hesapla
        float baseShiftX = 0.34f; // Temel lens shift x sýnýrý
        float baseMaxShiftY = 0.28f; // Temel lens shift y sýnýrý
        float baseMinShiftY = 1.1f; // Temel lens shift y sýnýrý
        minLensShiftX = -baseShiftX * aspectRatio;
        maxLensShiftX = baseShiftX * aspectRatio;
        minLensShiftY = -baseMinShiftY * aspectRatio;
        maxLensShiftY = baseMaxShiftY * aspectRatio;

        myCamera.lensShift = new Vector2(0,maxLensShiftY);
    }
}
