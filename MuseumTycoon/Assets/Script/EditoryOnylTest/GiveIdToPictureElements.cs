using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GiveIdToPictureElements : MonoBehaviour
{
    public bool updateNow;

    private void OnDrawGizmosSelected()
    {
        if (updateNow)
        {
            updateNow = false;
            PictureElement[] pictures = FindObjectsOfType<PictureElement>();
            int id = 1;
            foreach (var item in pictures)
            {
                item._pictureData.id = id;
                id++;
            }
        }
    }
}


