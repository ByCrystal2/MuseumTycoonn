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

    private void OnMouseDown()
    {
        if (isLocked && GameManager.instance.UIControl)
            return;

        if (!isActive)
        {
            //transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Pictures/1");
            UIController.instance.GetClickedPicture(true);
            transform.GetChild(1).GetComponent<LocationData>().SetVisittible(true);
            isActive = true;
        }
        else
        {
            //transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = MuseumManager.instance.EmptyPictureSprite;
            UIController.instance.GetClickedPicture(false);
            transform.GetChild(1).GetComponent<LocationData>().SetVisittible(false);
            isActive = false;
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
