using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsCameraController : MonoBehaviour
{
    public Camera camera; // Kamerayý Unity Editor'da sürükleyip buraya býrakýn
    public Transform character; // Karakteri Unity Editor'da sürükleyip buraya býrakýn
    public float maxDistance = 100f; // Ray'in maksimum mesafesi
    public float transparencyLevel = 0f; // Þeffaflýk deðeri (0.0 - 1.0)
    public float rayStartOffset = 0.5f; // Ray'in baþlangýç noktasýný kameranýn önüne taþýmak için offset
    [SerializeField] LayerMask targetLayer;

    private Ray ray;
    private GameObject previousHitObject; // Önceki çarpýlan obje

    void Update()
    {
        // Kamera pozisyonundan karaktere doðru ray gönderme
        Vector3 direction = (character.position - camera.transform.position).normalized;
        Vector3 rayStartPoint = camera.transform.position + direction * rayStartOffset;
        ray = new Ray(rayStartPoint, direction);

        RaycastHit hit;
        if (Physics.Raycast(rayStartPoint, direction, out hit, maxDistance, targetLayer, QueryTriggerInteraction.Ignore))
        {
            GameObject hitObject = hit.collider.gameObject;
            BoxCollider boxCollider = hitObject.GetComponent<BoxCollider>();
            // Eðer çarpýlan obje "Wall" katmanýndaysa
            if (LayerMask.LayerToName(hitObject.layer) == "Wall")
            {
                Debug.Log("Carpilan duvar: " + hitObject.name);

                // Çarpýlan objeyi tamamen þeffaf hale getirme
                SetObjectTransparency(hitObject, transparencyLevel);

                // Çarpýlan objenin BoxCollider'ýný devre dýþý býrak
                if (boxCollider != null)
                {
                    boxCollider.enabled = false;
                }

                // Ýþlemi sonlandýr
                return;
            }
            else
            {
                // Eðer çarpýlan obje bir duvar deðilse ve üzerinde BoxCollider varsa, BoxCollider'ý etkinleþtir
                if (boxCollider != null)
                {
                    boxCollider.enabled = true;
                }
            }
        }

        // Duvarla temas olmadýðýnda önceki çarpýlan objenin BoxCollider'ýný tekrar etkinleþtir
        ResetPreviousHitObjectCollider();
    }

    private void OnDrawGizmos()
    {
        // Ray'i sahnede çizmek için gizmoslarý kullan
        if (camera != null && character != null)
        {
            Vector3 direction = (character.position - camera.transform.position).normalized;
            Vector3 rayStartPoint = camera.transform.position + direction * rayStartOffset;
            ray = new Ray(rayStartPoint, direction);

            // Raycast iþlemi
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxDistance, targetLayer, QueryTriggerInteraction.Ignore))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(ray.origin, ray.direction * maxDistance);

                // Çarpma noktasýný göster
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(hit.point, 0.1f);

                // Eðer çarpýlan obje "Wall" katmanýndaysa
                GameObject hitObject = hit.collider.gameObject;
                if (LayerMask.LayerToName(hitObject.layer) == "Wall")
                {
                    return;
                }
            }

            // Ray'in maksimum mesafeye kadar çizilmesi
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

    void ResetPreviousHitObjectCollider()
    {
        // Eðer önceki çarpýlan obje varsa ve bir duvar deðilse, BoxCollider'ýný tekrar etkinleþtir
        if (previousHitObject != null && LayerMask.LayerToName(previousHitObject.layer) != "Wall")
        {
            BoxCollider previousBoxCollider = previousHitObject.GetComponent<BoxCollider>();
            if (previousBoxCollider != null)
            {
                previousBoxCollider.enabled = true;
            }

            // Önceki çarpýlan objeyi sýfýrla
            previousHitObject = null;
        }
    }
}
