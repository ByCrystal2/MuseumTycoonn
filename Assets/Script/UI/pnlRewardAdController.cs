using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class pnlRewardAdController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] GameObject imgGem;
    [SerializeField] GameObject imgGold;

    [SerializeField] Text txtAmount;

    [SerializeField] Image fillerBackground;
    [SerializeField] Image filler;

    public bool adStarting = false;
    private int adDuration;
    private float currentDuration = 0;
    private void Update()
    {
        if (adStarting && adDuration > 0)
        {
            currentDuration += Time.deltaTime;
            filler.fillAmount = Mathf.Clamp(currentDuration / adDuration, 0, 1);
            if (currentDuration >= adDuration)
            {
                adStarting = false;
                UIController.instance.CloseRewardAdPanel(true);
            }
        }
    }
    public void SetRewardAdUIS(RewardAdData _adData)
    {
        ResetUIValues();
        txtAmount.text = _adData.Amount.ToString();
        if (_adData.Type == ItemType.Gem)
        {
            imgGem.SetActive(true);
            fillerBackground.color = new Color(0, 193, 93);
        }
        else if (_adData.Type == ItemType.Gold)
        {
            imgGold.SetActive(true);
            fillerBackground.color = new Color(216, 170, 32);
        }
        StartWaitingWithRewardAdDuration(_adData.AdDuration);
    }
    public void ResetUIValues()
    {
        imgGem.SetActive(false);
        imgGold.SetActive(false);
        txtAmount.text = "";
        filler.fillAmount = 0;
        currentDuration = 0;
    }
    void StartWaitingWithRewardAdDuration(int _adDuration)
    {
        adDuration = _adDuration;
        adStarting = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
    }
}