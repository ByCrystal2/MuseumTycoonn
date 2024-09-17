using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIFade : MonoBehaviour
{
    CanvasGroup canvasGroup;
    public float timer = 0.5f;
    public bool IgnoreOnStart;
    private bool fadeOut = false;
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public Tween Fade(float alpha, float delay) { return canvasGroup.DOFade(alpha, delay); }
    //IEnumerator<Tween> IEFadeIn(float alpha, float delay)
    //{
    //    yield return canvasGroup.DOFade(alpha, delay);
    //    yield return null;
    //}

    private void OnEnable()
    {
        if (IgnoreOnStart)
        {
            IgnoreOnStart = false;
            return;
        }
        FadeIn();
    }

    public void FadeIn()
    {
        if (gameObject.activeSelf)
        {
            fadeOut = false;
            StopAllCoroutines();
            StartCoroutine(FadeCanvasGroup(0, 1, timer, false));
        }
    }

    public void FadeOut(bool _destroyOnEnd = false)
    {
        if (gameObject.activeSelf && !fadeOut)
        {
            fadeOut = true;
            StopAllCoroutines();
            StartCoroutine(FadeCanvasGroup(1, 0, timer, _destroyOnEnd));
        }
    }

    IEnumerator FadeCanvasGroup(float startAlpha, float targetAlpha, float duration, bool _destroyOnEnd)
    {
        float timer = 0f;

        while (timer < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            canvasGroup.alpha = alpha;
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        if (targetAlpha == 0)
        {
            if (!_destroyOnEnd)
            {
                fadeOut = false;
                gameObject.SetActive(false);
            }
            else
                Destroy(gameObject);
        }
    }

    public void SetDelayedFadeOut(float _seconds = 3, bool _destroyOnEnd = false)
    {
        StartCoroutine(DelayedFadeOut(_seconds, _destroyOnEnd));
    }

    IEnumerator DelayedFadeOut(float _seconds, bool _destroyOnEnd)
    {
        yield return new WaitForSeconds(_seconds);
        FadeOut(_destroyOnEnd);
    }
}
