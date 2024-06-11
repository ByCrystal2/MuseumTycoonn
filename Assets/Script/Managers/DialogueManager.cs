using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] GameObject PlayerCinemachineBrain;
    [SerializeField] GameObject TutorialCinemachineBrain;
    [SerializeField] Transform sceneTransPanel;
    [SerializeField] Text tutorialEndMessage;
    // UI
    [SerializeField] public GameObject DialoguePanel;
    public Animator tutorialNPCanimator;
    // UI
    private DialoguePanelVController currentDialogPanel;

    private Queue<string> sentences;
    public DialogueTrigger currentTrigger;
    public DialogHelper currentHelper;


    public static DialogueManager instance { get; private set; }
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
        sentences = new Queue<string>();
    }
    public void SetCurrentDialogTrigger(DialogueTrigger _trigger)
    {
        currentTrigger = _trigger;
    }
    public void SetCurrentDialogs(DialogHelper _helper, int value /*deger max dialogpanel objesinin cocuk sayisi kadar girilmeli.*/, string _name = "Anonymous")
    {
        currentHelper = _helper;
        int length = DialoguePanel.transform.childCount;
        for (int i = 0; i < length; i++)
            DialoguePanel.transform.GetChild(i).gameObject.SetActive(false);
        currentDialogPanel = DialoguePanel.transform.GetChild(value).GetComponent<DialoguePanelVController>();
        currentDialogPanel.nameText.text = _name;
        currentDialogPanel.gameObject.SetActive(true);
    }
    public void StartTutorial()//DialogueTrigger UnityEvents...
    {
        StartCoroutine(StartTutorialDialogue(currentHelper.Dialogs));
    }
    private IEnumerator StartTutorialDialogue(List<Dialog> _dialogs)
    {
        Debug.Log("Dialog Starting... First Dialog Message => " + _dialogs[0].Sentence);
        yield return new WaitForEndOfFrame();
        CinemachineTransition(true);
        //StartCoroutine(HoldAnimation(4, 1f,"startDialog",true));
        PlayerManager.instance.LockPlayer();
        UIController.instance.CloseJoystickObj(true);
        StartCoroutine(WaitForTutorialBrainTranstionEnding(_dialogs));
    }
    IEnumerator WaitForTutorialBrainTranstionEnding(List<Dialog> _dialogs)
    {
        yield return new WaitForSeconds(3);
        SetActivationDialoguePanel(true);

        sentences.Clear();
        foreach (Dialog dialog in _dialogs)
        {
            sentences.Enqueue(dialog.Sentence);
        }

        DisplayNextSentence();
    }
    public void EndTutorialDialogue()
    {
        SetActivationDialoguePanel(false);
        CinemachineTransition(false);
        StartCoroutine(HoldAnimation(1, 1f, "startDialog", false));
        PlayerManager.instance.UnLockPlayer();
        UIController.instance.CloseJoystickObj(false);
    }
    private void CinemachineTransition(bool _goTutorial)
    {
        TutorialCinemachineBrain.SetActive(_goTutorial);
        PlayerCinemachineBrain.SetActive(!_goTutorial);
    }
    public void SetActivationDialoguePanel(bool _active)
    {
        if (_active)
        {
           DialoguePanel.SetActive(true);
           DialoguePanel.GetComponent<CanvasGroup>().DOFade(1, 2);
        }
        else
        {            
            DialoguePanel.GetComponent<CanvasGroup>().DOFade(0, 1).OnComplete(() => DialoguePanel.SetActive(false));
        }
    }
    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            return;
        }
        StopAllCoroutines();
        StartCoroutine(waitSentences(sentences.Count));
    }
    IEnumerator waitSentences(int _sentencesCount)
    {
        for (int i = 0; i < _sentencesCount; i++)
        {
            yield return new WaitUntil(() =>!waitFirstSentence);
            yield return new WaitForSeconds(1f);
            string sentence = sentences.Dequeue();
            StartCoroutine(TypeSentence(sentence));
        }
        yield return new WaitUntil(() => !waitFirstSentence);
        currentHelper.EventEndingCovered.Invoke();
        yield return new WaitForSeconds(1f);
        SetActivationDialoguePanel(false);
        //EndTutorialDialogue();
    }
    IEnumerator HoldAnimation(int _duration, float _transitDuration, string _animString, bool _animStart)
    {
        for (int i = 0; i < _duration; i++)
        {
            yield return new WaitForSeconds(_transitDuration);
        }
        tutorialNPCanimator.SetBool(_animString, _animStart);
    }
    private bool waitFirstSentence = false;
    IEnumerator TypeSentence(string sentence)
    {
        waitFirstSentence = true;
        currentDialogPanel.dialogText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            currentDialogPanel.dialogText.text += letter;
            yield return new WaitForEndOfFrame();
        }
        waitFirstSentence = false;
    }
    public void SceneTransPanelActivation(bool _active)
    {
        if (_active)
        {
            sceneTransPanel.gameObject.SetActive(true);
            string message = tutorialEndMessage.text;
            tutorialEndMessage.text = "";
            sceneTransPanel.DOLocalMove(new Vector3(169, 2383, 0),5).OnComplete(() =>
            {
                tutorialEndMessage.GetComponent<CanvasGroup>().DOFade(1, 1.4f).OnComplete(() =>
                {
                    StartCoroutine(textWaiting(message, tutorialEndMessage));
                });
            });
        }
        else
        {
            sceneTransPanel.gameObject.SetActive(false);
        }
    }
    private IEnumerator textWaiting(string sentence, Text text)
    {
        foreach (char letter in sentence.ToCharArray())
        {
            text.text += letter;
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForEndOfFrame();
        FirebaseAuthManager.instance.CreateNewLoading();
    }
}
[System.Serializable]
public class Dialog
{
    [TextArea(3, 10)]
    public string Sentence;
    public Dialog(string _sentence)
    {
        Sentence = _sentence;
    }
}
public enum Steps
{
    Step1,
    Step2,
    Step3,
    Step4,
    Step5,
    Step6,
    Step7,
    Step8,
    Step9,
    Step10,
    Step11,
    Step12,
    Step13,
    Step14,
    Step15
}
