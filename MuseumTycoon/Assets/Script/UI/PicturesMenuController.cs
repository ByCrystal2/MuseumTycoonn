using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PicturesMenuController : MonoBehaviour
{
    [SerializeField] Sprite[] Pictures;
    [SerializeField] GameObject pictureSlotPrefab;
    [SerializeField] Transform pictureContent;
    [SerializeField] GameObject ddPainter;
    public int pictureCount;

    private void Awake()
    {
        //for (int i = 0; i < MuseumManager.instance.MyPictures.Count; i++)
        //{
        //    Pictures[i] = MuseumManager.instance.MyPictures[i].texture.;
        //}
    }
    void Start()
    {
        
    }

   
    void Update()
    {
        
    }

    private void OnEnable()
    {
        List<string> painterNames = new List<string>(Painter.instance.PainterDatas.Count);
        foreach (var painterData in Painter.instance.PainterDatas)
        {
            painterNames.Add(painterData.Name);
        }
        ddPainter.GetComponent<TMP_Dropdown>().AddOptions(painterNames);

        GameManager.instance.UIControl = true;
        foreach (var picture in Pictures)
        {
            GameObject newPicture = Instantiate(pictureSlotPrefab, pictureContent);
            pictureCount++;
            newPicture.GetComponent<Image>().sprite = picture;
            newPicture.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = pictureCount.ToString();
        }
    }
    private void OnDisable()
    {
        GameManager.instance.UIControl = false;
    }

    public Sprite GetRandomPicture()
    {        
        return Pictures[Random.Range(0, Pictures.Length)];
    }

    public void GetClickedImage() // btn* /pnlPicturesMenu / ItemContent / InventorySlot
    {

    }
}
