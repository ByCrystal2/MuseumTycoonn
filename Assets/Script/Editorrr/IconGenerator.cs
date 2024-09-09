using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class IconGenerator : MonoBehaviour
{
    public Vector3 cameraOffset = new Vector3(0, 0, 1);
    public Vector3 cameraRotation = new Vector3(0, 180, 0);
    public int resolution = 256;
    public string iconName = "Icon";
    public bool usePerspective = true;
    public void GenerateIcon()
    {
        // Create a new camera
        GameObject cameraObject = new GameObject("IconCamera");
        Camera camera = cameraObject.AddComponent<Camera>();

        // Set camera properties
        camera.backgroundColor = new Color(0, 0, 0, 0);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.farClipPlane = 50;

        // Adjust the camera position and rotation
        cameraObject.transform.position = transform.position + cameraOffset;
        cameraObject.transform.rotation = Quaternion.Euler(cameraRotation);

        // Adjust camera settings based on user selection
        if (usePerspective)
        {
            camera.orthographic = false;
        }
        else
        {
            camera.orthographic = true;
            Bounds bounds = CalculateBounds();
            camera.orthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.y) * 1.1f; // Slightly larger to ensure the object fits
        }

        // Create a RenderTexture to render the camera's view
        RenderTexture renderTexture = new RenderTexture(resolution, resolution, 24);
        camera.targetTexture = renderTexture;

        // Render the camera's view
        camera.Render();

        // Set the active RenderTexture to the one we just rendered to
        RenderTexture.active = renderTexture;

        // Create a new Texture2D and read the pixels from the RenderTexture
        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        texture.Apply();

        // Save the texture as a PNG file in the Resources folder
        byte[] bytes = texture.EncodeToPNG();
        string path = Path.Combine("Assets/Resources", (iconName == "Icon" ? transform.name : iconName) + ".png");
        File.WriteAllBytes(path, bytes);

        // Clean up
        RenderTexture.active = null;
        camera.targetTexture = null;
        DestroyImmediate(renderTexture);
        DestroyImmediate(cameraObject);

        // Refresh the AssetDatabase so the new file is recognized by Unity
        AssetDatabase.Refresh();

        Debug.Log($"Icon saved at: {path}");
    }

    private Bounds CalculateBounds()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(transform.position, Vector3.zero);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    private void OnDrawGizmos()
    {
        // Calculate the camera's position and rotation based on the offset and rotation values
        Vector3 cameraPosition = transform.position + cameraOffset;
        Quaternion cameraQuaternion = Quaternion.Euler(cameraRotation);

        // Draw the camera position
        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, cameraPosition);
        Gizmos.DrawWireSphere(cameraPosition, 0.1f);

        // Draw the camera's forward direction to visualize where it is pointing
        Gizmos.color = Color.yellow;
        Vector3 cameraForward = cameraQuaternion * Vector3.forward * 2;
        Gizmos.DrawRay(cameraPosition, cameraForward);

        // Draw a representation of the camera's frustum
        Gizmos.matrix = Matrix4x4.TRS(cameraPosition, cameraQuaternion, Vector3.one);
        Gizmos.DrawFrustum(Vector3.zero, 45, 2, 0.1f, 1.0f);
    }
}

[CustomEditor(typeof(IconGenerator))]
public class IconGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        IconGenerator iconGenerator = (IconGenerator)target;
        if (GUILayout.Button("Generate Icon"))
        {
            iconGenerator.GenerateIcon();
        }
    }
}
