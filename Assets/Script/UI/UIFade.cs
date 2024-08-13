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

    public void FadeIn(float alpha, float speed) => StartCoroutine(IEFadeIn(alpha, speed));
    IEnumerator IEFadeIn(float alpha, float speed)
    {
        canvasGroup.DOFade(alpha, speed);
        yield return null;
    }
}
