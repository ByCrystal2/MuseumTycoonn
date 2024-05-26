using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemActivation : MonoBehaviour
{
    [SerializeField] GameObject[] SpiderWebs;

    GameObject currentWeb => SpiderWebs[0];
    private void Awake()
    {
        if (!ShopController.instance.isAfterShopUIType)
            RandomSpiderWebActivation().SetActive(true);
        else
            currentWeb.SetActive(true);
    }

    public GameObject RandomSpiderWebActivation()
    {
        return SpiderWebs[Random.Range(0, SpiderWebs.Length)];        
    }


}

