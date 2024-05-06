using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GiftBoxBehaviour : MonoBehaviour
{
    [Header("Position")]
    [SerializeField, Range(0, 5)] private float posDuration;
    private Vector2 startPos;
    private Vector2 endPos;
    [Header("Scale")]
    [SerializeField, Range(0, 5)] private float scaleDuration;
    [SerializeField] private Vector2 scaleStart;
    [SerializeField] private Vector2 scaleEnd;
    [Header("Rotate")]
    [SerializeField, Range(0, 5)] private float rotateDuration;
    [SerializeField] private float rotateStartZ;
    [SerializeField] private float rotateEndZ;
  

    void Start()
    {
        startPos = transform.position;
        endPos = new Vector2(startPos.x + 0.25f, startPos.y);
        ShakeStart();
        ScaleStart();
        RotateStart();
    }

    void ShakeStart()
    {
        transform.DOMove(endPos, posDuration).SetEase(Ease.OutQuint).OnComplete(() => ShakeEnd());


    }
    void ShakeEnd()
    {
        transform.DOMove(startPos, posDuration).SetEase(Ease.OutQuint).OnComplete(() => ShakeStart());


    }
    void ScaleStart()
    {
        transform.DOScale(scaleEnd, scaleDuration).SetEase(Ease.OutBounce).OnComplete(() => ScaleEnd());
    }
    void ScaleEnd()
    {
        transform.DOScale(scaleStart, scaleDuration).SetEase(Ease.InOutQuad).OnComplete(() => ScaleStart());
    }
    void RotateStart()
    {
        transform.DOLocalRotate(new Vector3(0, 0, rotateEndZ), rotateDuration).SetEase(Ease.InOutQuad).OnComplete(() => RotateEnd());
    }
    void RotateEnd()
    {
        transform.DOLocalRotate(new Vector3(0, 0, rotateStartZ), rotateDuration).SetEase(Ease.InOutQuad).OnComplete(() => RotateStart());
    }    
}
