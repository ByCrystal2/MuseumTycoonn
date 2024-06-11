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
    RectTransform target;
    private void Start()
    {
        // Tutorial ba�lad���nda t�m UI elemanlar�n� gizle
        dimPanel.SetActive(false);
        highlightImage.SetActive(false);
        highlightText.gameObject.SetActive(false);
    }

    public void SetTarget(RectTransform _target)//UnityEvent addlistener.
    {
        target = _target;
        UnityEngine.Events.UnityEvent targetEvent = DialogueManager.instance.currentHelper.EventForFocusedObjClickHandler;
        if (targetEvent != null) SetMethodInTargetEvent(targetEvent);
        else Debug.Log("TargetEvent Null. target obj => " + target.name);
    }
    public void SetMethodInTargetEvent(UnityEngine.Events.UnityEvent _event)
    {
        FocusedObjClickHandler focusedObj = highlightImage.gameObject.AddComponent<FocusedObjClickHandler>();
        focusedObj.AddTargetEvent(_event);
    }
    Tweener hightLightTweener;
    public void ShowHighlight(string message)
    {
        // Dim panelini ve highlight elemanlar�n� etkinle�tir
        dimPanel.SetActive(true);
        highlightImage.SetActive(true);
        highlightText.gameObject.SetActive(true);

        // Dim paneli i�in fade in animasyonu
        dimPanel.GetComponent<CanvasGroup>().DOFade(0.9f, 0.5f);

        // Highlight eleman�n� hedef UI eleman�n�n �zerine konumland�r
        highlightImage.transform.position = target.position;
        highlightImage.GetComponent<RectTransform>().sizeDelta = target.sizeDelta;

        // Highlight metnini ayarla
        highlightText.text = message;
        highlightText.transform.position = new Vector3(target.position.x, target.position.y - target.sizeDelta.y / 2 - 20, target.position.z);

        // Highlight eleman�na animasyon ekleyin (�rne�in bir parlamas� i�in)
        hightLightTweener = highlightImage.GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }

    public void HideHighlight()
    {
        // Dim paneli ve highlight elemanlar� i�in fade out animasyonu
        dimPanel.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            hightLightTweener.Kill();
            dimPanel.SetActive(false);
            highlightImage.SetActive(false);
            highlightText.gameObject.SetActive(false);
        });
    }
}
