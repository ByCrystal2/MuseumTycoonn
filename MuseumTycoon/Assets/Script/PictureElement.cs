using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PictureElement : MonoBehaviour
{
    public PictureData _pictureData;
    private void OnMouseDown()
    {
        if (_pictureData.isLocked && GameManager.instance.UIControl)
            return;

        if (!_pictureData.isActive) 
        {
            
            //UIController.instance.GetClickedPicture(true,this);
            for (int i = 1; i < 6; i++)
                transform.GetChild(i).GetComponent<LocationData>().SetVisittible(true);
            _pictureData.isActive = true;
            PicturesMenuController.instance.AddPicture(this);
        }
        else
        {
            PicturesMenuController.instance.AddPicture(this);
            //PicturesMenuController.instance.CurrentPicture = null;
            //data = null;
            //for (int i = 1; i < 6; i++)
            //    transform.GetChild(i).GetComponent<LocationData>().SetVisittible(false);

            //isActive = false;
            //UpdateVisual();
        }
    }

    public void UpdateVisual(bool _isLoadGame = false)
    {
        Image im = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        PictureElementData ped = MuseumManager.instance.GetPictureElementData(_pictureData.TextureID);
        if (ped != null)
        {
            if(!MuseumManager.instance.CurrentActivePictures.Contains(this))
                MuseumManager.instance.CurrentActivePictures.Add(this);

            im.sprite = CatchTheColors.instance.TextureToSprite(ped.texture);
        }
        else
        {
            if (MuseumManager.instance.CurrentActivePictures.Contains(this))
                MuseumManager.instance.CurrentActivePictures.Remove(this);
            im.sprite = MuseumManager.instance.EmptyPictureSprite;
        }

        if(!_isLoadGame)
            GameManager.instance.Save();
    }

    void SaveThisPicture()
    {

    }
}

[System.Serializable]
public class PictureElementData
{
    public int id;
    public Texture2D texture;
    public List<MyColors> MostCommonColors = new List<MyColors>();
}

[System.Serializable]
public class PictureData
{
    //public PictureElementData data;
    public PainterData painterData;
    public int id;
    public int TextureID;
    public int RoomID;
    public bool isLocked;
    public bool isActive;
    public bool isFirst = true;
    public int RequiredGold;
}
