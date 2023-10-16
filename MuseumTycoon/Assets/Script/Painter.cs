using System.Collections;
using System.Collections.Generic;
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
        PainterData painter1 = new PainterData(1, "Pablo Picasso", "Spanish painter", 5, new Sprite[] { /* Resimleriniz burada */ });
        PainterData painter2 = new PainterData(2, "Vincent van Gogh", "Dutch painter", 4, new Sprite[] { /* Resimleriniz burada */ });

        // PainterData nesnelerini listeye ekleyebiliriz
        PainterDatas.Add(painter1);
        PainterDatas.Add(painter2);
    }

    public PainterData GetPainter(string painterName)
    {
        return PainterDatas.Find(x=> x.Name == painterName);
    }
}
