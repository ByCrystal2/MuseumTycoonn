using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DailyRewardItemOptions : MonoBehaviour,IPointerClickHandler
{
    public int MyItemID;
    [SerializeField] private TextMeshProUGUI MyNameText;
    [SerializeField] private TextMeshProUGUI MyPriceText;
    [SerializeField] private GameObject Light;
    [SerializeField] private GameObject[] CloseStars;
    [SerializeField] private GameObject[] OpenStars;
    private bool IsPurchased = false;
    private bool IsLocked = true;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsPurchased && !IsLocked)
        {
            DailyRewardsPanelController.instance.WinReward(this);
        }
    }
    public void SetClickable(bool _isPurchased, bool _isLocked)
    {
        IsPurchased = _isPurchased;
        IsLocked = _isLocked;
        if (IsPurchased && !IsLocked)
        {
            //Debug.Log(this.gameObject + " is not clickable and this is purchased!");
            DailyRewardsPanelController.instance.CreatePnlReceived(transform.parent);
        }
        else if (!IsPurchased && IsLocked)
        {
            //Debug.Log(this.gameObject + " is not Purchased and this is UnLocked!");
            DailyRewardsPanelController.instance.CreatePnlLocked(transform.parent);
        }
    }
    public void SetMyOptions(int _myItemID ,string _myName, float _myPrice, bool _lightClose, bool _isPurchased, bool _isLocked, byte _starCount = 0)
    {
        MyItemID = _myItemID;
        MyNameText.text = _myName;
        MyPriceText.text = _myPrice.ToString();
        Light.SetActive(_lightClose);
        IsPurchased = _isPurchased;
        IsLocked = _isLocked;
        SetClickable(IsPurchased, IsLocked);

        if (CloseStars != null && OpenStars != null)
        {
            for (int i = 0; i < CloseStars.Length; i++)
            {
                CloseStars[i].SetActive(true);
                OpenStars[i].SetActive(false);
            }

            for (int i = 0; i < OpenStars.Length; i++)
            {
                OpenStars[i].SetActive(true);
            }
        }
    }
}
