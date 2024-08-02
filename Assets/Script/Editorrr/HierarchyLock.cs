using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class HierarchyLock : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Museum Of Excesses/Find canvas in NpcBehaviour Children")]
    static void FindCanvasRenderers()
    {
        // �ncelikle NpcBehaviour script'ine sahip t�m objeleri buluyoruz
        NPCBehaviour[] npcObjects = GameObject.FindObjectsOfType<NPCBehaviour>();

        // Se�ilecek objeleri depolamak i�in bir liste
        List<GameObject> objectsToSelect = new List<GameObject>();

        foreach (NPCBehaviour npc in npcObjects)
        {
            // Child objelerini kontrol ediyoruz
            Canvas[] canvasRenderers = npc.GetComponentsInChildren<Canvas>();

            foreach (Canvas canvas in canvasRenderers)
            {
                // Bulunan objeyi listeye ekliyoruz
                objectsToSelect.Add(canvas.gameObject);
            }
        }

        // E�er herhangi bir obje bulunduysa, bu objeleri se�iyoruz
        if (objectsToSelect.Count > 0)
        {
            Selection.objects = objectsToSelect.ToArray();
            Debug.Log(objectsToSelect.Count + " canvas objesi bulundu ve se�ildi.");
        }
        else
        {
            Debug.Log("Hi�bir canvas objesi bulunamad�.");
        }
    }
#endif
}
