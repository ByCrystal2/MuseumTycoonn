using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        
    }
    public void StartTutorial()//DialogueTrigger UnityEvents...
    {
        StartCoroutine(StartTutorialDialogue(currentHelper.Dialogs));
    }
    private IEnumerator StartTutorialDialogue(List<Dialog> _dialogs)
    {
        if (_dialogs.Count > 0)
        Debug.Log("Dialog Starting... First Dialog Message => " + _dialogs[0].Sentence);

        currentDialogPanel.dialogText.text = "";
        yield return new WaitForEndOfFrame();
        CinemachineTransition(true);
        //StartCoroutine(HoldAnimation(4, 1f,"startDialog",true));
        PlayerManager.instance.LockPlayer();
        UIController.instance.CloseJoystickObj(true);
        if (_dialogs.Count <= 0)
        {
            currentHelper.EventEndingCovered.Invoke();
            Debug.Log("Mevcut dialog sayisi 0'dir. Helperin EndingCovered Eventi devreye girdi.");
        }
        else
        {
            currentDialogPanel.gameObject.SetActive(true);
            StartCoroutine(WaitForTutorialBrainTranstionEnding(_dialogs));
        }        
    }
    IEnumerator WaitForTutorialBrainTranstionEnding(List<Dialog> _dialogs)
    {
        yield return new WaitForSeconds(2);
        SetActivationDialoguePanel(true);

        sentences.Clear();
        foreach (Dialog dialog in _dialogs)
        {
            sentences.Enqueue(dialog.Sentence);
        }

        DisplayNextSentence();
    }
    public async void EndTutorialDialogue() // unity event
    {
        GameManager.instance.IsWatchTutorial = true;
        GameManager.instance.IsFirstGame = false;
        SetActivationDialoguePanel(false);
        await DatabaseWaitingDatas();
        UIController.instance.tutorialUISPanel.gameObject.SetActive(false);
        currentTrigger.gameObject.SetActive(false);
        PlayerManager.instance.UnLockPlayer();
        UIController.instance.CloseJoystickObj(false);
        UIController.instance.StartRightPanelUISBasePosAnim(false);
        List<NPCBehaviour> nps = FindObjectsOfType<NPCBehaviour>().ToList();
        foreach (var npc in nps)
        {
            npc.enabled = true;
            npc.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;
        }
#if UNITY_EDITOR
        FirestoreManager.instance.UpdateGameData("ahmet123", true);
#else
        FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUser().UserId,true);        
#endif
        CinemachineTransition(false);
        //GameManager.instance.Save();
    }
    private async System.Threading.Tasks.Task DatabaseWaitingDatas()
    {
        ItemData firstTableForPlayer = new ItemData(99999, "Vincent van Gogh", "Hediye Tablo", 1, 0, null, ItemType.Table, ShoppingType.Gold, 1, 3);
        PictureData newDatabaseItem = new PictureData();
        newDatabaseItem.TextureID = firstTableForPlayer.textureID;
        newDatabaseItem.RequiredGold = GameManager.instance.PictureChangeRequiredAmount;
        newDatabaseItem.painterData = new PainterData(firstTableForPlayer.ID, firstTableForPlayer.Description, firstTableForPlayer.Name, firstTableForPlayer.StarCount);
        newDatabaseItem.isActive = true;
        newDatabaseItem.isFirst = false;
        newDatabaseItem.isLocked = false;
        newDatabaseItem.id = 208;
        string userID = "";
#if UNITY_EDITOR
        userID = "ahmet123";
#else
        userID = FirebaseAuthManager.instance.GetCurrentUser().UserId;
#endif
        await FirestoreManager.instance.pictureDatasHandler.AddPictureIdWithUserId(userID, newDatabaseItem);
        await FirestoreManager.instance.pictureDatasHandler.AddPictureIdWithUserId(userID, MuseumManager.instance.InventoryPictures.Where(x => x.painterData.ID == 9999).SingleOrDefault());
        List<RoomData> activeRoomDatas = RoomManager.instance.RoomDatas.Where(x => x.isActive).ToList();
        await FirestoreManager.instance.skillDatasHandler.AddSkillWithUserId(userID, SkillTreeManager.instance.skillNodes.Where(x=> x.ID == 0).SingleOrDefault());
        await FirestoreManager.instance.skillDatasHandler.AddSkillWithUserId(userID, SkillTreeManager.instance.skillNodes.Where(x=> x.ID == 1).SingleOrDefault());
        await FirestoreManager.instance.roomDatasHandler.AddRoomsWithUserId(userID, activeRoomDatas);
        await FirestoreManager.instance.workerDatasHandler.AddWorkerWithUserId(userID, MuseumManager.instance.CurrentActiveWorkers.FirstOrDefault().MyDatas);
        await FirestoreManager.instance.UpdateGameData(userID);
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
        yield return new WaitForSeconds(0.8f);
        SetActivationDialoguePanel(false);
        yield return new WaitForSeconds(0.4f);
        currentHelper.EventEndingCovered.Invoke();
        //EndTutorialDialogue();
    }
    public void NextDialogueTriggerOverride()
    {
        Debug.Log("NextDialogueTriggerOverride Step => " + ((int)currentTrigger.currentStep + 1));
        currentTrigger.TriggerDialog(currentTrigger.currentStep + 1);
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
    Step15,
    Step16,
    Step17,
    Step18,
    Step19,
    Step20,
    Step21,
    Step22,
    Step23,
    Step24,
    Step25,
    Step26,
    Step27,
    Step28,
    Step29,
    Step30,
    Step31,
    Step32,
    Step33,
    Step34,
    Step35,
    Step36,
    Step37,
    Step38,
    Step39,
    Step40,
    Step41,
    Step42,
    Step43,
    Step44,
    Step45,
    Step46,
    Step47,
    Step48,
    Step49,
    Step50,
}
