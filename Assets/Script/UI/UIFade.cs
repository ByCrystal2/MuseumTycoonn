using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIFade : MonoBehaviour
{
    CanvasGroup canvasGroup;
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
}
