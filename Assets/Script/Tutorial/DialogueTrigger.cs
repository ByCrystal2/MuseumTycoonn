using HeneGames.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialog dialog;
    public void TriggerDialog()
    {
        DialogueManager.instance.StartTutorialDialogue(dialog);
    }
}
