using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class DoorBehaviour : MonoBehaviour
{
    private float targetAngle;
    private float smooth;
    private bool startBehaviour;
    private float tolerance = 0.1f;
    private void Update()
    {
        if (!startBehaviour) return;
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

        // Hedef a��ya yakla��p yakla�mad���n� kontrol et
        if (Quaternion.Angle(transform.localRotation, targetRotation) > tolerance)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smooth);
        }
        else
        {
            // Kap� hedef a��s�na ula�t���nda tam olarak o a��ya ayarla
            transform.localRotation = targetRotation;
            startBehaviour = false;
        }
    }
    public void DoorProcess(float _targetAngel, float _smooth)
    {
        targetAngle = _targetAngel;
        smooth = _smooth;
        startBehaviour = true;
    }
}
