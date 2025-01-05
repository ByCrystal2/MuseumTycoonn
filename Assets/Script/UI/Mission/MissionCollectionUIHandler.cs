using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
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

    CollectionHelper currentCollectionHelper;
    public void SetDatas(CollectionHelper _collectionHelper)
    {
        currentCollectionHelper = _collectionHelper;
        txtStartValue.text = _collectionHelper.StartValue.ToString();
        endValueText.text = _collectionHelper.EndValue.ToString();
        _currentCollectionType = _collectionHelper.missionCollectionType;
        imgIcon.sprite = iconSprites[(int)_collectionHelper.missionCollectionType];
    }
    public void UpdateUI()
    {
        if (currentCollectionHelper == null) { Debug.LogError("Mevcut CollectionHelper null!"); return; }
        txtStartValue.text = currentCollectionHelper.StartValue.ToString();
        endValueText.text = currentCollectionHelper.EndValue.ToString();
        _currentCollectionType = currentCollectionHelper.missionCollectionType;
        imgIcon.sprite = iconSprites[(int)currentCollectionHelper.missionCollectionType];
    }
}
