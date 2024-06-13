using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUISPanel : MonoBehaviour
{
    [SerializeField] private GameObject dimPanel;
    [SerializeField] private GameObject highlightImage;
    [SerializeField] private Text highlightText;

    [SerializeField] GameObject arrow;
    // Referans çözünürlük
    private Vector2 referenceResolution = new Vector2(1920, 1080);
    Vector3 targetPos;
    private void Start()
    {
        // Tutorial baþladýðýnda tüm UI elemanlarýný gizle
        dimPanel.SetActive(false);
        highlightImage.SetActive(false);
        highlightText.gameObject.SetActive(false);
    }

    public void SetTarget()//UnityEvent addlistener.
    {
        targetPos = DialogueManager.instance.currentHelper.TargetPos;
        UnityEngine.Events.UnityEvent targetEvent = DialogueManager.instance.currentHelper.EventForFocusedObjClickHandler;
        if (targetEvent != null) SetMethodInTargetEvent(targetEvent);
        else Debug.Log("TargetEvent Null.");
    }
    public void SetMethodInTargetEvent(UnityEngine.Events.UnityEvent _event)
    {
        FocusedObjClickHandler focusedObj = highlightImage.gameObject.AddComponent<FocusedObjClickHandler>();
        focusedObj.AddTargetEvent(_event);
    }
    Tweener hightLightTweener;
    Tween arrowTween;
    public void ShowHighlight(string message)
    {

        // Dim panelini ve highlight elemanlarýný etkinleþtir

        //Debug.Log("dimPanel.activeSelf => " + dimPanel.activeSelf);
        // Dim paneli için fade in animasyonu
        dimPanel.GetComponent<CanvasGroup>().DOFade(0.9f, 0.5f);

        // Hedef UI elemanýnýn dünya koordinatlarýný ekran koordinatlarýna çevir

        // Hedef pozisyonu mevcut ekran çözünürlüðüne göre ayarlayýn
        Vector2 adjustedPos = AdjustPositionForCurrentResolution(targetPos);

        highlightImage.GetComponent<RectTransform>().anchoredPosition = adjustedPos;
        arrow.GetComponent<RectTransform>().anchoredPosition = new Vector2(adjustedPos.x - 125, adjustedPos.y);
        highlightImage.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 130);
        highlightText.GetComponent<RectTransform>().anchoredPosition = new Vector3(adjustedPos.x, adjustedPos.y - 110, 0);
        
        highlightText.text = message;
        StartCoroutine(WaitForHidingHighLight());
        // Highlight elemanýna animasyon ekleyin (örneðin bir parlamasý için)
    }
    IEnumerator WaitForHidingHighLight()
    {
        yield return new WaitUntil(() => !hidingHighLigt);
        dimPanel.SetActive(true);
        highlightImage.SetActive(true);
        highlightText.gameObject.SetActive(true);
        arrow.SetActive(true);
        highlightImage.GetComponent<CanvasGroup>().alpha = 0.2f;
        hightLightTweener = highlightImage.GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetLoops(-1, LoopType.Yoyo);
        arrowTween = arrow.transform.DOLocalMoveX(arrow.GetComponent<RectTransform>().anchoredPosition.x - 35, 0.7f).SetLoops(-1, LoopType.Yoyo);
    }
    private Vector2 AdjustPositionForCurrentResolution(Vector3 originalPosition)
    {
        Vector2 currentResolution = new Vector2(Screen.width, Screen.height);

        // Referans çözünürlüðe göre orantýlý olarak pozisyonu ayarlayýn
        float scaleX = currentResolution.x / referenceResolution.x;
        float scaleY = currentResolution.y / referenceResolution.y;

        // Orantýlý pozisyon hesaplama
        float adjustedX = originalPosition.x * scaleX;
        float adjustedY = originalPosition.y * scaleY;

        return new Vector2(adjustedX, adjustedY);
    }
    bool hidingHighLigt;
    public void HideHighlight()
    {
        hidingHighLigt = true;
        // Dim paneli ve highlight elemanlarý için fade out animasyonu
        dimPanel.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            hightLightTweener.Kill();
            arrowTween.Kill();
            dimPanel.SetActive(false);
            highlightImage.SetActive(false);
            highlightText.gameObject.SetActive(false);
            arrow.SetActive(false);
            hidingHighLigt = false;
        });
    }
    [System.Serializable]
    public struct targetHelper
    {
        public RectTransform Transform;
        public Vector3 Position;
    }
}
