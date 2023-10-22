using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] GameObject pnlPicturesMenu;

    [Header("PictureInfos")]    
    
    [SerializeField] GameObject[] OpenStars;
    [SerializeField] GameObject[] CloseStars;
    [SerializeField] TextMeshProUGUI txtPainterName;
    [SerializeField] Image imgPicture;

    private PictureElement _LastSelectedPicture;


    [Header("MuseumInfos")]
    [SerializeField] GameObject pnlMuseumStats;
    [SerializeField] Button museumStatButton;
    [SerializeField] GameObject[] tabContents;
    [SerializeField] Button[] tabButtons;
    [SerializeField] Image[] tabButtonsIcons;
    [SerializeField] GameObject[] Pointers;
    public float ClickedFade = 1;
    public float OtherFade = 0.5f;
    public int currentTab = 0;

    [Header("General")]
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
    private void Start()
    {
        museumStatButton.onClick.AddListener(ShowMuseumStatsPanel);
        
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

    public void ShowTab(int tabIndex)
    {
        // Belirtilen sekme içeriðini etkinleþtir.
        Debug.Log(tabIndex);
        tabContents[tabIndex].SetActive(true);

        // Diðer sekmelerin içeriðini devre dýþý býrak.
        for (int i = 0; i < tabContents.Length; i++)
        {
            if (i != tabIndex)
            {
                tabContents[i].SetActive(false);
            }
        }

        // Belirtilen sekme düðmesini vurgula, diðerlerini vurgulamayý kaldýr.
        for (int i = 0; i < tabButtons.Length; i++)
        {
            if (i == tabIndex)
            {
                tabButtons[i].interactable = false;
                tabButtons[i].GetComponent<CanvasGroup>().alpha = ClickedFade;
            }
            else
            {
                tabButtons[i].interactable = true;
                tabButtons[i].GetComponent<CanvasGroup>().alpha = OtherFade;
            }
        }

        for (int i = 0; i < tabButtonsIcons.Length; i++)
        {
            if (i == tabIndex)
            {
                tabButtonsIcons[i].GetComponent<CanvasGroup>().alpha = ClickedFade;
            }
            else
            {
                tabButtonsIcons[i].GetComponent<CanvasGroup>().alpha = OtherFade;
            }
        }

        for (int i = 0; i < Pointers.Length; i++)
        {
            if (i != tabIndex)
            {
                Pointers[i].SetActive(false);
            }
        }

        currentTab = tabIndex;  // Þu anki sekme indeksi güncelle.
    }
    private void ShowMuseumStatsPanel()
    {
        if (!pnlMuseumStats.activeInHierarchy)
        {
            pnlMuseumStats.SetActive(true);        
            ShowTab(currentTab);
        }
        else
            pnlMuseumStats.SetActive(false);
    }
}
