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
        TableCommentEvaluationData cmmnt1 = new TableCommentEvaluationData(1, "Bu tablo hi�bir �ey ifade etmiyor. Neden burada?", 1);
        TableCommentEvaluationData cmmnt2 = new TableCommentEvaluationData(2, "Hi�bir �ey anlayamad�m. Neden buraday�m ki?", 1);
        TableCommentEvaluationData cmmnt3 = new TableCommentEvaluationData(3, "Tablo k�t�yd�. Bir anlam� yoktu.", 1);
        TableCommentEvaluationData cmmnt4 = new TableCommentEvaluationData(4, "Bu tablo beklentilerimi kar��lamad�. K�t� bir se�im.", 1);
        TableCommentEvaluationData cmmnt5 = new TableCommentEvaluationData(5, "Tablo beni hayal k�r�kl���na u�ratt�. Daha iyisini beklerdim.", 1);
        TableCommentEvaluationData cmmnt6 = new TableCommentEvaluationData(6, "Tablo anlams�zd�. Daha fazla bilgiye ihtiya� duyuyorum.", 1);
        TableCommentEvaluationData cmmnt7 = new TableCommentEvaluationData(7, "Bu tablo �ok s�k�c�. Sanatsal bir de�eri yok.", 1);
        TableCommentEvaluationData cmmnt8 = new TableCommentEvaluationData(8, "Tablo k�t� bir tercihti. Neden buraday�m?", 1);
        TableCommentEvaluationData cmmnt9 = new TableCommentEvaluationData(9, "Bu tablo ile ba�lant� kuramad�m. �lgimi �ekmedi.", 1);
        TableCommentEvaluationData cmmnt10 = new TableCommentEvaluationData(10, "K�t� bir tablo. Ressam�n yetene�i sorgulanmal�.", 1);
        //Two Star Comment
        TableCommentEvaluationData cmmnt11 = new TableCommentEvaluationData(11, "Tablo ortalama, ama biraz daha �ekici olabilirdi.", 2);
        TableCommentEvaluationData cmmnt12 = new TableCommentEvaluationData(12, "Potansiyeli var, ama daha fazla detay ve a��klama gerekli.", 2);
        TableCommentEvaluationData cmmnt13 = new TableCommentEvaluationData(13, "Tablo s�k�c�yd�, ama baz� ilgin� ��eler vard�.", 2);
        TableCommentEvaluationData cmmnt14 = new TableCommentEvaluationData(14, "Ortalama bir tablo. �yile�tirme gerekiyor.", 2);
        TableCommentEvaluationData cmmnt15 = new TableCommentEvaluationData(15, "Beklentilerimi tam olarak kar��lamad�. Daha fazla bilgiye ihtiya� var.", 2);
        TableCommentEvaluationData cmmnt16 = new TableCommentEvaluationData(16, "Bu tablo ile ba�lant� kuramad�m, ama baz� ilgin� detaylar vard�.", 2);
        TableCommentEvaluationData cmmnt17 = new TableCommentEvaluationData(17, "Tablo idare eder, ama daha fazla ilgi �ekici olabilir.", 2);
        TableCommentEvaluationData cmmnt18 = new TableCommentEvaluationData(18, "S�k�c� bir tablo. Daha fazla a��klama ve kontekst gerekiyor.", 2);
        TableCommentEvaluationData cmmnt19 = new TableCommentEvaluationData(19, "Biraz daha ilgi �ekici olabilirdi, ama beklentilerimin alt�nda kald�.", 2);
        TableCommentEvaluationData cmmnt20 = new TableCommentEvaluationData(20, "Tablo vasat. Daha fazla bilgiye ve a��klamaya ihtiya� var."
, 2);
        //Three Star Comment
        TableCommentEvaluationData cmmnt21 = new TableCommentEvaluationData(21, "Ortalama bir tablo. �yi, ama baz� eksiklikler var.", 3);
        TableCommentEvaluationData cmmnt22 = new TableCommentEvaluationData(22, "Fena de�il, ama baz� detaylar daha ilgi �ekici olabilirdi.", 3);
        TableCommentEvaluationData cmmnt23 = new TableCommentEvaluationData(23, "Tablo iyi bir deneyimdi, ama baz� a��klamalara ihtiya� var.", 3);
        TableCommentEvaluationData cmmnt24 = new TableCommentEvaluationData(24, "Ho� bir tablo, ancak baz� yerlerde eksiklik var.", 3);
        TableCommentEvaluationData cmmnt25 = new TableCommentEvaluationData(25, "Tablo beni etkiledi, ancak daha fazla a��klama gerekli.", 3);
        TableCommentEvaluationData cmmnt26 = new TableCommentEvaluationData(26, "Tablo ho�uma gitti, ama baz� yerlerde geli�tirmeye ihtiya� var.", 3);
        TableCommentEvaluationData cmmnt27 = new TableCommentEvaluationData(27, "G�zel bir tablo. Daha fazla detay ve kontekst eklenmeli.", 3);
        TableCommentEvaluationData cmmnt28 = new TableCommentEvaluationData(28, "Ortalaman�n �st�ndeydi, ama hala baz� eksiklikler var.", 3);
        TableCommentEvaluationData cmmnt29 = new TableCommentEvaluationData(29, "Tabloyu be�endim, ama daha fazla bilgiye ihtiya� var.", 3);
        TableCommentEvaluationData cmmnt30 = new TableCommentEvaluationData(30, "Potansiyeli var ve baz� detaylar �ok etkileyiciydi.", 3);
        //Four Star Comment
        TableCommentEvaluationData cmmnt31 = new TableCommentEvaluationData(31, "Bu tablo olduk�a etkileyici. Ressam�n yetene�i harika.", 4);
        TableCommentEvaluationData cmmnt32 = new TableCommentEvaluationData(32, "Tablo g�zel, ancak daha fazla detay eklenmeli.", 4);
        TableCommentEvaluationData cmmnt33 = new TableCommentEvaluationData(33, "Bu tablo b�y�leyici. Ressam�n sanat�n� takdir ediyorum.", 4);
        TableCommentEvaluationData cmmnt34 = new TableCommentEvaluationData(34, "Harika bir tablo, ancak daha fazla bilgiye ihtiya� duyuyorum.", 4);
        TableCommentEvaluationData cmmnt35 = new TableCommentEvaluationData(35, "Tablo ilgin�, ama biraz daha a��klama olmal�.", 4);
        TableCommentEvaluationData cmmnt36 = new TableCommentEvaluationData(36, "Ressam�n yetene�i dikkat �ekici. Bu tabloyu sevdim.", 4);
        TableCommentEvaluationData cmmnt37 = new TableCommentEvaluationData(37, "Tablo �ok g�zel. Daha fazla bilgi eklenmeli.", 4);
        TableCommentEvaluationData cmmnt38 = new TableCommentEvaluationData(38, "Bu tablo ho�uma gitti, ama daha fazla tarih bilgisi laz�m.", 4);
        TableCommentEvaluationData cmmnt39 = new TableCommentEvaluationData(39, "Ressam�n tarz�n� seviyorum. Bu tablo muhte�em.", 4);
        TableCommentEvaluationData cmmnt40 = new TableCommentEvaluationData(40, "G�zel bir tablo, ancak daha fazla arka plan bilgisi laz�m.", 4);
        //Five Star Comment
        TableCommentEvaluationData cmmnt41 = new TableCommentEvaluationData(41, "Muhte�em bir tablo! Her detay� hayranl�kla inceliyorum.", 5);
        TableCommentEvaluationData cmmnt42 = new TableCommentEvaluationData(42, "Harika bir resim. Ressam�n yetene�i ola�an�st�.", 5);
        TableCommentEvaluationData cmmnt43 = new TableCommentEvaluationData(43, "Tablo beni b�y�ledi. Sanat�n doruklar�na ula��yor.", 5);
        TableCommentEvaluationData cmmnt44 = new TableCommentEvaluationData(44, "M�kemmel bir tablo. Detaylar inan�lmaz.", 5);
        TableCommentEvaluationData cmmnt45 = new TableCommentEvaluationData(45, "Bu tabloyu hayranl�kla izliyorum. Ressam ger�ek bir deha.", 5);
        TableCommentEvaluationData cmmnt46 = new TableCommentEvaluationData(46, "Bu tablo bir ba�yap�t. Her ayr�nt�s� muhte�em.", 5);
        TableCommentEvaluationData cmmnt47 = new TableCommentEvaluationData(47, "Ressam�n tarz� b�y�leyici. Bu tablo herkesin g�rmesi gereken bir eser.", 5);
        TableCommentEvaluationData cmmnt48 = new TableCommentEvaluationData(48, "Tablo etkileyici. Daha fazla detay ��renmeliyim.", 5);
        TableCommentEvaluationData cmmnt49 = new TableCommentEvaluationData(49, "Bu tablo beni b�y�ledi. Ressam�n sanat�na hayran kald�m.", 5);
        TableCommentEvaluationData cmmnt50 = new TableCommentEvaluationData(50, "Muhte�em bir tablo. Ressam�n yetene�i s�n�rlar� a��yor.", 5);

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
