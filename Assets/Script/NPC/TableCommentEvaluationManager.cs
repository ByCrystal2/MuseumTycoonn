using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TableCommentEvaluationManager : MonoBehaviour
{
    public List<TableCommentEvaluationData> datas = new List<TableCommentEvaluationData>();

    public static TableCommentEvaluationManager instance { get; private set; }
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
    void Start()
    {
        
    }    
    public void AddAllNPCComments()
    {
        //One Star Comment
        TableCommentEvaluationData cmmnt1 = new TableCommentEvaluationData(1, "Bu tablo hiçbir þey ifade etmiyor. Neden burada?", 1);
        TableCommentEvaluationData cmmnt2 = new TableCommentEvaluationData(2, "Hiçbir þey anlayamadým. Neden buradayým ki?", 1);
        TableCommentEvaluationData cmmnt3 = new TableCommentEvaluationData(3, "Tablo kötüydü. Bir anlamý yoktu.", 1);
        TableCommentEvaluationData cmmnt4 = new TableCommentEvaluationData(4, "Bu tablo beklentilerimi karþýlamadý. Kötü bir seçim.", 1);
        TableCommentEvaluationData cmmnt5 = new TableCommentEvaluationData(5, "Tablo beni hayal kýrýklýðýna uðrattý. Daha iyisini beklerdim.", 1);
        TableCommentEvaluationData cmmnt6 = new TableCommentEvaluationData(6, "Tablo anlamsýzdý. Daha fazla bilgiye ihtiyaç duyuyorum.", 1);
        TableCommentEvaluationData cmmnt7 = new TableCommentEvaluationData(7, "Bu tablo çok sýkýcý. Sanatsal bir deðeri yok.", 1);
        TableCommentEvaluationData cmmnt8 = new TableCommentEvaluationData(8, "Tablo kötü bir tercihti. Neden buradayým?", 1);
        TableCommentEvaluationData cmmnt9 = new TableCommentEvaluationData(9, "Bu tablo ile baðlantý kuramadým. Ýlgimi çekmedi.", 1);
        TableCommentEvaluationData cmmnt10 = new TableCommentEvaluationData(10, "Kötü bir tablo. Ressamýn yeteneði sorgulanmalý.", 1);
        //Two Star Comment
        TableCommentEvaluationData cmmnt11 = new TableCommentEvaluationData(11, "Tablo ortalama, ama biraz daha çekici olabilirdi.", 2);
        TableCommentEvaluationData cmmnt12 = new TableCommentEvaluationData(12, "Potansiyeli var, ama daha fazla detay ve açýklama gerekli.", 2);
        TableCommentEvaluationData cmmnt13 = new TableCommentEvaluationData(13, "Tablo sýkýcýydý, ama bazý ilginç öðeler vardý.", 2);
        TableCommentEvaluationData cmmnt14 = new TableCommentEvaluationData(14, "Ortalama bir tablo. Ýyileþtirme gerekiyor.", 2);
        TableCommentEvaluationData cmmnt15 = new TableCommentEvaluationData(15, "Beklentilerimi tam olarak karþýlamadý. Daha fazla bilgiye ihtiyaç var.", 2);
        TableCommentEvaluationData cmmnt16 = new TableCommentEvaluationData(16, "Bu tablo ile baðlantý kuramadým, ama bazý ilginç detaylar vardý.", 2);
        TableCommentEvaluationData cmmnt17 = new TableCommentEvaluationData(17, "Tablo idare eder, ama daha fazla ilgi çekici olabilir.", 2);
        TableCommentEvaluationData cmmnt18 = new TableCommentEvaluationData(18, "Sýkýcý bir tablo. Daha fazla açýklama ve kontekst gerekiyor.", 2);
        TableCommentEvaluationData cmmnt19 = new TableCommentEvaluationData(19, "Biraz daha ilgi çekici olabilirdi, ama beklentilerimin altýnda kaldý.", 2);
        TableCommentEvaluationData cmmnt20 = new TableCommentEvaluationData(20, "Tablo vasat. Daha fazla bilgiye ve açýklamaya ihtiyaç var."
, 2);
        //Three Star Comment
        TableCommentEvaluationData cmmnt21 = new TableCommentEvaluationData(21, "Ortalama bir tablo. Ýyi, ama bazý eksiklikler var.", 3);
        TableCommentEvaluationData cmmnt22 = new TableCommentEvaluationData(22, "Fena deðil, ama bazý detaylar daha ilgi çekici olabilirdi.", 3);
        TableCommentEvaluationData cmmnt23 = new TableCommentEvaluationData(23, "Tablo iyi bir deneyimdi, ama bazý açýklamalara ihtiyaç var.", 3);
        TableCommentEvaluationData cmmnt24 = new TableCommentEvaluationData(24, "Hoþ bir tablo, ancak bazý yerlerde eksiklik var.", 3);
        TableCommentEvaluationData cmmnt25 = new TableCommentEvaluationData(25, "Tablo beni etkiledi, ancak daha fazla açýklama gerekli.", 3);
        TableCommentEvaluationData cmmnt26 = new TableCommentEvaluationData(26, "Tablo hoþuma gitti, ama bazý yerlerde geliþtirmeye ihtiyaç var.", 3);
        TableCommentEvaluationData cmmnt27 = new TableCommentEvaluationData(27, "Güzel bir tablo. Daha fazla detay ve kontekst eklenmeli.", 3);
        TableCommentEvaluationData cmmnt28 = new TableCommentEvaluationData(28, "Ortalamanýn üstündeydi, ama hala bazý eksiklikler var.", 3);
        TableCommentEvaluationData cmmnt29 = new TableCommentEvaluationData(29, "Tabloyu beðendim, ama daha fazla bilgiye ihtiyaç var.", 3);
        TableCommentEvaluationData cmmnt30 = new TableCommentEvaluationData(30, "Potansiyeli var ve bazý detaylar çok etkileyiciydi.", 3);
        //Four Star Comment
        TableCommentEvaluationData cmmnt31 = new TableCommentEvaluationData(31, "Bu tablo oldukça etkileyici. Ressamýn yeteneði harika.", 4);
        TableCommentEvaluationData cmmnt32 = new TableCommentEvaluationData(32, "Tablo güzel, ancak daha fazla detay eklenmeli.", 4);
        TableCommentEvaluationData cmmnt33 = new TableCommentEvaluationData(33, "Bu tablo büyüleyici. Ressamýn sanatýný takdir ediyorum.", 4);
        TableCommentEvaluationData cmmnt34 = new TableCommentEvaluationData(34, "Harika bir tablo, ancak daha fazla bilgiye ihtiyaç duyuyorum.", 4);
        TableCommentEvaluationData cmmnt35 = new TableCommentEvaluationData(35, "Tablo ilginç, ama biraz daha açýklama olmalý.", 4);
        TableCommentEvaluationData cmmnt36 = new TableCommentEvaluationData(36, "Ressamýn yeteneði dikkat çekici. Bu tabloyu sevdim.", 4);
        TableCommentEvaluationData cmmnt37 = new TableCommentEvaluationData(37, "Tablo çok güzel. Daha fazla bilgi eklenmeli.", 4);
        TableCommentEvaluationData cmmnt38 = new TableCommentEvaluationData(38, "Bu tablo hoþuma gitti, ama daha fazla tarih bilgisi lazým.", 4);
        TableCommentEvaluationData cmmnt39 = new TableCommentEvaluationData(39, "Ressamýn tarzýný seviyorum. Bu tablo muhteþem.", 4);
        TableCommentEvaluationData cmmnt40 = new TableCommentEvaluationData(40, "Güzel bir tablo, ancak daha fazla arka plan bilgisi lazým.", 4);
        //Five Star Comment
        TableCommentEvaluationData cmmnt41 = new TableCommentEvaluationData(41, "Muhteþem bir tablo! Her detayý hayranlýkla inceliyorum.", 5);
        TableCommentEvaluationData cmmnt42 = new TableCommentEvaluationData(42, "Harika bir resim. Ressamýn yeteneði olaðanüstü.", 5);
        TableCommentEvaluationData cmmnt43 = new TableCommentEvaluationData(43, "Tablo beni büyüledi. Sanatýn doruklarýna ulaþýyor.", 5);
        TableCommentEvaluationData cmmnt44 = new TableCommentEvaluationData(44, "Mükemmel bir tablo. Detaylar inanýlmaz.", 5);
        TableCommentEvaluationData cmmnt45 = new TableCommentEvaluationData(45, "Bu tabloyu hayranlýkla izliyorum. Ressam gerçek bir deha.", 5);
        TableCommentEvaluationData cmmnt46 = new TableCommentEvaluationData(46, "Bu tablo bir baþyapýt. Her ayrýntýsý muhteþem.", 5);
        TableCommentEvaluationData cmmnt47 = new TableCommentEvaluationData(47, "Ressamýn tarzý büyüleyici. Bu tablo herkesin görmesi gereken bir eser.", 5);
        TableCommentEvaluationData cmmnt48 = new TableCommentEvaluationData(48, "Tablo etkileyici. Daha fazla detay öðrenmeliyim.", 5);
        TableCommentEvaluationData cmmnt49 = new TableCommentEvaluationData(49, "Bu tablo beni büyüledi. Ressamýn sanatýna hayran kaldým.", 5);
        TableCommentEvaluationData cmmnt50 = new TableCommentEvaluationData(50, "Muhteþem bir tablo. Ressamýn yeteneði sýnýrlarý aþýyor.", 5);

        datas.Add(cmmnt1);
        datas.Add(cmmnt2);
        datas.Add(cmmnt3);
        datas.Add(cmmnt4);
        datas.Add(cmmnt5);
        datas.Add(cmmnt6);
        datas.Add(cmmnt7);
        datas.Add(cmmnt8);
        datas.Add(cmmnt9);
        datas.Add(cmmnt10);

        datas.Add(cmmnt11);
        datas.Add(cmmnt12);
        datas.Add(cmmnt13);
        datas.Add(cmmnt14);
        datas.Add(cmmnt15);
        datas.Add(cmmnt16);
        datas.Add(cmmnt17);
        datas.Add(cmmnt18);
        datas.Add(cmmnt19);
        datas.Add(cmmnt20);

        datas.Add(cmmnt21);
        datas.Add(cmmnt22);
        datas.Add(cmmnt23);
        datas.Add(cmmnt24);
        datas.Add(cmmnt25);
        datas.Add(cmmnt26);
        datas.Add(cmmnt27);
        datas.Add(cmmnt28);
        datas.Add(cmmnt29);
        datas.Add(cmmnt30);

        datas.Add(cmmnt31);
        datas.Add(cmmnt32);
        datas.Add(cmmnt33);
        datas.Add(cmmnt34);
        datas.Add(cmmnt35);
        datas.Add(cmmnt36);
        datas.Add(cmmnt37);
        datas.Add(cmmnt38);
        datas.Add(cmmnt39);
        datas.Add(cmmnt40);

        datas.Add(cmmnt41);
        datas.Add(cmmnt42);
        datas.Add(cmmnt43);
        datas.Add(cmmnt44);
        datas.Add(cmmnt45);
        datas.Add(cmmnt46);
        datas.Add(cmmnt47);
        datas.Add(cmmnt48);
        datas.Add(cmmnt49);
        datas.Add(cmmnt50);

    }
    public List<TableCommentEvaluationData> GetComment(float _starValue)
    {
        return datas.Where(x => x.StarValue == _starValue).ToList();        
    }

    public TableCommentEvaluationData GetComment(int _iD)
    {
        return datas.Find(x => x.ID == _iD);
    }
    public TableCommentEvaluationData GetComment(TableCommentEvaluationData _evaluationData)
    {
        return datas.Find(x => x.ID == _evaluationData.ID);
    }
}
