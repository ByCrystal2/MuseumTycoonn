using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectionInfoPanelController : MonoBehaviour
{
    [SerializeField] GameObject messageObj;
    [SerializeField] GameObject signalIcon;
    [SerializeField] Transform infoTextPanelBG;
    [SerializeField] Button infoButton;
    [SerializeField] Text txtInfo;
    private bool isScaledUp = false;
    private void Awake()
    {
        infoButton.onClick.AddListener(OnInfoButtonClick);
    }
    public void SetInfoText(string text)
    {
        txtInfo.text = text;
    }
    void OnInfoButtonClick()
    {
        if (!isScaledUp)
        {
            // Ölçek büyütme animasyonu ve panel açma
            infoButton.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.2f).SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    OnMessagePanelAnim(); // Panel açýlma animasyonu
                    isScaledUp = true;
                });
        }
        else
        {
            // Panel kapama animasyonu ve ölçek küçültme
            CloseMessagePanelAnim(() =>
            {
                infoButton.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.InOutQuad)
                    .OnComplete(() => isScaledUp = false);
            });
        }
    }
    void OnMessagePanelAnim()
    {
        messageObj.gameObject.SetActive(true);

        signalIcon.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.1f).SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                signalIcon.transform.DOScale(new Vector3(1f, 1f, 1f), 0.05f).SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        infoTextPanelBG.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutQuad)
                            .OnComplete(() =>
                            {
                                txtInfo.GetComponent<CanvasGroup>().DOFade(1, 0.1f);
                            });
                    });
            });
    }
    void CloseMessagePanelAnim(System.Action onComplete)
    {
        txtInfo.GetComponent<CanvasGroup>().DOFade(0, 0.2f)
            .OnComplete(() =>
            {
                infoTextPanelBG.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        signalIcon.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.2f).SetEase(Ease.InOutQuad)
                            .OnComplete(() =>
                            {
                                signalIcon.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                                    .OnComplete(() =>
                                    {
                                        messageObj.gameObject.SetActive(false);
                                        onComplete?.Invoke();
                                    });
                            });
                    });
            });
    }
}
