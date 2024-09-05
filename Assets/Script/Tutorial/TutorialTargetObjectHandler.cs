using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTargetObjectHandler : MonoBehaviour
{
    public int ID;
    public RectTransform targetTransform;
    bool isOptionsAdd = false;
    bool isTargetSettingComplated = false;
    private void Update()
    {
        if (targetTransform != null)
        {
            Debug.Log("targetTransform.position => " + targetTransform.position + "targetTransform.name => " + targetTransform.name);
        }
        if (!isOptionsAdd) return;
        if (gameObject.activeSelf)
        {
            isOptionsAdd = false;
            StartCoroutine(IESetOptions(ID, targetTransform));
        }
        
    }
    public void SetOptions(int _id, RectTransform _targetTransform)
    {
        isOptionsAdd = true;
        ID = _id;
        targetTransform = _targetTransform;
        isTargetSettingComplated = false;
        Debug.Log("Tutorial Target from void SetOptions method. name => "+targetTransform.name + " Position: " + targetTransform.position, targetTransform);
    }
    IEnumerator IESetOptions(int _id, RectTransform _targetTransform)
    {
        yield return new WaitForEndOfFrame();
        ID = _id;
        targetTransform = _targetTransform;
        isTargetSettingComplated = true;
        Debug.Log("Tutorial Target from IEnumerator IESetOptions method. => " + targetTransform.position);
    }
    public bool IsTargetSettingComplated()
    {
        return isTargetSettingComplated;
    }
}
