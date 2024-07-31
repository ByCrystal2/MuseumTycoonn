using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public int targetFrameRate = 60;
    public int sampleDuration = 5; // FPS'yi ölçmek için kaç saniye bekleyeceðimizi belirler
    private int frameCount = 0;
    private float elapsedTime = 0.0f;
    private bool isTesting = true;

    void Start()
    {
#if !UNITY_EDITOR
        Application.targetFrameRate = targetFrameRate;
#endif
    }

    void Update()
    {
        if (isTesting)
        {
            frameCount++;
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= sampleDuration)
            {
                float averageFPS = frameCount / elapsedTime;
                isTesting = false;

                SetGraphicsQuality(averageFPS);
                Destroy(gameObject,3);
            }
        }
    }

    void SetGraphicsQuality(float averageFPS)
    {
        if (averageFPS >= 50)
        {
            QualitySettings.SetQualityLevel(2); // High
            Debug.Log("Graphics set to High");
        }
        else if (averageFPS >= 30)
        {
            QualitySettings.SetQualityLevel(1); // Medium
            Debug.Log("Graphics set to Medium");
        }
        else
        {
            QualitySettings.SetQualityLevel(0); // Low
            Debug.Log("Graphics set to Low");
        }
    }
}
