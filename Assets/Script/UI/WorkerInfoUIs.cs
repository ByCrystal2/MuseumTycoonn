using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkerInfoUIs : MonoBehaviour
{
    public int workerID;
    [SerializeField] public TextMeshProUGUI txtFullName;
    [SerializeField] public TextMeshProUGUI txtAge;
    [SerializeField] public TextMeshProUGUI txtHeight;
    [SerializeField] public Text txtPrice;
    [SerializeField] public Image imgAvatar;
    [SerializeField] public Transform CloseStarsObj;
    [SerializeField] public Transform OpenStarsObj;

    private float myPrice;
    public void SetWorkerInfoUIs(int _id, string _fullName, int _age, float _height, int _rank, Image _avatar = null)
    {
        workerID = _id;
        txtFullName.text = _fullName;
        txtAge.text = _age.ToString();
        txtHeight.text = _height.ToString("000");
        myPrice = (GameManager.instance.BaseWorkerHiringPrice * _rank) + WorkerManager.instance.GetBaseHiringWorkerWithMuseumLevel();
        Debug.Log("MyPrice => " + (GameManager.instance.BaseWorkerHiringPrice * _rank).ToString() + WorkerManager.instance.GetBaseHiringWorkerWithMuseumLevel().ToString());
        txtPrice.text = myPrice.ToString();
        if (_rank != 0)
        {
            int length = CloseStarsObj.childCount;
            for (int i = 0; i < length; i++)
            {
                CloseStarsObj.GetChild(i).gameObject.SetActive(true);
                OpenStarsObj.GetChild(i).gameObject.SetActive(false);
            }
            int length1 = _rank;
            for (int i = 0; i < length1; i++)
            {
                OpenStarsObj.GetChild(i).gameObject.SetActive(true);
            }
        }
        if (_avatar != null)
            imgAvatar = _avatar;
    }
    public float GetMyPrice()
    {
        return myPrice;
    }
    public void SetMyPrice(float _price)
    {
        myPrice = _price;
    }
}
