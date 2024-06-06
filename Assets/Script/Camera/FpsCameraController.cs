using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsCameraController : MonoBehaviour
{
    public Camera camera; // Kameray� Unity Editor'da s�r�kleyip buraya b�rak�n
    public Transform character; // Karakteri Unity Editor'da s�r�kleyip buraya b�rak�n
    public float maxDistance = 100f; // Ray'in maksimum mesafesi
    public float transparencyLevel = 0f; // �effafl�k de�eri (0.0 - 1.0)
    void Update()
    {
        //// Kamera y�n�ne do�ru ray g�nderme
        //Ray ray = new Ray(character.position, camera.transform.forward);
        //RaycastHit hit;

        //// E�er ray bir objeye �arparsa
        //if (Physics.Raycast(ray, out hit, maxDistance))
        //{
        //    // �arp�lan objeye eri�me
        //    GameObject hitObject = hit.collider.gameObject;
        //    Debug.Log("�arp�lan obje: " + hitObject.name);

        //    if (LayerMask.LayerToName(hitObject.layer) == "Wall")
        //    {
        //        // �arp�lan objeyi tamamen �effaf hale getirme
        //        SetObjectTransparency(hitObject, transparencyLevel);

        //        // Collider'� devre d��� b�rakma
        //        hitObject.GetComponent<Collider>().enabled = false;

        //        // Belirli bir s�re sonra collider'� tekrar etkinle�tirme
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
                // E�er malzeme opakl��� destekliyorsa �effafl�k ayar� yap�l�r
                if (mat.HasProperty("_Color"))
                {
                    Color color = mat.color;
                    color.a = transparency;
                    mat.color = color;

                    // Shader ayarlar�n� �effafl�k i�in yapma
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