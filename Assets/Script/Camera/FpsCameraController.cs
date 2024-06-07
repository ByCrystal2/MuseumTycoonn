using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsCameraController : MonoBehaviour
{
    public Camera camera; // Kameray� Unity Editor'da s�r�kleyip buraya b�rak�n
    public Transform character; // Karakteri Unity Editor'da s�r�kleyip buraya b�rak�n
    public float maxDistance = 100f; // Ray'in maksimum mesafesi
    public float transparencyLevel = 0f; // �effafl�k de�eri (0.0 - 1.0)
    public float rayStartOffset = 0.5f; // Ray'in ba�lang�� noktas�n� kameran�n �n�ne ta��mak i�in offset
    [SerializeField] LayerMask targetLayer;

    private Ray ray;
    private GameObject previousHitObject; // �nceki �arp�lan obje

    void Update()
    {
        // Kamera pozisyonundan karaktere do�ru ray g�nderme
        Vector3 direction = (character.position - camera.transform.position).normalized;
        Vector3 rayStartPoint = camera.transform.position + direction * rayStartOffset;
        ray = new Ray(rayStartPoint, direction);

        RaycastHit hit;
        if (Physics.Raycast(rayStartPoint, direction, out hit, maxDistance, targetLayer, QueryTriggerInteraction.Ignore))
        {
            GameObject hitObject = hit.collider.gameObject;
            BoxCollider boxCollider = hitObject.GetComponent<BoxCollider>();
            // E�er �arp�lan obje "Wall" katman�ndaysa
            if (LayerMask.LayerToName(hitObject.layer) == "Wall")
            {
                Debug.Log("Carpilan duvar: " + hitObject.name);

                // �arp�lan objeyi tamamen �effaf hale getirme
                SetObjectTransparency(hitObject, transparencyLevel);

                // �arp�lan objenin BoxCollider'�n� devre d��� b�rak
                if (boxCollider != null)
                {
                    boxCollider.enabled = false;
                }

                // ��lemi sonland�r
                return;
            }
            else
            {
                // E�er �arp�lan obje bir duvar de�ilse ve �zerinde BoxCollider varsa, BoxCollider'� etkinle�tir
                if (boxCollider != null)
                {
                    boxCollider.enabled = true;
                }
            }
        }

        // Duvarla temas olmad���nda �nceki �arp�lan objenin BoxCollider'�n� tekrar etkinle�tir
        ResetPreviousHitObjectCollider();
    }

    private void OnDrawGizmos()
    {
        // Ray'i sahnede �izmek i�in gizmoslar� kullan
        if (camera != null && character != null)
        {
            Vector3 direction = (character.position - camera.transform.position).normalized;
            Vector3 rayStartPoint = camera.transform.position + direction * rayStartOffset;
            ray = new Ray(rayStartPoint, direction);

            // Raycast i�lemi
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxDistance, targetLayer, QueryTriggerInteraction.Ignore))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(ray.origin, ray.direction * maxDistance);

                // �arpma noktas�n� g�ster
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(hit.point, 0.1f);

                // E�er �arp�lan obje "Wall" katman�ndaysa
                GameObject hitObject = hit.collider.gameObject;
                if (LayerMask.LayerToName(hitObject.layer) == "Wall")
                {
                    return;
                }
            }

            // Ray'in maksimum mesafeye kadar �izilmesi
            Gizmos.color = Color.red;
            Gizmos.DrawRay(ray.origin, ray.direction * maxDistance);
        }
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

    void ResetPreviousHitObjectCollider()
    {
        // E�er �nceki �arp�lan obje varsa ve bir duvar de�ilse, BoxCollider'�n� tekrar etkinle�tir
        if (previousHitObject != null && LayerMask.LayerToName(previousHitObject.layer) != "Wall")
        {
            BoxCollider previousBoxCollider = previousHitObject.GetComponent<BoxCollider>();
            if (previousBoxCollider != null)
            {
                previousBoxCollider.enabled = true;
            }

            // �nceki �arp�lan objeyi s�f�rla
            previousHitObject = null;
        }
    }
}
