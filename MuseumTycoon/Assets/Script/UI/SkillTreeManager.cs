using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class SkillTreeManager : MonoBehaviour
{

    public List<SkillNode> skillNodes = new List<SkillNode>();

    public SkillNode SelectedSkill; // Tiklanan yetenek
    public GameObject SelectedSkillGameObject; // Tiklanan yetenegin gameobject'i
    public List<int> CurrentBuffs;

    public int DefaultMaxSkillLevel = 10;
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
        RefreshSkillBonuses();
    }
    public void AddSkillsForSkillTree()
    {
        SkillNode sn1 = new SkillNode(0, "Ziyaretçi Rehberi", "Oyuncuyu bilgilendirecek rehber.", "+5 Mutluluk", 0, 0, DefaultMaxSkillLevel,
            new List<eStat> { eStat.BaseHappiness }, new List<int> { 1,2,3,4,5,6,7,8,9,10 });
        SkillNode sn2 = new SkillNode(1, "Reklam Kampanyalarý", "Müzenizin tanýtýmýný yapmak için reklam kampanyalarý oluþturun.", "+5 Ziyaretçi Kapasitesi", 0, 0, DefaultMaxSkillLevel,
            new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 });
        SkillNode sn3 = new SkillNode(2, "Test", "Müzenizin tanýtýmýný", "+5 Ziyaretçi Kapasitesi", 2, 3000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn4 = new SkillNode(3, "Sanat Okulu Ýþbirliði", "Yerel sanat okullarýyla iþbirliði yaparak öðrencileri müzenize çekin.", "+5 Mutluluk, +5 Ziyaretçi Kapasitesi", 10, 5000,
            DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn5 = new SkillNode(4, "Özel Sergiler", "Özel sergiler düzenleyerek daha fazla ziyaretçi çekin.", "+10 Ziyaretçi Kapasitesi", 20, 10000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 }
        );
        SkillNode sn6 = new SkillNode(5, "Etkileþimli Eðitim", "Ziyaretçilere interaktif eðitim deneyimleri sunarak kültür kazançlarýný artýrýn.", "+10 Kültür Exp Kazanýmý", 30, 15000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn7 = new SkillNode(6, "Sanatçý Konuklar", "Ünlü sanatçýlarý müzenize davet edin ve ziyaretçi çekmeyi artýrýn.", "+15 Ziyaretçi Kapasitesi, +5 Mutluluk", 40, 20000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn8 = new SkillNode(7, "Müze Restorasyonu", "Müzenizin fiziksel durumunu iyileþtirerek ziyaretçi memnuniyetini artýrýn.", "+10 Mutluluk, +10 Ziyaretçi Kapasitesi", 50, 25000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn9 = new SkillNode(8, "Gece Açýlýþlar", "Gece açýlýþlarý düzenleyerek daha fazla ziyaretçiye ulaþýn.", "+15 Ziyaretçi Kapasitesi", 60, 30000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 }
        );
        SkillNode sn10 = new SkillNode(9, "VIP Rehberler", "VIP rehberlerle iþbirliði yaparak premium turlar sunun.", "+10 Mutluluk, +15 Ziyaretçi Kapasitesi", 70, 35000, DefaultMaxSkillLevel,
            new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn11 = new SkillNode(10, "Sanat Atölyeleri", "Sanat atölyeleri düzenleyerek öðretici deneyimler sunun.", "+15 Kültür Exp Kazanýmý", 80, 40000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 }
        );
        SkillNode sn12 = new SkillNode(11, "Uzay Sergisi", "Uzay temalý sergiler düzenleyerek daha geniþ bir kitleye ulaþýn.", "+20 Ziyaretçi Kapasitesi, +5 Mutluluk", 90, 45000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn13 = new SkillNode(12, "Kültür Festivali", "Kültür festivalleri düzenleyerek müzenizin popülaritesini artýrýn.", "+20 Ziyaretçi Kapasitesi", 100, 50000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 }
        );
        SkillNode sn14 = new SkillNode(13, "VR Sanat Deneyimi", "Sanal gerçeklik sanat deneyimleri sunarak teknolojik olarak öne çýkýn.", "+20 Kültür Exp Kazanýmý", 110, 55000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn15 = new SkillNode(14, "Müze Uzantýsý", "Müzenizin alanýný geniþleterek daha fazla eser sergileyin.", "+15 Ziyaretçi Kapasitesi, +10 Mutluluk", 120, 60000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 }
        );
        SkillNode sn16 = new SkillNode(15, "Arkeoloji Kazýlarý", "Arkeoloji kazýlarýna katýlarak benzersiz eserler bulun.", "+25 Kültür Exp Kazanýmý", 130, 65000, DefaultMaxSkillLevel,
            new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn17 = new SkillNode(16, "Sanatçý Ýþbirliði", "Ünlü sanatçýlarla iþbirliði yaparak özel koleksiyonlar oluþturun.", "+30 Ziyaretçi Kapasitesi, +10 Mutluluk", 140, 70000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn18 = new SkillNode(17, "Dijital Sanat Gösterileri", "Dijital sanat gösterileriyle modern sanat deneyimleri sunun.", "+25 Kültür Exp Kazanýmý", 150, 75000, DefaultMaxSkillLevel,
            new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn19 = new SkillNode(18, "Kültürel Etkinlikler", "Kültürel etkinlikler düzenleyerek müzenizi canlandýrýn.", "+40 Ziyaretçi Kapasitesi, +15 Mutluluk", 160, 80000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn20 = new SkillNode(19, "Müze Hava Körü", "Müzenizi her türlü hava koþulunda açýk tutun.", "+50 Kültür Exp Kazanýmý", 170, 85000, DefaultMaxSkillLevel,
            new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });

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

    public void CalculateForCurrentSkillEnoughLevelAndMoney(SkillNode _selectedSkill)
    {
        if (_selectedSkill.IsMoneyAndLevelEnough(MuseumManager.instance.GetCurrentGold(), MuseumManager.instance.GetCurrentSkillPoint()))
        {
            _selectedSkill.Lock(false);

        }
        else { _selectedSkill.Lock(true); }
    }

    public bool IsPurchased(SkillNode _selectedSkill)
    {
        return GetSelectedSkillNode(_selectedSkill.ID).IsPurchased;
    }

   
    public List<SkillNode> GetActiveSkillNodes()
    {
        return skillNodes.Where(x => x.IsPurchased).ToList();
    }

    public void RefreshSkillBonuses()
    {
        CurrentBuffs = new List<int>();
        for (int i = 0; i < 40; i++)
        {
            CurrentBuffs.Add(0);
        }
        List<SkillNode> activeSkills = GetActiveSkillNodes();
        foreach (var skill in activeSkills)
        {
            int _currentStatAmount = skill.buffs.Count;
            for (int i = 0; i < _currentStatAmount; i++)
            {
                int _currentAmount = skill.Amounts[(i) * skill.SkillMaxLevel + (skill.SkillCurrentLevel - 1)];
                CurrentBuffs[(int)skill.buffs[i]] += _currentAmount;
                if ( skill.buffs[i] == eStat.VisitorCapacity)
                {
                    skill.SkillRequiredPoint = _currentAmount;
                }
            }
        }
    }
}
