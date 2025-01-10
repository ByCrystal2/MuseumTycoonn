using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RedExclamationMarkHandler : MonoBehaviour
{// Bildirim niyetiyle ui objelerinin ustunde yanip sonen kirmizi unlem!
    [Header("Scale Options")]
    [SerializeField] float scaleInceraseDuration = 0.2f;
    [SerializeField] float scaleDeceraseDuration = 0.2f;
    [SerializeField] Vector3 defaultScaleValue = Vector3.one;
    [SerializeField] Vector3 targetScaleValue = Vector3.one * 2;

    Button targetButton;
    private void Awake()
    {
        targetButton = GetComponentInParent<Button>();
        if (targetButton != null )
        targetButton.onClick.AddListener(DestroyMe);
    }
    private void OnEnable()
    {
        EndTargetScaleValueAnim();
    }
    void EndTargetScaleValueAnim()
    {
        transform.DOScale(targetScaleValue, scaleInceraseDuration).OnComplete(() =>
        {
            StartDefaultScaleValueAnim();
        });
    }
    void StartDefaultScaleValueAnim()
    {
        transform.DOScale(defaultScaleValue, scaleDeceraseDuration).OnComplete(() =>
        {
            EndTargetScaleValueAnim();
        });
    }
    void DestroyMe()
    {
        targetButton.onClick.RemoveListener(DestroyMe);
        RedExclamationMarkManager.instance.RemoveMark(targetButton.gameObject.transform);
    }

}
