using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] GameObject PlayerCinemachineBrain;
    [SerializeField] GameObject TutorialCinemachineBrain;

    // UI
    [SerializeField] GameObject DialoguePanel;
    public Text nameText;
    public Text dialogText;
    public Animator tutorialNPCanimator;
    // UI

    private Queue<string> sentences;

    public static DialogueManager instance { get; private set; }
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        sentences = new Queue<string>();
        Dialog d1 = GameObject.FindWithTag("TutorialNPC").GetComponent<DialogueTrigger>().dialog;
        StartTutorialDialogue(d1);
    }
    public void StartTutorialDialogue(Dialog _dialog)
    {
        CinemachineTransition(true);
        StartCoroutine(HoldAnimation(4, 1f,"startDialog",true));
        PlayerManager.instance.LockPlayer();
        UIController.instance.CloseJoystickObj(true);
        StartCoroutine(WaitForTutorialBrainTranstionEnding(_dialog));
    }
    IEnumerator WaitForTutorialBrainTranstionEnding(Dialog _dialog)
    {
        yield return new WaitForSeconds(3);
        SetActivationDialoguePanel(true);

        sentences.Clear();
        foreach (string sentence in _dialog.sentences)
        {
            sentences.Enqueue(sentence);
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
    private void SetActivationDialoguePanel(bool _active)
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
        dialogText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForEndOfFrame();
        }
        waitFirstSentence = false;
    }
}
[System.Serializable]
public class Dialog
{
    public string name;
    [TextArea(3, 10)]
    public string[] sentences;

    public Dialog(string _name, string[] _sentences)
    {
        name = _name;
        sentences = _sentences;
    }
}
