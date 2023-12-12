using NSubstitute;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditorInternal.VersionControl;
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
        PictureUpdateButton.onClick.AddListener(UpdateTable);
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
        SetPicture(_ped.texture);

        for (int i = 0; i < CloseStars.Length; i++)
        {
            CloseStars[i].SetActive(true);
            OpenStars[i].SetActive(false);
        }

        lastClickedIndex = _index;
        txtPainterName.text = MuseumManager.instance.InventoryPictures[_index].painterData.Name;
        txtRequiredGold.text = "Gerekli Altýn: " + PictureChangeRequiredAmount;

        for (int i = 0; i < MuseumManager.instance.InventoryPictures[_index].painterData.StarCount; i++)
        {
            OpenStars[i].SetActive(true);
        }
    }

    public void AddPicture(PictureElement PE)
    {
        UpdatePicture();
        CurrentPicture = PE;
        SetCurrentPicture(PE);

        for (int i = 0; i < CloseStars.Length; i++)
        {
            CloseStars[i].SetActive(true);
            OpenStars[i].SetActive(false);
        }

        txtPainterName.text = PE._pictureData.painterData.Name;
        txtRequiredGold.text = "Gerekli Altýn: " + PictureChangeRequiredAmount;

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
    public void UpdateTable()
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

            PictureData _currentData = new PictureData();
            _currentData.TextureID = CurrentPicture._pictureData.TextureID;
            _currentData.painterData = new PainterData(CurrentPicture._pictureData.painterData);

            CurrentPicture._pictureData.TextureID = MuseumManager.instance.InventoryPictures[lastClickedIndex].TextureID;
            MuseumManager.instance.GetPictureElement(CurrentPicture._pictureData.id).UpdateVisual();

            CurrentPicture._pictureData.isFirst = false;

            MuseumManager.instance.InventoryPictures.RemoveAt(lastClickedIndex);
            if(!isFirst)
                MuseumManager.instance.InventoryPictures.Add(_currentData);
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
        if (PictureChangeRequiredAmount <= MuseumManager.instance.GetCurrentGold())
        {
            
            if (imgPicture.sprite == null)
            {
                SetPictureUpdateButton(false, "Yeterli", Color.white);
            }
            else if ( imgPicture.sprite != null && PE._pictureData.isFirst)
            {            
                SetPictureUpdateButton(true, "Ekle", Color.green);
            }
            else if (imgPicture.sprite != null && !PE._pictureData.isFirst)
            {
                SetPictureUpdateButton(true, "Deðiþtir", Color.Lerp(Color.green, new Color(1, 1, 0, 0.5f), 0.5f));
            }
        }
        else
        {
            Debug.Log("Picture Ýçin para Yetersiz.");
            Debug.Log(PictureChangeRequiredAmount);
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

    public void ExitPicturePanel()
    {        
        pnlPicturesMenu.SetActive(false);
        CurrentPicture = null;
    }
}
