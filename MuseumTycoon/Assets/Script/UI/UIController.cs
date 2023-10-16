using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] GameObject pnlPicturesMenu;

    [Header("PainterInfos")]
    [SerializeField] GameObject[] OpenStars;
    [SerializeField] GameObject[] CloseStars;
    [SerializeField] TextMeshProUGUI txtPainterName;
    
    public static UIController instance { get; private set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetClickedPicture(bool active) 
    {
        pnlPicturesMenu.SetActive(active);
    }

    public string GetDropDownSelectedPainter()
    {
        TMP_Dropdown dropdown = GameObject.FindObjectOfType<TMP_Dropdown>();
        int selectedOptionIndex = dropdown.value;
        string selectedOptionText = dropdown.options[selectedOptionIndex].text;

        return selectedOptionText;
    }

    public void DropDownValueChanged() // Dropdown*
    {
        PainterData newPainter = Painter.instance.GetPainter(GetDropDownSelectedPainter());
        if (newPainter == null)
        {
            Debug.Log("Null Painter");
            return;
        }
        txtPainterName.text = newPainter.Name;
        for (int i = 0; i < newPainter.StarCount; i++)
        {
            OpenStars[i].gameObject.SetActive(true);
        }
    }
}
