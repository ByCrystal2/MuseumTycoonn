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
        // Çevirilecek metinleri topla
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

        Debug.Log("Tüm diyaloglar baþarýyla çevrildi.");
    }

    public void TriggerDialog(Steps _whichStep)
    {
        DialogHelper helper = helpers.Where(x => x.WhichStep == _whichStep).SingleOrDefault();
        currentStep = helper.WhichStep;

        // Çevrilmiþ diyaloglarla iþlemi baþlat
        DialogueManager.instance.SetCurrentDialogTrigger(this);
        DialogueManager.instance.SetCurrentDialogs(helper, helper.versionOfDiaPnl, Name);
        helper.EventToBeCovered.Invoke();
        helpers.Remove(helper);
    }
    int helperIndex;
    private void OnValidate()
    {
        for (int i = 0; i < helpers.Count; i++)
        {
            // Liste öðesini geçici bir deðiþkene atýyoruz
            var helper = helpers[i];
            //int index = i;
            // Enum deðerini doðru þekilde atamak için açýk bir dönüþüm yapýyoruz
            if (i < System.Enum.GetValues(typeof(Steps)).Length)
            {
                if (helpers[i].ID == helpers[0].ID)
                {
                    helperIndex = helpers[i].ID;
                }
                helper.WhichStep = helperIndex > 0 ? (Steps)helperIndex - 1 : (Steps)helperIndex;  // i deðerini Steps enum'ýna dönüþtürerek atama yapýyoruz
                helper.ID = helperIndex;
            }
            else
            {
                Debug.LogWarning("Enum Steps'deki eleman sayýsý helpers listesindeki eleman sayýsýndan az.");
            }

            // Deðiþtirilen öðeyi geri listeye atýyoruz
            helpers[i] = helper;
            helperIndex++;
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