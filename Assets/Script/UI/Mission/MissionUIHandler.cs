using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionUIHandler : MonoBehaviour
{
    [Header("Mission Type Scripts")]
    [SerializeField] MissionCollectionUIHandler collectionUIHandler;

    public void MissionUIActivation(MissionType type)
    {
        switch (type)
        {
            case MissionType.Collection:
                collectionUIHandler.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    } 
}
