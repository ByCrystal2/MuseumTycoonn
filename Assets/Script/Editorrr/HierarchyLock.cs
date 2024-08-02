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
        // Öncelikle NpcBehaviour script'ine sahip tüm objeleri buluyoruz
        NPCBehaviour[] npcObjects = GameObject.FindObjectsOfType<NPCBehaviour>();

        // Seçilecek objeleri depolamak için bir liste
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

        // Eðer herhangi bir obje bulunduysa, bu objeleri seçiyoruz
        if (objectsToSelect.Count > 0)
        {
            Selection.objects = objectsToSelect.ToArray();
            Debug.Log(objectsToSelect.Count + " canvas objesi bulundu ve seçildi.");
        }
        else
        {
            Debug.Log("Hiçbir canvas objesi bulunamadý.");
        }
    }
#endif
}
