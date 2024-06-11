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
    public void TriggerDialog(Steps _whichStep)
    {
        DialogHelper helper = helpers.Where(x=> x.WhichStep == _whichStep).SingleOrDefault();
        currentStep = helper.WhichStep;
        DialogueManager.instance.SetCurrentDialogTrigger(this);
        DialogueManager.instance.SetCurrentDialogs(helper,helper.versionOfDiaPnl,Name);
        helper.EventToBeCovered.Invoke();
        helpers.Remove(helper);
    }
}
[System.Serializable]
public struct DialogHelper
{
    [SerializeField] [Range(0, 2)] public int versionOfDiaPnl;

    public List<Dialog> Dialogs;
    public Steps WhichStep;

    public UnityEvent EventToBeCovered;
    public UnityEvent EventEndingCovered;

    public UnityEvent EventForFocusedObjClickHandler;
}