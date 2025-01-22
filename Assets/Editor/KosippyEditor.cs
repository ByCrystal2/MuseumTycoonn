using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.Networking;
//using Unity.EditorCoroutines.Editor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BuildSettingsTool : EditorWindow
{
    private string keystorePath = "Assets/Important/user.keystore";
    private string keystoreFullPath = "";
    private string keystorePass = "password";
    private string keyAliasName = "kosippy";
    private string keyAliasPass = "password";

    [MenuItem("Kosippy/Build Settings Tool")]
    public static void ShowWindow()
    {
        GetWindow<BuildSettingsTool>("Build Settings Tool");
    }

    private void OnGUI()
    {
        string keystoreFPath = Path.GetFullPath(keystorePath);
        GUILayout.Label("Keystore Settings", EditorStyles.boldLabel);
        keystoreFullPath = EditorGUILayout.TextField("Keystore Path", keystoreFPath);
        keystorePass = EditorGUILayout.PasswordField("Keystore Password", keystorePass);
        keyAliasName = EditorGUILayout.TextField("Key Alias Name", keyAliasName);
        keyAliasPass = EditorGUILayout.PasswordField("Key Alias Password", keyAliasPass);

        if (GUILayout.Button("Configure for APK"))
        {
            ConfigureBuildSettings(false);
        }

        if (GUILayout.Button("Configure for AAB"))
        {
            ConfigureBuildSettings(true);
        }
    }

    private void ConfigureBuildSettings(bool isAAB)
    {
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = keystoreFullPath;
        PlayerSettings.Android.keystorePass = keystorePass;
        PlayerSettings.Android.keyaliasName = keyAliasName;
        PlayerSettings.Android.keyaliasPass = keyAliasPass;

        EditorUserBuildSettings.buildAppBundle = isAAB;

        // Additional configurations for AAB or APK can be added here
        Debug.Log(isAAB ? "Configured for AAB build" : "Configured for APK build");
    }
}
