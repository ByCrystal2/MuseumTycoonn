using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalTabPanelController : MonoBehaviour
{
    private Button previousButton = null;

    private void Start()
    {
        int length = transform.childCount;
        for (int i = 0; i < length; i++)
        {
            int siblingIndex = i; // Yerel deðiþken kullanýyoruz
            if (transform.GetChild(i).TryGetComponent(out Button button))
            {
                Debug.Log("Button.name => " + button.name);
                button.onClick.AddListener(() => InteractableChanged(siblingIndex));
            }
        }
    }

    private void OnEnable()
    {
        Button firstButton = null;
        int length = transform.childCount;
        for (int i = 0; i < length; i++)
        {
            if (transform.GetChild(i).TryGetComponent(out Button button))
            {
                firstButton = button;
                break;
            }
        }

        if (firstButton != null)
            InteractableChanged(firstButton.transform.GetSiblingIndex());
    }

    public void InteractableChanged(int siblingIndex)
    {
        Debug.Log("Intertable Chaned. SiblingIndex is " + siblingIndex);
        Button clickedButton = transform.GetChild(siblingIndex).GetComponent<Button>();

        // Eðer önceki bir buton seçiliyse, tekrar aktif hale getiriyoruz
        if (previousButton != null)
            previousButton.interactable = true;

        // Yeni týklanan butonun etkileþimini kapatýyoruz
        clickedButton.interactable = false;

        // Yeni týklanan butonu önceki buton olarak kaydediyoruz
        previousButton = clickedButton;
    }
}
