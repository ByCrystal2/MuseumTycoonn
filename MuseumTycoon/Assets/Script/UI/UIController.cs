using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class UIController : MonoBehaviour
{
    

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
    [SerializeField] Text InMuseumCurrentNPCCount;
    [SerializeField] Text TotalHappinessScore;
    [SerializeField] Text DailyEarnings;
    [SerializeField] Text CultureLevelInGlobal;

    [Header("Skill Tree")]
    public GameObject skillsContent;
    public GameObject skillInfoPanel;
    public TextMeshProUGUI SkillPointText;
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillDescriptionText;
    public TextMeshProUGUI skillEffectText;
    public TextMeshProUGUI skillRequiredPointText;
    public TextMeshProUGUI skillRequiredMoneyText;
    public Button unlockButton;

    [Header("Skill Tree RequiredPanel")]
    public GameObject SkillRequiredInfoPanel;
    public GameObject RequiredPoint;
    public GameObject RequiredMoney;
    public GameObject YeterliPoint;
    public GameObject YeterliMoney;

    

    public TextMeshProUGUI RequiredPointText;
    public TextMeshProUGUI RequiredMoneyText;
    [Header("NPC InformationPanel")]
    public GameObject NpcInformationPanel;
    [SerializeField] private Button NpcInfoPanelExitButton;

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
        SkillRequiredInfoPanel.SetActive(false);
        GoldText.text = "" + MuseumManager.instance.GetCurrentGold();
        GemText.text = "" + MuseumManager.instance.GetCurrentGem();
        CultureLevelText.text = "" + MuseumManager.instance.GetCurrentCultureLevel();
    }
    private void Start()
    {
        museumStatButton.onClick.AddListener(ShowMuseumStatsPanel);
        unlockButton.onClick.AddListener(BuySkill);
        NpcInfoPanelExitButton.onClick.AddListener(CloseNPCInformationPanel);
        MuseumManager.instance.CalculateAndAddTextAllInfos();
    }
    private void Update()
    {
        /* if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            // Fare týklanýrsa panelin etrafýna týklanýp týklanmadýðýný kontrol et.
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject != pnlPicturesMenu)
                {
                    // Panel dýþýna týklanýrsa paneli kapat.
                    pnlPicturesMenu.SetActive(false);
                }
            }
        }
        */
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
        //PainterData newPainter = Painter.instance.GetPainter(GetDropDownSelectedPainter());
        //if (newPainter == null)
        //{
        //    Debug.Log("Null Painter");
        //    return;
        //}
        //txtPainterName.text = newPainter.Name;
        //imgPicture.sprite = newPainter.Picture;
        //for (int i = 0; i < newPainter.StarCount; i++)
        //{
        //    OpenStars[i].gameObject.SetActive(true);
        //}
    }

    public void SetSelectedPicture(int _id)
    {
        _LastSelectedPicture._pictureData.TextureID = _id;
        _LastSelectedPicture.UpdateVisual();
    }
    
    bool shopCreated = false;
    public void ShowTab(int tabIndex)
    {
        StopCoroutine("SetActiveFalseWaiter");
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

        if (tabIndex == 1)
        {
            skillInfoPanel.SetActive(false);
            SkillRequiredInfoPanel.SetActive(false);
        }
        
        if (tabIndex == 2)
        {            
            if (!shopCreated)
            {
                Button allButton, gemButton, goldButton, tableButton;
                newShop = SetCreateGameobject(ShopPrefab, shopParent);
                allButton = newShop.transform.GetChild(0).transform.GetChild(0).GetComponent<Button>();
                gemButton = newShop.transform.GetChild(0).transform.GetChild(1).GetComponent<Button>();
                goldButton = newShop.transform.GetChild(0).transform.GetChild(2).GetComponent<Button>();
                tableButton = newShop.transform.GetChild(0).transform.GetChild(3).GetComponent<Button>();
                allButton.onClick.AddListener(ShopController.instance.GetAllItemsUpdate);
                gemButton.onClick.AddListener(ShopController.instance.GetGemItems);
                goldButton.onClick.AddListener(ShopController.instance.GetGoldItems);
                tableButton.onClick.AddListener(ShopController.instance.GetTableItems);
                
                spiderParent = newShop.transform.GetChild(1).transform;
                shopCreated = true;
                newSpider = SetCreateGameobject(SpiderPrefab, spiderParent);
                spiderOffset = newSpider.transform.position;

                ShopController.instance.GetAllItemsUpdate();
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
    public void ShowSkillInfo(int _id) // Skill Buttons
    {
        skillInfoPanel.SetActive(false);
        
        SkillNode selectedSkill = SkillTreeManager.instance.GetSelectedSkillNode(_id);
        

        SkillTreeManager.instance.SelectedSkill = selectedSkill;
        SkillTreeManager.instance.SelectedSkillGameObject = EventSystem.current.currentSelectedGameObject;
        Debug.Log("Selected Skill Gameobject => " + SkillTreeManager.instance.SelectedSkillGameObject.gameObject);
        if (SkillTreeManager.instance.SelectedSkillGameObject != null)
        {
            skillInfoPanel.transform.position = SkillTreeManager.instance.SelectedSkillGameObject.transform.position;
        }
        // Yetenek adý, açýklama ve etkiyi güncelle
        skillNameText.text = selectedSkill.SkillName;
        skillDescriptionText.text = selectedSkill.SkillDescription;
        skillEffectText.text = selectedSkill.SkillEffect;
        if (selectedSkill.SkillCurrentLevel < selectedSkill.SkillMaxLevel)
        {
            skillRequiredPointText.text = "Requires Point: " + selectedSkill.SkillRequiredPoint;
            skillRequiredMoneyText.text = "$" + selectedSkill.SkillRequiredMoney;
        }
        else
        {
            skillRequiredPointText.text = "";
            skillRequiredMoneyText.text = "";
        }

        SkillTreeManager.instance.CalculateForCurrentSkillEnoughLevelAndMoney(selectedSkill);
        // Kilidi açýlabilir veya açýlamazsa düðmeyi güncelle
        skillInfoPanel.SetActive(true);
        if (selectedSkill.IsPurchased && selectedSkill.SkillCurrentLevel == selectedSkill.SkillMaxLevel)
        {
            ChangedUnlockButton(false, "Max Seviye", Color.white);
            SkillRequiredInfoPanel.SetActive(false);
            return;
        }
        SkillUnLockButtonControl(selectedSkill);
        UpdateSkillInfos(selectedSkill);
    }

    public void BuySkill() // SKILL SATIN ALMA BUTONU!
    {
        SkillNode currentSkill = SkillTreeManager.instance.SelectedSkill;
        MuseumManager.instance.AddSkillPoint(10);  // TEHLIKELI KOD! TESTTEN SONRA SILINMELIDIR!
        if (MuseumManager.instance.GetCurrentGold() >= currentSkill.SkillRequiredMoney && MuseumManager.instance.GetCurrentSkillPoint() >= currentSkill.SkillRequiredPoint)
        {
            if (currentSkill.SkillCurrentLevel < currentSkill.SkillMaxLevel)
            {
                MuseumManager.instance.SpendingGold(currentSkill.SkillRequiredMoney);
                
                SkillTreeManager.instance.SelectedSkillGameObject.GetComponentInChildren<SkillAbilityAmountController>().IncreasingAbilityAmount(); //Skill Ability Amount Arttirma.
                
                
                SkillPointText.text = "" + (MuseumManager.instance.GetCurrentSkillPoint() - currentSkill.SkillRequiredPoint);
                MuseumManager.instance.SpendingSkillPoint(currentSkill.SkillRequiredPoint);
                SkillRequiredPointController(currentSkill);
                currentSkill.SkillCurrentLevel++;
                if (currentSkill.SkillCurrentLevel < currentSkill.SkillMaxLevel)
                {
                    skillNameText.text = currentSkill.SkillName;
                    skillDescriptionText.text = currentSkill.SkillDescription;
                    skillEffectText.text = currentSkill.SkillEffect;
                    skillRequiredPointText.text = "Requires Point: " + currentSkill.SkillRequiredPoint;
                    skillRequiredMoneyText.text = "$" + currentSkill.SkillRequiredMoney;
                }
                else
                {
                    skillRequiredPointText.text = "";
                    skillRequiredMoneyText.text = "";
                }
                
                

                
                Debug.Log($"Skill => {currentSkill.SkillName} Yetenek leveli => {currentSkill.SkillCurrentLevel} Yetenek Puaný => {currentSkill.SkillRequiredPoint}");

                
                if (currentSkill.SkillCurrentLevel > currentSkill.SkillMaxLevel)
                {
                    currentSkill.SkillCurrentLevel = currentSkill.SkillMaxLevel;
                }
                if (currentSkill.SkillCurrentLevel == currentSkill.SkillMaxLevel)
                {
                    ChangedUnlockButton(false, "Max Seviye", Color.white);
                }

                currentSkill.Purchased(true);
                
                SkillTreeManager.instance.RefreshSkillBonuses();
            }
            else
            {
                Debug.Log("Skill Max Level! => Current Skill Level: " + currentSkill.SkillCurrentLevel+"/"+currentSkill.SkillMaxLevel);
            }
        }
        else
        {
            Debug.Log($"Skill satin almak icin gerekli; paraniz ({MuseumManager.instance.GetCurrentGold()} => $:{currentSkill.SkillRequiredMoney}) ve/veya yetenek puaniniz ({MuseumManager.instance.GetCurrentSkillPoint()} => P:{currentSkill.SkillRequiredPoint} mevcut degildir.");
        }
        UIChangesControl();
    }
    
    private void SkillRequiredPointController(SkillNode _skill)
    {
        _skill.SkillRequiredPoint += SkillCurrentLevelControl(_skill.SkillCurrentLevel); // Base Value => 1
    }

    private int SkillCurrentLevelControl(int _skillLevel)
    {
        if (_skillLevel == 1)
        {
            return 1;
        }
        else if (_skillLevel == 2)
        {
            return 2;
        }        
        else if (_skillLevel == 4)
        {
            return 3;
        }
        else if (_skillLevel == 6)
        {
            return 4;
        }
        else if (_skillLevel == 9)
        {
            return 5;
        }
        else if (_skillLevel == 11)
        {
            return 6;
        }
        else if (_skillLevel == 14)
        {
            return 7;
        }
        else
        {
            return 0;
        }

    }
    public void ChangedUnlockButton(bool _interactable, string _text, Color _color)
    {
        unlockButton.interactable = _interactable;
        Text unlockBtnText = unlockButton.GetComponentInChildren<Text>();
        unlockBtnText.text = _text;
        unlockBtnText.color = _color;
    }
    public void UIChangesControl()
    {
        SkillNode currentSkill = SkillTreeManager.instance.SelectedSkill;
        if (currentSkill.IsPurchased && currentSkill.SkillCurrentLevel == currentSkill.SkillMaxLevel)
        {
            ChangedUnlockButton(false, "Max Seviye", Color.white);
            return;
        }
        else if (currentSkill.IsPurchased && currentSkill.SkillCurrentLevel != currentSkill.SkillMaxLevel && MuseumManager.instance.GetCurrentSkillPoint() >=currentSkill.SkillRequiredPoint && MuseumManager.instance.GetCurrentGold() >= currentSkill.SkillRequiredMoney)
        {
            ChangedUnlockButton(true, "Seviye Arttýr", Color.green);
            return;
        }
        else if (currentSkill.IsPurchased && currentSkill.SkillCurrentLevel != currentSkill.SkillMaxLevel && MuseumManager.instance.GetCurrentSkillPoint() < currentSkill.SkillRequiredPoint)
        {
            ChangedUnlockButton(false, "Kilitli", Color.red);
            return;
        }
        SkillTreeManager.instance.CalculateForCurrentSkillEnoughLevelAndMoney(currentSkill);
        SkillUnLockButtonControl(currentSkill);
        UpdateSkillInfos(currentSkill);
    }
    public void SkillUnLockButtonControl(SkillNode _selectedSkill)
    {
        
        if (!_selectedSkill.IsLocked &&_selectedSkill.SkillCurrentLevel != _selectedSkill.SkillMaxLevel)
        {
            ChangedUnlockButton(true, "Seviye Arttýr", Color.green);
        }
        else
        {
            ChangedUnlockButton(false, "Kilitli", Color.red);
        }
    }

    public void UpdateSkillInfos(SkillNode _selectedSkill)
    {
        if (skillInfoPanel.activeInHierarchy)
        {
            if (_selectedSkill.SkillRequiredPoint > MuseumManager.instance.GetCurrentSkillPoint())
            {
                RequiredPointText.text = "+" + (-(MuseumManager.instance.GetCurrentSkillPoint() - _selectedSkill.SkillRequiredPoint));
                RequiredPoint.SetActive(true);
                YeterliPoint.SetActive(false);
                SkillRequiredInfoPanel.SetActive(true);
            }
            else
            {
                RequiredPoint.SetActive(false);
                YeterliPoint.SetActive(true);
            }

            if (_selectedSkill.SkillRequiredMoney > MuseumManager.instance.GetCurrentGold())
            {
                RequiredMoneyText.text = "$" + (-(MuseumManager.instance.GetCurrentGold() - _selectedSkill.SkillRequiredMoney));
                RequiredMoney.SetActive(true);
                YeterliMoney.SetActive(false);
                SkillRequiredInfoPanel.SetActive(true);                
            }
            else
            {
                RequiredMoney.SetActive(false);
                YeterliMoney.SetActive(true);
            }

            if ((_selectedSkill.SkillRequiredPoint <= MuseumManager.instance.GetCurrentSkillPoint() && _selectedSkill.SkillRequiredMoney <= MuseumManager.instance.GetCurrentGold()))
            {
                StartCoroutine(SetActiveFalseWaiter(1.5f, SkillRequiredInfoPanel));                
            }
        }
    }

    IEnumerator SetActiveFalseWaiter(float _duration, GameObject _go)
    {        
        yield return new WaitForSeconds(_duration);
        _go.SetActive(false);
    }
    //public void StartShowSkillInfoPress() // Event Trigger / Down
    //{

    //}
    //public void EndShowSkillInfoPress() // Event Trigger / Up
    //{

    //}
    public void InMuseumCurrentNPCCountChanged(int _currentVisitorCount)
    {
        InMuseumCurrentNPCCount.text = _currentVisitorCount.ToString();
    }

    public void DailyVisitorCountChanged(int _dailyVisitorCount)
    {
        DailyVisitorCount.text = _dailyVisitorCount.ToString();
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
        CultureLevelInGlobal.text = _levelCount.ToString();
    }
    public void SkillPointCountChanged(float _pointCount)
    {
        SkillPointText.text = _pointCount.ToString();
    }

    public void CurrentTotalHappinessChanged(float _happiness)
    {
        TotalHappinessScore.text = "%" + _happiness.ToString();
    }
    public void AddCommentInGlobalTab(Sprite _profilPic, string _npcName, string _npcMessage, string _currentDate)
    {
        GameObject newComment = Instantiate(commentPrefab, commentParent);
        newComment.transform.GetChild(1).GetComponent<Image>().sprite = _profilPic;
        newComment.transform.GetChild(2).GetComponent<Text>().text = _npcName;
        newComment.transform.GetChild(3).GetComponent<Text>().text = _currentDate;
        newComment.transform.GetChild(4).GetComponent<Text>().text = _npcMessage;
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
    
    public void CloseNPCInformationPanel() //Button!
    {
        NpcInformationPanel.SetActive(false);
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
