using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class HierarchyLock : MonoBehaviour
{
    private static bool IsLocked
    {
        get
        {
            var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            var isLockedProperty = inspectorType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var focusedWindow = EditorWindow.focusedWindow;
            if (focusedWindow != null && focusedWindow.GetType() == inspectorType)
            {
                return (bool)isLockedProperty.GetValue(focusedWindow, null);
            }
            return false;
        }
        set
        {
            var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            var isLockedProperty = inspectorType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var focusedWindow = EditorWindow.focusedWindow;
            if (focusedWindow != null && focusedWindow.GetType() == inspectorType)
            {
                isLockedProperty.SetValue(focusedWindow, value, null);
                focusedWindow.Repaint();
            }
        }
    }

    [MenuItem("Tools/Toggle Inspector Lock %l")] // %l means Ctrl/Cmd + L
    private static void ToggleInspectorLock()
    {
        IsLocked = !IsLocked;
    }
}
