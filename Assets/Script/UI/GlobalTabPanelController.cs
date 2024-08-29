using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalTabPanelController : MonoBehaviour
{
    private void Start()
    {
        int length = transform.childCount;
        for (int i = 0; i < length; i++)
        {
            if (transform.GetChild(i).TryGetComponent(out Button button))
            {
                button.onClick.AddListener(() => InteractableChanged(button.transform.GetSiblingIndex()));
            }
        }
    }
    private void OnEnable()
    {
        Button firstButton = null;
        int length = transform.childCount;
        for (int i = 0; i < length; i++)
            if (transform.GetChild(i).TryGetComponent(out Button button))
            {
                firstButton = button;
                break;
            }
        if (firstButton != null)
            InteractableChanged(firstButton.transform.GetSiblingIndex());
    }
    public void InteractableChanged(int _siblingIndex)
    {
        Button clickedButton = transform.GetChild(_siblingIndex).GetComponent<Button>();

        int length = transform.childCount;
        for (int i = 0; i < length; i++)
            if(transform.GetChild(i).TryGetComponent(out Button button))
                button.interactable = true;

        clickedButton.interactable = false;
    }
}
