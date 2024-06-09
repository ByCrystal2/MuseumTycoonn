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
        // Tutorial baþladýðýnda tüm UI elemanlarýný gizle
        dimPanel.SetActive(false);
        highlightImage.SetActive(false);
        highlightText.gameObject.SetActive(false);
    }

    public void SetTarget(RectTransform _target)
    {
        target = _target;
    }
    public void ShowHighlight(string message)
    {
        // Dim panelini ve highlight elemanlarýný etkinleþtir
        dimPanel.SetActive(true);
        highlightImage.SetActive(true);
        highlightText.gameObject.SetActive(true);

        // Dim paneli için fade in animasyonu
        dimPanel.GetComponent<CanvasGroup>().DOFade(0.5f, 0.5f);

        // Highlight elemanýný hedef UI elemanýnýn üzerine konumlandýr
        highlightImage.transform.position = target.position;
        highlightImage.GetComponent<RectTransform>().sizeDelta = target.sizeDelta;

        // Highlight metnini ayarla
        highlightText.text = message;
        highlightText.transform.position = new Vector3(target.position.x, target.position.y - target.sizeDelta.y / 2 - 20, target.position.z);

        // Highlight elemanýna animasyon ekleyin (örneðin bir parlamasý için)
        highlightImage.GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }

    public void HideHighlight()
    {
        // Dim paneli ve highlight elemanlarý için fade out animasyonu
        dimPanel.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        {
            dimPanel.SetActive(false);
            highlightImage.SetActive(false);
            highlightText.gameObject.SetActive(false);
        });
    }
}
