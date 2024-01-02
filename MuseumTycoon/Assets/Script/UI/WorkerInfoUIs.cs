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

    public void SetWorkerInfoUIs(int _id, string _fullName, int _age, float _height, Image _avatar = null)
    {
        workerID = _id;
        txtFullName.text = _fullName;
        txtAge.text = _age.ToString();
        txtHeight.text = _height.ToString();
    }
}
