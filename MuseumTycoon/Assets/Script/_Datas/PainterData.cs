using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PainterData
{
    public int ID;
    public string Name;
    public string Description;
    public int StarCount;
    public Sprite[] Pictures;

    public PainterData(int iD, string name, string description, int starCount, Sprite[] pictures)
    {
        this.ID = iD;
        this.Name = name;
        this.Description = description;
        this.StarCount = starCount;
        this.Pictures = pictures;
    }

    public PainterData(PainterData painter)
    {
        this.ID = painter.ID;
        this.Name = painter.Name;
        this.Description = painter.Description;
        this.StarCount = painter.StarCount;
        this.Pictures = painter.Pictures;
    }



}
