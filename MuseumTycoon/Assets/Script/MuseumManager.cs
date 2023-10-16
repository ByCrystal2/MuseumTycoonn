using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuseumManager : MonoBehaviour
{
    public static MuseumManager instance { get; private set; }
    public List<PictureElementData> MyPictures = new List<PictureElementData>();

    public Sprite EmptyPictureSprite;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
        CatchTheColorForAll();
    }

    void CatchTheColorForAll()
    {
        int length = MyPictures.Count;
        for (int i = 0; i < length; i++)
        {
            MyPictures[i].id = (i + 1);
            MyPictures[i].MostCommonColors = CatchTheColors.instance.FindMostUsedColors(MyPictures[i].texture);
        }
    }
}
