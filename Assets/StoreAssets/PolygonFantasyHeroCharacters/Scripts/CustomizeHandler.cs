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
        //Test
        //OpenCustomizePanel();
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
#if UNITY_EDITOR
        FirestoreManager.instance.customizationDatasHandler.AddCustomizationDataWithUserId(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID, characterCustomizeData);
#endif
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
        playerCustomizeData = new PlayerCustomizeData(_playerCustomizeData);
        unlockedCustomizeElementIDs.Clear();
        int length = _unlockedCustomizeElementIDs.Count;
        for (int i = 0; i < length; i++)
            unlockedCustomizeElementIDs.Add(_unlockedCustomizeElementIDs[i]);
        LastSelectedCustomizeCategory = _lastSelectedCustomizeCategory;
        LastSelectedCustomizeHeader = _lastSelectedCustomizeHeader;
        LastSelectedColorHeader = _lastSelectedColorHeader;
    }
}