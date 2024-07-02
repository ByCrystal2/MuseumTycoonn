using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PainterData
{
    public int ID;
    public string Name;
    public string Description;
    public float StarCount;
    public List<NpcComment> npcComments;

    public PainterData(int iD, string name, string description, float starCount, List<NpcComment> _comments = null)
    {
        this.ID = iD;
        this.Name = name;
        this.Description = description;
        this.StarCount = starCount;
        if (_comments != null)
        {
            List<NpcComment> npcNew = new List<NpcComment>();
            foreach (var item in _comments)
                npcNew.Add(item);
            this.npcComments = npcNew;
        }
    }

    public PainterData(PainterData painter)
    {
        this.ID = painter.ID;
        this.Name = painter.Name;
        this.Description = painter.Description;
        this.StarCount = painter.StarCount;

        if (painter !=null)
        {
            if (painter.npcComments != null)
            {
                List<NpcComment> npcNew = new List<NpcComment>();
                foreach (var item in painter.npcComments)
                    npcNew.Add(item);
                this.npcComments = npcNew;
            }
        }
    }

    public PainterData()
    {
        
    }

    public void AddPainterNPCComments(List<NpcComment> npcCommentsList)
    {
        if (this.npcComments == null)
        {
            this.npcComments = new List<NpcComment>();
        }
        this.npcComments = npcCommentsList;
    }

    public void AddPainterNPCComment(NpcComment npcComment)
    {
        if (this.npcComments == null)
        {
            this.npcComments = new List<NpcComment>();
        }
        this.AddStar(npcComment.NpcStarRating);
        this.npcComments.Add(npcComment);
    }

    public void AddStar(float starRating)
    {       
        float katsayi = 0.2f; // Örnek bir katsayý
        float yeniSonuc = StarCount + (starRating * katsayi);

        // Sonucu güncelle
        StarCount = Mathf.Round(yeniSonuc); 
    }
}
