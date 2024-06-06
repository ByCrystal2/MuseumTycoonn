using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsCameraController : MonoBehaviour
{
    public Camera camera; // Kamerayý Unity Editor'da sürükleyip buraya býrakýn
    public Transform character; // Karakteri Unity Editor'da sürükleyip buraya býrakýn
    public float maxDistance = 100f; // Ray'in maksimum mesafesi
    public float transparencyLevel = 0f; // Þeffaflýk deðeri (0.0 - 1.0)
    void Update()
    {
        //// Kamera yönüne doðru ray gönderme
        //Ray ray = new Ray(character.position, camera.transform.forward);
        //RaycastHit hit;

        //// Eðer ray bir objeye çarparsa
        //if (Physics.Raycast(ray, out hit, maxDistance))
        //{
        //    // Çarpýlan objeye eriþme
        //    GameObject hitObject = hit.collider.gameObject;
        //    Debug.Log("Çarpýlan obje: " + hitObject.name);

        //    if (LayerMask.LayerToName(hitObject.layer) == "Wall")
        //    {
        //        // Çarpýlan objeyi tamamen þeffaf hale getirme
        //        SetObjectTransparency(hitObject, transparencyLevel);

        //        // Collider'ý devre dýþý býrakma
        //        hitObject.GetComponent<Collider>().enabled = false;

        //        // Belirli bir süre sonra collider'ý tekrar etkinleþtirme
        //        StartCoroutine(EnableCollider(hitObject.GetComponent<Collider>(), 2f));
        //    }            
        //}
    }
    void SetObjectTransparency(GameObject obj, float transparency)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material[] materials = renderer.materials;
            foreach (Material mat in materials)
            {
                // Eðer malzeme opaklýðý destekliyorsa þeffaflýk ayarý yapýlýr
                if (mat.HasProperty("_Color"))
                {
                    Color color = mat.color;
                    color.a = transparency;
                    mat.color = color;

                    // Shader ayarlarýný þeffaflýk için yapma
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
    System.Collections.IEnumerator EnableCollider(Collider collider, float delay)
    {
        yield return new WaitForSeconds(delay);
        collider.enabled = true;
    }
}