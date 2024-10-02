using Cinemachine;
using System;
using System.Collections.Generic;
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
    public Text SelectedHeaderText;

    [Header("Runtime Datas")]
    [SerializeField] private List<int> LockedElements = new();
    [SerializeField] private PlayerExtraCustomizeData TempCustomize;

    [SerializeField] private List<int> CurrentlyUnlockedIDs = new();

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
        UIInteractHandler.instance.AskQuestion("Karakter Özelleþtirmesi #Çeviri", "Deðiþiklikleri onaylýyor musunuz? #Çeviri",
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

        //SelectedHeaderText.text = LanguageDatabase.instance.GetText("CustomizeSlot_" + (int)_slot);
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
            string header2 = "Tanimlanmasi Gerek Header! Func: OnSelectedAnEquipment";
            string explanation2 = "Tanimlanmasi Gerek Desc! Func: OnSelectedAnEquipment";
            UIInteractHandler.instance.AskQuestion(header2, explanation2, null, null, null, null, null, null);
            return;
        }
        PlayChangeGenderSound();
        TempCustomize.AddOrOverrideCustomizeElement(_slot, _id);
        UpdateUI();
    }

    public void OnSelectedAnRightEquipment(CustomizeSlot _slot, int _id)
    {
        //Satin alma olayini yapma sadece kilitli de.
        if (LockedElements.Contains(_id) || LockedElements.Contains(_id + 100) || LockedElements.Contains(_id - 100))
        {
            string header2 = "Tanimlanmasi Gerek Header! Func: OnSelectedAnEquipment";
            string explanation2 = "Tanimlanmasi Gerek Desc! Func: OnSelectedAnEquipment";
            UIInteractHandler.instance.AskQuestion(header2, explanation2, null, null, null, null, null, null);
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
            string header2 = "Tanimlanmasi Gerek Header! Func: OnSelectedAnEquipment";
            string explanation2 = "Tanimlanmasi Gerek Desc! Func: OnSelectedAnEquipment";
            UIInteractHandler.instance.AskQuestion(header2, explanation2, null, null, null, null, null, null);
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
        AppearanceButton.transform.GetChild(0).GetComponent<Text>().text = "Gorunum Buttonu Texti";
        ColorsButton.transform.GetChild(0).GetComponent<Text>().text = "Renkler button Texti";
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
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1002,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1004,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1005,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1006,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1007,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1008,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1009,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1010,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1011,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1012,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1013,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1014,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1015,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1016,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1017,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1018,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1019,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1020,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1021,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1022,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 1023,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });
        #endregion

        #region FACIALHAIR_ITEMS 3001-3018

        CustomizeItems.Add(new()
        {
            ID = 3001,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3002,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3004,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3005,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3006,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3007,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3008,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3009,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3010,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3011,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3012,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3013,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3014,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3015,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3016,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3017,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 3018,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion

        #region TORSO_ITEMS 4003-4029

        CustomizeItems.Add(new()
        {
            ID = 4003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                new(){ Stat = eStat.VisitorCapacity, Amount = 30 }, //test
                new(){ Stat = eStat.BaseHappiness, Amount = 10 },
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4004,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
            {
                new(){ Stat = eStat.VisitorCapacity, Amount = 10 }, //test
                new(){ Stat = eStat.MuseumEnterPrice, Amount = 10 },
            }
        });

        CustomizeItems.Add(new()
        {
            ID = 4005,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4006,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4007,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4008,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4009,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4010,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4011,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4012,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4013,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4014,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4015,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4016,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4017,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4018,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4019,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4020,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4021,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4022,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4023,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4024,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4025,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4026,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4027,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4028,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 4029,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion

        #region ARMUPPER_ITEMS 5001-5021

        CustomizeItems.Add(new()
        {
            ID = 5001,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5002,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5004,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5005,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5006,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5007,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5008,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5009,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5010,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5011,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5012,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5013,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5014,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5015,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5016,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5017,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5018,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5019,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5020,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 5021,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion

        #region ARMLOWER_ITEMS 7001-7019

        CustomizeItems.Add(new()
        {
            ID = 7001,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7002,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7004,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7005,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7006,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7007,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7008,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7009,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7010,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7011,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7012,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7013,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7014,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7015,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7016,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7017,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7018,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 7019,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion

        #region HAND_ITEMS 9001-9018

        CustomizeItems.Add(new()
        {
            ID = 9001,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9002,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9004,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9005,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9006,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9007,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9008,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9009,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9010,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9011,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9012,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9013,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9014,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9015,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9016,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9017,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 9018,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion

        #region HIP_ITEMS 11003-11029

        CustomizeItems.Add(new()
        {
            ID = 11003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11004,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11005,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11006,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11007,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11008,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11009,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11010,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11011,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11012,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11013,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11014,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11015,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11016,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11017,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11018,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11019,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11020,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11021,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11022,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11023,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11024,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11025,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11026,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11027,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11028,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 11029,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion

        #region LEG_ITEMS 12001-12020

        CustomizeItems.Add(new()
        {
            ID = 12001,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12002,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12004,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12005,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12006,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12007,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12008,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12009,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12010,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12011,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12012,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12013,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12014,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12015,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12016,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12017,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12018,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12019,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 12020,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion

        #region HELMET_ITEMS 14001_14013

        CustomizeItems.Add(new()
        {
            ID = 14001,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14002,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14004,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14005,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14006,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14007,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14008,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14009,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14010,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14011,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14012,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 14013,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion

        #region EYEBROW_ITEMS 102001-102010

        CustomizeItems.Add(new()
        {
            ID = 102001,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 102002,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 102003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 102004,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 102005,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 102006,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 102007,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 102008,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 102009,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 102010,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion

        #region HAIR_ITEMS 103001-103038

        CustomizeItems.Add(new()
        {
            ID = 103001,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103002,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103004,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103005,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103006,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103007,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103008,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103009,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103010,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103011,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103012,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103013,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103014,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103015,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103016,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103017,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103018,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103019,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103020,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103021,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103022,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103023,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103024,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103025,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103026,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103027,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103028,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103029,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103030,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103031,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103032,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103033,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103034,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103035,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103036,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103037,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 103038,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion

        #region HAT_ITEMS 104001-140024

        CustomizeItems.Add(new()
        {
            ID = 104001,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104002,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104004,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104005,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104006,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104007,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104008,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104009,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104010,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104011,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104012,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104013,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104014,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104015,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104016,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104017,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104018,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104019,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104020,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104021,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104022,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104023,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 104024,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion

        #region MASK_ITEMS 105001-105004

        CustomizeItems.Add(new()
        {
            ID = 105001,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 105002,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 105003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 105004,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion

        #region HELMETATTACHMENT_ITEMS 106001-106013

        CustomizeItems.Add(new()
        {
            ID = 106001,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106002,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106004,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106005,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106006,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106007,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106008,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106009,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106010,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106011,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106012,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 106013,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion

        #region BACKATTACHMENT_ITEMS 107001-107015

        CustomizeItems.Add(new()
        {
            ID = 107001,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107002,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107004,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107005,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107006,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107007,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107008,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107009,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107010,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107011,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107012,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107013,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107014,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 107015,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion

        #region SHOULDERATTACHMENT_ITEMS 108001-108021

        CustomizeItems.Add(new()
        {
            ID = 108001,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108002,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108004,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108005,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108006,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108007,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108008,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108009,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108010,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108011,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108012,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108013,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108014,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108015,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108016,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108017,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108018,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108019,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108020,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 108021,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion

        #region ELBOWATTACHMENT_ITEMS 110001-110006

        CustomizeItems.Add(new()
        {
            ID = 110001,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 110002,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 110003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 110004,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 110005,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 110006,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion

        #region HIPATTACHMENT_ITEMS 112001-112012

        CustomizeItems.Add(new()
        {
            ID = 112001,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112002,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112004,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112005,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112006,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112007,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112008,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112001,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112009,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112010,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112011,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 112012,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion

        #region KNEEATTACHMENT_ITEMS 113001-113011

        CustomizeItems.Add(new()
        {
            ID = 113001,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113002,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113004,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113005,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113006,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113007,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113008,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113009,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113010,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 113011,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion

        #region ELFEAR_ITEMS 115001-115003

        CustomizeItems.Add(new()
        {
            ID = 115001,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 115002,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        CustomizeItems.Add(new()
        {
            ID = 115003,
            ItemName = "TextID_LanguageID",
            ItemDesc = "TextID_LanguageID",
            Rarity = CustomizeRarity.Common,
            UnlockPrice = 0,
            Bonuses = new()
        });

        #endregion
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