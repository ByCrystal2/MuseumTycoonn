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
        PainterData painter1 = new PainterData(1, "Pablo Picasso", "Spanish painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[0].texture));      
        PainterData painter2 = new PainterData(2, "Pablo Picasso", "Spanish painter2", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[1].texture));      
        PainterData painter3 = new PainterData(3, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[2].texture));
        PainterData painter4 = new PainterData(4, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[3].texture));
        PainterData painter5 = new PainterData(5, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[4].texture));
        PainterData painter6 = new PainterData(6, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[5].texture));
        PainterData painter7 = new PainterData(7, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[6].texture));
        PainterData painter8 = new PainterData(8, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[7].texture));
        PainterData painter9 = new PainterData(9, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[8].texture));
        PainterData painter10 = new PainterData(10, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[9].texture));
        PainterData painter11 = new PainterData(11, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[10].texture));
        PainterData painter12 = new PainterData(12, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[11].texture));
        PainterData painter13 = new PainterData(13, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[12].texture));
        PainterData painter14 = new PainterData(14, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[13].texture));
        PainterData painter15 = new PainterData(15, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[14].texture));
        PainterData painter16 = new PainterData(16, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[15].texture));
        PainterData painter17 = new PainterData(17, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[16].texture));
        PainterData painter18 = new PainterData(18, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[17].texture));
        PainterData painter19 = new PainterData(19, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[18].texture));
        PainterData painter20 = new PainterData(20, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[19].texture));
        PainterData painter21 = new PainterData(21, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[20].texture));
        PainterData painter22 = new PainterData(22, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[21].texture));
        PainterData painter23 = new PainterData(23, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[22].texture));
        PainterData painter24 = new PainterData(24, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[23].texture));
        PainterData painter25 = new PainterData(25, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[24].texture));
        PainterData painter26 = new PainterData(26, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[25].texture));
        PainterData painter27 = new PainterData(27, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[26].texture));
        PainterData painter28 = new PainterData(28, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[27].texture));
        PainterData painter29 = new PainterData(29, "Vincent van Gogh", "Dutch painter", 0, CatchTheColors.instance.TextureToSprite(MuseumManager.instance.MyPictures[28].texture));

        //painter1.npcComments.Add(new NpcComment { NpcName = "Ahmet", NpcReviewComments = new List<string> { "Güzell", "Etkileyici" }, NpcStar = 4.5f, ArtID = painter1 });
        //painter2.npcComments.Add(new NpcComment { NpcName = "Koray", NpcReviewComments = new List<string> { "Fena Deðil", "Göz Alýcý" }, NpcStar = 2.5f, ArtID = painter2 });
        
        PainterDatas.Add(painter1);
        PainterDatas.Add(painter2);
    }

    public void AddNpcCommentToPainter(string npcName, List<string> comments, float rating, PainterData painter)
    {
        NpcComment npcComment = new NpcComment
        {
            NpcName = npcName,
            NpcReviewComments = comments,
            NpcStar = rating,
        };
        painter.AddPainterNPCComment(npcComment);           
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
