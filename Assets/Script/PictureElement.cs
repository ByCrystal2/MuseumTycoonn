using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class PictureElement : MonoBehaviour
{
    public PictureData _pictureData;
    private void OnMouseDown()
    {
        if (_pictureData.isLocked || !UIController.instance.AllUIPanelClosed()) // Kilitliyse veya UI objesi arkasindan tiklaniyorsa islem yapma.
            return;

        PicturesMenuController.instance.AddPicture(this);
    }

    public void SetImage(bool _set)
    {
        Debug.Log("Image set successfully! _pictureData id: " + _pictureData.id + " name: " + transform.name + " / set=> " + _set);
        for (int i = 1; i < 6; i++)
            transform.GetChild(i).GetComponent<LocationData>().SetVisittible(_set, true);
    }

    public void UpdateVisual(bool _isLoadGame = false)
    {
        StartCoroutine(IEUpdateVisual(_isLoadGame));
    }

    IEnumerator IEUpdateVisual(bool _isLoadGame)
    {
        int stack = 100;
        while (_pictureData.TextureID == 0)
        {
            yield return new WaitForEndOfFrame();
            stack--;
            if (stack < 0)
                break;  
        }

        Image im = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        Debug.Log("_pictureData.TextureID : " + _pictureData.TextureID);
        PictureElementData ped = MuseumManager.instance.GetPictureElementData(_pictureData.TextureID);
        if (ped != null)
        {
            if (!MuseumManager.instance.CurrentActivePictures.Contains(this))
                MuseumManager.instance.CurrentActivePictures.Add(this);

            Debug.Log("ped null degil!");
            im.sprite = CatchTheColors.instance.TextureToSprite(ped.texture);
        }
        else
        {
            if (MuseumManager.instance.CurrentActivePictures.Contains(this))
                MuseumManager.instance.CurrentActivePictures.Remove(this);
            im.sprite = MuseumManager.instance.EmptyPictureSprite;
            Debug.Log("ped null!");
        }

        if (!_isLoadGame)
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
    public bool isLocked = true;
    public bool isActive;
    public bool isFirst = true;
    public int RequiredGold;
}
