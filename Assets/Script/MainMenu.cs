using DG.Tweening;
using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button StartGameButton;
    [SerializeField] Transform LanguageButtonsContent;
    [SerializeField] RectTransform emptyObj;
    public bool CanSetNewLanguage = true;
    public static MainMenu instance {  get; private set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        int length = LanguageButtonsContent.childCount;
        for (int i = 0; i < length; i++)
            if (LanguageButtonsContent.GetChild(i).TryGetComponent(out Language _language))
            {
                Button languageButton = _language.GetComponent<Button>();
                languageButton.onClick.AddListener(() => NewLanguageButtonAnim());
            }
    }
    void Start()
    {
        AudioManager.instance.PlayMusicOfMenu();
        StartGameButton.onClick.AddListener(OnStartButtonClick);
        NewLanguageButtonAnim();
        languageChangedNotification = NotificationManager.instance.GetNotificationWithID(1);
    }
    Notification languageChangedNotification;
    float maxLanguageChangedTime = 10, currentTime = 0f;
    int languageChangedCount;
    private void Update()
    {
        if (!CanSetNewLanguage) return;
        currentTime += Time.deltaTime;
        if (currentTime <= maxLanguageChangedTime)
        {
            if (languageChangedNotification.AlertCount >= languageChangedNotification.TriggerAlertNumber)
            {
                CanSetNewLanguage = false;
                currentTime = 0f;
                languageChangedCount = 0;
            }            
        }
        else
        {
            currentTime = 0f;
            languageChangedCount = 0;
            languageChangedNotification.ResetNotification();
        }
    }
    public void ResetLanguageChangedValues()
    {
        currentTime = 0f;
        languageChangedCount = 0;
        CanSetNewLanguage = true;
        Debug.Log("ResetLanguageChangedValues");
    }
    void OnStartButtonClick()
    {
        FirebaseAuthManager.instance.CreateNewLoading();
    }
    public void NewLanguageButtonAnim()
    {
        StartCoroutine(IENewLanguageButtonAnim());
    }
    private Button beforeButton;
    public IEnumerator IENewLanguageButtonAnim()
    {
        yield return new WaitUntil(() => GameManager.instance.DatabaseLanguageProgressComplated);
        if (languageChangedNotification.AlertCount >= languageChangedNotification.TriggerAlertNumber) yield return null;
            int length = LanguageButtonsContent.childCount;
        for (int i = 0; i < length; i++)
        {
            Transform child = LanguageButtonsContent.GetChild(i);

            if (child.TryGetComponent(out Language languageButton))
            {
                if (languageButton.LanguageString == GameManager.instance.GameLanguage &&
                    child.TryGetComponent(out LayoutElement layoutElement))
                {
                    Button currentButton = child.GetComponent<Button>();

                    if (beforeButton == currentButton)
                        yield break;

                    ResetEffectAllButtons();

                    beforeButton = currentButton;

                    Vector3 defaultPos = child.position;
                    RectTransform rectTransform = child.GetComponent<RectTransform>();
                    Vector2 defaultSizeDelta = rectTransform.sizeDelta;
                    layoutElement.ignoreLayout = true;

                    int siblingIndex = child.GetSiblingIndex() + 1;
                    GameObject forLayoutElement = Instantiate(emptyObj.gameObject, LanguageButtonsContent);
                    forLayoutElement.transform.SetSiblingIndex(siblingIndex);
                    forLayoutElement.GetComponent<RectTransform>().sizeDelta = defaultSizeDelta;

                    rectTransform.position = defaultPos;
                    rectTransform.sizeDelta = defaultSizeDelta;
                    rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + 40, 0.3f);
                    rectTransform.DOScale(1.2f, 0.3f);
                    NotificationManager.instance.SendNotification(languageChangedNotification, new SenderHelper(WhoSends.System,9999),2);
                    languageChangedCount++;
                    Debug.Log("languageChangedCount++ => " + languageChangedCount);
                    break;
                }
            }
        }
    }

    private void ResetEffectAllButtons()
    {
        int length = LanguageButtonsContent.childCount;

        for (int i = 0; i < length; i++)
        {
            Transform child = LanguageButtonsContent.GetChild(i);

            if (child.TryGetComponent(out Language languageButton))
            {
                if (child.TryGetComponent(out LayoutElement layoutElement))
                {
                    layoutElement.ignoreLayout = false;
                    child.localScale = Vector3.one;
                }
            }
            else
            {
                Destroy(child.gameObject);
            }
        }
    }
}
