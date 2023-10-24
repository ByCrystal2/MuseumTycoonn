using DG.Tweening;
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

    [Header("MuseumTabs")]
    [SerializeField] Transform shopParent;
    [SerializeField] GameObject ShopPrefab;

    [Header("General Infos")]
    //Left
    [SerializeField] GameObject commentPrefab;
    [SerializeField] Transform commentParent;    
    //Right
    [SerializeField] Text TotalVisitorsCommentCount;
    [SerializeField] Text DailyVisitorCount;
    [SerializeField] Text TotalHappinessScore;
    [SerializeField] Text DailyEarnings;
    [SerializeField] Text CultureLevelInGlobal;

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
    bool shopCreated = false;
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
        
        if (tabIndex == 2)
        {
            if (!shopCreated)
            {
                newShop = SetCreateGameobject(ShopPrefab, shopParent);
                spiderParent = newShop.transform.GetChild(0).transform;
                shopCreated = true;
                newSpider = SetCreateGameobject(SpiderPrefab, spiderParent);
                spiderOffset = newSpider.transform.position;
            }            
            
            SpiderMove(newSpider);
        }
        else
        {
            if (newSpider != null)
            {
                spiderTween.Kill(true);
                spiderTween2.Kill(true);
                newSpider.transform.position = spiderOffset;
            }
        }

        if (tabIndex == 0)
        {

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
            else
            {
                Pointers[i].SetActive(true);
            }
        }

        currentTab = tabIndex;  // Þu anki sekme indeksi güncelle.
    }

    public void InMuseumCurrentNPCCountChanged(int _visitorCount)
    {
        DailyVisitorCount.text = _visitorCount.ToString();
    }

    public void InMuseumDailyEarningChanged(float _earning)
    {
        DailyEarnings.text = _earning.ToString();
    }

    public void TotalVisitorsCommentCountChanged(float _commentCount)
    {
        TotalVisitorsCommentCount.text = _commentCount.ToString();
    }

    public void CultureLevelCountChanged(int _levelCount)
    {
        CultureLevelText.text = _levelCount.ToString();
    }

    public void AddCommentInGlobalTab(Sprite _profilPic, string _npcName, string _npcMessage, string _currentDate)
    {
        GameObject newComment = Instantiate(commentPrefab, commentParent);
        newComment.transform.GetChild(1).GetComponent<Image>().sprite = _profilPic;
        newComment.transform.GetChild(2).GetComponent<Text>().text = _npcName;
        newComment.transform.GetChild(3).GetComponent<Text>().text = _npcMessage;
        newComment.transform.GetChild(4).GetComponent<Text>().text = _currentDate;
        Debug.Log("Global Stats'a Yorum Eklendi.");
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

    [SerializeField] GameObject SpiderPrefab;
    [SerializeField] Transform spiderParent;
    Tween spiderTween;
    Tween spiderTween2;
    GameObject newSpider;
    GameObject newShop;
    Vector3 spiderOffset;
    float swingDuration = 10f;
    float swingHeight = 1f;
    public void SpiderMove(GameObject _newSpider)    
    {        
        if (_newSpider != null)
        {
            // Ýlk pozisyon
            Vector3 startPos = spiderOffset;

            // Ýlk sallama konumu
            Vector3 midPos = startPos + new Vector3(0,-500,0) * swingHeight;
            Vector3 endPos = midPos + new Vector3(0, 100, 0);

            // Ýlk sallama
            spiderTween = _newSpider.transform.DOMove(midPos, swingDuration / 2).SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                // Ýkinci sallama ve baþlangýç konumuna dönme
                spiderTween2 = _newSpider.transform.DOMove(endPos, swingDuration / 2).SetEase(Ease.InOutQuad);                    
            }); ;
        }
    }

    public GameObject SetCreateGameobject(GameObject go, Transform parent)
    {
        if (go != null && parent != null )
        {
            GameObject obj = Instantiate(go, parent);
            return obj;
        }
        return new GameObject("Null Obj");
    }
}
