using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCUI : MonoBehaviour
{
    public Image StressBackgroundImage;
    public Image StressFillerImage;
    public Transform EffectsParent;

    private float _currentStress = 20;
    Coroutine UpdateVisualCoroutine;

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
                StressFillerImage.fillAmount = _currentStress / 100f;

                int _emojiID = Mathf.FloorToInt(_currentStress / 17f);
                //Debug.Log("_emojiID: " + _emojiID);
                StressBackgroundImage.sprite = NpcManager.instance.StressEmojis[_emojiID];
                //StressFillerImage.sprite = NpcManager.instance.StressEmojis[_emojiID];
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
                StressFillerImage.fillAmount = _currentStress / 100f;

                int _emojiID = Mathf.RoundToInt(_currentStress / 17f);
                //Debug.Log("_emojiID: " + _emojiID);
                StressBackgroundImage.sprite = NpcManager.instance.StressEmojis[_emojiID];
                //StressFillerImage.sprite = NpcManager.instance.StressEmojis[_emojiID];
                //Debug.Log("After = Decreasing => _currentStress: " + _currentStress + " / targetstress: " + _targetStress);
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
}
