using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NpcComment
{
    public string NpcName;
    public List<string> NpcReviewComments;
    public float NpcStarRating;
    public float MaxNpcStarRating;
    public int PictureElementDataID;    
}
