using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constant : MonoBehaviour
{
    private const string iAPIDCompany = "com_kosippysudio_";
    public string IAPIDCompany { get { return iAPIDCompany; } }

    private const string iAPIDGame = "museumtycoon_";
    public string IAPIDGame { get { return iAPIDGame; } }
    public static Constant instance { get; set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
    }
}
