using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTargetObjectHandler : MonoBehaviour
{
    public int ID;
    public RectTransform targetTransform;
    public void SetOptions(int _id, RectTransform _targetTransform)
    {
        ID = _id;
        targetTransform = _targetTransform;
    }
}
