using NSubstitute;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;



public class PicturesMenuController : MonoBehaviour
{
    [SerializeField] public List<int> TextureIds = new List<int>();
    [SerializeField] GameObject pictureSlotPrefab;
    [SerializeField] Transform pictureContent;
    [SerializeField] GameObject ddPainter;

    [SerializeField] public GameObject pnlPicturesMenu;

    [Header("PictureInfos")]
    [SerializeField] GameObject[] OpenStars;
    [SerializeField] GameObject[] CloseStars;
    [SerializeField] public TextMeshProUGUI txtPainterName;
    [SerializeField] public Text txtDescription;
    [SerializeField] public TextMeshProUGUI txtRequiredGold;
    [SerializeField] Image imgPicture;
    [SerializeField] Sprite defaultPicture;
    [SerializeField] Button PictureUpdateButton;
    [SerializeField] Button ExitPanelButton;

    [Header("Testing Ads Panel")]
    [SerializeField] GameObject TestingAdsPanel;
    [SerializeField] Button AdsPanelActivationButton;
    [SerializeField] Button LoadRewardVideoButton;
    [SerializeField] Button LoadNormalVideoButton;

    [SerializeField] Button RewardVideoButton;
    [SerializeField] Button NormalVideoButton;
    [SerializeField] Button RemoveAdsButton;

    public PictureElement CurrentPicture;

    public int pictureCount;
    

    public int lastClickedIndex = -1;

    public List<string> PictureStrings = new List<string>(); // 0 => eklendi | 1 => guncellendi (degistirildi) | 2 => tablo sec | 3 => yeterli | 4 => ekle | 5 => degistir | 6 => yetersiz
    public static PicturesMenuController instance { get; private set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        PictureStrings = new List<string>() { "Added", "Updated", "Select Painting", "Enough", "Add", "Update", "Insufficient" };
    }

    private void Start()
    {
        PictureUpdateButton.onClick.AddListener(UpdateTablo);
        ExitPanelButton.onClick.AddListener(ExitPicturePanel);
    }

    public void UpdatePicture()
    {
        int length = pictureContent.childCount;
        for (int i = length - 1; i >= 0; i--)
            Destroy(pictureContent.GetChild(i).gameObject);

        TextureIds.Clear();

        foreach (var item in MuseumManager.instance.InventoryPictures)
            TextureIds.Add(item.TextureID);
        
        for (int i = 0; i < TextureIds.Count; i++)
        {
            GameObject newPicture = Instantiate(pictureSlotPrefab, pictureContent);
            if(!GameManager.instance.IsWatchTutorial)
            {
                TutorialTargetObjectHandler target = newPicture.AddComponent<TutorialTargetObjectHandler>();
                target.SetOptions(1, newPicture.GetComponent<RectTransform>());
                DialogueManager.instance.TargetObjectHandlers.Add(target);
            }
            int index = i + 1;
            if (index == TextureIds.Count - 1)
            {
                index = i;
            }

            //if (index == 0)
            //{
            //    index = 1;
            //}
            Debug.Log("Picture Index: " + index);
            PictureElementData ped = MuseumManager.instance.GetPictureElementData(TextureIds[i]);
            int u = i;
            newPicture.GetComponent<Button>().onClick.AddListener(() => GetClickedImage(u, ped));
            newPicture.GetComponent<Image>().sprite = CatchTheColors.instance.TextureToSprite(ped.texture);
            newPicture.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = (newPicture.transform.GetSiblingIndex() + 1).ToString();
            pictureCount++;
        }
    }

