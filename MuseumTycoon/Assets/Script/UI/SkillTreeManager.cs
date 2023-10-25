using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SkillTreeManager : MonoBehaviour
{
    
    List<SkillNode> skillNodes = new List<SkillNode>();

    private SkillNode selectedSkill; // Týklanan yetenek
    public static SkillTreeManager instance { get; private set; }
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
    public void AddSkillsForSkillTree()
    {
        SkillNode sn1 = new SkillNode(0, "Ziyaretçi Rehberi", "Oyuncuyu bilgilendirecek rehber.", "+5 Mutluluk", 0, 0);
        SkillNode sn2 = new SkillNode(1, "Reklam Kampanyalarý", "Müzenizin tanýtýmýný yapmak için reklam kampanyalarý oluþturun.", "+5 Ziyaretçi Kapasitesi", 0,0);
        SkillNode sn3 = new SkillNode(3, "Sanat Okulu Ýþbirliði", "Yerel sanat okullarýyla iþbirliði yaparak öðrencileri müzenize çekin.", "+5 Mutluluk, +5 Ziyaretçi Kapasitesi", 10, 5000);
        SkillNode sn4 = new SkillNode(4, "Özel Sergiler", "Özel sergiler düzenleyerek daha fazla ziyaretçi çekin.", "+10 Ziyaretçi Kapasitesi", 20, 10000);
        SkillNode sn5 = new SkillNode(5, "Etkileþimli Eðitim", "Ziyaretçilere interaktif eðitim deneyimleri sunarak kültür kazançlarýný artýrýn.", "+10 Kültür Exp Kazanýmý", 30, 15000);
        SkillNode sn6 = new SkillNode(6, "Sanatçý Konuklar", "Ünlü sanatçýlarý müzenize davet edin ve ziyaretçi çekmeyi artýrýn.", "+15 Ziyaretçi Kapasitesi, +5 Mutluluk", 40, 20000);
        SkillNode sn7 = new SkillNode(7, "Müze Restorasyonu", "Müzenizin fiziksel durumunu iyileþtirerek ziyaretçi memnuniyetini artýrýn.", "+10 Mutluluk, +10 Ziyaretçi Kapasitesi", 50, 25000);
        SkillNode sn8 = new SkillNode(8, "Gece Açýlýþlar", "Gece açýlýþlarý düzenleyerek daha fazla ziyaretçiye ulaþýn.", "+15 Ziyaretçi Kapasitesi", 60, 30000);
        SkillNode sn9 = new SkillNode(9, "VIP Rehberler", "VIP rehberlerle iþbirliði yaparak premium turlar sunun.", "+10 Mutluluk, +15 Ziyaretçi Kapasitesi", 70, 35000);
        SkillNode sn10 = new SkillNode(10, "Sanat Atölyeleri", "Sanat atölyeleri düzenleyerek öðretici deneyimler sunun.", "+15 Kültür Exp Kazanýmý", 80, 40000);
        SkillNode sn11 = new SkillNode(11, "Uzay Sergisi", "Uzay temalý sergiler düzenleyerek daha geniþ bir kitleye ulaþýn.", "+20 Ziyaretçi Kapasitesi, +5 Mutluluk", 90, 45000);
        SkillNode sn12 = new SkillNode(12, "Kültür Festivali", "Kültür festivalleri düzenleyerek müzenizin popülaritesini artýrýn.", "+20 Ziyaretçi Kapasitesi", 100, 50000);
        SkillNode sn13 = new SkillNode(13, "VR Sanat Deneyimi", "Sanal gerçeklik sanat deneyimleri sunarak teknolojik olarak öne çýkýn.", "+20 Kültür Exp Kazanýmý", 110, 55000);
        SkillNode sn14 = new SkillNode(14, "Müze Uzantýsý", "Müzenizin alanýný geniþleterek daha fazla eser sergileyin.", "+15 Ziyaretçi Kapasitesi, +10 Mutluluk", 120, 60000);
        SkillNode sn15 = new SkillNode(15, "Arkeoloji Kazýlarý", "Arkeoloji kazýlarýna katýlarak benzersiz eserler bulun.", "+25 Kültür Exp Kazanýmý", 130, 65000);
        SkillNode sn16 = new SkillNode(16, "Sanatçý Ýþbirliði", "Ünlü sanatçýlarla iþbirliði yaparak özel koleksiyonlar oluþturun.", "+30 Ziyaretçi Kapasitesi, +10 Mutluluk", 140, 70000);
        SkillNode sn17 = new SkillNode(17, "Dijital Sanat Gösterileri", "Dijital sanat gösterileriyle modern sanat deneyimleri sunun.", "+25 Kültür Exp Kazanýmý", 150, 75000);
        SkillNode sn18 = new SkillNode(18, "Kültürel Etkinlikler", "Kültürel etkinlikler düzenleyerek müzenizi canlandýrýn.", "+40 Ziyaretçi Kapasitesi, +15 Mutluluk", 160, 80000);
        SkillNode sn19 = new SkillNode(19, "Müze Hava Körü", "Müzenizi her türlü hava koþulunda açýk tutun.", "+50 Kültür Exp Kazanýmý", 170, 85000);

        // Tüm yetenekleri ekleyin
        skillNodes.Add(sn1);
        skillNodes.Add(sn2);
        skillNodes.Add(sn3);
        skillNodes.Add(sn4);
        skillNodes.Add(sn5);
        skillNodes.Add(sn6);
        skillNodes.Add(sn7);
        skillNodes.Add(sn8);
        skillNodes.Add(sn9);
        skillNodes.Add(sn10);
        skillNodes.Add(sn11);
        skillNodes.Add(sn12);
        skillNodes.Add(sn13);
        skillNodes.Add(sn14);
        skillNodes.Add(sn15);
        skillNodes.Add(sn16);
        skillNodes.Add(sn17);
        skillNodes.Add(sn18);
        skillNodes.Add(sn19);

    }
   
    public SkillNode GetSelectedSkillNode(int _id)
    {        
        return skillNodes.Find(x => x.ID == _id);
        
    }
    

    

    

}
