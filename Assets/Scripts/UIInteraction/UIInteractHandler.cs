using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInteractHandler : MonoBehaviour
{
    public static UIInteractHandler instance { get; private set; }

    [Header("UI Ogeleri")]
    public CanvasGroup Panel;
    public Text headerText;
    public Text explanationText;
    public Button yesButton;
    public Button noButton;
    public Button okayButton;

    [Header("Fade Ayari")]
    [Tooltip("Fade effectinin kac saniye surecegini belirleyen sayisal deger")]
    public float FadeTimeInSeconds = 0.5f;

    public delegate void QuestionAction(object parameter = null);

    private void Awake()
    {
        if (instance)
            return;

        instance = this;
    }

    public void AskQuestion(string header, string explanation, QuestionAction yesAction = null, QuestionAction noAction = null, QuestionAction okayAction = null, object parameterYes = null, object parameterNo = null, object parameterOkay = null)
    {
        headerText.text = header;
        explanationText.text = explanation;

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();
        okayButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(HidePanel);
        noButton.onClick.AddListener(HidePanel);
        okayButton.onClick.AddListener(HidePanel);
        bool okayUI = true;
        if (yesAction != null)
        {
            yesButton.onClick.AddListener(() => yesAction(parameterYes));
            okayUI = false;
        }
        if (noAction != null)
        {
            noButton.onClick.AddListener(() => noAction(parameterNo));
            okayUI = false;
        }
        if (okayAction != null)
            okayButton.onClick.AddListener(() => okayAction(parameterOkay));

        yesButton.gameObject.SetActive(!okayUI);
        noButton.gameObject.SetActive(!okayUI);
        okayButton.gameObject.SetActive(okayUI);
        ShowPanel();
    }

    private void ShowPanel()
    {
        if (!Panel.gameObject.activeSelf)
            Panel.gameObject.SetActive(true);

        FadeIn();
    }

    private void HidePanel()
    {
        FadeOut();
    }

    public void FadeIn()
    {
        if (Panel.gameObject.activeSelf)
        {
            Debug.Log("Fade in now");
            StopAllCoroutines();
            StartCoroutine(FadeCanvasGroup(0, 1, FadeTimeInSeconds));
        }
    }

    public void FadeOut(bool _destroyOnEnd = false)
    {
        if (Panel.gameObject.activeSelf)
        {
            Debug.Log("Fade out now");
            StopAllCoroutines();
            StartCoroutine(FadeCanvasGroup(1, 0, FadeTimeInSeconds));
        }
    }

    IEnumerator FadeCanvasGroup(float startAlpha, float targetAlpha, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            Panel.alpha = alpha;
            timer += Time.deltaTime;
            yield return null;
        }

        Panel.alpha = targetAlpha;
        if (targetAlpha == 0)
            gameObject.SetActive(false);
    }
}
