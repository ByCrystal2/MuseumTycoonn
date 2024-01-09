using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DailyRewardItemOptions : MonoBehaviour
{
    public int MyItemID;
    [SerializeField] private TextMeshProUGUI MyNameText;
    [SerializeField] private TextMeshProUGUI MyPriceText;
    [SerializeField] private GameObject Light;
    [SerializeField] private GameObject[] CloseStars;
    [SerializeField] private GameObject[] OpenStars;

    public void SetMyOptions(int _myItemID ,string _myName, float _myPrice, bool _lightClose, byte _starCount = 0)
    {
        MyItemID = _myItemID;
        MyNameText.text = _myName;
        MyPriceText.text = _myPrice.ToString();
        Light.SetActive(_lightClose);

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
