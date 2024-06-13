using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class UIController : MonoBehaviour
{
    private PictureElement _LastSelectedPicture;
    [SerializeField] public pnlRewardAdController RewardAdController;
    [SerializeField] public RoomUISPanelController roomUISPanelController;
    [SerializeField] public TutorialUISPanel tutorialUISPanel;
    [SerializeField] List<GameObject> IsGameObjectOverPanels;
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
    [SerializeField] ScrollRect newShopParentScrollRect;

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

    [Header("FortuneWheel")]
    [SerializeField] GameObject pnlFortuneWheel;
    //[SerializeField] Button pnlFortuneWheelOnButton;
    [SerializeField] Button pnlFortuneWheelCloseButton;


    [Header("MidBotUIs")]
    [SerializeField] GameObject LeftUIsPanel;
    [SerializeField] GameObject RigthUIsPanel;
    [Header("GameModsUIS")]
    [SerializeField] GameObject JoystickObj;
    [Header("Skill Tree")]
    public GameObject skillsContent;
    //skill info
    public GameObject skillInfoPanel;
    public TextMeshProUGUI SkillPointText;
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillDescriptionText;
    public TextMeshProUGUI skillEffectText;
    public TextMeshProUGUI skillRequiredPointText;
    public TextMeshProUGUI skillRequiredMoneyText;
    public Button unlockButton;
    [SerializeField] private GameObject[] InfoPointerUIs;
    [SerializeField] private Transform[] InfoPointers;

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
    [SerializeField] private List<Image> ArtistImages;
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

    [SerializeField] private GameObject WorkerInventoryTabPanel;
    [SerializeField] private Button W_WorkerTypesButton;

    [SerializeField] private Button W_InventoryTabSecurity;
    [SerializeField] private Button W_InventoryTabHouseKeeper;
    [SerializeField] private Button W_InventoryTabMusician;
    [SerializeField] private Button W_InventoryTabReceptionist;
    [SerializeField] private Button W_InventoryTabBrochureSeller;
    [Header("Daily Reward Panel")]
    [SerializeField] GameObject DailyRewardPanel;
    [SerializeField] Button DailyRewardPanelOnButton;
    [SerializeField] public Transform[] DailyRewardContents;
    [SerializeField] GameObject DailyRewardGemPrefab;
    [SerializeField] GameObject DailyRewardGoldPrefab;
    [SerializeField] GameObject DailyRewardTablePrefab;

    [Header("Room Editing Panel")]
    [SerializeField] GameObject RoomEditingPanel;

    [Header("General")]
    public Image CultureFillBar;
    public Text CultureLevelText, GoldText, GemText;
    [SerializeField] GameObject MoneysObj;
    //UI
    [SerializeField] private RectTransform LeftUISBackground;
    [SerializeField] private Transform LeftUIArrow;
    [SerializeField] private Button btnLeftUIOpen;

    [SerializeField] private Transform BaseBtnBookPos;
    [SerializeField] private Transform BaseBtnWorkerMarketPos;
    [SerializeField] private Transform BaseBtnWorkerAssignmentPos;

    [SerializeField] private Transform ActivePnlBtnBookDefaultPos;
    [SerializeField] private Transform ActivePnlBtnWorkerMarketDefaultPos;
    [SerializeField] private Transform ActivePnlBtnWorkerAssignmentDefaultPos;

    //[SerializeField] GameObject EditModeCanvas;
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
        if (SceneManager.GetActiveScene().name == Scenes.TutorialLevel.ToString()) return;
        DontDestroyOnLoad(this);
        GemText.text = "0";
        SkillRequiredInfoPanel.SetActive(false);
        GoldText.text = "" + MuseumManager.instance.GetCurrentGold();
        GemText.text = "" + MuseumManager.instance.GetCurrentGem();
        CultureLevelText.text = "" + MuseumManager.instance.GetCurrentCultureLevel();
        WorkerPanelDefaultPos = WorkerContent.position;
        defaultGemTextPos = GemText.transform.localPosition;
        defaultGoldTextPos = GoldText.transform.localPosition;
    }
    private void Start()
    {
        if (SceneManager.GetActiveScene().name == Scenes.TutorialLevel.ToString()) return;
        LeftUISBackground.gameObject.SetActive(false);
        defaultBtnBookPos = museumStatButton.transform.position;
        defaultBtnWorkerMarketPos = WorkerPanelOnButton.transform.position;
        defaultBtnWorkerAssignmentPos = WorkerAssignmentPanelOnButton.transform.position;
        btnLeftUIOpen.onClick.AddListener(() => StartRightPanelUISBasePosAnim(false));
        museumStatButton.onClick.AddListener(ShowMuseumStatsPanel);
        unlockButton.onClick.AddListener(BuySkill);
        NpcInfoPanelExitButton.onClick.AddListener(CloseNPCInformationPanel);
        //pnlFortuneWheelOnButton.onClick.AddListener(ShowFortuneWheelanel);
        pnlFortuneWheelCloseButton.onClick.AddListener(() => CloseFortuneWheelPanel(true));
        // WorkerMarket
        WorkerPanelOnButton.onClick.AddListener(AddWorkersInContent);
        btnSecurityTab.onClick.AddListener(() => GetDesiredWorkersInContent(WorkerType.Security, btnSecurityTab));
        btnHouseKeeperTab.onClick.AddListener(() => GetDesiredWorkersInContent(WorkerType.Housekeeper, btnHouseKeeperTab));
        btnMusicianTab.onClick.AddListener(() => GetDesiredWorkersInContent(WorkerType.Musician, btnMusicianTab));
        btnReceptionistTab.onClick.AddListener(() => GetDesiredWorkersInContent(WorkerType.Receptionist, btnReceptionistTab));
        btnBrochureSellerTab.onClick.AddListener(() => GetDesiredWorkersInContent(WorkerType.BrochureSeller, btnBrochureSellerTab));

        // WorkerAssignment
        WorkerAssignmentPanelOnButton.onClick.AddListener(AddInventoryWorkersInAssignmentPanelContent);
        W_InventoryTabSecurity.onClick.AddListener(() => GetDesiredInventoryWorkersInContent(WorkerType.Security, W_InventoryTabSecurity));
        W_InventoryTabHouseKeeper.onClick.AddListener(() => GetDesiredInventoryWorkersInContent(WorkerType.Housekeeper, W_InventoryTabHouseKeeper));
        W_InventoryTabMusician.onClick.AddListener(() => GetDesiredInventoryWorkersInContent(WorkerType.Musician, W_InventoryTabMusician));
        W_InventoryTabReceptionist.onClick.AddListener(() => GetDesiredInventoryWorkersInContent(WorkerType.Receptionist, W_InventoryTabReceptionist));
        W_InventoryTabBrochureSeller.onClick.AddListener(() => GetDesiredInventoryWorkersInContent(WorkerType.BrochureSeller, W_InventoryTabBrochureSeller));
        WorkerInventoryTabPanel.SetActive(false);
        W_WorkerTypesButton.gameObject.SetActive(true);
        W_WorkerTypesButton.onClick.AddListener(W_WorkerTypesButtonControl);

        // DailyRewardPanel 
        DailyRewardPanelOnButton.onClick.AddListener(() => ActiveInHierarchyDailyRewardPanelControl());

        MuseumManager.instance.CalculateAndAddTextAllInfos();
        
    }

    private bool isPointerOverUI;
    private void Update()
    {
        isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
    }
    public void GetClickedPicture(bool active, PictureElement _lastSelectedPicture)
    {
        _LastSelectedPicture = _lastSelectedPicture;
        SetSelectedPicture(0);
        //pnlPicturesMenu.SetActive(active);
    }
    private bool rightAnimOpen = false;
    private bool rightAnimOpenOverWrite = false;

    bool leftUIArrowClickable = true;
    public void StartRightPanelUISBasePosAnim(bool _overWrite = false)
    {
        if (!leftUIArrowClickable) return;
        if (_overWrite)
        {
            museumStatButton.enabled = false;
            WorkerPanelOnButton.enabled = false;
            WorkerAssignmentPanelOnButton.enabled = false;
            DailyRewardPanelOnButton.enabled = false;
            if (!rightAnimOpenOverWrite)
            {
                leftUIArrowClickable = false;
                btnLeftUIOpen.gameObject.SetActive(false);
                LeftUIArrow.DOLocalRotate(new Vector3(0, 0, 180), 0.2f);
                LeftUISBackground.DOScaleX(1, 0.4f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    LeftUISBackground.gameObject.SetActive(false);
                    leftUIArrowClickable = true;
                    museumStatButton.enabled = true;
                    WorkerPanelOnButton.enabled = true;
                    WorkerAssignmentPanelOnButton.enabled = true;
                    DailyRewardPanelOnButton.enabled = true;
                });
                LeftUISBackground.DOScaleY(1, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    
                });
            }
            else
            {
                leftUIArrowClickable = false;
                btnLeftUIOpen.gameObject.SetActive(true);
                LeftUISBackground.gameObject.SetActive(true);
                LeftUISBackground.DOScaleX(14, 0.05f).SetEase(Ease.Linear);
                LeftUISBackground.DOScaleY(50, 0.1f).OnUpdate(() =>
                {
                    museumStatButton.gameObject.SetActive(true);
                    WorkerPanelOnButton.gameObject.SetActive(true);
                    WorkerAssignmentPanelOnButton.gameObject.SetActive(true);
                    DailyRewardPanelOnButton.gameObject.SetActive(true);
                }).SetEase(Ease.Linear).OnComplete(() =>
                {
                    leftUIArrowClickable = true;
                    museumStatButton.enabled = true;
                    WorkerPanelOnButton.enabled = true;
                    WorkerAssignmentPanelOnButton.enabled = true;
                    DailyRewardPanelOnButton.enabled = true;
                });
            }
            rightAnimOpen = !rightAnimOpen;
            LeftUIArrow.DOLocalRotate(new Vector3(0, 0, 0), 0.2f);
            rightAnimOpenOverWrite = !rightAnimOpenOverWrite;
            return;
        }
        museumStatButton.enabled = false;
        WorkerPanelOnButton.enabled = false;
        WorkerAssignmentPanelOnButton.enabled = false;
        DailyRewardPanelOnButton.enabled = false;
        if (!rightAnimOpen)
        {
            leftUIArrowClickable = false;
            btnLeftUIOpen.GetComponent<Button>().enabled = false;
            LeftUISBackground.gameObject.SetActive(true);
            LeftUIArrow.DOLocalRotate(new Vector3(0, 0, 0), 0.2f);
            LeftUISBackground.DOScaleX(14, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
            {
                LeftUISBackground.DOScaleY(50, 0.3f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    museumStatButton.transform.DOMove(BaseBtnBookPos.position, 0.1f).SetEase(Ease.Linear);
                    WorkerPanelOnButton.transform.DOMove(BaseBtnWorkerMarketPos.position, 0.1f).SetEase(Ease.Linear);
                    WorkerAssignmentPanelOnButton.transform.DOMove(BaseBtnWorkerAssignmentPos.position, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
                    {
                        leftUIArrowClickable = true;
                        btnLeftUIOpen.GetComponent<Button>().enabled = true;
                        museumStatButton.enabled = true;
                        WorkerPanelOnButton.enabled = true;
                        WorkerAssignmentPanelOnButton.enabled = true;
                        DailyRewardPanelOnButton.enabled = true;
                    });
                    
                });
            });


        }
        else
        {
            leftUIArrowClickable = false;
            btnLeftUIOpen.GetComponent<Button>().enabled = false;
            LeftUIArrow.DOLocalRotate(new Vector3(0, 0, 180), 0.2f);
            museumStatButton.transform.DOMove(defaultBtnBookPos, 0.05f).SetEase(Ease.Linear);
            WorkerPanelOnButton.transform.DOMove(defaultBtnWorkerMarketPos, 0.05f).SetEase(Ease.Linear);
            WorkerAssignmentPanelOnButton.transform.DOMove(defaultBtnWorkerAssignmentPos, 0.05f).SetEase(Ease.Linear).OnComplete(() =>
            {
                LeftUISBackground.DOScaleX(1, 0.4f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    LeftUISBackground.gameObject.SetActive(false);
                    leftUIArrowClickable = true;
                    btnLeftUIOpen.GetComponent<Button>().enabled = true;
                    museumStatButton.enabled = true;
                    WorkerPanelOnButton.enabled = true;
                    WorkerAssignmentPanelOnButton.enabled = true;
                    DailyRewardPanelOnButton.enabled = true;
                });
                LeftUISBackground.DOScaleY(1, 0.2f).SetEase(Ease.Linear);
            });
        }
        
        rightAnimOpen = !rightAnimOpen;
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
            newShopParentScrollRect.content = (RectTransform)newShop.transform.GetChild(2).transform;
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
            int point = (int)SkillTreeManager.instance.SelectedSkillGameObject.GetComponent<BaseSkillOptions>().MyPoint;
            foreach (GameObject pointUI in InfoPointerUIs)
            {
                pointUI.SetActive(false);
            }
            InfoPointerUIs[point].SetActive(true);
            // InfoPointers[point].position;
            skillInfoPanel.transform.position = SkillTreeManager.instance.SelectedSkillGameObject.GetComponent<BaseSkillOptions>().GetMyCurrentPointTransform().position;
        }
        // Yetenek adi, aciklama ve etkiyi guncelle
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
    public void ForTutorialUnityEventSkillInfoShow(GameObject _selectedSkillObj)
    {
        skillInfoPanel.SetActive(false);

        SkillNode selectedSkill = SkillTreeManager.instance.GetSelectedSkillNode(0);


        SkillTreeManager.instance.SelectedSkill = selectedSkill;
        SkillTreeManager.instance.SelectedSkillGameObject = _selectedSkillObj;
        Debug.Log("Selected Skill Gameobject => " + SkillTreeManager.instance.SelectedSkillGameObject.gameObject);
        if (SkillTreeManager.instance.SelectedSkillGameObject != null)
        {
            int point = (int)SkillTreeManager.instance.SelectedSkillGameObject.GetComponent<BaseSkillOptions>().MyPoint;
            foreach (GameObject pointUI in InfoPointerUIs)
            {
                pointUI.SetActive(false);
            }
            InfoPointerUIs[point].SetActive(true);
            // InfoPointers[point].position;
            skillInfoPanel.transform.position = SkillTreeManager.instance.SelectedSkillGameObject.GetComponent<BaseSkillOptions>().GetMyCurrentPointTransform().position;
        }
        // Yetenek adi, aciklama ve etkiyi guncelle
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
    public void SkillInfoPanelActivation(bool _active)
    {
        skillInfoPanel.SetActive(_active);
    }

    public void BuySkill() // SKILL SATIN ALMA BUTONU!
    {
        SkillNode currentSkill = SkillTreeManager.instance.SelectedSkill;
        //MuseumManager.instance.AddSkillPoint(10);  // TEHLIKELI KOD! TESTTEN SONRA SILINMELIDIR!
        if (MuseumManager.instance.GetCurrentGold() >= currentSkill.SkillRequiredMoney && MuseumManager.instance.GetCurrentSkillPoint() >= currentSkill.SkillRequiredPoint)
        {
            if (currentSkill.SkillCurrentLevel < currentSkill.SkillMaxLevel)
            {
                MuseumManager.instance.SpendingGold(currentSkill.SkillRequiredMoney);
                SkillTreeManager.instance.SelectedSkillGameObject.GetComponentInChildren<SkillAbilityAmountController>().IncreasingAbilityAmount(); //Skill Ability Amount Arttirma.


                SkillPointText.text = "" + (MuseumManager.instance.GetCurrentSkillPoint() - currentSkill.SkillRequiredPoint);
                MuseumManager.instance.SpendingSkillPoint(currentSkill.SkillRequiredPoint);
                SkillRequiredPointAndMoneyController(currentSkill);
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
                GoogleAdsManager.instance.ShowInterstitialAd();
            }
            else
            {
                Debug.Log("Skill Max Level! => Current Skill Level: " + currentSkill.SkillCurrentLevel + "/" + currentSkill.SkillMaxLevel);
            }
        }
        else
        {
            InsufficientGoldEffect();
            Debug.Log($"Skill satin almak icin gerekli; paraniz ({MuseumManager.instance.GetCurrentGold()} => $:{currentSkill.SkillRequiredMoney}) ve/veya yetenek puaniniz ({MuseumManager.instance.GetCurrentSkillPoint()} => P:{currentSkill.SkillRequiredPoint} mevcut degildir.");
        }
        UIChangesControl();
    }
    private void SkillRequiredPointAndMoneyController(SkillNode _skill)
    {
        _skill.SkillRequiredPoint += SkillCurrentLevelControl(_skill.SkillCurrentLevel).skillPoint; // Base Value => 1
        _skill.SkillRequiredMoney = _skill.SkillRequiredMoney + (_skill.SkillRequiredMoney * SkillCurrentLevelControl(_skill.SkillCurrentLevel).skillMoney);
    }

    private (int skillPoint, float skillMoney) SkillCurrentLevelControl(int _skillLevel)
    {
        if (_skillLevel == 1)
        {
            return (1, 0.5f);
        }
        else if (_skillLevel == 2)
        {
            return (2, 0.6f);
        }
        else if (_skillLevel == 4)
        {
            return (3, 0.7f);
        }
        else if (_skillLevel == 6)
        {
            return (4, 0.8f);
        }
        else if (_skillLevel == 9)
        {
            return (5, 0.9f);
        }
        else if (_skillLevel == 11)
        {
            return (6, 1f);
        }
        else if (_skillLevel == 14)
        {
            return (7, 1.1f);
        }
        else
        {
            return (0, 1.2f);
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
        else if (currentSkill.IsPurchased && currentSkill.SkillCurrentLevel != currentSkill.SkillMaxLevel && MuseumManager.instance.GetCurrentSkillPoint() >= currentSkill.SkillRequiredPoint && MuseumManager.instance.GetCurrentGold() >= currentSkill.SkillRequiredMoney)
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

        if (!_selectedSkill.IsLocked && _selectedSkill.SkillCurrentLevel != _selectedSkill.SkillMaxLevel)
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
    Vector3 defaultGoldTextPos;
    public void InsufficientGoldEffect()
    {

        GoldText.color = Color.red;

        GoldText.transform.DOShakePosition(0.5f, new Vector3(10, 0, 0), 10, 90, false, true).OnComplete(() =>
        {
            string hexColor = "#FAE254";
            Color color;
            if (ColorUtility.TryParseHtmlString(hexColor, out color))
            {
                GoldText.color = color;
            }
            else
            {
                Debug.LogError("Geçersiz hexadecimal renk kodu: " + hexColor);
            }

            GoldText.transform.localPosition = defaultGoldTextPos;
        });
        AudioManager.instance.PlayDesiredSoundEffect(SoundEffectType.InsufficientGoldAndGem);
    }

    Vector3 defaultGemTextPos;
    public void InsufficientGemEffect()
    {
        GemText.color = Color.red;

        GemText.transform.DOShakePosition(0.5f, new Vector3(10, 0, 0), 10, 90, false, true).OnComplete(() =>
        {
            string hexColor = "#8FF871";
            Color color;
            if (ColorUtility.TryParseHtmlString(hexColor, out color))
            {
                GemText.color = color;
            }
            else
            {
                Debug.LogError("Geçersiz hexadecimal renk kodu: " + hexColor);
            }
            GemText.transform.localPosition = defaultGemTextPos;
        });
        AudioManager.instance.PlayDesiredSoundEffect(SoundEffectType.InsufficientGoldAndGem);
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
    private bool museumStatActive = false, workerHirinActive = false, workerAssigmentActive = false, dailyRewardActive = false;
    public void ShowMuseumStatsPanel()
    {
        if (!museumStatActive)
        {
            RightUIPanelController.instance.CloseEditObj(true);
            CloseJoystickObj(true);
            GeneralButtonActivation(true, museumStatButton);
            ShowTab(currentTab);
            pnlMuseumStats.SetActive(true);
            museumStatActive=true;
        }
        else
        {
            CloseJoystickObj(false);
            RightUIPanelController.instance.CloseEditObj(false);
            pnlMuseumStats.SetActive(false);
            GeneralButtonActivation(false);
            museumStatActive = false;
        }
    }
    private void ShowFortuneWheelanel()
    {
        if (!pnlFortuneWheel.activeInHierarchy)
        {
            RightUIPanelController.instance.CloseEditObj(true);
            CloseJoystickObj(true);
            //GeneralButtonActivation(true, pnlFortuneWheelOnButton);
            CloseFortuneWheelPanel(false);
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

        for (int i = 0; i < ArtistImages.Count; i++)
            ArtistImages[i].gameObject.SetActive(false);
        for (int i = 0; i < ArtistTexts.Count; i++)
            ArtistTexts[i].gameObject.SetActive(false);

        int length1 = ArtistTexts.Count;
        if (_LikedArtist != null && _LikedArtist.Count > 0)
        {
            for (int i = 0; i < length1; i++)
            {
                if (i < _LikedArtist.Count)
                {
                    if (_LikedArtist[i] != null)
                    {
                        ArtistTexts[i].gameObject.SetActive(true);
                        ArtistImages[i].gameObject.SetActive(true);
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
        if (GameManager.instance.GetCurrentGameMode() == GameMode.FPS)
        {
            PlayerManager.instance.UnLockPlayer();
        }
        NpcInformationPanel.SetActive(false);
        if (NpcManager.instance.CurrentNPC != null)
        {
            NpcManager.instance.CurrentNPC.SetMyCamerasActivation(false, false);
        }
    }

    public void AddWorkersInContent()
    {
        if (!workerHirinActive)
        {
            RightUIPanelController.instance.CloseEditObj(true);
            CloseJoystickObj(true);
            GeneralButtonActivation(true, WorkerPanelOnButton);
            GetDesiredWorkersInContent(WorkerType.Security, btnSecurityTab);
            WorkerPanel.SetActive(true);
            workerHirinActive = true;
        }
        else
        {
            RightUIPanelController.instance.CloseEditObj(false);
            CloseJoystickObj(false);
            WorkerPanel.SetActive(false);
            GeneralButtonActivation(false);
            workerHirinActive = false;
        }
    }
    public void CloseWorkerShopPanel(bool _close)
    {
        WorkerPanel.SetActive(!_close);
    }
    public void CloseWorkerAssignmentPanel(bool _close)
    {
        WorkerAssignmentPanel.SetActive(!_close);
    }
    public void CloseDailyRewardPanel(bool _close)
    {
        DailyRewardPanel.SetActive(!_close);
    }
    public void ActiveInHierarchyDailyRewardPanelControl()
    {
        GoogleAdsManager.instance.ShowInterstitialAd();
        if (!dailyRewardActive)
        {
            GeneralButtonActivation(true, DailyRewardPanelOnButton);
            RightUIPanelController.instance.CloseEditObj(true);
            CloseJoystickObj(true);
            DailyRewardPanel.SetActive(true);
            dailyRewardActive = true;
        }
        else
        {
            CloseJoystickObj(false);
            RightUIPanelController.instance.CloseEditObj(false);
            DailyRewardPanel.SetActive(false);
            GeneralButtonActivation(false, DailyRewardPanelOnButton);
            dailyRewardActive = false;
        }
    }
    public void CloseMuseumStatsPanel(bool _close)
    {
        pnlMuseumStats.SetActive(!_close);
    }
    public void CloseFortuneWheelPanel(bool _close)
    {
        if (_close)
        {
            CloseJoystickObj(false);
            RightUIPanelController.instance.CloseEditObj(false);
            GeneralButtonActivation(false);
        }
        pnlFortuneWheel.SetActive(!_close);
    }
    public void CloseCultureExpObj(bool _close)
    {
        CultureFillBar.gameObject.SetActive(!_close);
    }
    public void CloseLeftUIsPanel(bool _close)
    {
        LeftUIsPanel.SetActive(!_close);
    }
    public void CloseMoneysObj(bool _close)
    {
        MoneysObj.SetActive(!_close);
    }
    public void CloseJoystickObj(bool _close)
    {
        JoystickObj.SetActive(!_close);
    }
    public void CloseRewardAdPanel(bool _close)
    {
        RewardAdController.gameObject.SetActive(!_close);
        Debug.Log("RewardAdController is activeSelf => " + !_close + " its gameoject name is => " + RewardAdController.gameObject.name);
    }
    public void CloseEditModeCanvas(bool _close)
    {
        //EditModeCanvas.SetActive(!_close);
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

        List<WorkerBehaviour> workers = WorkerManager.instance.GetWorkersInMarket().Where(x => x.workerType == _wType).OrderBy(x => x.MyScript.Level).ToList();
        foreach (WorkerBehaviour worker in workers)
        {
            GameObject newSecurityObj = Instantiate(WorkerPrefabV2_V1, WorkerContent);
            Debug.Log("worker.MyScript.Level => " + worker.MyScript.Level);
            newSecurityObj.GetComponent<WorkerInfoUIs>().SetWorkerInfoUIs(worker.ID, worker.MyScript.Name, worker.MyScript.Age, worker.MyScript.Height, worker.MyScript.Level);
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
            DailyRewardPanelOnButton.gameObject.SetActive(!_buttonActive);
            //pnlFortuneWheelOnButton.gameObject.SetActive(!_buttonActive);

            GameObject _activeGO = _activePnlButton.gameObject;
            if (_activeGO == museumStatButton.gameObject)
            {
                _activePnlButton.gameObject.transform.position = ActivePnlBtnBookDefaultPos.position;
                _activePnlButton.gameObject.SetActive(_buttonActive);
                StartRightPanelUISBasePosAnim(true);
            }
            else if (_activeGO == WorkerPanelOnButton.gameObject)
            {
                _activePnlButton.gameObject.transform.position = ActivePnlBtnWorkerMarketDefaultPos.position;
                _activePnlButton.gameObject.SetActive(_buttonActive);
                StartRightPanelUISBasePosAnim(true);
            }
            else if (_activeGO == WorkerAssignmentPanelOnButton.gameObject)
            {
                _activePnlButton.gameObject.transform.position = ActivePnlBtnWorkerAssignmentDefaultPos.position;
                _activePnlButton.gameObject.SetActive(_buttonActive);
                StartRightPanelUISBasePosAnim(true);
            }
            else if (_activeGO == DailyRewardPanelOnButton.gameObject)
            {
                _activePnlButton.gameObject.SetActive(_buttonActive);
                btnLeftUIOpen.gameObject.SetActive(false);
                LeftUIArrow.DOLocalRotate(new Vector3(0, 0, 180), 0.2f);
                museumStatButton.transform.DOMove(defaultBtnBookPos, 0.05f).SetEase(Ease.Linear);
                WorkerPanelOnButton.transform.DOMove(defaultBtnWorkerMarketPos, 0.05f).SetEase(Ease.Linear);
                WorkerAssignmentPanelOnButton.transform.DOMove(defaultBtnWorkerAssignmentPos, 0.05f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    LeftUISBackground.DOScaleX(1, 0.4f).SetEase(Ease.Linear).OnComplete(() =>
                    {
                        LeftUISBackground.gameObject.SetActive(false);
                    });
                    LeftUISBackground.DOScaleY(1, 0.2f).SetEase(Ease.Linear);
                });
                rightAnimOpenOverWrite = false;
                rightAnimOpen = false;
            }
        }
        else
        {
            if (_activePnlButton != null && _activePnlButton.gameObject == DailyRewardPanelOnButton.gameObject)
            {
                museumStatButton.gameObject.SetActive(true);
                WorkerPanelOnButton.gameObject.SetActive(true);
                WorkerAssignmentPanelOnButton.gameObject.SetActive(true);
                DailyRewardPanelOnButton.gameObject.SetActive(true);
                btnLeftUIOpen.gameObject.SetActive(true);
                //StartRightPanelUISBasePosAnim(true);
                Debug.Log("_activePnlButton.gameObject => " + _activePnlButton.gameObject);
                return;
            }
            museumStatButton.transform.position = BaseBtnBookPos.position;
            WorkerPanelOnButton.transform.position = BaseBtnWorkerMarketPos.position;
            WorkerAssignmentPanelOnButton.transform.position = BaseBtnWorkerAssignmentPos.position;
            museumStatButton.gameObject.SetActive(true);
            WorkerPanelOnButton.gameObject.SetActive(true);
            WorkerAssignmentPanelOnButton.gameObject.SetActive(true);
            DailyRewardPanelOnButton.gameObject.SetActive(true);
            //pnlFortuneWheelOnButton.gameObject.SetActive(true);
            StartRightPanelUISBasePosAnim(true);
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
        if (!workerAssigmentActive)
        {
            RightUIPanelController.instance.CloseEditObj(true);
            CloseJoystickObj(true);
            GeneralButtonActivation(true, WorkerAssignmentPanelOnButton);

            ClearAssignmentRoomsButtonContent();
            ClearWorkerContent(WorkerAssignContent);
            WorkerInventoryTabPanel.SetActive(true);
            W_WorkerTypesButton.gameObject.SetActive(false);
            InventoryWorkerTabButtonsOn();
            WorkerAssignmentPanel.SetActive(true);
            workerAssigmentActive = true;
        }
        else
        {
            RightUIPanelController.instance.CloseEditObj(false);
            CloseJoystickObj(false);
            WorkerAssignmentPanel.SetActive(false);
            GeneralButtonActivation(false);
            workerAssigmentActive = false;
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
        WorkerInventoryTabPanel.SetActive(false);
        W_WorkerTypesButton.gameObject.SetActive(true);
        if (_clikedButton != null)
        {
            InventoryWorkerTabButtonsOn();
            _clikedButton.interactable = false;
        }
        ClearWorkerContent(WorkerInventoryContent);

        List<WorkerBehaviour> workers = WorkerManager.instance.GetWorkersInInventory().Where(x => x.workerType == _wType).OrderBy(x => x.MyScript.Level).ToList();
        foreach (WorkerBehaviour worker in workers)
        {
            GameObject newSecurityObj = Instantiate(InventoryWorkerPrefab_V1, WorkerInventoryContent);
            Debug.Log("worker.MyScript.Level => " + worker.MyScript.Level);
            newSecurityObj.GetComponent<WorkerInfoUIs>().SetWorkerInfoUIs(worker.ID, worker.MyScript.Name, worker.MyScript.Age, worker.MyScript.Height, worker.MyScript.Level);
        }
        Debug.Log($"Worker Turu => {_wType} olan Isciler Envantere Listelendi.");
    }

    public void AddDesiredChooseRoomsInContent(int _roomID, int _workerID, Color _color, string _cellNumber, bool _interectable)
    {
        GameObject newAssignmentRoom = Instantiate(AssignmentRoomPrefab_V1, WorkerAssignContent);
        WorkerAssignmentRoomButton _newAssing = newAssignmentRoom.GetComponent<WorkerAssignmentRoomButton>();
        _newAssing.AssignmentRoomButton(_roomID, _workerID, _color, _cellNumber, _interectable);
    }

    public void W_WorkerTypesButtonControl()
    {
        W_WorkerTypesButton.gameObject.SetActive(false);
        ClearAssignmentRoomsButtonContent();
        ClearWorkerContent(WorkerInventoryContent);
        WorkerInventoryTabPanel.gameObject.SetActive(true);
        InventoryWorkerTabButtonsOn();
    }

    public void SetUpdateWeeklyRewards()
    {
        ClearDailyRewardContents();
        List<ItemData> currentItems = new List<ItemData>();
        for (int i = 0; i < ItemManager.instance.CurrentDailyRewardItems.Count; i++)
        {
            currentItems.Add(ItemManager.instance.CurrentDailyRewardItems[i]);
        }
        int length = currentItems.Count;
        for (int i = 0; i < length; i++)
        {
            switch (currentItems[i].CurrentItemType)
            {
                case ItemType.None:
                    break;
                case ItemType.Gem:
                    GameObject _newDailyRewardGem = Instantiate(DailyRewardGemPrefab, DailyRewardContents[i]);
                    _newDailyRewardGem.GetComponent<DailyRewardItemOptions>().SetMyOptions(currentItems[i].ID, currentItems[i].Name, currentItems[i].Amount, true, currentItems[i].IsPurchased, currentItems[i].IsLocked);
                    break;
                case ItemType.Gold:
                    GameObject _newDailyRewardGold = Instantiate(DailyRewardGoldPrefab, DailyRewardContents[i]);
                    _newDailyRewardGold.GetComponent<DailyRewardItemOptions>().SetMyOptions(currentItems[i].ID, currentItems[i].Name, currentItems[i].Amount, true, currentItems[i].IsPurchased, currentItems[i].IsLocked);
                    break;
                case ItemType.Table:
                    GameObject _newDailyRewardTable = Instantiate(DailyRewardTablePrefab, DailyRewardContents[i]);
                    _newDailyRewardTable.GetComponent<DailyRewardItemOptions>().SetMyOptions(currentItems[i].ID, currentItems[i].Name, currentItems[i].Amount, true, currentItems[i].IsPurchased, currentItems[i].IsLocked, currentItems[i].StarCount);
                    break;
                case ItemType.All:
                    break;
                default:
                    break;
            }

        }
    }
    public void ClearDailyRewardContents()
    {
        int length = DailyRewardContents.Length;
        for (int i = 0; i < length; i++)
        {
            int length1 = DailyRewardContents[i].childCount;
            for (int k = 0; k < length1; k++)
            {
                Destroy(DailyRewardContents[i].GetChild(k).gameObject);
            }
        }
    }

    public void SetActivationRoomEditingPanel(bool _active)
    {
        RoomEditingPanel.SetActive(_active);
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
            Vector3 midPos = startPos + new Vector3(0, -500, 0) * swingHeight;
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
        if (go != null && parent != null)
        {
            GameObject obj = Instantiate(go, parent);
            return obj;
        }
        return new GameObject("Null Obj");
    }

    public bool IsPointerOverAnyUI()
    {
        //if (Input.touchCount > 0)
        //{
        //    Touch t1 = Input.GetTouch(0);
        //    Debug.Log("t1.position => " + t1.position);
        //    Ray ray = Camera.main.ScreenPointToRay(t1.position);
        //    RaycastHit hit;

        //    if (Physics.Raycast(ray, out hit))
        //    {
        //        if (hit.collider.CompareTag("UI"))
        //        {
        //            return true;
        //        }                
        //    }           
        //}
        //return false;

        //if (Input.touchCount > 0)
        //{
        //    Touch t1 = Input.GetTouch(0);
        //    Debug.Log("t1.position => " + t1.position);

        //    if (EventSystem.current.IsPointerOverGameObject(t1.fingerId))
        //    {
        //        Debug.Log(t1.fingerId + " ID'li dokunma UI ile cakisti.");
        //        return true;
        //    }
        //}
        //return false;
        
        return isPointerOverUI;
    }
    
}
