using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeHandler : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("Karakterin Tum kiyafetlerini tutan parenta atanmis Customize Componenti")]
    public Customize TargetCustomize;
    public Material MainMat;
    public FlexibleColorPicker ColorPicker;

    [Header("Customize Cameras")]
    public CinemachineVirtualCamera CustomizeCamHead;
    public CinemachineVirtualCamera CustomizeCamChest;
    public CinemachineVirtualCamera CustomizeCamLeg;
    public Camera RenderCamera;

    [Header("Customize Main Panel")]
    public GameObject CustomizePanel;
    public GameObject ElementsHeaderPanel;
    public GameObject ElementsPanel;
    public GameObject ColorHeaderPanel;
    public GameObject ColorHolderPanel;
    public GameObject DrawPanel;

    [Header("Header UI Elements")]
    public Transform HeadersParent;
    public Transform ElementsParent;
    public Transform ColorHeaderParent;
    public GameObject CustomizeUIPrefab;
    public Color HeaderPassiveColor;
    public Color HeaderActiveColor;

    [Header("Buttons")]
    public Button MaleButton;
    public Button FemaleButton;
    public Button AppearanceButton;
    public Button ColorsButton;
    public Button Set1Button;
    public Button Set2Button;
    public Button Set3Button;
    public Button DrawButton;
    public Button ClaimButton;
    public Button ExitButton;
    public Button SaveButton; //Mevcut slotu save eder hepsini deðil.

    [Header("Localization")]
    public Text GainItemCategoryText;
    public Transform HeaderTextsContent;
    public List<string> onSelectedAnEquipmentStrings = new List<string>();
    public List<string> onExitProcessStrings = new List<string>();
    [Header("Runtime Datas")]
    [SerializeField] private List<int> LockedElements = new();
    [SerializeField] private PlayerExtraCustomizeData TempCustomize;

    [SerializeField] private List<int> CurrentlyUnlockedIDs = new();
    public const int baseUnlockedItemPrice = 250;
    [Header("Default Colors")]
    [SerializeField] private List<Color> DefaultColors;
    [SerializeField] private bool ApplyColor;

    [Header("Bonuses From Items")]
    public List<CustomizeBonus> ActiveBonuses = new();

    [Header("Character Customize Data")]
    public CharacterCustomizeData characterCustomizeData;

    public static CustomizeHandler instance { get; private set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        InitCustomizeItems();
    }
    private void Start()
    {    
        MaleButton.onClick.AddListener(OnClickedMaleButton);
        FemaleButton.onClick.AddListener(OnClickedFemaleButton);
        AppearanceButton.onClick.AddListener(OnClickedAppearanceButton);
        ColorsButton.onClick.AddListener(OnClickedColorsButton);
        Set1Button.onClick.AddListener(()=>OnSetSelected(0));
        Set2Button.onClick.AddListener(()=>OnSetSelected(1));
        Set3Button.onClick.AddListener(()=>OnSetSelected(2));
        DrawButton.onClick.AddListener(OnClickedDrawItemButton);
        ClaimButton.onClick.AddListener(OnClickedClaimButton);
        ExitButton.onClick.AddListener(ExitButtonProcesses);
        if (SaveButton != null)
            SaveButton.onClick.AddListener(SaveButtonProcesses);
    }
    void ExitButtonProcesses()
    {

        UIInteractHandler.instance.AskQuestion(onExitProcessStrings[0], onExitProcessStrings[1],
            (yes) =>
            {
                SwitchCustomizePanel();
                FirestoreManager.instance.customizationDatasHandler.AddCustomizationDataWithUserId(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID, characterCustomizeData,true);
                RightUIPanelController.instance.UIVisibleClose(false);
                PlayerManager.instance.UnLockPlayer();
                UIController.instance.CloseJoystickObj(false);
                UpdateEquipmentStats();
            },null,null,null,null,null);
    }
    void SaveButtonProcesses()
    {
        return; //bu method devre disi birakilmistir.
        UIInteractHandler.instance.AskQuestion("Özelleþtirmeyi Kaydet #Çeviri", "Deðiþiklikleri onaylýyor musunuz? (Mevcut Slotu Kaydeder.) #Çeviri",
            (yes) =>
            {
                FirestoreManager.instance.customizationDatasHandler.AddCustomizationDataWithUserId(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID, characterCustomizeData, false);
                UpdateEquipmentStats();
            }, null, null, null, null, null);
    }

    public void CustomizationInit()
    {
        CurrentlyUnlockedIDs = new();
        DefaultColors = new();
        DefaultColors.Add(new Color(0.2431373f, 0.4196079f, 0.6196079f, 1));
        DefaultColors.Add(new Color(0.8196079f, 0.6431373f, 0.2980392f, 1));
        DefaultColors.Add(new Color(0.282353f, 0.2078432f, 0.1647059f, 1));
        DefaultColors.Add(new Color(0.5960785f, 0.6117647f, 0.627451f, 1));
        DefaultColors.Add(new Color(0.372549f, 0.3294118f, 0.2784314f, 1));
        DefaultColors.Add(new Color(0.1764706f, 0.1960784f, 0.2156863f, 1));
        DefaultColors.Add(new Color(0.345098f, 0.3764706f, 0.3960785f, 1));
        DefaultColors.Add(new Color(0.2627451f, 0.2117647f, 0.1333333f, 1));
        DefaultColors.Add(new Color(1f, 0.8000001f, 0.682353f, 1));
        DefaultColors.Add(new Color(0.8039216f, 0.7019608f, 0.6313726f, 1));
        DefaultColors.Add(new Color(0.9294118f, 0.6862745f, 0.5921569f, 1));
        DefaultColors.Add(new Color(0.2283196f, 0.5822246f, 0.7573529f, 1));
        DefaultColors.Add(new Color(0.2283196f, 0.5822246f, 0.7573529f, 1));

        int length = HeadersParent.childCount;
        for (int i = 0; i < length; i++)
        {
            Transform header = HeadersParent.GetChild(i);
            CustomizeSlot slot = (CustomizeSlot)int.Parse(header.name.Split('_')[0]);
            header.GetComponent<Button>().onClick.AddListener(() => OnAnHeaderSelected(slot));
        }

        length = ColorHeaderParent.childCount;
        for (int i = 0; i < length; i++)
        {
            Transform header = ColorHeaderParent.GetChild(i);
            int lambda = i;
            CustomizeColor slot = (CustomizeColor)lambda;
            header.GetComponent<Button>().onClick.AddListener(() => OnAnColorHeaderSelected(slot));
        }

        UpdateLockedElementIDs();

        TempCustomize = GetCustomizeData();
        TargetCustomize.UpdateVisual(TempCustomize);
        SetMaterialColors(TempCustomize.Colors);
        UpdateEquipmentStats();
    }

    public void OnAnHeaderSelected(CustomizeSlot _slot)
    {
        CustomizeSlot lastSlot = GetLastOpenedHeader();
        int length = CurrentlyUnlockedIDs.Count;
        for (int i = length - 1; i >= 0; i--)
        {
            int catID = CurrentlyUnlockedIDs[i] / 1000;
            if(catID == (int)lastSlot)
                CurrentlyUnlockedIDs.RemoveAt(i);
        }
        
        foreach (Transform item in HeadersParent)
        {
            item.GetChild(2).GetComponent<Image>().color = (CustomizeSlot)int.Parse(item.name.Split('_')[0]) == _slot ? HeaderActiveColor : HeaderPassiveColor;
            item.GetChild(3).gameObject.SetActive(false);
        }

        SetLastOpenedPanel(_slot);
        UpdateUI();
        PlayButtonSound();

        if (ElementsPanel.activeSelf)
            ElementsPanel.GetComponent<UIFade>().FadeIn();
        else
            ElementsPanel.SetActive(true);

        if(CustomizeCamHead.Priority == -99)
            CustomizeCamHead.Follow.parent.localEulerAngles = Vector3.zero;
        if(CustomizeCamChest.Priority == -99)
            CustomizeCamChest.Follow.parent.localEulerAngles = Vector3.zero;
        if(CustomizeCamLeg.Priority == -99)
            CustomizeCamLeg.Follow.parent.localEulerAngles = Vector3.zero;

        if (_slot == CustomizeSlot.Head || _slot == CustomizeSlot.FacialHair || _slot == CustomizeSlot.Helmet || _slot == CustomizeSlot.Helmet_Attachment || _slot == CustomizeSlot.Eyebrows || _slot == CustomizeSlot.Hair || _slot == CustomizeSlot.Hat || _slot == CustomizeSlot.Mask || _slot == CustomizeSlot.Elf_Ear)
        {
            CustomizeCamHead.Priority = 900;
            CustomizeCamChest.Priority = -99;
            CustomizeCamLeg.Priority = -99;
        }
        else if (_slot == CustomizeSlot.Torso || _slot == CustomizeSlot.Arm_Lower_Left || _slot == CustomizeSlot.Arm_Upper_Right || _slot == CustomizeSlot.Arm_Lower_Right ||
            _slot == CustomizeSlot.Arm_Lower_Left || _slot == CustomizeSlot.Hand_Right || _slot == CustomizeSlot.Hand_Left || _slot == CustomizeSlot.Back_Attachment ||
            _slot == CustomizeSlot.Shoulder_Attachment_Right || _slot == CustomizeSlot.Shoulder_Attachment_Left ||
            _slot == CustomizeSlot.Elbow_Attachment_Left || _slot == CustomizeSlot.Elbow_Attachment_Right)
        {
            CustomizeCamHead.Priority = -99;
            CustomizeCamChest.Priority = 900;
            CustomizeCamLeg.Priority = -99;
        }
        else if(_slot == CustomizeSlot.Hip || _slot == CustomizeSlot.Hip_Attachment || _slot == CustomizeSlot.Leg_Right || _slot == CustomizeSlot.Leg_Left ||
            _slot == CustomizeSlot.Knee_Attachment_Left || _slot == CustomizeSlot.Knee_Attachment_Right)
        {
            CustomizeCamHead.Priority = -99;
            CustomizeCamChest.Priority = -99;
            CustomizeCamLeg.Priority = 900;
        }
        if (!HeaderTextsContent.gameObject.activeSelf)
            HeaderTextsContent.gameObject.SetActive(true);
        HeaderTextActivation((int)_slot);
        //SelectedHeaderText.text = LanguageDatabase.instance.GetText("CustomizeSlot_" + (int)_slot);
    }
    void HeaderTextActivation(int _index)
    {
        int length = HeaderTextsContent.childCount;
        int indexResult = -9999;
        for (int i = 0; i < length; i++)
        {
            GameObject headerText = HeaderTextsContent.GetChild(i).gameObject;
            GameObject header = HeadersParent.GetChild(i).gameObject;
            if (indexResult == -9999 && int.Parse(header.name.Split('_')[0]) == _index)
            {
                indexResult = header.transform.GetSiblingIndex();
            }
            if (headerText.activeSelf)
                headerText.SetActive(false);
        }
        if (!(indexResult == -9999))
            HeaderTextsContent.GetChild(indexResult).gameObject.SetActive(true);
        else
            Debug.LogError($"Hata! {_index} numarasina ait bir header bulunamadi!");
    }
    public void SwitchCustomizePanel()
    {
        if (CustomizePanel.activeSelf)
            CloseCustomizePanel();
        else
            OpenCustomizePanel();
    }

    public void OpenCustomizePanel()
    {
        TempCustomize.CustomizeElements = GetCustomizeData().CustomizeElements;

        RenderCamera.gameObject.SetActive(true);
        if (CustomizePanel.activeSelf)
            CustomizePanel.GetComponent<UIFade>().FadeIn();
        else
            CustomizePanel.SetActive(true);

        if (TempCustomize.isFemale)
            OnClickedFemaleButton();
        else
            OnClickedMaleButton();

        int panel = GetLastSelectedHeader();
        if(panel == 0)
            OnClickedAppearanceButton();
        else
            OnClickedColorsButton();

        UpdateLockedElementIDs();
        UpdateLocalize();
        OnSetSelected(characterCustomizeData.playerCustomizeData.selectedCustomizeSlot);
    }

    public void CloseCustomizePanel()
    {
        if (CustomizePanel.activeSelf)
            CustomizePanel.GetComponent<UIFade>().FadeOut();

        CustomizeCamHead.Priority = -99;
        CustomizeCamChest.Priority = -99;
        CustomizeCamLeg.Priority = -99;
        RenderCamera.gameObject.SetActive(false);

        SaveSet(GetSetID());

        CurrentlyUnlockedIDs = new();

//#if UNITY_EDITOR
//        FirestoreManager.instance.customizationDatasHandler.AddCustomizationDataWithUserId(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID, characterCustomizeData);
//#endif
    }

    List<CustomizeElement> GetDefaultElements()
    {
        List<CustomizeElement> _default = new();

        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Head, elementID = 1001 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.FacialHair, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Torso, elementID = 4004 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Arm_Upper_Right, elementID = 5001 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Arm_Upper_Left, elementID = 6001 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Arm_Lower_Right, elementID = 7001 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Arm_Lower_Left, elementID = 8001 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Hand_Right, elementID = 9001 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Hand_Left, elementID = 10001 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Hip, elementID = 11004 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Leg_Right, elementID = 12003 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Leg_Left, elementID = 13003 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Helmet, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Eyebrows, elementID = 102003 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Hair, elementID = 103004 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Hat, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Mask, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Helmet_Attachment, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Back_Attachment, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Shoulder_Attachment_Right, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Shoulder_Attachment_Left, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Elbow_Attachment_Right, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Elbow_Attachment_Left, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Hip_Attachment, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Knee_Attachment_Right, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Knee_Attachment_Left, elementID = 0 });
        _default.Add(new CustomizeElement() { customizeSlot = CustomizeSlot.Elf_Ear, elementID = 0 });

        return _default;
    }

    void PlayButtonSound()
    {
        //AudioManager.instance.PlayButtonClickUISound();
    }

    void PlayChangeGenderSound()
    {
        //AudioManager.instance.PlayAddJewelUISound();
    }

    public void OnClickedMaleButton()
    {
        MaleButton.transform.GetChild(1).GetComponent<Image>().color = HeaderActiveColor;
        FemaleButton.transform.GetChild(1).GetComponent<Image>().color = HeaderPassiveColor;
        TempCustomize.isFemale = false;
        PlayChangeGenderSound();
        UpdateUI();
    }

    public void OnClickedFemaleButton()
    {
        MaleButton.transform.GetChild(1).GetComponent<Image>().color = HeaderPassiveColor;
        FemaleButton.transform.GetChild(1).GetComponent<Image>().color = HeaderActiveColor;
        TempCustomize.isFemale = true;
        PlayChangeGenderSound();
        UpdateUI();
    }

    public void OnClickedAppearanceButton()
    {
        ApplyColor = false;

        if (ColorHeaderPanel.activeSelf)
            ColorHeaderPanel.GetComponent<UIFade>().FadeOut();
        if (ColorHolderPanel.activeSelf)
            ColorHolderPanel.GetComponent<UIFade>().FadeOut();

        if (ElementsPanel.activeSelf)
            ElementsPanel.GetComponent<UIFade>().FadeIn();
        else
            ElementsPanel.SetActive(true);

        if (ElementsHeaderPanel.activeSelf)
            ElementsHeaderPanel.GetComponent<UIFade>().FadeIn();
        else
            ElementsHeaderPanel.SetActive(true);

        characterCustomizeData.LastSelectedCustomizeHeader = 0;
        AppearanceButton.transform.GetChild(1).GetComponent<Image>().color = HeaderActiveColor;
        ColorsButton.transform.GetChild(1).GetComponent<Image>().color = HeaderPassiveColor;
        PlayButtonSound();
        UpdateUI();

        OnAnHeaderSelected(GetLastOpenedHeader());
    }

    public void OnClickedColorsButton()
    {
        if (ElementsPanel.activeSelf)
            ElementsPanel.GetComponent<UIFade>().FadeOut();
        if (ElementsHeaderPanel.activeSelf)
            ElementsHeaderPanel.GetComponent<UIFade>().FadeOut();

        if (ColorHeaderPanel.activeSelf)
            ColorHeaderPanel.GetComponent<UIFade>().FadeIn();
        else
            ColorHeaderPanel.SetActive(true);

        if (ColorHolderPanel.activeSelf)
            ColorHolderPanel.GetComponent<UIFade>().FadeIn();
        else
            ColorHolderPanel.SetActive(true);

        ApplyColor = true;

        characterCustomizeData.LastSelectedCustomizeHeader = 1;
        AppearanceButton.transform.GetChild(1).GetComponent<Image>().color = HeaderPassiveColor;
        ColorsButton.transform.GetChild(1).GetComponent<Image>().color = HeaderActiveColor;
        PlayButtonSound();
        UpdateUI();

        OnAnColorHeaderSelected(GetLastOpenedColorHeader());
    }

    public void OnAnColorHeaderSelected(CustomizeColor _color)
    {
        foreach (Transform item in ColorHeaderParent)
            item.GetChild(1).GetComponent<Image>().color = HeaderPassiveColor;

        ColorHeaderParent.GetChild((int)_color).GetChild(1).GetComponent<Image>().color = HeaderActiveColor;

        List<Color> c = GetCustomizeData().Colors;
        ColorPicker.SetColor(c[(int)_color]);
        SetLastOpenedColorPanel(_color);
        UpdateUI();
        PlayButtonSound();

        if (ColorHolderPanel.activeSelf)
            ColorHolderPanel.GetComponent<UIFade>().FadeIn();
        else
            ColorHolderPanel.SetActive(true);

        if (CustomizeCamChest.Priority == -99)
            CustomizeCamChest.Follow.parent.localEulerAngles = Vector3.zero;

        CustomizeCamChest.Priority = 900;
        if (HeaderTextsContent.gameObject.activeSelf)
            HeaderTextsContent.gameObject.SetActive(false);
    }

    public void OnSetSelected(int _setID)
    {
        SaveSet(GetSetID());
        ChangeActiveChangeID(_setID);
        Set1Button.transform.GetChild(1).GetComponent<Image>().color = _setID == 0 ? HeaderActiveColor : HeaderPassiveColor;
        Set2Button.transform.GetChild(1).GetComponent<Image>().color = _setID == 1 ? HeaderActiveColor : HeaderPassiveColor;
        Set3Button.transform.GetChild(1).GetComponent<Image>().color = _setID == 2 ? HeaderActiveColor : HeaderPassiveColor;
        PlayChangeGenderSound();
        UpdateUI();
        SetMaterialColors(TempCustomize.Colors);
    }

    public void UpdateUI()
    {
        int length = ElementsParent.childCount;
        for (int i = length - 1; i >= 0; i--)
            Destroy(ElementsParent.GetChild(i).gameObject);

        int panel = characterCustomizeData.LastSelectedCustomizeHeader;
        if (panel == 0) //Appearance Panel
        {
            foreach (var item in CurrentlyUnlockedIDs)
            {
                int categoryUnlocked = item / 1000;
                foreach (Transform header in HeadersParent)
                    if(int.Parse(header.name.Split('_')[0]) == categoryUnlocked)
                        header.GetChild(3).gameObject.SetActive(true);
            }
            CustomizeSlot slot = GetLastOpenedHeader();
            Vector2Int itemIDs = GetStartAndEndIDs(slot);
            bool isMale = !TempCustomize.isFemale && itemIDs.x < 100000 || slot == CustomizeSlot.FacialHair;
            bool canUnequip = IsUnequippable(slot);
            bool symetrical = IsSymetrical(slot);

            if (TempCustomize.isFemale && slot == CustomizeSlot.FacialHair)
                itemIDs = new Vector2Int(0, -99);

            if (canUnequip)
            {
                GameObject unequipButton = Instantiate(CustomizeUIPrefab, ElementsParent);
                Image icon = unequipButton.transform.GetChild(0).GetComponent<Image>();
                Image lockedArea = unequipButton.transform.GetChild(1).GetComponent<Image>();
                Image border = unequipButton.transform.GetChild(2).GetComponent<Image>();

                Image leftButton = unequipButton.transform.GetChild(3).GetComponent<Image>();
                Image rightButton = unequipButton.transform.GetChild(4).GetComponent<Image>();
                if (!symetrical)
                {
                    leftButton.gameObject.SetActive(false);
                    rightButton.gameObject.SetActive(false);

                    int equippedID = TempCustomize.GetEquippedElementBySlot(slot).elementID;
                    border.color = equippedID == 0 ? HeaderActiveColor : HeaderPassiveColor;
                }
                else
                {
                    CustomizeSlot secondarySlot = (CustomizeSlot)((int)slot + 1);
                    int equippedRightID = TempCustomize.GetEquippedElementBySlot(slot).elementID;
                    int equippedLeftID = TempCustomize.GetEquippedElementBySlot(secondarySlot).elementID - 1000;
                    border.color = equippedRightID == 0 || equippedLeftID == 0 ? HeaderActiveColor : HeaderPassiveColor;
                    Color weakerActive = HeaderActiveColor;
                    Color weakerPassive = HeaderPassiveColor;
                    weakerActive.a = 0.3f;
                    weakerPassive.a = 0.1f;
                    rightButton.color = equippedRightID == 0 ? weakerActive : weakerPassive;
                    leftButton.color = equippedLeftID == 0 ? weakerActive : weakerPassive;
                }

                icon.sprite = Resources.Load<Sprite>("Customize/" + 0);
                Color a = icon.color;
                a.a = 1f;
                icon.color = a;

                lockedArea.gameObject.SetActive(false);
                unequipButton.GetComponent<Button>().onClick.AddListener(() => OnSelectedAnEquipment(slot, 0));
            }

            for (int i = itemIDs.x; i <= itemIDs.y; i++)
            {
                GameObject newElement = Instantiate(CustomizeUIPrefab, ElementsParent);
                bool locked = LockedElements.Contains(i);

                int finalID = i;
                if (isMale)
                    finalID = i + 100;

                Image icon = newElement.transform.GetChild(0).GetComponent<Image>();
                Image lockedArea = newElement.transform.GetChild(1).GetComponent<Image>();
                Image border = newElement.transform.GetChild(2).GetComponent<Image>();

                Image leftButton = newElement.transform.GetChild(3).GetComponent<Image>();
                Image rightButton = newElement.transform.GetChild(4).GetComponent<Image>();

                GameObject notificationImage = newElement.transform.GetChild(5).gameObject;
                notificationImage.SetActive(CurrentlyUnlockedIDs.Contains(finalID) || CurrentlyUnlockedIDs.Contains(i));

                if (!symetrical)
                {
                    leftButton.gameObject.SetActive(false);
                    rightButton.gameObject.SetActive(false);

                    int equippedID = TempCustomize.GetEquippedElementBySlot(slot).elementID;
                    border.color = equippedID == i || equippedID == finalID ? HeaderActiveColor : HeaderPassiveColor;

                    int lambda = i;
                    newElement.GetComponent<Button>().onClick.AddListener(() => OnSelectedAnEquipment(slot, lambda));
                }
                else
                {
                    CustomizeSlot secondarySlot = (CustomizeSlot)((int)slot + 1);
                    int equippedRightID = TempCustomize.GetEquippedElementBySlot(slot).elementID;
                    int equippedLeftID = TempCustomize.GetEquippedElementBySlot(secondarySlot).elementID - 1000;
                    border.color = equippedRightID == i || equippedRightID == finalID || equippedLeftID == i || equippedLeftID == finalID ? HeaderActiveColor : HeaderPassiveColor;
                    Color weakerActive = HeaderActiveColor;
                    Color weakerPassive = HeaderPassiveColor;
                    weakerActive.a = 0.45f;
                    weakerPassive.a = 0.6f;
                    rightButton.color = equippedRightID == i || equippedRightID ==finalID ? weakerActive : weakerPassive;
                    leftButton.color = equippedLeftID == i || equippedLeftID == finalID ? weakerActive : weakerPassive;

                    int lambda = i;

                    newElement.GetComponent<Button>().onClick.AddListener(() => { OnSelectedAnRightEquipment(slot, lambda); OnSelectedAnLeftEquipment(secondarySlot, lambda + 1000); });
                    rightButton.GetComponent<Button>().onClick.AddListener(() => OnSelectedAnRightEquipment(slot, lambda));
                    leftButton.GetComponent<Button>().onClick.AddListener(() => OnSelectedAnLeftEquipment(secondarySlot, lambda + 1000));
                }

                icon.sprite = Resources.Load<Sprite>("Customize/" + finalID);
                Color a = icon.color;
                a.a = locked ? 0.1f : 1f;
                icon.color = a;

                lockedArea.gameObject.SetActive(locked);

                
            }
        }
        else //Colors Panel
        {
            List<Color> colors = GetCustomizeData().Colors;
            length = ColorHeaderParent.childCount;
            for (int i = 0; i < length; i++)
                ColorHeaderParent.GetChild(i).GetChild(0).GetComponent<Image>().color = colors[i];

        }

        TargetCustomize.UpdateVisual(TempCustomize);
    }

    public void OnSelectedAnEquipment(CustomizeSlot _slot, int _id)
    {
        //Satin alma kismi burayi tetiklemeli.
        if (LockedElements.Contains(_id) || LockedElements.Contains(_id + 100))
        {
            CustomizeItem customizeItem = CustomizeItems.Where(x => x.ID == _id).SingleOrDefault();
            string header2 = $"{onSelectedAnEquipmentStrings[0]} ({customizeItem.ItemName})";
            string explanation2 = $"{onSelectedAnEquipmentStrings[1]}: {customizeItem.UnlockPrice}";
            UIInteractHandler.instance.AskQuestion(header2, explanation2, (yes) =>
            {
                BuyCustomizeElement(customizeItem);
            }, (no) =>
            {
                
            }, null, null, null, null);
            return;
        }
        PlayChangeGenderSound();
        TempCustomize.AddOrOverrideCustomizeElement(_slot, _id);
        UpdateUI();
    }

    public void BuyCustomizeElement(CustomizeItem _item)
    {
        if (_item.UnlockPrice > MuseumManager.instance.GetCurrentGold())
        {
            UIController.instance.InsufficientGoldEffect();
            Debug.Log(_item.ItemName + " Adli Customize Item'i alacak yeterli para bulunmamaktadir. (Extra gerekli Para => " + (_item.UnlockPrice - MuseumManager.instance.GetCurrentGold()).ToString());
            return;
        }
        else
            MuseumManager.instance.SpendingGold(_item.UnlockPrice);


        characterCustomizeData.unlockedCustomizeElementIDs.Add(_item.ID);
        //CustomizeItem customizeItem = CustomizeItems.Where(x => x.ID == _item.ID).SingleOrDefault();
        //customizeItem = new CustomizeItem(_item);
        UpdateUnlockedItemsPrice();
        UpdateLockedElementIDs();
        UpdateUI();
    }
    public void OnSelectedAnRightEquipment(CustomizeSlot _slot, int _id)
    {
        //Satin alma olayini yapma sadece kilitli de.
        if (LockedElements.Contains(_id) || LockedElements.Contains(_id + 100) || LockedElements.Contains(_id - 100))
        {
            CustomizeItem customizeItem = CustomizeItems.Where(x => x.ID == _id).SingleOrDefault();
            string header2 = $"{onSelectedAnEquipmentStrings[0]} ({customizeItem.ItemName})";
            string explanation2 = $"{onSelectedAnEquipmentStrings[1]}: {customizeItem.UnlockPrice}";
            UIInteractHandler.instance.AskQuestion(header2, explanation2, (yes) =>
            {
                BuyCustomizeElement(customizeItem);
            }, (no) =>
            {

            }, null, null, null, null);
            return;
        }
        PlayChangeGenderSound();
        TempCustomize.AddOrOverrideCustomizeElement(_slot, _id);
        UpdateUI();
    }

    public void OnSelectedAnLeftEquipment(CustomizeSlot _slot, int _id)
    {
        //Satin alma olayini yapma sadece kilitli de.
        int contID = _id - 1000;
        if (LockedElements.Contains(contID) || LockedElements.Contains(contID) || LockedElements.Contains(contID))
        {
            CustomizeItem customizeItem = CustomizeItems.Where(x => x.ID == contID).SingleOrDefault();
            string header2 = $"{onSelectedAnEquipmentStrings[0]} ({customizeItem.ItemName})";
            string explanation2 = $"{onSelectedAnEquipmentStrings[1]}: {customizeItem.UnlockPrice}";
            UIInteractHandler.instance.AskQuestion(header2, explanation2, (yes) =>
            {
                BuyCustomizeElement(customizeItem);
            }, (no) =>
            {

            }, null, null, null, null);
            return;
        }
        PlayChangeGenderSound();
        TempCustomize.AddOrOverrideCustomizeElement(_slot, _id);
        UpdateUI();
    }

    public void RotateCurrentActiveCamera(float _rotationX, float _rotationY)
    {
        if(CustomizeCamHead.Priority == 900)
            CustomizeCamHead.Follow.parent.Rotate(0, -_rotationY, 0, Space.World);
        if(CustomizeCamChest.Priority == 900)
            CustomizeCamChest.Follow.parent.Rotate(0, -_rotationY, 0, Space.World);
        if(CustomizeCamLeg.Priority == 900)
            CustomizeCamLeg.Follow.parent.Rotate(0, -_rotationY, 0, Space.World);
    }

    private void FixedUpdate()
    {
        if (ApplyColor)
        {
            CustomizeColor cc = GetLastOpenedColorHeader();
            List<Color> c = GetCustomizeData().Colors;
            c[(int)cc] = ColorPicker.color;
            ColorHeaderParent.GetChild((int)cc).GetChild(0).GetComponent<Image>().color = ColorPicker.color;
            SetMaterialColors(c);
        }
    }

    public void OnClickedDrawItemButton()
    {
        //int total = 286;
        //int current = LockedElements.Count;
        //int n = total - current + 1;
        //if(current <= 0)
        //{
        //    string header2 = LanguageDatabase.instance.GetText("Draw_Item_Max_Header");
        //    string explanation2 = LanguageDatabase.instance.GetText("Draw_Item_Max_Desc");
        //    UIInteractHandler.instance.AskQuestion(header2, explanation2, null, null, null, null, null, null);
        //    return;
        //}

        //float price = GetChestPrice(n);
        //float gold = GameManager.instance.currentActiveSaveData.Gold;
        //if (price > gold)
        //{
        //    string header2 = LanguageDatabase.instance.GetText("Draw_Item_Not_Enough_Gold_Header");
        //    string explanation2 = LanguageDatabase.instance.GetText("Draw_Item_Not_Enough_Gold_Desc");
        //    explanation2 = explanation2.Replace("{%price}", "<color=cyan>" + price.ToString("F2") + "</color>");
        //    UIInteractHandler.instance.AskQuestion(header2, explanation2, null, null, null, null, null, null);
        //    return;
        //}

        //string header = LanguageDatabase.instance.GetText("Draw_Item_Header");
        //string explanation = LanguageDatabase.instance.GetText("Draw_Item_Desc");
        //explanation = explanation.Replace("{%price}", "<color=cyan>" + price.ToString("F2") + "</color>");
        //UIInteractHandler.instance.AskQuestion(header, explanation, OnAcceptedDraw, null, null, new object[] { price }, null, null);
    }

    //public void OnAcceptedDraw(object parameter = null)
    //{
    //    if (parameter is object[] param)
    //    {
    //        float myGold = GameManager.instance.currentActiveSaveData.Gold;
    //        float price = 0;
    //        if (param[0] is float _price)
    //            price = _price;

    //        if (DrawPanel.activeSelf)
    //            DrawPanel.GetComponent<UIFade>().FadeIn();
    //        else
    //            DrawPanel.SetActive(true);

    //        int length = LockedElements.Count;
    //        int gainedID = LockedElements[UnityEngine.Random.Range(0, length)];

    //        int category = gainedID / 1000;
    //        GainItemCategoryText.text = LanguageDatabase.instance.GetText("CustomizeSlot_" + category);

    //        bool isMale = !TempCustomize.isFemale && gainedID < 100000 || category == (int)CustomizeSlot.FacialHair;
    //        int finalID = gainedID;
    //        if (isMale)
    //            finalID += 100;

    //        CurrentlyUnlockedIDs.Add(gainedID);
    //        CurrentlyUnlockedIDs.Add(gainedID + 100);
    //        DrawPanel.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Customize/" + finalID);

    //        GameManager.instance.AddAnUnlockedAppearanceElement(gainedID, price);
    //        GameAudioManager.instance.PlayPayUISound();

    //        Invoke(nameof(DelayedEarnItemSound), 1);
    //        UpdateLockedElementIDs();
    //        UpdateUI();
    //    }
    //    else
    //    {
    //        Debug.LogError("parameter(s) does not fit! " + parameter);
    //        return;
    //    }
    //}

    void DelayedEarnItemSound()
    {
        //GameAudioManager.instance.PlayAddJewelUISound();
    }

    public void OnClickedClaimButton()
    {
        if (DrawPanel.activeSelf)
            DrawPanel.GetComponent<UIFade>().FadeOut();
    }

    public void UpdateLocalize()
    {
        DrawButton.transform.GetChild(0).GetComponent<Text>().text = "Draw Buttonu Texti";
        //AppearanceButton.transform.GetChild(0).GetComponent<Text>().text = "Gorunum Buttonu Texti";
        //ColorsButton.transform.GetChild(0).GetComponent<Text>().text = "Renkler button Texti";
        ClaimButton.transform.GetChild(0).GetComponent<Text>().text = "Odulu al Button Texti.";
    }

    void SetLastOpenedPanel(CustomizeSlot _slot)
    {
        characterCustomizeData.LastSelectedCustomizeCategory = (int)_slot;
    }

    void SetLastOpenedColorPanel(CustomizeColor _color)
    {
        characterCustomizeData.LastSelectedColorHeader = (int)_color;
    }

    PlayerExtraCustomizeData GetCustomizeData()
    {
        if (characterCustomizeData.playerCustomizeData == null)
            characterCustomizeData.playerCustomizeData = new();

        if (characterCustomizeData.playerCustomizeData.AllCustomizeData == null)
            characterCustomizeData.playerCustomizeData.AllCustomizeData = new();

        while (characterCustomizeData.playerCustomizeData.AllCustomizeData.Count < 3)
            characterCustomizeData.playerCustomizeData.AllCustomizeData.Add(new() { ID = characterCustomizeData.playerCustomizeData.AllCustomizeData.Count, CustomizeElements = GetDefaultElements(), isFemale = false });

        int selected = characterCustomizeData.playerCustomizeData.selectedCustomizeSlot;
        foreach (var item in characterCustomizeData.playerCustomizeData.AllCustomizeData)
        {
            if (item.Colors == null)
                item.Colors = new();

            if (item.Colors.Count == 0)
                foreach (var item2 in DefaultColors)
                    item.Colors.Add(item2);
        }
        

        return characterCustomizeData.playerCustomizeData.AllCustomizeData[selected];
    }

    void SetCustomizeDataToSave(List<CustomizeElement> _data)
    {
        int selected = characterCustomizeData.playerCustomizeData.selectedCustomizeSlot;
        characterCustomizeData.playerCustomizeData.AllCustomizeData[selected].CustomizeElements = _data;
    }

    void ChangeActiveChangeID(int _newID)
    {
        characterCustomizeData.playerCustomizeData.selectedCustomizeSlot = _newID;
        TempCustomize = characterCustomizeData.playerCustomizeData.AllCustomizeData[_newID];
        if(TempCustomize.isFemale)
            OnClickedFemaleButton();
        else
            OnClickedMaleButton();

        int panel = GetLastSelectedHeader();
        if (panel == 0)
            OnClickedAppearanceButton();
        else
            OnClickedColorsButton();
    }

    void SaveSet(int _setID)
    {
        characterCustomizeData.playerCustomizeData.AllCustomizeData[_setID] = TempCustomize;
        //Debug.Log("Karakteri burada Save Edebilirsiniz.");
        //GameManager.instance.SaveGame();

    }

    public int GetSetID()
    {
        return characterCustomizeData.playerCustomizeData.selectedCustomizeSlot;
    }

    CustomizeSlot GetLastOpenedHeader()
    {
        if (characterCustomizeData.LastSelectedCustomizeCategory <= 0)
            characterCustomizeData.LastSelectedCustomizeCategory = 1;
        return (CustomizeSlot)characterCustomizeData.LastSelectedCustomizeCategory;
    }

    CustomizeColor GetLastOpenedColorHeader()
    {
        if (characterCustomizeData.LastSelectedColorHeader < 0)
            characterCustomizeData.LastSelectedColorHeader = 0;
        return (CustomizeColor)characterCustomizeData.LastSelectedColorHeader;
    }

    int GetLastSelectedHeader()
    {
        return characterCustomizeData.LastSelectedCustomizeHeader;
    }

    bool IsUnequippable(CustomizeSlot _slot)
    {
        return (int)_slot >= (int)CustomizeSlot.Helmet || (int)_slot == (int)CustomizeSlot.FacialHair;
    }

    bool IsSymetrical(CustomizeSlot _slot)
    {
        return (_slot == CustomizeSlot.Hand_Left || _slot == CustomizeSlot.Hand_Right ||
            _slot == CustomizeSlot.Arm_Lower_Left || _slot == CustomizeSlot.Arm_Lower_Right ||
            _slot == CustomizeSlot.Arm_Upper_Left || _slot == CustomizeSlot.Arm_Upper_Right ||
            _slot == CustomizeSlot.Leg_Right || _slot == CustomizeSlot.Leg_Left ||
            _slot == CustomizeSlot.Shoulder_Attachment_Left || _slot == CustomizeSlot.Shoulder_Attachment_Right ||
            _slot == CustomizeSlot.Elbow_Attachment_Left || _slot == CustomizeSlot.Elbow_Attachment_Right ||
            _slot == CustomizeSlot.Knee_Attachment_Left || _slot == CustomizeSlot.Knee_Attachment_Right
            );
    }

    public float GetChestPrice(int n)
    {
        float a = 0.07f;  // Scaling factor
        float b = 2f;     // Exponent for quadratic growth
        float c = 10f;    // Base price

        // Ensure n is between 1 and 300
        n = Mathf.Clamp(n, 1, 300);

        // Formula to calculate the price
        float price = a * Mathf.Pow(n, b) + c;

        return price;
    }

    void UpdateLockedElementIDs()
    {
        LockedElements = new();

        Vector2Int head = GetStartAndEndIDs(CustomizeSlot.Head);
        for (int i = head.x + 5; i <= head.y; i++)
            if(!characterCustomizeData.unlockedCustomizeElementIDs.Contains(i))
                LockedElements.Add(i);

        Vector2Int torso = GetStartAndEndIDs(CustomizeSlot.Torso);
        for (int i = torso.x + 5; i <= torso.y; i++)
            if (!characterCustomizeData.unlockedCustomizeElementIDs.Contains(i))
                LockedElements.Add(i);

        Vector2Int Arm_Upper = GetStartAndEndIDs(CustomizeSlot.Arm_Upper_Right);
        for (int i = Arm_Upper.x + 4; i <= Arm_Upper.y; i++)
            if (!characterCustomizeData.unlockedCustomizeElementIDs.Contains(i))
                LockedElements.Add(i);

        Vector2Int Arm_Lower = GetStartAndEndIDs(CustomizeSlot.Arm_Lower_Right);
        for (int i = Arm_Lower.x + 3; i <= Arm_Lower.y; i++)
            if (!characterCustomizeData.unlockedCustomizeElementIDs.Contains(i))
                LockedElements.Add(i);

        Vector2Int Hand = GetStartAndEndIDs(CustomizeSlot.Hand_Right);
        for (int i = Hand.x + 2; i <= Hand.y; i++)
            if (!characterCustomizeData.unlockedCustomizeElementIDs.Contains(i))
                LockedElements.Add(i);

        Vector2Int Hip = GetStartAndEndIDs(CustomizeSlot.Hip);
        for (int i = Hip.x + 4; i <= Hip.y; i++)
            if (!characterCustomizeData.unlockedCustomizeElementIDs.Contains(i))
                LockedElements.Add(i);

        Vector2Int Leg = GetStartAndEndIDs(CustomizeSlot.Leg_Right);
        for (int i = Leg.x + 2; i <= Leg.y; i++)
            if (!characterCustomizeData.unlockedCustomizeElementIDs.Contains(i))
                LockedElements.Add(i);

        Vector2Int helmet = GetStartAndEndIDs(CustomizeSlot.Helmet);
        for (int i = helmet.x; i <= helmet.y; i++)
            if (!characterCustomizeData.unlockedCustomizeElementIDs.Contains(i))
                LockedElements.Add(i);

        Vector2Int hair = GetStartAndEndIDs(CustomizeSlot.Hair);
        for (int i = hair.x + 8; i <= hair.y; i++)
            if (!characterCustomizeData.unlockedCustomizeElementIDs.Contains(i))
                LockedElements.Add(i);

        Vector2Int hat = GetStartAndEndIDs(CustomizeSlot.Hat);
        for (int i = hat.x; i <= hat.y; i++)
            if (!characterCustomizeData.unlockedCustomizeElementIDs.Contains(i))
                LockedElements.Add(i);

        Vector2Int mask = GetStartAndEndIDs(CustomizeSlot.Mask);
        for (int i = mask.x; i <= mask.y; i++)
            if (!characterCustomizeData.unlockedCustomizeElementIDs.Contains(i))
                LockedElements.Add(i);

        Vector2Int helmetAtt = GetStartAndEndIDs(CustomizeSlot.Helmet_Attachment);
        for (int i = helmetAtt.x; i <= helmetAtt.y; i++)
            if (!characterCustomizeData.unlockedCustomizeElementIDs.Contains(i))
                LockedElements.Add(i);

        Vector2Int back = GetStartAndEndIDs(CustomizeSlot.Back_Attachment);
        for (int i = back.x; i <= back.y; i++)
            if (!characterCustomizeData.unlockedCustomizeElementIDs.Contains(i))
                LockedElements.Add(i);

        Vector2Int shoulderAtt = GetStartAndEndIDs(CustomizeSlot.Shoulder_Attachment_Right);
        for (int i = shoulderAtt.x; i <= shoulderAtt.y; i++)
            if (!characterCustomizeData.unlockedCustomizeElementIDs.Contains(i))
                LockedElements.Add(i);

        Vector2Int elbow = GetStartAndEndIDs(CustomizeSlot.Elbow_Attachment_Right);
        for (int i = elbow.x; i <= elbow.y; i++)
            if (!characterCustomizeData.unlockedCustomizeElementIDs.Contains(i))
                LockedElements.Add(i);

        Vector2Int hipAtt = GetStartAndEndIDs(CustomizeSlot.Hip_Attachment);
        for (int i = hipAtt.x; i <= hipAtt.y; i++)
            if (!characterCustomizeData.unlockedCustomizeElementIDs.Contains(i))
                LockedElements.Add(i);

        Vector2Int Knee = GetStartAndEndIDs(CustomizeSlot.Knee_Attachment_Right);
        for (int i = Knee.x; i <= Knee.y; i++)
            if (!characterCustomizeData.unlockedCustomizeElementIDs.Contains(i))
                LockedElements.Add(i);

        Vector2Int elf = GetStartAndEndIDs(CustomizeSlot.Elf_Ear);
        for (int i = elf.x; i <= elf.y; i++)
            if (!characterCustomizeData.unlockedCustomizeElementIDs.Contains(i))
                LockedElements.Add(i);
    }

    Vector2Int GetStartAndEndIDs(CustomizeSlot _slot)
    {
        if (_slot == CustomizeSlot.None)
            return Vector2Int.zero;
        else if (_slot == CustomizeSlot.Head)
            return new Vector2Int(1001,1023);
        else if (_slot == CustomizeSlot.FacialHair)
            return new Vector2Int(3001, 3018);
        else if (_slot == CustomizeSlot.Torso)
            return new Vector2Int(4003, 4029);
        else if (_slot == CustomizeSlot.Arm_Upper_Right || _slot == CustomizeSlot.Arm_Upper_Left)
            return new Vector2Int(5001, 5021);
        else if (_slot == CustomizeSlot.Arm_Lower_Right || _slot == CustomizeSlot.Arm_Lower_Left)
            return new Vector2Int(7001, 7019);
        else if (_slot == CustomizeSlot.Hand_Right || _slot == CustomizeSlot.Hand_Left)
            return new Vector2Int(9001, 9018);
        else if (_slot == CustomizeSlot.Hip)
            return new Vector2Int(11003, 11029);
        else if (_slot == CustomizeSlot.Leg_Right || _slot == CustomizeSlot.Leg_Left)
            return new Vector2Int(12001, 12020);
        else if (_slot == CustomizeSlot.Helmet)
            return new Vector2Int(14001, 14013);
        else if (_slot == CustomizeSlot.Eyebrows)
            return new Vector2Int(102001, 102010);
        else if (_slot == CustomizeSlot.Hair)
            return new Vector2Int(103001, 103038);
        else if (_slot == CustomizeSlot.Hat)
            return new Vector2Int(104001, 104024);
        else if (_slot == CustomizeSlot.Mask)
            return new Vector2Int(105001, 105004);
        else if (_slot == CustomizeSlot.Helmet_Attachment)
            return new Vector2Int(106001, 106013);
        else if (_slot == CustomizeSlot.Back_Attachment)
            return new Vector2Int(107001, 107015);
        else if (_slot == CustomizeSlot.Shoulder_Attachment_Right || _slot == CustomizeSlot.Shoulder_Attachment_Left)
            return new Vector2Int(108001, 108021);
        else if (_slot == CustomizeSlot.Elbow_Attachment_Right || _slot == CustomizeSlot.Elbow_Attachment_Left)
            return new Vector2Int(110001, 110006);
        else if (_slot == CustomizeSlot.Hip_Attachment)
            return new Vector2Int(112001, 112012);
        else if (_slot == CustomizeSlot.Knee_Attachment_Right || _slot == CustomizeSlot.Knee_Attachment_Left)
            return new Vector2Int(113001, 113011);
        else if (_slot == CustomizeSlot.Elf_Ear)
            return new Vector2Int(115001, 115003);

        return Vector2Int.zero;
    }

    public void SetMaterialColors(List<Color> _colors)
    {
        foreach (CustomizeColor item in Enum.GetValues(typeof(CustomizeColor)))
            MainMat.SetColor(item.ToString(), _colors[(int)item]);
    }

    private void OnApplicationQuit()
    {
        SetMaterialColors(DefaultColors);
    }

    public void UpdateEquipmentStats()
    {
        return; //Gecici sureligine bu method devre disidir. (Yeni versiyon guncellemeleriyle geri getirilebilinir.)
        ActiveBonuses = new();

        foreach (var item in TempCustomize.CustomizeElements)
        {
            CustomizeItem c = GetItemWithID(item.elementID);
            if (c.ID != 0) //structs null olamaz, eger bulunaz ise 0 idli dondurecek.
            {
                foreach (var bonus in c.Bonuses)
                {
                    bool contains = false;
                    foreach (var currentBonus in ActiveBonuses)
                    {
                        if (currentBonus.Stat == bonus.Stat)
                        {
                            currentBonus.Amount += bonus.Amount;
                            contains = true;
                        }
                    }

                    if (!contains)
                    {
                        ActiveBonuses.Add(new()
                        {
                            Stat = bonus.Stat,
                            Amount = bonus.Amount,
                        });
                    }
                }
            }
        }

        Debug.Log("Equipment Stats Are Refreshed!");
    }

    public float GetBonusAmountOf(eStat _stat)
    {
        foreach (var item in ActiveBonuses)
        {
            if (item.Stat == _stat)
            {
                return item.Amount;
            }
        }

        return 0;
    }

    [SerializeField] private List<CustomizeItem> CustomizeItems;
    void InitCustomizeItems()
    {
        CustomizeItems = new();

        //ItemID_Name, ItemID_Desc gibi bir kalip da kullanabilirsin textler icin, oyle bir yontem daha saglikli olabilir.
        CustomizeItems.Add(new() { ID = 0, ItemName = "TextID_LanguageID", ItemDesc = "TextID_LanguageID", Rarity = CustomizeRarity.Common, UnlockPrice = 0, 
            Bonuses = new() });

        #region HEAD_ITEMS 1001-1023

        CustomizeItems.Add(new()
        {
            ID = 1001,
            ItemName = "Curator's Cap",
            ItemDesc = "Feel like a curator ready to unveil the dusty pages of history!",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1002,
            ItemName = "Pharaoh's Crown",
            ItemDesc = "A golden crown symbolizing the grandeur of ancient Egypt.",
            Rarity = CustomizeRarity.Rare,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1003,
            ItemName = "Explorer's Hat",
            ItemDesc = "The perfect choice for discovering lost treasures!",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1004,
            ItemName = "Time Traveler's Goggles",
            ItemDesc = "Stay ahead with a glimpse into different eras of history!",
            Rarity = CustomizeRarity.Epic,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1005,
            ItemName = "Artist's Beret",
            ItemDesc = "Channel your inner Renaissance artist!",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1006,
            ItemName = "Knight's Helm",
            ItemDesc = "A helmet that carries the honor of medieval battles.",
            Rarity = CustomizeRarity.Rare,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1007,
            ItemName = "Samurai Headband",
            ItemDesc = "Show your courage with this symbol of honor and discipline.",
            Rarity = CustomizeRarity.Rare,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1008,
            ItemName = "Viking Horned Helmet",
            ItemDesc = "A fierce helmet embodying the spirit of northern warriors.",
            Rarity = CustomizeRarity.Rare,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1009,
            ItemName = "Futuristic Headgear",
            ItemDesc = "Step into tomorrow with this cutting-edge accessory.",
            Rarity = CustomizeRarity.Legendary,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1010,
            ItemName = "Ancient Laurel Wreath",
            ItemDesc = "A symbol of victory and wisdom from ancient times.",
            Rarity = CustomizeRarity.Rare,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1011,
            ItemName = "Scholar's Hat",
            ItemDesc = "Perfect for those who love uncovering the mysteries of the past.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1012,
            ItemName = "Pirate's Bandana",
            ItemDesc = "A relic from the high seas! Arrr!",
            Rarity = CustomizeRarity.Rare,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1013,
            ItemName = "Detective's Cap",
            ItemDesc = "Uncover secrets with the sharpest mind in the room.",
            Rarity = CustomizeRarity.Epic,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1014,
            ItemName = "Steampunk Hat",
            ItemDesc = "Combine the elegance of the past with futuristic gears.",
            Rarity = CustomizeRarity.Legendary,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1015,
            ItemName = "Cultural Mask",
            ItemDesc = "Celebrate the diverse traditions of the world.",
            Rarity = CustomizeRarity.Rare,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1016,
            ItemName = "Space Explorer's Helmet",
            ItemDesc = "Explore beyond the stars with this iconic headgear.",
            Rarity = CustomizeRarity.Legendary,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1017,
            ItemName = "Roman Centurion Helmet",
            ItemDesc = "A sign of leadership in the ancient Roman army.",
            Rarity = CustomizeRarity.Rare,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1018,
            ItemName = "Crown of Legends",
            ItemDesc = "Worn by those whose stories echo through time.",
            Rarity = CustomizeRarity.Legendary,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1019,
            ItemName = "Museum Guide Headset",
            ItemDesc = "The essential tool for guiding visitors to knowledge.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1020,
            ItemName = "Jester's Hat",
            ItemDesc = "Bring fun and laughter to your museum tours!",
            Rarity = CustomizeRarity.Rare,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1021,
            ItemName = "Chef's Hat",
            ItemDesc = "Perfect for running the museum's cafe with style.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1022,
            ItemName = "Ancient Guardian Mask",
            ItemDesc = "Wear the protection of an ancient sentinel.",
            Rarity = CustomizeRarity.Epic,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1023,
            ItemName = "Archaeologist's Hat",
            ItemDesc = "A classic hat for those who dig deep into history!",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });
        #endregion

        #region FACIALHAIR_ITEMS 3001-3018

        CustomizeItems.Add(new()
        {
            ID = 3001,
            ItemName = "Classic Beard",
            ItemDesc = "A timeless look for the modern gentleman.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3002,
            ItemName = "Handlebar Mustache",
            ItemDesc = "A bold statement for those with flair.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3003,
            ItemName = "Full Goatee",
            ItemDesc = "Sharp and sophisticated, perfect for the thinker.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3004,
            ItemName = "Soul Patch",
            ItemDesc = "A small touch of style below the lip.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3005,
            ItemName = "Chevron Mustache",
            ItemDesc = "Wide and bold, ideal for the confident.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3006,
            ItemName = "Short Boxed Beard",
            ItemDesc = "A neat and defined look for any occasion.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3007,
            ItemName = "Balbo Beard",
            ItemDesc = "Stylish and versatile, a favorite among icons.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3008,
            ItemName = "Stubble Beard",
            ItemDesc = "Effortlessly rugged and casual.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3009,
            ItemName = "Ducktail Beard",
            ItemDesc = "Tapered to perfection for a unique look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3010,
            ItemName = "Circle Beard",
            ItemDesc = "A rounded and polished choice for the refined.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3011,
            ItemName = "Horseshoe Mustache",
            ItemDesc = "A daring and tough style for the bold.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3012,
            ItemName = "Van Dyke Beard",
            ItemDesc = "Elegant and artistic, named after the great painter.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3013,
            ItemName = "French Fork Beard",
            ItemDesc = "A split-end style for the adventurous spirit.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3014,
            ItemName = "Extended Goatee",
            ItemDesc = "For those who want to enhance the classic goatee.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3015,
            ItemName = "Imperial Mustache",
            ItemDesc = "An extravagant look fit for royalty.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3016,
            ItemName = "Lumberjack Beard",
            ItemDesc = "Thick, full, and ready for the wilderness.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3017,
            ItemName = "Chin Curtain",
            ItemDesc = "A defining look for a distinctive face.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3018,
            ItemName = "Mutton Chops",
            ItemDesc = "Retro and bold, a choice for the daring.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });
        #endregion

        #region TORSO_ITEMS 4003-4029

        CustomizeItems.Add(new()
        {
            ID = 4003,
            ItemName = "Classic Torso",
            ItemDesc = "A basic torso design suitable for any exhibition.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4004,
            ItemName = "Visitor-Friendly Torso",
            ItemDesc = "This torso increases visitor attraction and comfort.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4005,
            ItemName = "Elegant Torso",
            ItemDesc = "A refined torso with a touch of sophistication.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4006,
            ItemName = "Robust Torso",
            ItemDesc = "A sturdy and dependable torso design.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4007,
            ItemName = "Modern Torso",
            ItemDesc = "A contemporary torso style that suits modern exhibits.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4008,
            ItemName = "Minimalist Torso",
            ItemDesc = "A sleek and simple torso design with minimal features.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4009,
            ItemName = "Vintage Torso",
            ItemDesc = "A classic torso design with a vintage touch.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4010,
            ItemName = "Sophisticated Torso",
            ItemDesc = "A torso design that exudes sophistication and style.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4011,
            ItemName = "Luxury Torso",
            ItemDesc = "A luxurious torso design that adds elegance to any space.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4012,
            ItemName = "Grand Torso",
            ItemDesc = "A grandiose torso that enhances the aesthetic of any museum.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4013,
            ItemName = "Innovative Torso",
            ItemDesc = "A unique torso design that stands out in any collection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4014,
            ItemName = "Creative Torso",
            ItemDesc = "A torso with bold and creative features.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4015,
            ItemName = "Stylish Torso",
            ItemDesc = "A fashionable torso design that appeals to modern tastes.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4016,
            ItemName = "Elegant Armor Torso",
            ItemDesc = "A torso with a sturdy and elegant armored appearance.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4017,
            ItemName = "Royal Torso",
            ItemDesc = "A royal and majestic torso design.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4018,
            ItemName = "Casual Torso",
            ItemDesc = "A relaxed and comfortable torso for informal settings.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4019,
            ItemName = "Sleek Torso",
            ItemDesc = "A sleek torso design with a modern finish.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4020,
            ItemName = "Bold Torso",
            ItemDesc = "A bold and striking torso that demands attention.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4021,
            ItemName = "Functional Torso",
            ItemDesc = "A practical and functional torso with a utilitarian design.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4022,
            ItemName = "Luxe Torso",
            ItemDesc = "A luxurious torso designed for the most opulent exhibits.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4023,
            ItemName = "Architectural Torso",
            ItemDesc = "A torso inspired by cutting-edge architectural design.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4024,
            ItemName = "Futuristic Torso",
            ItemDesc = "A torso designed with futuristic elements for a bold look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4025,
            ItemName = "Industrial Torso",
            ItemDesc = "A torso with industrial design elements, combining form and function.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4026,
            ItemName = "Classic Armor Torso",
            ItemDesc = "A torso designed with a classic armored appearance.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4027,
            ItemName = "Steampunk Torso",
            ItemDesc = "A torso with a steampunk style, featuring gears and vintage elements.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4028,
            ItemName = "Sculpted Torso",
            ItemDesc = "A torso design inspired by classical sculptures.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4029,
            ItemName = "Cultural Torso",
            ItemDesc = "A torso that reflects diverse cultural influences.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                // No changes here
            }
        });


        #endregion

        #region ARMUPPER_ITEMS 5001-5021

        CustomizeItems.Add(new()
        {
            ID = 5001,
            ItemName = "Arm Upper - Basic Armor",
            ItemDesc = "A basic armor piece for the upper arm. Provides minimal protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5002,
            ItemName = "Arm Upper - Reinforced Armor",
            ItemDesc = "A reinforced upper arm armor offering extra defense and durability.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5003,
            ItemName = "Arm Upper - Tactical Padding",
            ItemDesc = "Lightweight tactical padding to reduce impact on the upper arm.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5004,
            ItemName = "Arm Upper - Steel Guard",
            ItemDesc = "A steel guard that strengthens upper arm defenses against heavy blows.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5005,
            ItemName = "Arm Upper - Carbon Fiber Plate",
            ItemDesc = "A lightweight, high-durability carbon fiber plate for the upper arm.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5006,
            ItemName = "Arm Upper - Energy Shield",
            ItemDesc = "An advanced energy shield embedded in the upper arm for temporary protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5007,
            ItemName = "Arm Upper - Flame Resistant Armor",
            ItemDesc = "Flame-resistant armor to protect the upper arm in fiery environments.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5008,
            ItemName = "Arm Upper - Bulletproof Shield",
            ItemDesc = "A bulletproof shield attached to the upper arm to deflect projectiles.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5009,
            ItemName = "Arm Upper - Reinforced Exoskeleton",
            ItemDesc = "An exoskeleton that boosts the strength of the upper arm for heavy lifting.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5010,
            ItemName = "Arm Upper - Heavy Duty Armor",
            ItemDesc = "Heavy-duty armor designed for the upper arm, providing extreme protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5011,
            ItemName = "Arm Upper - Tactical Vest Attachment",
            ItemDesc = "A tactical vest attachment for additional upper arm protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5012,
            ItemName = "Arm Upper - Shock Absorber",
            ItemDesc = "Shock-absorbing padding to reduce the force of impacts to the upper arm.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5013,
            ItemName = "Arm Upper - Tactical Sleeve",
            ItemDesc = "A tactical sleeve for extra mobility while offering some protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5014,
            ItemName = "Arm Upper - Thermal Insulation",
            ItemDesc = "Thermal insulation for the upper arm, perfect for extreme temperatures.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5015,
            ItemName = "Arm Upper - Kinetic Absorber",
            ItemDesc = "A device that absorbs kinetic energy and redistributes it to enhance movement.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5016,
            ItemName = "Arm Upper - Stealth Mode Camo",
            ItemDesc = "Camouflage for the upper arm that blends with the surroundings for stealth.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5017,
            ItemName = "Arm Upper - High Velocity Armor",
            ItemDesc = "Armor designed for high-speed environments, reducing drag while protecting the arm.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5018,
            ItemName = "Arm Upper - Magnetic Shield",
            ItemDesc = "A magnetic shield attached to the upper arm that repels certain metals.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5019,
            ItemName = "Arm Upper - Smart Armor",
            ItemDesc = "Armor with integrated sensors that adapts to different threats.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5020,
            ItemName = "Arm Upper - Adaptive Combat Gear",
            ItemDesc = "Gear that adjusts to combat conditions to maximize performance in any situation.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5021,
            ItemName = "Arm Upper - Impact Resistant Plate",
            ItemDesc = "A special plate that resists high-impact forces, ideal for dangerous situations.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });
        #endregion

        #region ARMLOWER_ITEMS 7001-7019

        CustomizeItems.Add(new()
        {
            ID = 7001,
            ItemName = "Arm Lower - Ironclad Guard",
            ItemDesc = "A robust iron guard offering extra protection for the lower arm.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7002,
            ItemName = "Arm Lower - Razor Edge Bracer",
            ItemDesc = "A sleek bracer with sharp edges for precise combat movements.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7003,
            ItemName = "Arm Lower - Steelwind Protector",
            ItemDesc = "A wind-resistant lower arm guard made from high-quality steel.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7004,
            ItemName = "Arm Lower - Titan Grip Plate",
            ItemDesc = "A heavy-duty plate for the lower arm, designed to withstand immense force.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7005,
            ItemName = "Arm Lower - Phantom Shadow Guard",
            ItemDesc = "A lightweight lower arm guard designed for stealth and agility.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7006,
            ItemName = "Arm Lower - Dragon's Claw Protector",
            ItemDesc = "A fierce lower arm guard resembling a dragon's claw, perfect for close combat.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7007,
            ItemName = "Arm Lower - Vanguard Defender",
            ItemDesc = "A solid lower arm defender designed to protect during front-line combat.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7008,
            ItemName = "Arm Lower - Stormstrike Bracer",
            ItemDesc = "A bracer designed to channel storm energy into powerful strikes.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7009,
            ItemName = "Arm Lower - Guardian Shield",
            ItemDesc = "A sturdy shield designed to provide exceptional defense for the lower arm.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7010,
            ItemName = "Arm Lower - Frostbite Guard",
            ItemDesc = "A chilling lower arm guard imbued with the power of ice.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7011,
            ItemName = "Arm Lower - Blazing Fury Bracer",
            ItemDesc = "A fiery bracer designed to enhance offensive capabilities.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7012,
            ItemName = "Arm Lower - Reaver's Claw Plate",
            ItemDesc = "A brutal lower arm plate with jagged edges for cutting through enemies.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7013,
            ItemName = "Arm Lower - Specter Gauntlet",
            ItemDesc = "A ghostly gauntlet that grants enhanced stealth and agility.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7014,
            ItemName = "Arm Lower - Thunderstrike Bracer",
            ItemDesc = "A bracer capable of channeling lightning to enhance strike power.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7015,
            ItemName = "Arm Lower - Viper's Fang Guard",
            ItemDesc = "A venomous lower arm guard that strikes fear into enemies.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7016,
            ItemName = "Arm Lower - Crimson Talon Plate",
            ItemDesc = "A sharp-edged plate that tears through enemy defenses with ease.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7017,
            ItemName = "Arm Lower - Iron Fang Protector",
            ItemDesc = "A durable protector forged from iron, perfect for combat situations.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7018,
            ItemName = "Arm Lower - Blightborn Gauntlet",
            ItemDesc = "A gauntlet filled with dark power, corrupting those it strikes.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7019,
            ItemName = "Arm Lower - Celestial Bracer",
            ItemDesc = "A mystical bracer that glows with celestial energy, boosting agility.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });
        #endregion

        #region HAND_ITEMS 9001-9018

        CustomizeItems.Add(new()
        {
            ID = 9001,
            ItemName = "Gloves of the Phoenix",
            ItemDesc = "Gloves forged from the ashes of a phoenix, granting fiery power.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9002,
            ItemName = "Venomous Claw Gauntlets",
            ItemDesc = "Gauntlets with venomous claws that poison enemies with each strike.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9003,
            ItemName = "Dragon's Breath Gloves",
            ItemDesc = "Gloves imbued with the power of dragons, capable of summoning fire.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9004,
            ItemName = "Frostbite Gauntlets",
            ItemDesc = "Cold-as-ice gloves that freeze enemies on impact.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9005,
            ItemName = "Thunderstrike Gloves",
            ItemDesc = "Gauntlets that channel the power of lightning for electrifying attacks.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9006,
            ItemName = "Celestial Gloves",
            ItemDesc = "Gloves blessed by the stars, enhancing agility and perception.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9007,
            ItemName = "Viper Fang Gauntlets",
            ItemDesc = "Gauntlets designed to mimic the deadly precision of a viper’s strike.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9008,
            ItemName = "Gale Force Gloves",
            ItemDesc = "Gloves capable of creating powerful gusts of wind with every punch.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9009,
            ItemName = "Titan Gauntlets",
            ItemDesc = "Massive gloves forged from the strongest materials, providing immense strength.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9010,
            ItemName = "Spectral Touch Gloves",
            ItemDesc = "Ghostly gloves that grant the wearer the ability to phase through walls.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9011,
            ItemName = "Inferno Fist Gauntlets",
            ItemDesc = "Gauntlets imbued with a fiery inferno that enhances punching power.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9012,
            ItemName = "Shattered Earth Gloves",
            ItemDesc = "Gauntlets that harness the energy of the earth, creating tremors with every hit.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9013,
            ItemName = "Silent Strike Gloves",
            ItemDesc = "Gloves crafted for stealth, making no sound when attacking.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9014,
            ItemName = "Moonlit Gloves",
            ItemDesc = "Gloves that absorb moonlight to restore health during night battles.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9015,
            ItemName = "Hunter's Gauntlets",
            ItemDesc = "Gauntlets crafted for hunters, providing enhanced tracking and precision.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9016,
            ItemName = "Eagle's Grip Gauntlets",
            ItemDesc = "Gloves with the precision and strength of an eagle's talons.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9017,
            ItemName = "Crimson Fury Gauntlets",
            ItemDesc = "Gloves designed for battle, igniting the wearer's fury for greater damage.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9018,
            ItemName = "Cloak of the Phantom Gloves",
            ItemDesc = "Phantom-like gloves that grant invisibility when in shadows.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });


        #endregion

        #region HIP_ITEMS 11003-11029

        CustomizeItems.Add(new()
        {
            ID = 11003,
            ItemName = "Hip_Enhancer",
            ItemDesc = "A sleek item designed to enhance the shape of your hips.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11004,
            ItemName = "Hip_Belt",
            ItemDesc = "A stylish belt that accentuates the hips and waist.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11005,
            ItemName = "Hip_Jewelry",
            ItemDesc = "A decorative piece of jewelry that adds a touch of elegance to your hips.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11006,
            ItemName = "Hip_Pouch",
            ItemDesc = "A trendy pouch that hangs around the hips for a fashionable look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11007,
            ItemName = "Hip_Pants",
            ItemDesc = "A pair of pants designed to enhance your hips and give a flattering silhouette.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11008,
            ItemName = "Hip_Harness",
            ItemDesc = "A bold harness that draws attention to the hips.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11009,
            ItemName = "Hip_Wrap",
            ItemDesc = "A fabric wrap that adds a sophisticated touch to your hips.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11010,
            ItemName = "Hip_Corset",
            ItemDesc = "A corset designed to shape and accentuate the hips.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11011,
            ItemName = "Hip_ThighHighs",
            ItemDesc = "A pair of thigh-high socks that emphasize the curves of the hips.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11012,
            ItemName = "Hip_Ring",
            ItemDesc = "A chic ring designed to be worn around the hips.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11013,
            ItemName = "Hip_Legging",
            ItemDesc = "A pair of leggings that accentuate the hips and thighs.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11014,
            ItemName = "Hip_Clip",
            ItemDesc = "A clip that adds an edgy look to the hip area.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11015,
            ItemName = "Hip_Skirt",
            ItemDesc = "A stylish skirt that adds volume to the hip area.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11016,
            ItemName = "Hip_Bag",
            ItemDesc = "A small bag designed to be worn at the hip for practicality and style.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11017,
            ItemName = "Hip_Lace",
            ItemDesc = "A lace accessory that delicately wraps around the hips.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11018,
            ItemName = "Hip_Pads",
            ItemDesc = "Padded inserts designed to enhance the curves of the hips.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11019,
            ItemName = "Hip_Sash",
            ItemDesc = "A wide sash worn around the hips for added style.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11020,
            ItemName = "Hip_Gloves",
            ItemDesc = "Gloves that draw attention to the hip area, perfect for a unique style.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11021,
            ItemName = "Hip_Bracelet",
            ItemDesc = "A bracelet designed to rest at the hips, adding a touch of flair.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11022,
            ItemName = "Hip_Tassels",
            ItemDesc = "A pair of decorative tassels worn around the hips for extra flair.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11023,
            ItemName = "Hip_Cuffs",
            ItemDesc = "Cuffs worn around the hips for a bold, fashionable statement.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11024,
            ItemName = "Hip_FabricBelt",
            ItemDesc = "A fabric belt designed to highlight the hips.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11025,
            ItemName = "Hip_Shorts",
            ItemDesc = "Shorts that hug the hips, offering both comfort and style.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11026,
            ItemName = "Hip_FittedJacket",
            ItemDesc = "A fitted jacket designed to accentuate the hips and waistline.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11027,
            ItemName = "Hip_Chain",
            ItemDesc = "A chain that rests on the hips for a bold, trendy look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11028,
            ItemName = "Hip_FurTrim",
            ItemDesc = "A fur trim that adds texture and volume to the hips.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11029,
            ItemName = "Hip_Paint",
            ItemDesc = "A temporary body paint design that highlights the hips.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });
        #endregion

        #region LEG_ITEMS 12001-12020

        CustomizeItems.Add(new()
        {
            ID = 12001,
            ItemName = "Leg_Armor",
            ItemDesc = "A durable piece of armor that protects the legs during combat.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12002,
            ItemName = "Leg_Guards",
            ItemDesc = "Protective guards for your legs, providing both style and safety.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12003,
            ItemName = "Leg_Boots",
            ItemDesc = "Sturdy boots that offer great protection and comfort for long journeys.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12004,
            ItemName = "Leg_Pads",
            ItemDesc = "Padding for your legs that enhances agility and comfort.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12005,
            ItemName = "Leg_Leggings",
            ItemDesc = "Sleek leggings that provide freedom of movement and a stylish appearance.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12006,
            ItemName = "Leg_Gaiters",
            ItemDesc = "Stylish gaiters that protect your legs while adding a fashionable touch.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12007,
            ItemName = "Leg_Tights",
            ItemDesc = "Tight-fitting legwear that enhances mobility and provides a sleek look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12008,
            ItemName = "Leg_Braces",
            ItemDesc = "Supportive braces that stabilize the legs, reducing fatigue during activities.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12009,
            ItemName = "Leg_Socks",
            ItemDesc = "Comfortable socks that provide warmth and support for the legs.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12010,
            ItemName = "Leg_Spats",
            ItemDesc = "Trendy leg coverings that add style while protecting your legs.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12011,
            ItemName = "Leg_Bandages",
            ItemDesc = "Medical bandages designed to support and protect injured legs.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12012,
            ItemName = "Leg_Cuffs",
            ItemDesc = "Decorative cuffs worn around the legs to enhance fashion and comfort.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12013,
            ItemName = "Leg_Footwear",
            ItemDesc = "Footwear designed for comfort and style, perfect for long walks.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12014,
            ItemName = "Leg_Shorts",
            ItemDesc = "Comfortable shorts that offer freedom and flexibility for active movements.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12015,
            ItemName = "Leg_Thong",
            ItemDesc = "A minimalistic leg garment that offers great mobility while keeping things stylish.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12016,
            ItemName = "Leg_Pantyhose",
            ItemDesc = "Soft pantyhose that enhance the leg's appearance and comfort.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12017,
            ItemName = "Leg_Spats",
            ItemDesc = "An alternative to leggings, these spats provide a snug fit while allowing movement.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12018,
            ItemName = "Leg_Wraps",
            ItemDesc = "A versatile wrap that can be styled around the legs for both comfort and aesthetics.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12019,
            ItemName = "Leg_Fur",
            ItemDesc = "A luxurious fur accessory that adds a cozy and stylish look to your legs.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12020,
            ItemName = "Leg_Straps",
            ItemDesc = "Straps worn around the legs to add both style and support.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });
        #endregion

        #region HELMET_ITEMS 14001_14013

        CustomizeItems.Add(new()
        {
            ID = 14001,
            ItemName = "Basic Helmet",
            ItemDesc = "A simple, reliable helmet offering basic protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14002,
            ItemName = "Steel Helmet",
            ItemDesc = "A helmet made from steel, providing better protection against impacts.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14003,
            ItemName = "Spiked Helmet",
            ItemDesc = "A helmet with spikes, adding both style and a touch of danger.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14004,
            ItemName = "Knight's Helmet",
            ItemDesc = "A helmet used by knights, offering solid protection and a regal appearance.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14005,
            ItemName = "Viking Helmet",
            ItemDesc = "A horned helmet, inspired by the Vikings, symbolizing strength and courage.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14006,
            ItemName = "Leather Helmet",
            ItemDesc = "A lightweight leather helmet, offering basic protection with comfort.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14007,
            ItemName = "Explorer's Helmet",
            ItemDesc = "A helmet designed for explorers, equipped with a headlamp for night adventures.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14008,
            ItemName = "Combat Helmet",
            ItemDesc = "A military-style helmet designed for maximum protection during combat.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14009,
            ItemName = "Battle Helmet",
            ItemDesc = "A helmet designed for battles, offering great protection with a tough exterior.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14010,
            ItemName = "Cybernetic Helmet",
            ItemDesc = "A high-tech helmet with integrated cybernetic enhancements for tactical advantage.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14011,
            ItemName = "Cloak Helmet",
            ItemDesc = "A lightweight helmet designed for stealth and agility in various environments.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14012,
            ItemName = "Samurai Helmet",
            ItemDesc = "A helmet with ancient samurai design, combining protection with honor.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14013,
            ItemName = "Pirate Helmet",
            ItemDesc = "A helmet resembling the ones worn by pirates, complete with an eye patch.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });
        #endregion

        #region EYEBROW_ITEMS 102001-102010

        CustomizeItems.Add(new()
        {
            ID = 102001,
            ItemName = "Natural Eyebrows",
            ItemDesc = "A pair of natural eyebrows with a slight arch, perfect for a neutral look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 102002,
            ItemName = "Thick Eyebrows",
            ItemDesc = "Bold and thick eyebrows that make a strong statement.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 102003,
            ItemName = "Arched Eyebrows",
            ItemDesc = "Perfectly arched eyebrows that add a touch of elegance and definition.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 102004,
            ItemName = "Straight Eyebrows",
            ItemDesc = "Straight, natural eyebrows that create a minimalist, sleek look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 102005,
            ItemName = "Curved Eyebrows",
            ItemDesc = "Softly curved eyebrows for a gentle, welcoming appearance.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 102006,
            ItemName = "Thin Eyebrows",
            ItemDesc = "Delicate, thin eyebrows that add subtle charm to your face.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 102007,
            ItemName = "High Arched Eyebrows",
            ItemDesc = "Bold, high arched eyebrows for a striking and dramatic look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 102008,
            ItemName = "Rounded Eyebrows",
            ItemDesc = "Soft, rounded eyebrows that enhance a friendly and youthful expression.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 102009,
            ItemName = "Faded Eyebrows",
            ItemDesc = "Lightly faded eyebrows that offer a subtle and natural appearance.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 102010,
            ItemName = "Bold Eyebrows",
            ItemDesc = "Thick, bold eyebrows that make a bold and confident statement.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });
        #endregion

        #region HAIR_ITEMS 103001-103038

        CustomizeItems.Add(new()
        {
            ID = 103001,
            ItemName = "Short Curly Hair",
            ItemDesc = "A stylish short curly hair that adds a youthful appearance.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103002,
            ItemName = "Long Straight Hair",
            ItemDesc = "A sleek and long straight hair for a more elegant look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103003,
            ItemName = "Messy Hair",
            ItemDesc = "A messy, tousled hairstyle for a carefree vibe.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103004,
            ItemName = "Bun Hairstyle",
            ItemDesc = "A neat bun hairstyle perfect for a polished appearance.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103005,
            ItemName = "Ponytail",
            ItemDesc = "A simple and practical ponytail hairstyle for an active look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103006,
            ItemName = "Undercut",
            ItemDesc = "An edgy undercut hairstyle for those who like to stand out.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103007,
            ItemName = "Wavy Bob",
            ItemDesc = "A short, wavy bob for a chic, fashionable look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103008,
            ItemName = "Afro",
            ItemDesc = "A bold, full-bodied afro hairstyle that brings character.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103009,
            ItemName = "Buzz Cut",
            ItemDesc = "A simple and clean buzz cut for a no-fuss style.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103010,
            ItemName = "Braids",
            ItemDesc = "Stylish braids to add some texture and personality.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103011,
            ItemName = "Top Knot",
            ItemDesc = "A trendy top knot for a laid-back yet fashionable look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103012,
            ItemName = "Side Swept Hair",
            ItemDesc = "A sophisticated side-swept look for an elegant style.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103013,
            ItemName = "Spiked Hair",
            ItemDesc = "A bold, spiked hairstyle that brings energy to any look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103014,
            ItemName = "Mohawk",
            ItemDesc = "A daring mohawk hairstyle for those who want to stand out.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103015,
            ItemName = "Flat Top",
            ItemDesc = "A stylish flat top haircut, perfect for a retro look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103016,
            ItemName = "Pompadour",
            ItemDesc = "A sleek pompadour hairstyle for a polished, vintage look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103017,
            ItemName = "Quiff",
            ItemDesc = "A voluminous quiff hairstyle that adds style and flair.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103018,
            ItemName = "Flat Hair",
            ItemDesc = "A straight and flat hairstyle for a minimalistic appearance.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103019,
            ItemName = "Curls",
            ItemDesc = "Full-bodied curls that add volume and personality.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103020,
            ItemName = "Afro Puff",
            ItemDesc = "A puffy, voluminous afro style that exudes confidence.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103021,
            ItemName = "French Braid",
            ItemDesc = "An elegant French braid hairstyle perfect for any occasion.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103022,
            ItemName = "Double Ponytail",
            ItemDesc = "A playful double ponytail hairstyle for a cute and fun look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103023,
            ItemName = "Pixie Cut",
            ItemDesc = "A short and chic pixie cut that adds a modern touch.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103024,
            ItemName = "Wavy Hair",
            ItemDesc = "Loose waves that bring a relaxed and natural vibe.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103025,
            ItemName = "Messy Bun",
            ItemDesc = "A casual, messy bun perfect for a laid-back day.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103026,
            ItemName = "Long Curly Hair",
            ItemDesc = "Long, flowing curls for a dramatic, bold look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103027,
            ItemName = "Shaggy Hair",
            ItemDesc = "A shaggy hairstyle for a rock and roll vibe.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103028,
            ItemName = "Feathered Hair",
            ItemDesc = "Feathered layers that create a soft, voluminous look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103029,
            ItemName = "Choppy Bob",
            ItemDesc = "A modern choppy bob with edgy layers.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103030,
            ItemName = "Slicked Back Hair",
            ItemDesc = "Slicked-back hair for a smooth, polished appearance.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103031,
            ItemName = "Beach Waves",
            ItemDesc = "Loose, beachy waves for a carefree, summer look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103032,
            ItemName = "High Ponytail",
            ItemDesc = "A high ponytail hairstyle for a dynamic and energetic appearance.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103033,
            ItemName = "Braided Top Knot",
            ItemDesc = "A combination of a braid and top knot for a unique style.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103034,
            ItemName = "Half Up Half Down",
            ItemDesc = "A relaxed half-up, half-down hairstyle for an effortless look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103035,
            ItemName = "Double Buns",
            ItemDesc = "Two fun buns for a playful and cute appearance.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103036,
            ItemName = "Loose Waves",
            ItemDesc = "Loose waves for a relaxed, effortless beauty.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });
        CustomizeItems.Add(new()
        {
            ID = 103037,
            ItemName = "Twisted Updo",
            ItemDesc = "A stylish twisted updo perfect for formal occasions.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103038,
            ItemName = "Loose Curls",
            ItemDesc = "Loose curls that create a soft and voluminous look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });
        #endregion

        #region HAT_ITEMS 104001-140024

        CustomizeItems.Add(new()
        {
            ID = 104001,
            ItemName = "Classic Top Hat",
            ItemDesc = "A timeless classic top hat for any occasion.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104002,
            ItemName = "Baseball Cap",
            ItemDesc = "A casual baseball cap for a relaxed look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104003,
            ItemName = "Cowboy Hat",
            ItemDesc = "A rugged cowboy hat, perfect for the wild west.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104004,
            ItemName = "Fedora",
            ItemDesc = "A stylish fedora hat for a sharp, sophisticated look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104005,
            ItemName = "Beanie",
            ItemDesc = "A warm beanie for cold weather, keeping you cozy and stylish.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104006,
            ItemName = "Bucket Hat",
            ItemDesc = "A laid-back bucket hat for sunny days and outdoor adventures.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104007,
            ItemName = "Panama Hat",
            ItemDesc = "A light and breathable Panama hat for tropical climates.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104008,
            ItemName = "Top Knot Headband",
            ItemDesc = "A headband with a top knot for a cute and practical accessory.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104009,
            ItemName = "Visor Cap",
            ItemDesc = "A sporty visor cap to keep the sun out of your eyes.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104010,
            ItemName = "Beret",
            ItemDesc = "A fashionable beret that adds a touch of French chic.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104011,
            ItemName = "Biker Helmet",
            ItemDesc = "A durable helmet designed for bikers and adventurers.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104012,
            ItemName = "Visor Hat",
            ItemDesc = "A classic visor hat with an adjustable strap for comfort.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104013,
            ItemName = "Sun Hat",
            ItemDesc = "A wide-brimmed sun hat to protect you from the sun's rays.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104014,
            ItemName = "Cloche Hat",
            ItemDesc = "A vintage cloche hat that adds a touch of elegance to your look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104015,
            ItemName = "Snow Hat",
            ItemDesc = "A warm hat designed for snow and cold weather.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104016,
            ItemName = "Safari Hat",
            ItemDesc = "A practical safari hat with a wide brim for outdoor exploration.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104017,
            ItemName = "Newsboy Cap",
            ItemDesc = "A classic newsboy cap for a stylish yet casual look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104018,
            ItemName = "Trilby Hat",
            ItemDesc = "A sharp and stylish trilby hat, perfect for a night out.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104019,
            ItemName = "Bowler Hat",
            ItemDesc = "A formal bowler hat that adds a sophisticated touch.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104020,
            ItemName = "Hiking Cap",
            ItemDesc = "A durable hiking cap that provides protection for your outdoor adventures.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104021,
            ItemName = "Hooded Cap",
            ItemDesc = "A cap with a hood for extra warmth and protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104022,
            ItemName = "Retro Baseball Cap",
            ItemDesc = "A vintage-style baseball cap with a throwback design.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104023,
            ItemName = "Detective Hat",
            ItemDesc = "A mysterious detective hat for those who like to solve puzzles.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104024,
            ItemName = "Crown Hat",
            ItemDesc = "A regal crown to wear like royalty.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });
        #endregion

        #region MASK_ITEMS 105001-105004
        CustomizeItems.Add(new()
        {
            ID = 105001,
            ItemName = "Basic Mask",
            ItemDesc = "A simple mask for basic protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 105002,
            ItemName = "Sporty Mask",
            ItemDesc = "A sleek mask designed for active use.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 105003,
            ItemName = "Mystic Mask",
            ItemDesc = "A mysterious mask that enhances your aura.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 105004,
            ItemName = "Elegant Mask",
            ItemDesc = "A sophisticated mask with a touch of class.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });
        #endregion

        #region HELMETATTACHMENT_ITEMS 106001-106013

        CustomizeItems.Add(new()
        {
            ID = 106001,
            ItemName = "Feathered Plume",
            ItemDesc = "A decorative feathered plume to add flair to your helmet.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106002,
            ItemName = "Horsehair Crest",
            ItemDesc = "A striking crest made from horsehair, perfect for showing off.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106003,
            ItemName = "Golden Tasseled Ornament",
            ItemDesc = "A luxurious golden tassel to enhance your helmet's appearance.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106004,
            ItemName = "Mystic Feather",
            ItemDesc = "A mystical feather that adds a magical touch to your helmet.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106005,
            ItemName = "Spiked Crest",
            ItemDesc = "A spiked crest that gives your helmet a fierce, intimidating look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106006,
            ItemName = "Lunar Horn",
            ItemDesc = "A small horn-like accessory with a glow, symbolizing lunar power.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106007,
            ItemName = "Crimson Feather",
            ItemDesc = "A fiery red feather that adds a bold touch to your helmet.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106008,
            ItemName = "Silver Tassel",
            ItemDesc = "A sleek silver tassel that shimmers under the light.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106009,
            ItemName = "Mystic Mane",
            ItemDesc = "A flowing mane that gives your helmet a majestic look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106010,
            ItemName = "Golden Horn",
            ItemDesc = "A shining golden horn, perfect for making a bold statement.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106011,
            ItemName = "Wind Dancer Plume",
            ItemDesc = "A fine feathered plume that seems to dance in the wind.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106012,
            ItemName = "Celestial Feather",
            ItemDesc = "A feather said to come from the heavens, bringing luck and grace.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106013,
            ItemName = "Ember Crest",
            ItemDesc = "A fiery crest that adds a flame-like appearance to your helmet.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });
        #endregion

        #region BACKATTACHMENT_ITEMS 107001-107015

        CustomizeItems.Add(new()
        {
            ID = 107001,
            ItemName = "Phoenix Wings",
            ItemDesc = "Majestic wings that radiate fiery light, adding an ethereal glow to your character.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107002,
            ItemName = "Dragon Tail",
            ItemDesc = "A powerful and imposing dragon tail, perfect for showing off your fierceness.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107003,
            ItemName = "Angel's Halo",
            ItemDesc = "A glowing halo that hovers above your character, emanating a divine aura.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107004,
            ItemName = "Wolf Fur",
            ItemDesc = "A wild and untamed wolf fur draped over your back, symbolizing strength and courage.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107005,
            ItemName = "Feathered Mantle",
            ItemDesc = "A cloak made of soft feathers, giving a graceful and mysterious appearance.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107006,
            ItemName = "Luminous Wings",
            ItemDesc = "Radiant wings that emit a soft glow, casting light in dark places.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107007,
            ItemName = "Storm Cloud Cloak",
            ItemDesc = "A cloak that mimics the storm clouds, creating a powerful presence around you.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107008,
            ItemName = "Shadowy Veil",
            ItemDesc = "A dark veil that obscures your figure, making you seem mysterious and elusive.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107009,
            ItemName = "Celestial Cape",
            ItemDesc = "A cape that seems to be made of the night sky, dotted with sparkling stars.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107010,
            ItemName = "Golden Banner",
            ItemDesc = "A shining golden banner draped across your back, a symbol of grandeur.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107011,
            ItemName = "Eagle Feathers",
            ItemDesc = "A collection of eagle feathers, adding a sense of freedom and pride.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107012,
            ItemName = "Fire Tail",
            ItemDesc = "A tail engulfed in flames, giving off heat and light as it flicks around.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107013,
            ItemName = "Silken Shawl",
            ItemDesc = "A soft and flowing shawl that adds elegance and refinement to your outfit.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107014,
            ItemName = "Vine Cloak",
            ItemDesc = "A cloak woven from living vines, blending nature and style.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107015,
            ItemName = "Silver Wings",
            ItemDesc = "Gleaming silver wings that add an angelic aura to your appearance.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });
        #endregion

        #region SHOULDERATTACHMENT_ITEMS 108001-108021

        CustomizeItems.Add(new()
        {
            ID = 108001,
            ItemName = "ShoulderPad_1",
            ItemDesc = "A standard shoulder pad for basic protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108002,
            ItemName = "ShoulderPad_2",
            ItemDesc = "A reinforced shoulder pad offering better durability.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108003,
            ItemName = "ShoulderPad_3",
            ItemDesc = "An advanced shoulder pad with added defense capabilities.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108004,
            ItemName = "ShoulderPad_4",
            ItemDesc = "A premium shoulder pad with enhanced durability and design.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108005,
            ItemName = "ShoulderPad_5",
            ItemDesc = "A reinforced shoulder pad with superior defense and style.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108006,
            ItemName = "ShoulderPad_6",
            ItemDesc = "A lightweight shoulder pad that offers moderate protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108007,
            ItemName = "ShoulderPad_7",
            ItemDesc = "A tactical shoulder pad designed for maximum agility and protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108008,
            ItemName = "ShoulderPad_8",
            ItemDesc = "An experimental shoulder pad with futuristic design and strength.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108009,
            ItemName = "ShoulderPad_9",
            ItemDesc = "A battle-tested shoulder pad that increases defense against heavy attacks.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108010,
            ItemName = "ShoulderPad_10",
            ItemDesc = "A shoulder pad made from lightweight materials for increased mobility.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108011,
            ItemName = "ShoulderPad_11",
            ItemDesc = "A custom shoulder pad designed for comfort and durability.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108012,
            ItemName = "ShoulderPad_12",
            ItemDesc = "A premium shoulder pad with adaptive protection that molds to the user.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108013,
            ItemName = "ShoulderPad_13",
            ItemDesc = "An ultra-modern shoulder pad with superior defense and mobility.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108014,
            ItemName = "ShoulderPad_14",
            ItemDesc = "A high-tech shoulder pad with integrated energy shields for enhanced protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108015,
            ItemName = "ShoulderPad_15",
            ItemDesc = "A stylish shoulder pad that offers both fashion and protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108016,
            ItemName = "ShoulderPad_16",
            ItemDesc = "A tactical shoulder pad designed for high-intensity combat situations.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108017,
            ItemName = "ShoulderPad_17",
            ItemDesc = "An ergonomic shoulder pad that provides enhanced comfort and protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108018,
            ItemName = "ShoulderPad_18",
            ItemDesc = "A state-of-the-art shoulder pad with integrated armor plating for added defense.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108019,
            ItemName = "ShoulderPad_19",
            ItemDesc = "A sleek shoulder pad with enhanced mobility and protection for fast-paced combat.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108020,
            ItemName = "ShoulderPad_20",
            ItemDesc = "A durable shoulder pad with superior defense against all types of attacks.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108021,
            ItemName = "ShoulderPad_21",
            ItemDesc = "A custom-designed shoulder pad that combines style and high-performance protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });
        #endregion

        #region ELBOWATTACHMENT_ITEMS 110001-110006

        CustomizeItems.Add(new()
        {
            ID = 110001,
            ItemName = "Basic Elbow Guard",
            ItemDesc = "A simple elbow guard providing basic protection during movement.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 110002,
            ItemName = "Standard Elbow Guard",
            ItemDesc = "A standard elbow guard offering moderate protection for casual activities.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 110003,
            ItemName = "Reinforced Elbow Guard",
            ItemDesc = "An elbow guard with reinforced padding for added protection in active scenarios.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 110004,
            ItemName = "Premium Elbow Guard",
            ItemDesc = "A premium elbow guard offering exceptional comfort and protection for intense use.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 110005,
            ItemName = "Tactical Elbow Guard",
            ItemDesc = "A tactical elbow guard designed for heavy-duty protection in high-risk environments.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 110006,
            ItemName = "Lightweight Elbow Guard",
            ItemDesc = "A lightweight elbow guard designed for quick movements while still offering moderate protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion

        #region HIPATTACHMENT_ITEMS 112001-112012

        CustomizeItems.Add(new()
        {
            ID = 112001,
            ItemName = "BasicHipProtector",
            ItemDesc = "A simple hip protector that offers basic comfort and protection for everyday use.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112002,
            ItemName = "StandardHipProtector",
            ItemDesc = "A standard hip protector designed for moderate protection during casual activities.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112003,
            ItemName = "ReinforcedHipProtector",
            ItemDesc = "A reinforced hip protector offering additional protection for more active situations.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112004,
            ItemName = "PremiumHipProtector",
            ItemDesc = "A premium hip protector designed for high comfort and protection during intense use.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112005,
            ItemName = "TacticalHipProtector",
            ItemDesc = "A tactical hip protector built for maximum protection in high-risk environments.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112006,
            ItemName = "LightweightHipProtector",
            ItemDesc = "A lightweight hip protector that offers a good balance between comfort and mobility.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112007,
            ItemName = "AdaptiveHipProtector",
            ItemDesc = "An adaptive hip protector that adjusts to the user's movements for a custom fit.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112008,
            ItemName = "SportHipProtector",
            ItemDesc = "A sport-specific hip protector designed for athletes to prevent injuries during high-impact activities.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112009,
            ItemName = "UltraFlexHipProtector",
            ItemDesc = "An ultra-flexible hip protector designed for optimal movement without sacrificing protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112010,
            ItemName = "HeavyDutyHipProtector",
            ItemDesc = "A heavy-duty hip protector designed to withstand tough conditions and provide top-level protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112011,
            ItemName = "EnduranceHipProtector",
            ItemDesc = "An endurance-focused hip protector that provides lasting comfort and protection for extended wear.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112012,
            ItemName = "ProHipProtector",
            ItemDesc = "A professional-grade hip protector designed for high-performance use in extreme conditions.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });
        #endregion

        #region KNEEATTACHMENT_ITEMS 113001-113011

        CustomizeItems.Add(new()
        {
            ID = 113001,
            ItemName = "Basic Knee Guard",
            ItemDesc = "A simple knee guard providing basic protection during movement.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113002,
            ItemName = "Standard Knee Guard",
            ItemDesc = "A standard knee guard offering moderate protection for casual activities.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113003,
            ItemName = "Reinforced Knee Guard",
            ItemDesc = "A knee guard with reinforced padding for added protection in active scenarios.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113004,
            ItemName = "Premium Knee Guard",
            ItemDesc = "A premium knee guard offering exceptional comfort and protection for intense use.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113005,
            ItemName = "Tactical Knee Guard",
            ItemDesc = "A tactical knee guard designed for heavy-duty protection in high-risk environments.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113006,
            ItemName = "Lightweight Knee Guard",
            ItemDesc = "A lightweight knee guard designed for quick movements while still offering moderate protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113007,
            ItemName = "Shock Absorbent Knee Guard",
            ItemDesc = "A knee guard with advanced shock absorption to reduce impact during intense actions.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113008,
            ItemName = "Heavy Duty Knee Guard",
            ItemDesc = "A heavy-duty knee guard providing maximum protection for the most extreme environments.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113009,
            ItemName = "Ergonomic Knee Guard",
            ItemDesc = "An ergonomic knee guard designed for optimal comfort and movement without sacrificing protection.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113010,
            ItemName = "Slim Fit Knee Guard",
            ItemDesc = "A slim fit knee guard offering discreet protection while maintaining full range of motion.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113011,
            ItemName = "Elite Knee Guard",
            ItemDesc = "An elite knee guard offering superior protection and comfort for high-performance activities.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });


        #endregion

        #region ELFEAR_ITEMS 115001-115003

        CustomizeItems.Add(new()
        {
            ID = 115001,
            ItemName = "Basic Elf Ears",
            ItemDesc = "Simple, natural-looking elf ears for a classic fantasy appearance.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 115002,
            ItemName = "Elegant Elf Ears",
            ItemDesc = "Sleek and refined elf ears designed for a more sophisticated and graceful look.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 115003,
            ItemName = "Mystic Elf Ears",
            ItemDesc = "Mysterious and otherworldly elf ears with intricate designs, perfect for magical beings.",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });
        #endregion

        UpdateUnlockedItemsPrice();
    }
    public void UpdateUnlockedItemsPrice()
    {
        int length = CustomizeItems.Count;
        for (int i = 0; i < length; i++)
        {
            int unlockedElementIdsCount = characterCustomizeData.unlockedCustomizeElementIDs.Count <= 0 ? 1 : characterCustomizeData.unlockedCustomizeElementIDs.Count;
            int levelMultiplierValue = (MuseumManager.instance.GetCurrentCultureLevel() / 10) <= 1 ? 1 : Mathf.FloorToInt(MuseumManager.instance.GetCurrentCultureLevel() / 10);

            CustomizeItem currentItem = CustomizeItems[i];
            currentItem.UnlockPrice = (unlockedElementIdsCount * 10) + baseUnlockedItemPrice + ( levelMultiplierValue * baseUnlockedItemPrice );
            CustomizeItems[i] = currentItem;
        }
    }
    public CustomizeItem GetItemWithID(int _id)
    {
        foreach (var item in CustomizeItems)
        {
            if (item.ID == _id)
            {
                return new(item);
            }
        }

        return CustomizeItems[0];
    }
}

[System.Serializable]
public class CustomizeElement
{
    public CustomizeSlot customizeSlot;
    public int elementID;
}

[System.Serializable]
public class PlayerExtraCustomizeData
{
    public int ID;
    public bool isFemale;
    public List<Color> Colors;
    public List<CustomizeElement> CustomizeElements;

    public CustomizeElement GetEquippedElementBySlot(CustomizeSlot _slot)
    {
        foreach (var item in CustomizeElements)
            if (item.customizeSlot == _slot)
                return item;

        return new();
    }

    public void AddOrOverrideCustomizeElement(CustomizeSlot _slot, int _id)
    {
        bool contains = false;
        foreach (var item in CustomizeElements)
        {
            if (item.customizeSlot == _slot)
            {
                contains = true;
                item.elementID = _id;
                break;
            }
        }

        if(!contains)
            CustomizeElements.Add(new() { customizeSlot = _slot, elementID = _id });
    }
}

[System.Serializable]
public class PlayerCustomizeData
{
    public int selectedCustomizeSlot;
    public List<PlayerExtraCustomizeData> AllCustomizeData;
    #region Constructors
    public PlayerCustomizeData(int _selectedCustomizeSlot, List<PlayerExtraCustomizeData> _allCustomizeData)
    {
        selectedCustomizeSlot = _selectedCustomizeSlot;
        AllCustomizeData = new();
        AllCustomizeData.Clear();
        int length = _allCustomizeData.Count;
        for (int i = 0; i < length; i++)
            AllCustomizeData.Add(_allCustomizeData[i]);
    }
    public PlayerCustomizeData(PlayerCustomizeData _copy)
    {
        selectedCustomizeSlot= _copy.selectedCustomizeSlot;
        AllCustomizeData.Clear();
        int length = _copy.AllCustomizeData.Count;
        for (int i = 0; i < length; i++)
            AllCustomizeData.Add(_copy.AllCustomizeData[i]);
    }
    public PlayerCustomizeData() { }
    #endregion
}

[System.Serializable]
public struct CustomizeItem
{
    public int ID;
    public string ItemName;
    public string ItemDesc;
    public CustomizeRarity Rarity;
    public List<CustomizeBonus> Bonuses;
    public float UnlockPrice;

    public CustomizeItem(CustomizeItem _item)
    {
        ID = _item.ID;
        ItemName = _item.ItemName;
        ItemDesc = _item.ItemDesc;
        Rarity = _item.Rarity;
        UnlockPrice = _item.UnlockPrice;
        Bonuses = new();
        foreach (var item in _item.Bonuses)
        {
            Bonuses.Add(new()
            {
                Stat = item.Stat,
                Amount = item.Amount,
            });
        }    
    }
}

[System.Serializable]
public class CustomizeBonus
{
    public eStat Stat;
    public float Amount;
}

public enum CustomizeRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
}

public enum CustomizeSlot
{
    None = 0,
    Head = 1,
    FacialHair = 3,
    Torso = 4,
    Arm_Upper_Right = 5,
    Arm_Upper_Left = 6,
    Arm_Lower_Right = 7,
    Arm_Lower_Left = 8,
    Hand_Right = 9,
    Hand_Left = 10,
    Hip = 11,
    Leg_Right = 12,
    Leg_Left = 13,
    Helmet = 14,
    Eyebrows = 102,
    Hair = 103,
    Hat = 104,
    Mask = 105,
    Helmet_Attachment = 106,
    Back_Attachment = 107,
    Shoulder_Attachment_Right = 108,
    Shoulder_Attachment_Left = 109,
    Elbow_Attachment_Right = 110,
    Elbow_Attachment_Left = 111,
    Hip_Attachment = 112,
    Knee_Attachment_Right = 113,
    Knee_Attachment_Left = 114,
    Elf_Ear = 115,
}

public enum CustomizeColor
{
    _Color_Primary,
	_Color_Secondary,
	_Color_Leather_Primary,
	_Color_Metal_Primary,
	_Color_Leather_Secondary,
	_Color_Metal_Dark,
	_Color_Metal_Secondary,
	_Color_Hair,
	_Color_Skin,
	_Color_Stubble,
	_Color_Scar,
	_Color_BodyArt,
	_Color_Eyes,
}
[System.Serializable]
public class CharacterCustomizeData
{
    public PlayerCustomizeData playerCustomizeData;
    public List<int> unlockedCustomizeElementIDs;
    public int LastSelectedCustomizeCategory;
    public int LastSelectedCustomizeHeader;
    public int LastSelectedColorHeader;

    public void SetDatas(PlayerCustomizeData _playerCustomizeData, List<int> _unlockedCustomizeElementIDs, int _lastSelectedCustomizeCategory, int _lastSelectedCustomizeHeader, int _lastSelectedColorHeader)
    {
        playerCustomizeData = _playerCustomizeData;
        unlockedCustomizeElementIDs = new();
        unlockedCustomizeElementIDs.Clear();
        int length = _unlockedCustomizeElementIDs.Count;
        for (int i = 0; i < length; i++)
            unlockedCustomizeElementIDs.Add(_unlockedCustomizeElementIDs[i]);
        LastSelectedCustomizeCategory = _lastSelectedCustomizeCategory;
        LastSelectedCustomizeHeader = _lastSelectedCustomizeHeader;
        LastSelectedColorHeader = _lastSelectedColorHeader;
    }
}