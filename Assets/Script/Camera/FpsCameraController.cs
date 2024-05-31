using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsCameraController : MonoBehaviour
{
    public Camera camera; // Kameray� Unity Editor'da s�r�kleyip buraya b�rak�n
    public Transform character; // Karakteri Unity Editor'da s�r�kleyip buraya b�rak�n
    public float maxDistance = 100f; // Ray'in maksimum mesafesi
    public float opacity = 1.0f; // Opakl�k de�eri (0.0 - 1.0)

    void Update()
    {
        // Kamera y�n�ne do�ru ray g�nderme
        Ray ray = new Ray(character.position, camera.transform.forward);
        RaycastHit hit;

        // E�er ray bir objeye �arparsa
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            // �arp�lan objeye eri�me
            GameObject hitObject = hit.collider.gameObject;
            Debug.Log("�arp�lan obje: " + hitObject.name);

            // �arp�lan objeyi �effaf hale getirme
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
