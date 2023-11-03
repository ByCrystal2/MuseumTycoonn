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
    [SerializeField] List<Sprite> Pictures;
    [SerializeField] GameObject pictureSlotPrefab;
    [SerializeField] Transform pictureContent;
    [SerializeField] GameObject ddPainter;

    [SerializeField] GameObject pnlPicturesMenu;

    [Header("PictureInfos")]

    [SerializeField] GameObject[] OpenStars;
    [SerializeField] GameObject[] CloseStars;
    [SerializeField] TextMeshProUGUI txtPainterName;
    [SerializeField] TextMeshProUGUI txtRequiredGold;
    [SerializeField] Image imgPicture;
    [SerializeField] Button PictureUpdateButton;
    [SerializeField] Button ExitPanelButton;

    public Texture2D CurrentTexture;
    public PictureElement CurrentPicture;

    public int pictureCount;

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

    public void AddPicture(PictureElement PE)
    {
        CurrentPicture = PE;
        SetCurrentPicture(PE);
        
        for (int i = 0; i < CloseStars.Length; i++)
        {
            CloseStars[i].SetActive(true);
            OpenStars[i].SetActive(false);
        }

        txtPainterName.text = PE.painterData.Name;
        txtRequiredGold.text = "Gerekli Altýn: " + PE.RequiredGold;
        
        for (int i = 0; i < PE.painterData.StarCount; i++)
        {
            OpenStars[i].SetActive(true);
        }

        pnlPicturesMenu.SetActive(false);
        
        List<Texture2D> textures = new List<Texture2D>();
        foreach (var picture in MuseumManager.instance.GetPicturesTexture())
        {
            textures.Add(picture);
        }
        foreach (var texture in textures)
        {
            Pictures.Add(CatchTheColors.instance.TextureToSprite(texture));
        }


        GameManager.instance.UIControl = true;
        for (int i = 0; i < Pictures.Count; i++)
        {
            GameObject newPicture = Instantiate(pictureSlotPrefab, pictureContent);
            int index = i + 1;
            if (index == Pictures.Count-1)
            {
                index = i;
            }
            
            //if (index == 0)
            //{
            //    index = 1;
            //}
            Debug.Log("Picture Index: " + index);
            newPicture.GetComponent<Button>().onClick.AddListener(() => UIController.instance.GetClickedImage(index));
            newPicture.GetComponent<Image>().sprite = Pictures[i];
            newPicture.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = i.ToString();
            pictureCount++;
        }
        pictureCount = 0;
        pnlPicturesMenu.SetActive(true);
        Pictures = new List<Sprite>();
    }

    public void SetPictureUpdateButton(bool _interactable, string _name, Color _color)
    {
        PictureUpdateButton.interactable = _interactable;
        PictureUpdateButton.transform.GetChild(0).GetComponent<Text>().text = _name;
        PictureUpdateButton.transform.GetChild(0).GetComponent<Text>().color = _color;
    }
    public void UpdateTable()
    {
        if (CurrentPicture != null && CurrentTexture != null && CurrentPicture.RequiredGold <= MuseumManager.instance.GetCurrentGold())
        {
            MuseumManager.instance.SpendingGold(CurrentPicture.RequiredGold);
            CurrentPicture.data.texture = CurrentTexture;
            if (CurrentPicture.isFirst)
            {
                SetPictureUpdateButton(false, "Eklendi", Color.white);                
                MuseumManager.instance.GetPictureElement(CurrentPicture.id).RequiredGold = Mathf.RoundToInt(CurrentPicture.RequiredGold * 0.5f);
                txtRequiredGold.text = "Gerekli Altýn: " + CurrentPicture.RequiredGold;
            }
            else
            {
                SetPictureUpdateButton(false, "Deðiþtirildi", Color.white);
            }
            MuseumManager.instance.GetPictureElement(CurrentPicture.id).UpdateVisual();

            CurrentPicture.isFirst = false;
            
        }
    }

    public void SetCurrentPicture(PictureElement PE)
    {
        if (PE == null) { return; }
        if (!PE.isFirst)
        {
            imgPicture.sprite = CatchTheColors.instance.TextureToSprite(PE.data.texture);
        }
        if (PE.RequiredGold <= MuseumManager.instance.GetCurrentGold())
        {
            
            if (imgPicture.sprite == null)
            {
                SetPictureUpdateButton(false, "Yeterli", Color.white);
            }
            else if ( imgPicture.sprite != null && CurrentTexture != null && PE.isFirst)
            {            
                SetPictureUpdateButton(true, "Ekle", Color.green);
            }
            else if (imgPicture.sprite != null && CurrentTexture != null && !PE.isFirst)
            {
                SetPictureUpdateButton(true, "Deðiþtir", Color.Lerp(Color.green, new Color(1, 1, 0, 0.5f), 0.5f));
            }
        }
        else
        {
            Debug.Log("Picture Ýçin para Yetersiz.");
            Debug.Log(PE.RequiredGold);
            SetPictureUpdateButton(false, "Yetersiz", Color.red);
        }

    }
    public void SetPicture(Texture2D texture2D)
    {
        imgPicture.sprite = CatchTheColors.instance.TextureToSprite(texture2D);
        if (CurrentPicture.RequiredGold <= MuseumManager.instance.GetCurrentGold())
        {
            Debug.Log("Picture Ýçin para Yeterli.");
            Debug.Log(CurrentPicture.RequiredGold);

            if (CurrentPicture.isFirst)
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
        CurrentTexture = texture2D;        
    }

    public void ExitPicturePanel()
    {        
        pnlPicturesMenu.SetActive(false);
        CurrentPicture = null;
        CurrentTexture = null;
    }
    public Sprite GetRandomPicture()
    {        
        return Pictures[Random.Range(0, Pictures.Count)];
    }

    
}
