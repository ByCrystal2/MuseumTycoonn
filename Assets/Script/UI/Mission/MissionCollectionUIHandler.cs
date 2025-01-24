using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MissionCollectionUIHandler : MonoBehaviour
{
    public CollectionInfoPanelController infoPanelController;
    [SerializeField] Sprite[] iconSprites;    
    [Header("Value UIs")]
    [SerializeField] Image imgIcon;
    [SerializeField] Text txtStartValue;
    [SerializeField] Text endValueText;
    [SerializeField] Text txtLifeTime;
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
    public void StartMissionLifeTime(float _missionLifeTime) => StartCoroutine(IEStartMissionLifeTime(_missionLifeTime));
    IEnumerator IEStartMissionLifeTime(float _missionLifeTime)
    {
        if (_missionLifeTime == null) { Debug.LogError("Gonderilen gorev null!"); yield break; }
        float lifeTime = _missionLifeTime;
        while (lifeTime > 0)
        {
            int minutes = Mathf.FloorToInt(lifeTime / 60f);
            int seconds = Mathf.FloorToInt(lifeTime % 60f);

            txtLifeTime.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            lifeTime -= 1f;
            yield return new WaitForSeconds(1f);
        }
        txtLifeTime.text = "00:00";
        MissionManager.instance.collectionHandler.MissionOfCollectionTimeEnding();
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
