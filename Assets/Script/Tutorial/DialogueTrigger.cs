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

    private bool isTranslateEnding;
    private void Awake()
    {
        if (GameManager.instance.IsWatchTutorial) return;
        isTranslateEnding = false;
        // Çevirilecek metinleri topla
        List<string> textsToTranslate = new List<string>();
        foreach (var helper in helpers)
        {
            foreach (var dialog in helper.Dialogs)
            {
                textsToTranslate.Add(dialog.Sentence);
            }
        }

        GameManager.instance.BulkTranslateAndAssignAsync(textsToTranslate, (translatedTexts) =>
        {
            int index = 0;
            foreach (var helper in helpers)
            {
                for (int i = 0; i < helper.Dialogs.Count; i++)
                {
                    if (index < translatedTexts.Count)
                    {
                        helper.Dialogs[i] = new Dialog(translatedTexts[index]);
                        index++;
                    }
                }
            }
            isTranslateEnding = true;
            Debug.Log("Tüm diyaloglar baþarýyla çevrildi.");
        });
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
}
[System.Serializable]
public struct DialogHelper
{
    public Vector3 TargetPos;
    [SerializeField] [Range(0, 2)] public int versionOfDiaPnl;
    [SerializeField] public bool placeArrowToRight;
    public List<Dialog> Dialogs;
    public Steps WhichStep;

    public UnityEvent EventToBeCovered;
    public UnityEvent EventEndingCovered;

    public UnityEvent EventForFocusedObjClickHandler;
}