    public void GetClickedImage(int _index, PictureElementData _ped)
    {
        tableClicked = true;
        SetPicture(_ped.texture);

        for (int i = 0; i < CloseStars.Length; i++)
        {
            CloseStars[i].SetActive(true);
            OpenStars[i].SetActive(false);
        }

        lastClickedIndex = _index;
        txtPainterName.text = MuseumManager.instance.InventoryPictures[_index].painterData.Description;
        txtDescription.text = MuseumManager.instance.InventoryPictures[_index].painterData.Name;
        txtRequiredGold.text =""+GameManager.instance.PictureChangeRequiredAmount;

        for (int i = 0; i < MuseumManager.instance.InventoryPictures[_index].painterData.StarCount; i++)
        {
            OpenStars[i].SetActive(true);
        }
        GoldControledButtonShape();
    }
    public void ForTutorialUnityEvent()
    {
        PictureElementData _ped = MuseumManager.instance.GetPictureElementData(TextureIds[0]);
        tableClicked = true;
        Debug.Log("CurrentPicture is => " + CurrentPicture._pictureData.painterData.Name);
        //CurrentPicture = new PictureElement();
        //CurrentPicture._pictureData = MuseumManager.instance.InventoryPictures[0];
        SetPicture(_ped.texture);

        for (int i = 0; i < CloseStars.Length; i++)
        {
            CloseStars[i].SetActive(true);
            OpenStars[i].SetActive(false);
        }

        lastClickedIndex = 0;
        txtPainterName.text = MuseumManager.instance.InventoryPictures[0].painterData.Description;
        txtDescription.text = MuseumManager.instance.InventoryPictures[0].painterData.Name;
        txtRequiredGold.text = "" + GameManager.instance.PictureChangeRequiredAmount;

        for (int i = 0; i < MuseumManager.instance.InventoryPictures[0].painterData.StarCount; i++)
        {
            OpenStars[i].SetActive(true);
        }
        GoldControledButtonShape();
    }
    public void AddPicture(PictureElement PE)
    {
        //if (!PE._pictureData.isActive)
        //{
        //    Debug.Log("Tiklanan Tablo aktif degil.");
        //    return;
        //}
        UpdatePicture();
        CurrentPicture = PE;
        SetCurrentPicture(PE);

        for (int i = 0; i < CloseStars.Length; i++)
        {
            CloseStars[i].SetActive(true);
            OpenStars[i].SetActive(false);
        }

        txtPainterName.text = PE._pictureData.painterData.Description;
        txtDescription.text = PE._pictureData.painterData.Name;
        txtRequiredGold.text = "" + GameManager.instance.PictureChangeRequiredAmount;

        for (int i = 0; i < PE._pictureData.painterData.StarCount; i++)
        {
            OpenStars[i].SetActive(true);
        }

        pnlPicturesMenu.SetActive(false);
        
        List<Texture2D> textures = new List<Texture2D>();
        foreach (var picture in MuseumManager.instance.GetPicturesTexture())
        {
            textures.Add(picture);
        }

        GameManager.instance.UIControl = true;
        pictureCount = 0;
        pnlPicturesMenu.SetActive(true);
    }

