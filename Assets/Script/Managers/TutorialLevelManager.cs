using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLevelManager : MonoBehaviour
{
    private void Start()
    {
        DialogueTrigger kingTrigger = GameObject.FindWithTag("TutorialNPC").GetComponent<DialogueTrigger>();
        kingTrigger.TriggerDialog(Steps.Step1);
    }
}
