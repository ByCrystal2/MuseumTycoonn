using NSubstitute;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;



public class PicturesMenuController : MonoBehaviour
{
    [SerializeField] public List<int> TextureIds = new List<int>();
    [SerializeField] GameObject pictureSlotPrefab;
    [SerializeField] Transform pictureContent;
    [SerializeField] GameObject ddPainter;

    [SerializeField] GameObject pnlPicturesMenu;

    [Header("PictureInfos")]
    [SerializeField] GameObject[] OpenStars;
    [SerializeField] GameObject[] CloseStars;
    [SerializeField] public TextMeshProUGUI txtPainterName;
    [SerializeField] public TextMeshProUGUI txtRequiredGold;
    [SerializeField] Image imgPicture;
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
    public int PictureChangeRequiredAmount = 250;

    public int lastClickedIndex = -1;

    public static PicturesMenuController instance { get; set; }
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
        PictureUpdateButton.onClick.AddListener(UpdateTablo);
        ExitPanelButton.onClick.AddListener(ExitPicturePanel);
        AdsPanelActivationButton.onClick.AddListener(TestingAdsPanelActivation);
        LoadRewardVideoButton.onClick.AddListener(TestingLoadRewardVideo);
        LoadNormalVideoButton.onClick.AddListener(TestingLoadNormalVideo);

        RewardVideoButton.onClick.AddListener(TestingShowRewardVideo);
        NormalVideoButton.onClick.AddListener(TestingShowNormalVideo);
        RemoveAdsButton.onClick.AddListener(RemoveAllAds);
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
            newPicture.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = i.ToString();
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
        txtPainterName.text = MuseumManager.instance.InventoryPictures[_index].painterData.Name;
        txtRequiredGold.text =""+PictureChangeRequiredAmount;

