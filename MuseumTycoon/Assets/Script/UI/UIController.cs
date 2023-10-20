using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] GameObject pnlPicturesMenu;

    [Header("PainterInfos")]
    [SerializeField] GameObject[] OpenStars;
    [SerializeField] GameObject[] CloseStars;
    [SerializeField] TextMeshProUGUI txtPainterName;
    [SerializeField] Image imgPicture;

    private PictureElement _LastSelectedPicture;

    public Image CultureFillBar;
    public Text CultureLevelText, GoldText, GemText;

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
        GemText.text = "0";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetClickedPicture(bool active, PictureElement _lastSelectedPicture) 
    {
        _LastSelectedPicture = _lastSelectedPicture;
        SetSelectedPicture(0);
        //pnlPicturesMenu.SetActive(active);
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
        imgPicture.sprite = newPainter.Picture;
        for (int i = 0; i < newPainter.StarCount; i++)
        {
            OpenStars[i].gameObject.SetActive(true);
        }
    }

    public void SetSelectedPicture(int _id)
    {
        _LastSelectedPicture.data = MuseumManager.instance.MyPictures[Random.Range(0, MuseumManager.instance.MyPictures.Count)];
        _LastSelectedPicture.UpdateVisual();
    }
}
