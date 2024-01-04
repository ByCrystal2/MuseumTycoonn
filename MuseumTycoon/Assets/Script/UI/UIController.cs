using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    //UI
    [SerializeField] private TextMeshProUGUI txtFullName;
    [SerializeField] private TextMeshProUGUI txtHappiness;
    [SerializeField] private TextMeshProUGUI txtStress;
    [SerializeField] private TextMeshProUGUI txtToilet;
    [SerializeField] private TextMeshProUGUI txtEducation;
    [SerializeField] private List<Image> LikedColorImages;
    [SerializeField] private List<TextMeshProUGUI> ArtistTexts;

    [Header("Worker Market Panel")]
    public Transform WorkerContent; 
    public GameObject WorkerPrefabV2_V1;
    //UI
    public GameObject WorkerPanel;
    private Vector3 WorkerPanelDefaultPos;
    public Button WorkerPanelOnButton;
    public Button btnSecurityTab;
    public Button btnHouseKeeperTab;
    public Button btnMusicianTab;
    public Button btnReceptionistTab;
    public Button btnBrochureSellerTab;

    [Header("Worker Assignment Panel")]    
    //UI
    [SerializeField] private GameObject WorkerAssignmentPanel;
    [SerializeField] private Button WorkerAssignmentPanelOnButton;
    // => AssignmentRooms
    [SerializeField] private Transform WorkerAssignContent;
    [SerializeField] private GameObject AssignmentRoomPrefab_V1;

    // => WorkerInventory
    [SerializeField] private Transform WorkerInventoryContent;
    [SerializeField] private GameObject InventoryWorkerPrefab_V1;

    [SerializeField] private Button W_InventoryTabSecurity;
    [SerializeField] private Button W_InventoryTabHouseKeeper;
    [SerializeField] private Button W_InventoryTabMusician;
    [SerializeField] private Button W_InventoryTabReceptionist;
    [SerializeField] private Button W_InventoryTabBrochureSeller;
    [Header("General")]
    public Image CultureFillBar;
    public Text CultureLevelText, GoldText, GemText;
    //UI
    [SerializeField] private Transform ActivePnlBtnBookDefaultPos;
    [SerializeField] private Transform ActivePnlBtnWorkerMarketDefaultPos;
    [SerializeField] private Transform ActivePnlBtnWorkerAssignmentDefaultPos;
    private Vector3 defaultBtnBookPos, defaultBtnWorkerMarketPos, defaultBtnWorkerAssignmentPos;
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
        WorkerPanelDefaultPos = WorkerContent.position;
    }
    private void Start()
    {
        defaultBtnBookPos = museumStatButton.transform.position;
        defaultBtnWorkerMarketPos = WorkerPanelOnButton.transform.position;
        defaultBtnWorkerAssignmentPos = WorkerAssignmentPanelOnButton.transform.position;
        museumStatButton.onClick.AddListener(ShowMuseumStatsPanel);
        unlockButton.onClick.AddListener(BuySkill);
        NpcInfoPanelExitButton.onClick.AddListener(CloseNPCInformationPanel);

        // WorkerMarket
        WorkerPanelOnButton.onClick.AddListener(AddWorkersInContent);
        btnSecurityTab.onClick.AddListener(() => GetDesiredWorkersInContent(WorkerType.Security,btnSecurityTab));
        btnHouseKeeperTab.onClick.AddListener(() => GetDesiredWorkersInContent(WorkerType.Housekeeper, btnHouseKeeperTab));
        btnMusicianTab.onClick.AddListener(() => GetDesiredWorkersInContent(WorkerType.Musician, btnMusicianTab));
        btnReceptionistTab.onClick.AddListener(() => GetDesiredWorkersInContent(WorkerType.Receptionist, btnReceptionistTab));
        btnBrochureSellerTab.onClick.AddListener(() => GetDesiredWorkersInContent(WorkerType.BrochureSeller, btnBrochureSellerTab));

        // WorkerMarket
        WorkerAssignmentPanelOnButton.onClick.AddListener(AddInventoryWorkersInAssignmentPanelContent);
        W_InventoryTabSecurity.onClick.AddListener(() => GetDesiredInventoryWorkersInContent(WorkerType.Security, W_InventoryTabSecurity));
        W_InventoryTabHouseKeeper.onClick.AddListener(() => GetDesiredInventoryWorkersInContent(WorkerType.Housekeeper, W_InventoryTabHouseKeeper));
        W_InventoryTabMusician.onClick.AddListener(() => GetDesiredInventoryWorkersInContent(WorkerType.Musician, W_InventoryTabMusician));
        W_InventoryTabReceptionist.onClick.AddListener(() => GetDesiredInventoryWorkersInContent(WorkerType.Receptionist, W_InventoryTabReceptionist));
        W_InventoryTabBrochureSeller.onClick.AddListener(() => GetDesiredInventoryWorkersInContent(WorkerType.BrochureSeller, W_InventoryTabBrochureSeller));
        MuseumManager.instance.CalculateAndAddTextAllInfos();
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
            GeneralButtonActivation(true,museumStatButton);
            ShowTab(currentTab);
            pnlMuseumStats.SetActive(true);
        }
        else
        {
            pnlMuseumStats.SetActive(false);
            GeneralButtonActivation(false);
        }
    }
    public void SetNPCInfoPanelUIs(string _Fullname, float _Happiness, float _Stress, float _Toilet, float _Education, List<MyColors> _LikedColors, List<string> _LikedArtist)
    {
        txtFullName.text = _Fullname;
        txtHappiness.text = "%" + _Happiness.ToString();
        txtStress.text = "%" + _Stress.ToString();
        txtToilet.text = "%" + _Toilet.ToString();
        txtEducation.text = _Education.ToString();
        List<Color> likedColors = CatchTheColors.instance.MyColorsControl(_LikedColors);
        int length = LikedColorImages.Count;
        if (_LikedColors != null && _LikedColors.Count > 0)
        {
            for (int i = 0; i < length; i++)
            {
                if (i < _LikedColors.Count)
                {
                    if (likedColors[i] != null)
                    {
                        LikedColorImages[i].color = likedColors[i];
                    }
                }
                else break;
            }
        }
        else
            Debug.Log("NPC'nin Color Listesi Bos!");

        
        int length1 = ArtistTexts.Count;        
        if (_LikedArtist != null && _LikedArtist.Count > 0)
        {
            for (int i = 0; i < length1; i++)
            {
                if (i < _LikedArtist.Count)
                {
                    if (_LikedArtist[i] != null)
                    {
                        ArtistTexts[i].text = _LikedArtist[i];
                    }
                }
                else break;
            }
        }
        else
            Debug.Log("NPC'nin Artist Listesi Bos!");
    }
    public void CloseNPCInformationPanel() //Button!
    {
        NpcInformationPanel.SetActive(false);
        NpcManager.instance.CurrentNPC.SetMyCamerasActivation(false, false);
    }

    public void AddWorkersInContent()
    {
        if (!WorkerPanel.activeInHierarchy)
        {
            GeneralButtonActivation(true,WorkerPanelOnButton);
            GetDesiredWorkersInContent(WorkerType.Security, btnSecurityTab);
            WorkerPanel.SetActive(true);
        }
        else
        {
            WorkerPanel.SetActive(false);
            GeneralButtonActivation(false);
        }
    }
   

    public void GetDesiredWorkersInContent(WorkerType _wType, Button _clikedButton = null)
    {        
        if (_clikedButton != null)
        {
            WorkerTabButtonsOn();
            _clikedButton.interactable = false;
            WorkerContent.position = WorkerPanelDefaultPos;
        }
        ClearWorkerContent(WorkerContent);
        
        List<WorkerBehaviour> workers = WorkerManager.instance.GetWorkersInMarket().Where(x=> x.workerType == _wType).OrderBy(x => x.MyScript.Level).ToList();
        foreach (WorkerBehaviour worker in workers)
        {
            GameObject newSecurityObj = Instantiate(WorkerPrefabV2_V1, WorkerContent);
            newSecurityObj.GetComponent<WorkerInfoUIs>().SetWorkerInfoUIs(worker.ID,worker.MyScript.Name, worker.MyScript.Age, worker.MyScript.Height);
        }
        Debug.Log($"Worker Turu => {_wType} olan Isciler Listelendi.");
    }
    public void GeneralButtonActivation(bool _buttonActive, Button _activePnlButton = null)
    {
        if (_buttonActive)
        {
            museumStatButton.gameObject.SetActive(!_buttonActive);
            WorkerPanelOnButton.gameObject.SetActive(!_buttonActive);
            WorkerAssignmentPanelOnButton.gameObject.SetActive(!_buttonActive);

            GameObject _activeGO = _activePnlButton.gameObject;
            if (_activeGO == museumStatButton.gameObject)
            {
                _activePnlButton.gameObject.transform.position = ActivePnlBtnBookDefaultPos.position;
                _activePnlButton.gameObject.SetActive(_buttonActive);
            }
            else if (_activeGO == WorkerPanelOnButton.gameObject)
            {
                _activePnlButton.gameObject.transform.position = ActivePnlBtnWorkerMarketDefaultPos.position;
                _activePnlButton.gameObject.SetActive(_buttonActive);
            }
            else if (_activeGO == WorkerAssignmentPanelOnButton.gameObject)
            {
                _activePnlButton.gameObject.transform.position = ActivePnlBtnWorkerAssignmentDefaultPos.position;
                _activePnlButton.gameObject.SetActive(_buttonActive);
            }

        }
        else
        {
            museumStatButton.transform.position = defaultBtnBookPos;
            WorkerPanelOnButton.transform.position = defaultBtnWorkerMarketPos;
            WorkerAssignmentPanelOnButton.transform.position = defaultBtnWorkerAssignmentPos;
            museumStatButton.gameObject.SetActive(true);
            WorkerPanelOnButton.gameObject.SetActive(true);
            WorkerAssignmentPanelOnButton.gameObject.SetActive(true);

        }

    }
    public void WorkerTabButtonsOn()
    {
        btnSecurityTab.interactable = true;
        btnHouseKeeperTab.interactable = true;
        btnMusicianTab.interactable = true;
        btnReceptionistTab.interactable = true;
        btnBrochureSellerTab.interactable = true;
    }
    public void InventoryWorkerTabButtonsOn()
    {
        W_InventoryTabSecurity.interactable = true;
        W_InventoryTabHouseKeeper.interactable = true;
        W_InventoryTabMusician.interactable = true;
        W_InventoryTabReceptionist.interactable = true;
        W_InventoryTabBrochureSeller.interactable = true;
    }
    public void ClearWorkerContent(Transform _Content)
    {
        Debug.Log("Worker Content Temizledi.");
        int length = _Content.childCount;
        for (int i = 0; i < length; i++)
        {
            GameObject destroyObj = _Content.GetChild(i).gameObject;
            Destroy(destroyObj);
        }
    }

    public void AddInventoryWorkersInAssignmentPanelContent()
    {
        if (!WorkerAssignmentPanel.activeInHierarchy)
        {
            GeneralButtonActivation(true,WorkerAssignmentPanelOnButton);

            ClearAssignmentRoomsButtonContent();
            ClearWorkerContent(WorkerAssignContent);
            GetDesiredInventoryWorkersInContent(WorkerType.Security, W_InventoryTabSecurity);
            WorkerAssignmentPanel.SetActive(true);
        }
        else
        {
            WorkerAssignmentPanel.SetActive(false);
            GeneralButtonActivation(false);
        }


    }
    public void ClearAssignmentRoomsButtonContent()
    {
        int length = WorkerAssignContent.childCount;
        for (int i = 0; i < length; i++)
        {
            Destroy(WorkerAssignContent.GetChild(i).gameObject);
        }
        Debug.Log("Isci atama oda butonlari temizlendi.");
    }
    public void GetDesiredInventoryWorkersInContent(WorkerType _wType, Button _clikedButton = null)
    {
        if (_clikedButton != null)
        {
            InventoryWorkerTabButtonsOn();
            _clikedButton.interactable = false;
        }
        ClearWorkerContent(WorkerInventoryContent);

        List<WorkerBehaviour> workers = WorkerManager.instance.GetWorkersInInventory().Where(x => x.workerType == _wType).OrderBy(x=> x.MyScript.Level).ToList();
        foreach (WorkerBehaviour worker in workers)
        {
            GameObject newSecurityObj = Instantiate(InventoryWorkerPrefab_V1, WorkerInventoryContent);
            newSecurityObj.GetComponent<WorkerInfoUIs>().SetWorkerInfoUIs(worker.ID, worker.MyScript.Name, worker.MyScript.Age, worker.MyScript.Height,worker.MyScript.Level);
        }
        Debug.Log($"Worker Turu => {_wType} olan Isciler Envantere Listelendi.");
    }

    public void AddDesiredChooseRoomsInContent(int _workerID, Color _color, string _cellNumber, bool _interectable)
    {
        GameObject newAssignmentRoom = Instantiate(AssignmentRoomPrefab_V1, WorkerAssignContent);
        WorkerAssignmentRoomButton _newAssing = newAssignmentRoom.GetComponent<WorkerAssignmentRoomButton>();
        _newAssing.AssignmentRoomButton(_workerID,_color, _cellNumber, _interectable);
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
