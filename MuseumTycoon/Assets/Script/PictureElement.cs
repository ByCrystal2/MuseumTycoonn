using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PictureElement : MonoBehaviour
{
    public PictureElementData data;
    public PainterData painterData;
    public int id;
    public bool isLocked;
    public bool isActive;
    public bool isFirst = true;
    public int RequiredGold;
    private void OnMouseDown()
    {
        if (isLocked && GameManager.instance.UIControl)
            return;

        if (!isActive) 
        {
            
            //UIController.instance.GetClickedPicture(true,this);
            for (int i = 1; i < 6; i++)
                transform.GetChild(i).GetComponent<LocationData>().SetVisittible(true);
            PicturesMenuController.instance.AddPicture(this);
            
            
            isActive = true;
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

    public void UpdateVisual()
    {
        Image im = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        if (data != null)
        {
            if(!MuseumManager.instance.MyPictureObjects.Contains(this))
                MuseumManager.instance.MyPictureObjects.Add(this);
            im.sprite = CatchTheColors.instance.TextureToSprite(data.texture);
        }
        else
        {
            if (MuseumManager.instance.MyPictureObjects.Contains(this))
                MuseumManager.instance.MyPictureObjects.Remove(this);
            im.sprite = MuseumManager.instance.EmptyPictureSprite;
        }
    }
}

[System.Serializable]
public class PictureElementData
{
    [HideInInspector] public int id;
    public Texture2D texture;
    public List<MyColors> MostCommonColors = new List<MyColors>();
}