        for (int i = 0; i < MuseumManager.instance.InventoryPictures[_index].painterData.StarCount; i++)
        {
            OpenStars[i].SetActive(true);
        }
        GoldControledButtonShape();
    }

    public void AddPicture(PictureElement PE)
    {
        if (!PE._pictureData.isActive)
        {
            Debug.Log("Tiklanan Tablo aktif degil.");
            return;
        }
        UpdatePicture();
        CurrentPicture = PE;
        SetCurrentPicture(PE);

        for (int i = 0; i < CloseStars.Length; i++)
        {
            CloseStars[i].SetActive(true);
            OpenStars[i].SetActive(false);
        }

        txtPainterName.text = PE._pictureData.painterData.Name;
        txtRequiredGold.text = "" + PictureChangeRequiredAmount;

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
        if (CurrentPicture != null && PictureChangeRequiredAmount <= MuseumManager.instance.GetCurrentGold())
        {
            MuseumManager.instance.SpendingGold(PictureChangeRequiredAmount);
            if (CurrentPicture._pictureData.isFirst)
            {
                SetPictureUpdateButton(false, "Eklendi", Color.white);                
                //MuseumManager.instance.GetPictureElement(PictureChangeRequiredAmount = Mathf.RoundToInt(CurrentPicture._pictureData.RequiredGold * 0.5f);
                txtRequiredGold.text = "Gerekli Altýn: " + PictureChangeRequiredAmount;
            }
            else
            {
                isFirst = false;
                SetPictureUpdateButton(false, "Deðiþtirildi", Color.white);
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
            wallData.isActive = keepisActive;
            wallData.isLocked = keepisLocked;
            wallData.RequiredGold = currentInventory.RequiredGold;

            CurrentPicture._pictureData = wallData;
            //-----------

            MuseumManager.instance.InventoryPictures.RemoveAt(lastClickedIndex);
            if(!isFirst)
                MuseumManager.instance.InventoryPictures.Add(inventoryData);
            MuseumManager.instance.GetPictureElement(CurrentPicture._pictureData.id).UpdateVisual();
            UpdatePicture();
        }
    }

    public void SetCurrentPicture(PictureElement PE)
    {
        Debug.Log("Set current picture!");
        if (PE == null) { return; }
        if (!PE._pictureData.isFirst)
        {
            Debug.Log("ID: " + PE._pictureData.TextureID);
            PictureElementData ped = MuseumManager.instance.GetPictureElementData(PE._pictureData.TextureID);
            imgPicture.sprite = CatchTheColors.instance.TextureToSprite(ped.texture);
        }
        SetPictureUpdateButton(false, "Tablo Seçin", Color.white);

    }
    private bool tableClicked = false;
    public void GoldControledButtonShape()
    {
        if (PictureChangeRequiredAmount <= MuseumManager.instance.GetCurrentGold() && tableClicked)
        {

            if (imgPicture.sprite == null)
            {
                SetPictureUpdateButton(false, "Yeterli", Color.white);
            }
            else if (imgPicture.sprite != null && CurrentPicture._pictureData.isFirst && pictureContent.childCount >= 0)
            {
                SetPictureUpdateButton(true, "Ekle", Color.green);
            }
            else if (imgPicture.sprite != null && !CurrentPicture._pictureData.isFirst)
            {
                SetPictureUpdateButton(true, "Deðiþtir", Color.Lerp(Color.green, new Color(1, 1, 0, 0.5f), 0.5f));
            }
        }
        else if (PictureChangeRequiredAmount <= MuseumManager.instance.GetCurrentGold() && !tableClicked)
        {
            SetPictureUpdateButton(false, "Tablo Seçin", Color.white);
        }
        else
        {
            Debug.Log("Picture Ýçin para Yetersiz.");
            SetPictureUpdateButton(false, "Yetersiz", Color.red);
        }
    }
    public void SetPicture(Texture2D texture2D)
    {
        Debug.Log("SetPicture");
        imgPicture.sprite = CatchTheColors.instance.TextureToSprite(texture2D);
        if (PictureChangeRequiredAmount <= MuseumManager.instance.GetCurrentGold())
        {
            Debug.Log("Picture Ýçin para Yeterli.");
            Debug.Log(PictureChangeRequiredAmount);

            if (CurrentPicture._pictureData.isFirst)
            {
                SetPictureUpdateButton(true, "Ekle", Color.green);
            }
            
            else
            {
                SetPictureUpdateButton(true, "Deðiþtir", Color.Lerp(Color.green, new Color(1, 1, 0, 0.5f), 0.5f));
            }
        }
        else
        {
            SetPictureUpdateButton(false, "Yetersiz", Color.red);
        }     
    }

    public void TestingLoadRewardVideo() //Button
    {
        UnityAdsManager.instance.LoadRewardedAd();
        Debug.Log("Test Load Reward Video Ýþlemi Tamamlandý.");
    }
    public void TestingLoadNormalVideo() //Button
    {
        UnityAdsManager.instance.LoadInterstitialAd();
        Debug.Log("Test Load Normal Video Ýþlemi Tamamlandý.");
    }
    public void TestingShowRewardVideo() //Button
    {        
        UnityAdsManager.instance.ShowRewardedAd();
        Debug.Log("Test Show Reward Video Ýþlemi Tamamlandý.");
    }
    public void TestingShowNormalVideo() //Button
    {
        UnityAdsManager.instance.ShowNonRewardedAd();
        Debug.Log("Test Show Normal Video Ýþlemi Tamamlandý.");
    }
    public void RemoveAllAds()
    {
        UnityAdsManager.instance.adsData.RemovedAllAds = true;
    }
    public void ExitPicturePanel()
    {        
        pnlPicturesMenu.SetActive(false);
        tableClicked = false;
        CurrentPicture = null;
    }

    public void TestingAdsPanelActivation()
    {
        if (!TestingAdsPanel.activeInHierarchy)        
            TestingAdsPanel.SetActive(true);        
        else
            TestingAdsPanel.SetActive(false);
    }
}

