using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ToggleClickability : MonoBehaviour
{
    [MenuItem("Tools/Kosippy/Make Selected Objects Not Clickable %#&n")]
    private static void MakeSelectedObjectsNotClickable()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            obj.hideFlags |= HideFlags.NotEditable;
            EditorUtility.SetDirty(obj);
        }
    }

    [MenuItem("Tools/Kosippy/Make Selected Objects Clickable %#&m")]
    private static void MakeSelectedObjectsClickable()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            obj.hideFlags &= ~HideFlags.NotEditable;
            EditorUtility.SetDirty(obj);
        }
    }
}
