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
    public void TriggerDialog(Steps _whichStep)
    {
        DialogHelper helper = helpers.Where(x=> x.WhichStep == _whichStep).SingleOrDefault();
        DialogueManager.instance.SetCurrentDialogs(helper);
        helper.EventToBeCovered.Invoke();
    }
}
[System.Serializable]
public struct DialogHelper
{
    public List<Dialog> Dialogs;
    public Steps WhichStep;

    public UnityEvent EventToBeCovered;
    public UnityEvent EventEndingCovered;
}