using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NpcCommentManager : MonoBehaviour
{
    public List<NpcComment> npcComments = new List<NpcComment>();

    public static NpcCommentManager instance {  get; private set; }
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
    public void AddNpcComment(string npcName, List<string> npcReviewComment, float npcStar, PainterData painter)
    {
        NpcComment comment = new NpcComment
        {
            NpcName = npcName,
            NpcReviewComments = npcReviewComment,
            NpcStar = npcStar,
            Painter = painter
        };
        npcComments.Add(comment);
    }

    public List<NpcComment> GetPainterWithNpcComments(string npcAdi)
    {
        return npcComments.Where(y => y.NpcName == npcAdi).ToList();
    }

    public List<NpcComment> GetTotalNPCCommentsWithNPC(string npcName)
    {
        return this.npcComments.Where(x=> x.NpcName == npcName).ToList();
    }
}

/* Kullaným Örneði
NpcCommentManager manager = new NpcCommentManager();
manager.EkleNpcYorum("Npc1", "Harika bir resim!", 5.0f, "Ressam1");
manager.EkleNpcYorum("Npc1", "Çok güzel bir çalýþma!", 4.5f, "Ressam2");
manager.EkleNpcYorum("Npc2", "Eh iþte bir resim.", 3.0f, "Ressam1");

List<NpcComment> npc1Yorumlar = manager.GetNpcYorumlar("Npc1");
foreach (var yorum in npc1Yorumlar)
{
    Console.WriteLine($"Npc: {yorum.NpcAdi}, Yorum: {yorum.NpcYorumu}, Yýldýz: {yorum.NpcYildizi}, Ressam: {yorum.RessamAdi}");
}

float ressam1ToplamYorum = manager.ToplamYorumSayisi("Ressam1");
Console.WriteLine($"Ressam1 için toplam yorum sayýsý: {ressam1ToplamYorum}");
}
*/
