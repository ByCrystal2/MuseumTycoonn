using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DanceButtonsPanelController : MonoBehaviour
{
    List<Button> danceButtons = new List<Button>();
    private void Awake()
    {
        int length = transform.childCount;
        for (int i = 0; i < length; i++)
            if (transform.GetChild(i).TryGetComponent(out Button danceButton))
                danceButtons.Add(danceButton);
    }
    private void OnEnable()
    {
        FadeButtons(1, 0.3f);
    }
    private void OnDisable()
    {
        FadeButtons(0, 0.1f);
    }
    void FadeButtons(float endValue, float duration)
    {
        int length = danceButtons.Count;
        for (int i = 0; i < length; i++)
            if (danceButtons[i].TryGetComponent(out CanvasGroup buttonCanvasGroup))
                buttonCanvasGroup.DOFade(endValue, duration);
    }
}
