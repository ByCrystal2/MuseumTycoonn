using HeneGames.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class DialogueTrigger : MonoBehaviour
{
    public string Name;
    public List<DialogHelper> helpers = new List<DialogHelper>();
    public Steps currentStep;

    //private bool isTranslateEnding;
    private void Awake()
    {
        if (GameManager.instance.IsWatchTutorial) return;
        //isTranslateEnding = false;
        // �evirilecek metinleri topla
        AddTranslatedMessagesInDialogs();
    }
    void AddTranslatedMessagesInDialogs()
    {
        if (!LanguageDatabase.instance.TranslationWillBeProcessed) return;
        List<LanguageData> dialogLanguageDatas = LanguageDatabase.instance.Language.DialogsMessages;

        foreach (var helper in helpers)
        {
            int helperID = helper.ID;
            List<LanguageData> targetLanguageDatas = dialogLanguageDatas.Where(x => x.TargetID == helperID).ToList();

            for (int i = 0; i < helper.Dialogs.Count; i++)
            {
                if (i < targetLanguageDatas.Count)
                {
                    helper.Dialogs[i] = new Dialog(targetLanguageDatas[i].ActiveLanguage);
                }
            }
        }

        Debug.Log("T�m diyaloglar ba�ar�yla �evrildi.");
    }

    public void TriggerDialog(Steps _whichStep)
    {
        DialogHelper helper = helpers.Where(x => x.WhichStep == _whichStep).SingleOrDefault();
        currentStep = helper.WhichStep;

        // �evrilmi� diyaloglarla i�lemi ba�lat
        DialogueManager.instance.SetCurrentDialogTrigger(this);
        DialogueManager.instance.SetCurrentDialogs(helper, helper.versionOfDiaPnl, Name);
        helper.EventToBeCovered.Invoke();
        helpers.Remove(helper);
    }
    private void OnValidate()
    {
        for (int i = 0; i < helpers.Count; i++)
        {
            // Liste ��esini ge�ici bir de�i�kene at�yoruz
            var helper = helpers[i];

            // Enum de�erini do�ru �ekilde atamak i�in a��k bir d�n���m yap�yoruz
            if (i < System.Enum.GetValues(typeof(Steps)).Length)
            {
                helper.WhichStep = (Steps)i;  // i de�erini Steps enum'�na d�n��t�rerek atama yap�yoruz
            }
            else
            {
                Debug.LogWarning("Enum Steps'deki eleman say�s� helpers listesindeki eleman say�s�ndan az.");
            }

            // De�i�tirilen ��eyi geri listeye at�yoruz
            helpers[i] = helper;
        }
    }
}
[System.Serializable]
public struct DialogHelper
{
    public int ID;
    public Vector3 TargetPos;
    [SerializeField] [Range(0, 2)] public int versionOfDiaPnl;
    [SerializeField] public bool placeArrowToRight;
    public List<Dialog> Dialogs;
    public Steps WhichStep;

    public UnityEvent EventToBeCovered;
    public UnityEvent EventEndingCovered;

    public UnityEvent EventForFocusedObjClickHandler;
}