using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookCamLooker : MonoBehaviour
{
    GameObject camLooker;
    private void Awake()
    {
        camLooker = GameObject.FindWithTag("CamLooker");
    }
    private void Update()
    {
        this.transform.LookAt(camLooker.transform);
    }
}
