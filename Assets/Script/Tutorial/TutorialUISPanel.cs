using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialUISPanel : MonoBehaviour
{
    [SerializeField] private GameObject dimPanel;
    [SerializeField] private GameObject highlightImage;
    [SerializeField] private Text highlightText;

    [SerializeField] GameObject arrow;
    // Referans ��z�n�rl�k
    private Vector2 referenceResolution = new Vector2(1920, 1080);
    Vector3 targetPos;
    RectTransform target;
    TutorialTargetObjectHandler tutorialTargetWithId;
    Vector3 defaultArrowPos;
    Quaternion defaultArrowRot;

    [SerializeField] Vector3 rightArrowPos;
    [SerializeField] Quaternion rightArrowRot;
    int setTargetValue = 0;
    Canvas defaultCanvas;
    EventSystem eventSystem;
    GraphicRaycaster raycaster;
    private void Awake()
    {
        eventSystem = EventSystem.current;
        defaultCanvas = GetComponentInParent<Canvas>();
        raycaster = defaultCanvas.GetComponent<GraphicRaycaster>();
        defaultArrowPos = arrow.transform.localPosition;
        defaultArrowRot = arrow.transform.localRotation;
    }
    private void Start()
    {
        // Tutorial ba�lad���nda t�m UI elemanlar�n� gizle
        dimPanel.SetActive(false);
        highlightImage.SetActive(false);
        highlightText.gameObject.SetActive(false);
    }

    public void SetTarget(RectTransform _target)//UnityEvent addlistener.
    {
        setTargetValue = 0;
        //arrow.transform.localPosition = defaultArrowPos;
        if (_target != null)
            target = _target;
        EventSending();
    }
    public void SetTarget(int _targetID)
    {
        setTargetValue = 1;
        //arrow.transform.localPosition = defaultArrowPos;
        TutorialTargetObjectHandler target = FindObjectsOfType<TutorialTargetObjectHandler>().Where(x => x.ID == _targetID).SingleOrDefault();
        if (target != null)
        {
            Debug.Log("Tutorial target object name => " + target.name);
            tutorialTargetWithId = target;
        }            
        else
            Debug.Log("Tutorial target is not found and null. Sent ID:"+_targetID);

        EventSending();
    }
    public void SetTarget(GameObject _target3DObj)//UnityEvent addlistener.
    {
        setTargetValue = 2;
        //arrow.transform.localPosition = defaultArrowPos;
        Vector3 worldPosition = _target3DObj.transform.position;

        // D�nya pozisyonunu ekran pozisyonuna �evir
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        // Ekran pozisyonunu 2D vekt�r olarak al
        Vector2 uiPosition = new Vector2(screenPosition.x, screenPosition.y);
        targetPos = uiPosition;
        EventSending();
    }
    private void EventSending()
    {
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
        // Dim panelini ve highlight elemanlar�n� etkinle�tir

        //Debug.Log("dimPanel.activeSelf => " + dimPanel.activeSelf);
        // Dim paneli i�in fade in animasyonu
        dimPanel.GetComponent<CanvasGroup>().DOFade(0.9f, 0.5f);

        StartCoroutine(IEShowHightLight(message));
    }
    IEnumerator IEShowHightLight(string _message)
    {
        // Hedef UI eleman�n�n d�nya koordinatlar�n� ekran koordinatlar�na �evir
        // Hedef pozisyonu mevcut ekran ��z�n�rl���ne g�re ayarlay�n
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        Vector3 targetPosition = Vector3.zero;
        if (target == null && setTargetValue == 2)
        {
            Debug.Log("Highlight target is null!");
            targetPosition = targetPos;        
        }
        else if (target != null && setTargetValue == 0)
        {
            Debug.Log("Highlight target is not null!");
            targetPosition = target.position;
        }
        else if (setTargetValue == 1 && tutorialTargetWithId != null)
        {
            yield return new WaitUntil(() => tutorialTargetWithId.IsTargetSettingComplated());
            Debug.Log("Tutorial Target Object Handler Pos => " + tutorialTargetWithId.targetTransform.position);
            targetPosition = tutorialTargetWithId.targetTransform.position;
        }
        highlightImage.GetComponent<RectTransform>().position = targetPosition;
        highlightImage.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 130);
        
        //PlaceArrow();
        highlightText.text = _message;
        StartCoroutine(WaitForHidingHighLight());
        target = null;
        tutorialTargetWithId = null;
        setTargetValue = -1;
    }
    IEnumerator WaitForHidingHighLight()
    {
        yield return new WaitUntil(() => !hidingHighLigt);
        if (DialogueManager.instance.currentHelper.placeArrowToRight)
        {
            arrow.transform.localPosition = rightArrowPos;
            arrow.transform.localRotation = rightArrowRot;
        }
        else
        {
            arrow.transform.localPosition = defaultArrowPos;
            arrow.transform.localRotation = defaultArrowRot;
        }
        dimPanel.SetActive(true);
        highlightImage.SetActive(true);
        highlightText.gameObject.SetActive(true);
        arrow.SetActive(true);
        highlightImage.GetComponent<CanvasGroup>().alpha = 0.2f;
        hightLightTweener = highlightImage.GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetLoops(-1, LoopType.Yoyo);
        if (DialogueManager.instance.currentHelper.placeArrowToRight)
            arrowTween = arrow.transform.DOLocalMoveX(arrow.transform.localPosition.x + 30, 0.4f).SetLoops(-1, LoopType.Yoyo);
        else
            arrowTween = arrow.transform.DOLocalMoveX(arrow.transform.localPosition.x - 30, 0.4f).SetLoops(-1, LoopType.Yoyo);

    }

    private Vector2 AdjustPositionForCurrentResolution(Vector3 originalPosition)
    {
        Vector2 currentResolution = new Vector2(Screen.width, Screen.height);

        // Referans ��z�n�rl��e g�re orant�l� olarak pozisyonu ayarlay�n
        float scaleX = currentResolution.x / referenceResolution.x;
        float scaleY = currentResolution.y / referenceResolution.y;

        // Orant�l� pozisyon hesaplama
        float adjustedX = originalPosition.x * scaleX;
        float adjustedY = originalPosition.y * scaleY;

        return new Vector2(adjustedX, adjustedY);
    }
    bool hidingHighLigt;
    public void HideHighlight()
    {
        hidingHighLigt = true;
        // Dim paneli ve highlight elemanlar� i�in fade out animasyonu
        //dimPanel.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
        //{
            //arrow.transform.localPosition = defaultArrowPos;
            hightLightTweener.Kill();
            arrowTween.Kill();
            //dimPanel.SetActive(false);
            highlightImage.SetActive(false);
            highlightText.gameObject.SetActive(false);
            arrow.SetActive(false);
            hidingHighLigt = false;
        //});
    }
    public void SetActivationDimPanel(bool _active)
    {
        dimPanel.SetActive(_active);
    }
    [System.Serializable]
    public struct targetHelper
    {
        public RectTransform Transform;
        public Vector3 Position;
    }
}
