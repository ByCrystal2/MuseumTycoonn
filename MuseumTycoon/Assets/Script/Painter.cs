using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Painter : MonoBehaviour
{
    public List<PainterData> PainterDatas = new List<PainterData>();
    public static Painter instance { get; private set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
    }

    public void AddBasePainters()
    {
        PainterData painter1 = new PainterData(1, "Pablo Picasso", "Spanish painter", 0, new Sprite[] { });      
        PainterData painter2 = new PainterData(2, "Vincent van Gogh", "Dutch painter", 0, new Sprite[] {  });

        painter1.npcComments.Add(new NpcComment { NpcName = "Ahmet", NpcReviewComments = new List<string> { "Güzell", "Etkileyici" }, NpcStar = 4.5f, Painter = painter1 });
        painter2.npcComments.Add(new NpcComment { NpcName = "Koray", NpcReviewComments = new List<string> { "Fena Deðil", "Göz Alýcý" }, NpcStar = 2.5f, Painter = painter2 });
        
        PainterDatas.Add(painter1);
        PainterDatas.Add(painter2);
    }

    public void AddNpcCommentToPainter(string painterName, string npcName, List<string> comments, float rating)
    {
        PainterData painter = GetPainter(painterName);
        if (painter != null)
        {
            NpcComment npcComment = new NpcComment
            {
                NpcName = npcName,
                NpcReviewComments = comments,
                NpcStar = rating,
                Painter = painter
            };
            painter.AddPainterNPCComment(npcComment);           
        }
    }

    public PainterData GetPainter(string painterName)
    {
        return PainterDatas.Find(x=> x.Name == painterName);
    }

    public List<NpcComment> GetPainterNPCComments(string painterName)
    {
        PainterData currentPainter = GetPainter(painterName);
        List<NpcComment> currentPainterNpcComments = currentPainter.npcComments;
        return currentPainterNpcComments;
    }
    public int TotalPainterNPCCommentsCount(string getPainter)
    {
        PainterData painter = GetPainter(getPainter);
        if (painter != null)
        {
            return painter.npcComments.Count;
        }
        return 0;
    }
}
