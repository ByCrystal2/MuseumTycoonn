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
    [SerializeField] public Image imgAvatar;
    [SerializeField] public Transform CloseStarsObj;
    [SerializeField] public Transform OpenStarsObj;

    public void SetWorkerInfoUIs(int _id, string _fullName, int _age, float _height, int _level = 0, Image _avatar = null)
    {
        workerID = _id;
        txtFullName.text = _fullName;
        txtAge.text = _age.ToString();
        txtHeight.text = _height.ToString("000.0");

        if (_level != 0)
        {
            int length = CloseStarsObj.childCount;
            for (int i = 0; i < length; i++)
            {
                CloseStarsObj.GetChild(i).gameObject.SetActive(true);
                OpenStarsObj.GetChild(i).gameObject.SetActive(false);
            }
            int length1 = OpenStarsObj.childCount;
            for (int i = 0; i < length1; i++)
            {
                OpenStarsObj.GetChild(i).gameObject.SetActive(true);
            }
        }
    }
}
