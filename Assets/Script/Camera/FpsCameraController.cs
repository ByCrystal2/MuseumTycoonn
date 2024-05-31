using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsCameraController : MonoBehaviour
{
    public Camera camera; // Kamerayý Unity Editor'da sürükleyip buraya býrakýn
    public Transform character; // Karakteri Unity Editor'da sürükleyip buraya býrakýn
    public float maxDistance = 100f; // Ray'in maksimum mesafesi
    public float opacity = 1.0f; // Opaklýk deðeri (0.0 - 1.0)

    void Update()
    {
        // Kamera yönüne doðru ray gönderme
        Ray ray = new Ray(character.position, camera.transform.forward);
        RaycastHit hit;

        // Eðer ray bir objeye çarparsa
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            // Çarpýlan objeye eriþme
            GameObject hitObject = hit.collider.gameObject;
            Debug.Log("Çarpýlan obje: " + hitObject.name);

            // Çarpýlan objeyi þeffaf hale getirme
            Renderer renderer = hitObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                foreach (Material mat in renderer.materials)
                {
                    Debug.Log("Mat => " + mat.name);
                    Color color = mat.color;
                    color.a = opacity;
                    mat.color = color;
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                }
            }
        }
    }
}
