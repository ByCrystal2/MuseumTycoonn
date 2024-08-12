using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHandler : MonoBehaviour
{
    List<Transform> spawnTransformList = new List<Transform>();
    public static SpawnHandler instance { get; private set; }
    private void Awake()
    {
        instance = this;
        int length = transform.childCount;
        for (int i = 0; i < length; i++)
            if (transform.GetChild(i).TryGetComponent(out NPCVersionGroup nPCVersionGroup))
                spawnTransformList.Add(nPCVersionGroup.transform);
    }
    private void SpawnNpcs(int _value)
    {
        for (int i = 0; i < spawnTransformList.Count; i++)
            for (int k = 0; k < _value; k++)
                spawnTransformList[i].GetChild(k).gameObject.SetActive(true);
    }
    public void StartSpawnProcess()
    {
        float capacity = MuseumManager.instance.GetMuseumCurrentCapacity();
        Debug.Log("NPC Spawn Process is starting... Current Capacity is => " + capacity);
        float multiplier = 0.75f;
        int targetNpcCount = ((int)((capacity * multiplier) / 8) + 3);
        SpawnNpcs(targetNpcCount);
    }

}
