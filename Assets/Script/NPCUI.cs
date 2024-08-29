using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class NPCUI : MonoBehaviour
{
    public Sprite ProfileSprite;
    [Header("Edit Mode")]
    public GameObject GuiltyImage;
    public SpriteRenderer StressBackgroundImage;
    public SpriteRenderer StressFillerImage;
    public Transform StressMasker;
    public GameObject StressHolder;

    [Header("TPS Mode")]
    public GameObject TPS_GuiltyImage;
    public SpriteRenderer TPS_StressFillerImage;
    public Transform TPS_StressMasker;
    public GameObject TPS_StressHolder;

    public Transform EffectsParent;

    public bool isGuiltyNow;
    private float _currentStress = 0;
    Coroutine UpdateVisualCoroutine;

    public void SetNPCasGuilty(bool _isGuilty)
    {
        isGuiltyNow = _isGuilty;
        GuiltyImage.gameObject.SetActive(_isGuilty);
        TPS_GuiltyImage.gameObject.SetActive(_isGuilty);
        StressHolder.gameObject.SetActive(!_isGuilty);
        //StressFillerImage.gameObject.SetActive(!_isGuilty);
    }

    public void UpdateStressBar(float _targetStress)
    {
        if(UpdateVisualCoroutine != null)
            StopCoroutine(UpdateVisualCoroutine);
        if (_currentStress == _targetStress)
            return;
        UpdateVisualCoroutine = StartCoroutine(UpdateVisual(_targetStress, _currentStress < _targetStress));
    }

    public void PlayEmotionEffect(NpcEmotionEffect _effect)
    {
        EffectsParent.GetChild((int)_effect).GetComponent<ParticleSystem>().Play();
    }

    IEnumerator UpdateVisual(float _targetStress, bool _isIncrease)
    {
        if (_isIncrease)
        {
            while (_currentStress < _targetStress)
            {
                //Debug.Log("Increasing => _currentStress: " + _currentStress + " / targetstress: " + _targetStress);
                _currentStress += Time.deltaTime * 20;
                if (_currentStress > _targetStress)
                    _currentStress = _targetStress;
                if (_currentStress > 100)
                    _currentStress = 100;

                Color col = HsvToRgb(_currentStress / 100f, 1f, 1f);
                StressFillerImage.color = col;
                TPS_StressFillerImage.color = col;

                float normalizedStress = _currentStress / 100;
                float currentScaleY = normalizedStress * 2.4f;
                StressMasker.transform.localScale = new Vector3(StressMasker.transform.localScale.x, currentScaleY, StressMasker.transform.localScale.z);
                float tps_CurrentScaleY = normalizedStress * 0.24f;
                TPS_StressMasker.transform.localScale = new Vector3(TPS_StressMasker.transform.localScale.x, tps_CurrentScaleY, TPS_StressMasker.transform.localScale.z);

                float targetPos = -1 * ((2.4f - (StressMasker.transform.localScale.y)) / 2);
                StressMasker.transform.localPosition = new Vector3(-0.05f, 2.37f, targetPos);
                float tps_TargetPos = -10 * ((0.24f - (TPS_StressMasker.transform.localScale.y)) / 4);
                TPS_StressMasker.transform.localPosition = new Vector3(0, 0, tps_TargetPos);

                int _emojiID = Mathf.FloorToInt(_currentStress / 17f);
                //StressBackgroundImage.sprite = NpcManager.instance.StressEmojis[_emojiID];

                yield return new WaitForSeconds(0.1f);
            }
            
        }
        else
        {
            while (_currentStress > _targetStress)
            {
                //Debug.Log("Before = Decreasing => _currentStress: " + _currentStress + " / targetstress: " + _targetStress);
                _currentStress -= Time.deltaTime * 20;
                if (_currentStress < _targetStress)
                    _currentStress = _targetStress;

                if (_currentStress < 0)
                    _currentStress = 0;
                Color col = HsvToRgb(_currentStress / 100f, 1f, 1f);
                StressFillerImage.color = col;
                TPS_StressFillerImage.color = col;

                float normalizedStress = _currentStress / 100;
                float currentScaleY = normalizedStress * 2.4f;
                StressMasker.transform.localScale = new Vector3(StressMasker.transform.localScale.x, currentScaleY, StressMasker.transform.localScale.z);
                float tps_CurrentScaleY = normalizedStress * 0.24f;
                TPS_StressMasker.transform.localScale = new Vector3(TPS_StressMasker.transform.localScale.x, tps_CurrentScaleY, TPS_StressMasker.transform.localScale.z);

                float targetPos = -1 * ((2.4f - (StressMasker.transform.localScale.y)) / 2);
                StressMasker.transform.localPosition = new Vector3(-0.05f, 2.37f, targetPos);
                float tps_TargetPos = -10 * ((0.24f - (TPS_StressMasker.transform.localScale.y)) / 4);
                TPS_StressMasker.transform.localPosition = new Vector3(0, 0, tps_TargetPos);

                int _emojiID = Mathf.FloorToInt(_currentStress / 17f);
                //StressBackgroundImage.sprite = NpcManager.instance.StressEmojis[_emojiID];
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    // HSV renk uzayından RGB'ye dönüş fonksiyonu
    private Color HsvToRgb(float h, float s, float v)
    {
        int i = Mathf.FloorToInt(h * 6);
        float f = h * 6 - i;
        float p = v * (1 - s);
        float q = v * (1 - f * s);
        float t = v * (1 - (1 - f) * s);

        switch (i % 6)
        {
            case 0: return new Color(v, t, p);
            case 1: return new Color(q, v, p);
            case 2: return new Color(p, v, t);
            case 3: return new Color(p, q, v);
            case 4: return new Color(t, p, v);
            case 5: return new Color(v, p, q);
            default: return Color.black;
        }
    }

    float Map(float value, float inMin, float inMax, float outMin, float outMax)
    {
        return (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }

    public void LookAtCamera()
    {
        TPS_GuiltyImage.transform.parent.LookAt(NpcManager.instance.TPSCamera);
    }

#if UNITY_EDITOR
    public bool DoldurBabaComponentleri;
    private void OnDrawGizmosSelected()
    {
        if (DoldurBabaComponentleri)
        {
            DoldurBabaComponentleri = false;

            GuiltyImage = transform.GetChild(10).gameObject;
            GuiltyImage.transform.localPosition = new Vector3(0.34f, 2.39f, 0);
            StressBackgroundImage = transform.GetChild(9).GetChild(0).GetComponent<SpriteRenderer>();
            StressFillerImage = transform.GetChild(9).GetChild(2).GetComponent<SpriteRenderer>();
            StressMasker = transform.GetChild(9).GetChild(1);
            StressMasker.localPosition = new Vector3(-0.05f, 2.37f, -1.2f);
            StressMasker.localScale = new Vector3(2.4f, 0, 1);
            StressHolder = transform.GetChild(9).gameObject;
            StressHolder.transform.localPosition = Vector3.zero;

            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(GuiltyImage);
            EditorUtility.SetDirty(StressBackgroundImage);
            EditorUtility.SetDirty(StressFillerImage);
            EditorUtility.SetDirty(StressMasker);
            EditorUtility.SetDirty(StressHolder);

            Transform tpsParent = transform.GetChild(11);
            tpsParent.localPosition = new Vector3(0,2.78f,0);
            TPS_GuiltyImage = tpsParent.GetChild(1).gameObject;
            TPS_GuiltyImage.transform.localPosition = new Vector3(0.15f,0.1f,0);
            TPS_StressHolder = tpsParent.GetChild(0).gameObject;
            TPS_StressFillerImage = TPS_StressHolder.transform.GetChild(2).GetComponent<SpriteRenderer>();
            TPS_StressMasker = TPS_StressHolder.transform.GetChild(1);

            EditorUtility.SetDirty(TPS_GuiltyImage);
            EditorUtility.SetDirty(TPS_StressHolder);
            EditorUtility.SetDirty(TPS_StressFillerImage);
            EditorUtility.SetDirty(TPS_StressMasker);
        }   
    }
#endif
}
