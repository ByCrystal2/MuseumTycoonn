using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NpcComment
{
    public string NpcName { get; set; }
    public List<string> NpcReviewComments { get; set; }

    private float npcStar;
    public float NpcStar { get { return npcStar; } set { if (value > 5 && value < 0) return; else { npcStar = value; } } }

    public int CommentPainterID;
}
