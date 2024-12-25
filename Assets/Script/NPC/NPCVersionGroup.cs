using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCVersionGroup : MonoBehaviour
{
    private void Awake()
    {
        int length = transform.childCount;
        for (int i = 0; i < length; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    public void RandomizeChildWithSibling(int childIndex)
    {
        Transform npcTransform = transform.GetChild(childIndex);
        int randomIndex = Random.Range(0, 18);
        npcTransform.SetSiblingIndex(randomIndex);
    }
}