    public void SetPictureUpdateButton(bool _interactable, string _name, Color _color)
    {
        PictureUpdateButton.interactable = _interactable;
        PictureUpdateButton.transform.GetChild(0).GetComponent<Text>().text = _name;
        PictureUpdateButton.transform.GetChild(0).GetComponent<Text>().color = _color;
    }
    public void UpdateTablo()
    {
        Debug.Log("Update Table CurrentPicture is null:" + (CurrentPicture == null));
        bool isFirst = true;
        if (CurrentPicture != null && GameManager.instance.PictureChangeRequiredAmount <= MuseumManager.instance.GetCurrentGold())
        {            
            MuseumManager.instance.SpendingGold(GameManager.instance.PictureChangeRequiredAmount);
            if (CurrentPicture._pictureData.isFirst)
            {
                SetPictureUpdateButton(false, PictureStrings[0], Color.white);
                CurrentPicture._pictureData.isActive = true;
                //MuseumManager.instance.GetPictureElement(PictureChangeRequiredAmount = Mathf.RoundToInt(CurrentPicture._pictureData.RequiredGold * 0.5f);
                txtRequiredGold.text = ""+ GameManager.instance.PictureChangeRequiredAmount;
            }
            else
            {
                isFirst = false;
                SetPictureUpdateButton(false, PictureStrings[1], Color.white);
                CurrentPicture._pictureData.isActive = true;
            }
            //CurrentPicture._pictureData.isFirst = false;

            Debug.Log("lastClickedIndex: " + lastClickedIndex);

            int keepID = CurrentPicture._pictureData.id;
            bool keepisActive = CurrentPicture._pictureData.isActive;
            bool keepisLocked = CurrentPicture._pictureData.isLocked;

            PictureData inventoryData = new PictureData();
            inventoryData.painterData = new PainterData(CurrentPicture._pictureData.painterData);
            inventoryData.TextureID = CurrentPicture._pictureData.TextureID;
            inventoryData.id = CurrentPicture._pictureData.id;
            inventoryData.isFirst = false;
            inventoryData.isActive = CurrentPicture._pictureData.isActive;
            inventoryData.isLocked = CurrentPicture._pictureData.isLocked;
            inventoryData.RequiredGold = CurrentPicture._pictureData.RequiredGold;

            PictureData currentInventory = MuseumManager.instance.InventoryPictures[lastClickedIndex];
            PictureData wallData = new PictureData();
            wallData.painterData = new PainterData(currentInventory.painterData);
            wallData.TextureID = currentInventory.TextureID;
            wallData.id = keepID;
            wallData.isFirst = false;
            wallData.isActive = false;
            wallData.isLocked = keepisLocked;
            wallData.RequiredGold = currentInventory.RequiredGold;

            CurrentPicture._pictureData = wallData;
            //-----------

            MuseumManager.instance.InventoryPictures.RemoveAt(lastClickedIndex);
            if(!isFirst)
                MuseumManager.instance.InventoryPictures.Add(inventoryData);
            MuseumManager.instance.GetPictureElement(CurrentPicture._pictureData.id).UpdateVisual();
            //Data json'dan cekilmeli (envanterde ki data duvarda ki dataya, duvarda ki datanin envanterde ki dataya gelis kismi.)
            Debug.Log("inventoryData.painterData.ID => " + inventoryData.painterData.ID + " wallData.painterData.ID => " + wallData.painterData.ID);
            Debug.Log("inventoryData.isActive => " + inventoryData.isActive + " wallData.isActive => " + wallData.isActive);
            //FirestoreManager.instance.firestoreItemsManager.AddSkillWithUserId(FirebaseAuthManager.instance.GetCurrentUser().UserId,CurrentPicture._pictureData.id); // asil kod bu. Testten sonra buna gecilmeli!
            UpdatePicture();
            CurrentPicture.SetImage(!CurrentPicture._pictureData.isLocked);
            StartCoroutine(nameof(WaitForSpendingGoldPicture));
        }
        else
        {
            UIController.instance.InsufficientGoldEffect();
        }
    }
    public IEnumerator WaitForSpendingGoldPicture()
    {
        yield return new WaitForSeconds(0.5f);
        ExitPicturePanel();
    }
    public void SetCurrentPicture(PictureElement PE)
    {
        Debug.Log("Set current picture!");
        if (PE == null) { return; }
        if (!PE._pictureData.isFirst)
        {
            Debug.Log("ID: " + PE._pictureData.TextureID);
            PictureElementData ped = MuseumManager.instance.GetPictureElementData(PE._pictureData.TextureID);
            Debug.Log("ped.texture.activeMipmapLimit => " + ped.texture.activeMipmapLimit);
            imgPicture.sprite = CatchTheColors.instance.TextureToSprite(ped.texture);
        }
        else
        {
            imgPicture.sprite = defaultPicture;
        }
        SetPictureUpdateButton(false, PictureStrings[2], Color.white);

    }
    private bool tableClicked = false;
    public void GoldControledButtonShape()
    {
        if (GameManager.instance.PictureChangeRequiredAmount <= MuseumManager.instance.GetCurrentGold() && tableClicked)
        {

            if (imgPicture.sprite == null)
            {
                SetPictureUpdateButton(false, PictureStrings[3], Color.white);
            }
            else if (imgPicture.sprite != null && CurrentPicture._pictureData.isFirst && pictureContent.childCount >= 0)
            {
                SetPictureUpdateButton(true, PictureStrings[4], Color.green);
            }
            else if (imgPicture.sprite != null && !CurrentPicture._pictureData.isFirst)
            {
                SetPictureUpdateButton(true, PictureStrings[5], Color.Lerp(Color.green, new Color(1, 1, 0, 0.5f), 0.5f));
            }
        }
        else if (GameManager.instance.PictureChangeRequiredAmount <= MuseumManager.instance.GetCurrentGold() && !tableClicked)
        {
            SetPictureUpdateButton(false, PictureStrings[2], Color.white);
        }
        else
        {
            Debug.Log("Picture Ýçin para Yetersiz.");
            SetPictureUpdateButton(false, PictureStrings[6], Color.red);
        }
    }
    public void SetPicture(Texture2D texture2D)
    {
        Debug.Log("SetPicture");
        imgPicture.sprite = CatchTheColors.instance.TextureToSprite(texture2D);
        if (GameManager.instance.PictureChangeRequiredAmount <= MuseumManager.instance.GetCurrentGold())
        {
            Debug.Log("Picture Ýçin para Yeterli.");
            Debug.Log(GameManager.instance.PictureChangeRequiredAmount);

            if (CurrentPicture._pictureData.isFirst)
            {
                SetPictureUpdateButton(true, PictureStrings[4], Color.green);
            }
            
            else
            {
                SetPictureUpdateButton(true, PictureStrings[5], Color.Lerp(Color.green, new Color(1, 1, 0, 0.5f), 0.5f));
            }
        }
        else
        {
            SetPictureUpdateButton(false, PictureStrings[6], Color.red);
        }     
    }    
    
    public void ExitPicturePanel()
    {
        RightUIPanelController.instance.UIVisibleClose(false);
        UIController.instance.CloseJoystickObj(false);
        pnlPicturesMenu.SetActive(false);
        tableClicked = false;
        CurrentPicture = null;
        if (GameManager.instance.GetCurrentGameMode() == GameMode.FPS)
        {
            PlayerManager.instance.UnLockPlayer();
        }
    }

    public void TestingAdsPanelActivation()
    {
        if (!TestingAdsPanel.activeInHierarchy)        
            TestingAdsPanel.SetActive(true);        
        else
            TestingAdsPanel.SetActive(false);
    }
}

