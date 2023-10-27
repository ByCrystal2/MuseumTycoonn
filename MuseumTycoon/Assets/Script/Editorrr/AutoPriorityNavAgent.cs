using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPriorityNavAgent : MonoBehaviour
{
    public bool activated;

    private void OnDrawGizmosSelected()
    {
        if (activated)
        {
            activated = false;
            SetAgentPriorities();
        }
    }

    void SetAgentPriorities()
    {
        Debug.Log("Fonksiyon calisti.");
        int length = transform.childCount;
        for (int i = 0; i < length; i++)
            transform.GetChild(i).name = "Npc_" + (i+1);
        
    }
}
