using System.Collections.Generic;
using UnityEngine;

public class BetweenCamPlayerHandler : MonoBehaviour
{
    [SerializeField] private Camera m_Camera;
    [SerializeField] private Transform target;
    [Header("Runtime List")]
    public List<FadedObject> ActiveBlockers = new List<FadedObject>();

    [Header("Layer Option")]
    public LayerMask BlockableLayer;
    private int RayCastIgnoreLayer;

    [Header("Checker")]
    [Tooltip("If you write 2 here. Every 2 seconds faded objects will be checked for still stays in front of camera")]
    public float maxDistanceMultiplierTillPlayer = 0.8f;
    public float stillCheckerSeconds = 1;
    private float stillCheckerTimer;

    [Header("Material For Non URP Shaders")]
    public Material FadeMat;
    private void Start()
    {
        RayCastIgnoreLayer = LayerMask.NameToLayer("Faded");
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (target == null)
            return;

        if (stillCheckerTimer < Time.time)
        {
            CleanNonBlockers();
            stillCheckerTimer = Time.time + stillCheckerSeconds;
            return;
        }

        RaycastHit hit;
        Vector3 direction = (target.position - m_Camera.transform.position).normalized;
        Ray ray = new Ray(m_Camera.transform.position, direction);
        float distance = Vector3.Distance(m_Camera.transform.position, target.position) * maxDistanceMultiplierTillPlayer;
        if (Physics.Raycast(ray, out hit, distance, BlockableLayer, QueryTriggerInteraction.Ignore))
        {
            AddNewBlocker(hit.transform.gameObject);
        }
        Debug.DrawRay(m_Camera.transform.position, direction * maxDistanceMultiplierTillPlayer);
    }

    public void AddNewBlocker(GameObject _new)
    {
        bool exist = false;
        foreach (var item in ActiveBlockers)
            if (item.RootGameObject == _new)
                return;

        MakeFadeOpaque MFO;
        if (!_new.TryGetComponent(out MFO))
            MFO = _new.AddComponent<MakeFadeOpaque>();

        List<Material> materials = new List<Material>();
        if (_new.TryGetComponent(out Renderer r))
        {
            int length = r.materials.Length;
            for (int i = 0; i < length; i++)
            {
                Material m = new Material(r.materials[i]);
                m.name = m.name.Replace(" (Instance)", "");
                materials.Add(m);
            }
        }

        FadedObject _f = new FadedObject();
        _f.RootGameObject = _new;
        _f.OriginalMaterial = materials;
        _f.OriginalLayer = _new.layer;
        _f.makeFadeOpaque = MFO;
        ActiveBlockers.Add(_f);
        //int length1 = ActiveBlockers.Count;
        //for (int i = 0; i < length1; i++)
        //{
        //    FadedObject fadedObject = ActiveBlockers[i];
        //    if (fadedObject.makeFadeOpaque.TryGetComponent(out Collider _col))
        //        _col.enabled = false;
        //}
        MFO.SetFaded(_f.OriginalMaterial, FadeMat);

        _new.layer = RayCastIgnoreLayer;
    }

    public void CleanNonBlockers()
    {
        int length = ActiveBlockers.Count;
        if (length == 0)
            return;

        int blockerCount = ActiveBlockers.Count;
        for (int i = blockerCount - 1; i >= 0; i--)
        {
            if (ActiveBlockers[i].RootGameObject == null)
            {
                ActiveBlockers.RemoveAt(i);
                continue;
            }
            ActiveBlockers[i].RootGameObject.layer = ActiveBlockers[i].OriginalLayer;
            ActiveBlockers[i].makeFadeOpaque.SetOpaque(ActiveBlockers[i].OriginalMaterial);
            if (ActiveBlockers[i].RootGameObject.TryGetComponent(out Renderer r))
                r.materials = ActiveBlockers[i].OriginalMaterial.ToArray();
        }
        //int length1 = ActiveBlockers.Count;
        //for (int i = 0; i < length1; i++)
        //{
        //    FadedObject fadedObject = ActiveBlockers[i];
        //    if (fadedObject.makeFadeOpaque.TryGetComponent(out Collider _col))
        //        _col.enabled = true;
        //}
        ActiveBlockers.Clear();
        for (int i = 0; i < length + 1; i++)
        {
            RaycastHit hit;
            Vector3 direction = (target.position - m_Camera.transform.position).normalized;
            Ray ray = new Ray(m_Camera.transform.position, direction);
            float distance = Vector3.Distance(m_Camera.transform.position , target.position) * maxDistanceMultiplierTillPlayer;
            if (Physics.Raycast(ray, out hit, distance, BlockableLayer, QueryTriggerInteraction.Ignore))
            {
                AddNewBlocker(hit.transform.gameObject);
            }
        }
    }

    [System.Serializable]
    public struct FadedObject
    {
        public GameObject RootGameObject;
        public List<Material> OriginalMaterial;
        public int OriginalLayer;
        public MakeFadeOpaque makeFadeOpaque;
    } 
}
