using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemActivation : MonoBehaviour
{
    [SerializeField] GameObject[] SpiderWebs;

    private void Awake()
    {
        RandomSpiderWebActivation().SetActive(true);
    }

    public GameObject RandomSpiderWebActivation()
    {
        return SpiderWebs[Random.Range(0, SpiderWebs.Length)];
    }
}
