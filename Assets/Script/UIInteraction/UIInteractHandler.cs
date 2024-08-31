using System.Collections;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
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
        DontDestroyOnLoad(gameObject);
    }

    public void AskQuestion(string header, string explanation, QuestionAction yesAction = null, QuestionAction noAction = null, QuestionAction okayAction = null, object parameterYes = null, object parameterNo = null, object parameterOkay = null)
    {
        Canvas gameSceneCanvas = FindObjectOfType<PicturesMenuController>().GetComponent<Canvas>();
        if (gameSceneCanvas != null)
        {
            int childCount = gameSceneCanvas.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                if (i==childCount-1)
                {
                    Panel.transform.SetParent(gameSceneCanvas.transform);
                    Panel.transform.SetSiblingIndex(i-1);
                }
            }
        }

        yesButton.interactable = true;
        noButton.interactable = true;
        okayButton.interactable = true;

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

        if (yesAction == null && noAction == null && okayAction == null)
        {
            yesButton.gameObject.SetActive(false);
            noButton.gameObject.SetActive(false);
            okayButton.gameObject.SetActive(false);
        }
        else
        {
            yesButton.gameObject.SetActive(!okayUI);
            noButton.gameObject.SetActive(!okayUI);
            okayButton.gameObject.SetActive(okayUI);
        }
        if (!GameManager.instance.IsWatchTutorial)
        {
            TutorialTargetObjectHandler target = yesButton.transform.GetChild(2).gameObject.AddComponent<TutorialTargetObjectHandler>();
            target.SetOptions(4, target.GetComponent<RectTransform>());
            DialogueManager.instance.TargetObjectHandlers.Add(target);
        }
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
        yesButton.interactable = false;
        noButton.interactable = false;
        okayButton.interactable = false;
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
            Panel.gameObject.SetActive(false);
    }
}
