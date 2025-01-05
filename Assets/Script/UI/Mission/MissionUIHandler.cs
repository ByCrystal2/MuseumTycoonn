using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionUIHandler : MonoBehaviour
{
    [Header("Mission Type Scripts")]
    public MissionCollectionUIHandler collectionUIHandler;

    public void MissionUIActivation(MissionType type, bool _active)
    {
        GameObject currentMissionTypeObj = null;
        switch (type)
        {
            case MissionType.Collection:
                currentMissionTypeObj = collectionUIHandler.gameObject;
                break;
            case MissionType.NPC:
                // NPC UI
                break;
            default:
                break;
        }
        currentMissionTypeObj.SetActive(_active);
    } 
}
