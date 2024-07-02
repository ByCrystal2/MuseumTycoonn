using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class FocusedObjClickHandler : MonoBehaviour, IPointerClickHandler
{
    private UnityEvent targetMedhodProcess;
    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        targetMedhodProcess.Invoke();
        Debug.Log("TargetMethod tetiklendi.");
        Component destroyComp = gameObject.GetComponent<FocusedObjClickHandler>();
        UIController.instance.tutorialUISPanel.HideHighlight();
        if (DialogueManager.instance.currentTrigger.helpers.Count <= 0)
            Debug.Log(DialogueManager.instance.currentTrigger.Name + " adli diaogcunun dialoglarý sonlandi...");
        else
        {
            DialogueManager.instance.currentTrigger.TriggerDialog(DialogueManager.instance.currentTrigger.currentStep + 1);
            Debug.Log("Siradaki step calisiyor: Step" + (int)(DialogueManager.instance.currentTrigger.currentStep + 1));
        }
        Destroy(destroyComp);
    }
    public void AddTargetEvent(UnityEvent _event)
    { // Gonderilen method evente baglandi. (Event cagrildigi zaman gonderilen method islenecek.)
        targetMedhodProcess = _event;
    }
}
