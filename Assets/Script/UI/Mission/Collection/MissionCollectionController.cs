using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableObjects", menuName = "Mission/Collection")]
public class MissionCollectionController : ScriptableObject
{
    [SerializeField] List<GameObject> CollectionPrefabs = new List<GameObject>(); // Siralama MissionCollectionType'in enum siralamasýna gore olmali!
    public GameObject GetCollectionPrefab(MissionCollectionType _type)
    {
        return CollectionPrefabs[(int)_type];
    }
}
