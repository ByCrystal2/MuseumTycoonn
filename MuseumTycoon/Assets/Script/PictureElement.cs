using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PictureElement : MonoBehaviour
{
    public int id;
    public bool isLocked;
    public bool isActive;

    private void OnMouseDown()
    {
        if (isLocked)
            return;

        if (!isActive)
        {
            transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Pictures/1");
            transform.GetChild(1).GetComponent<LocationData>().SetVisittible(true);
            isActive = true;
        }
        else
        {
            transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = MuseumManager.instance.EmptyPictureSprite;
            transform.GetChild(1).GetComponent<LocationData>().SetVisittible(false);
            isActive = false;
        }
    }
}
