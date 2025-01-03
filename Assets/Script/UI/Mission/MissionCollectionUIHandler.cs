using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionCollectionUIHandler : MonoBehaviour
{
    [SerializeField] Sprite[] iconSprites;    
    [Header("Value UIs")]
    [SerializeField] Image imgIcon;
    [SerializeField] Text txtStartValue;
    [SerializeField] Text endValueText;
    MissionCollectionType _currentCollectionType;
    
    public void SetDatas(int startValue, int endValue, MissionCollectionType currentCollectionType)
    {
        txtStartValue.text = startValue.ToString();
        endValueText.text = endValue.ToString();
        _currentCollectionType = currentCollectionType;
        imgIcon.sprite = iconSprites[(int)currentCollectionType];
    }
}
