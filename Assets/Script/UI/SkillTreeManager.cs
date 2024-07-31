using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class SkillTreeManager : MonoBehaviour
{

    public List<SkillNode> skillNodes = new List<SkillNode>();
    public List<GameObject> skillObjects = new List<GameObject>();
    
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
        SkillNode sn1 = new SkillNode(0, "Ziyaretçi Rehberi", "Oyuncuyu bilgilendirecek rehber.", "Mutluluk", 0, 0, 1,
            new List<eStat> { eStat.BaseHappiness }, new List<int> { 5 });
        SkillNode sn2 = new SkillNode(1, "Reklam Kampanyalarý", "Müzenizin tanýtýmýný yapmak için reklam kampanyalarý oluþturun.", "Ziyaretçi Kapasitesi", 0, 0, 1,
            new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn3 = new SkillNode(2, "Sanat Okulu Ýþbirliði", "Yerel sanat okullarýyla iþbirliði yaparak öðrencileri müzenize çekin.", "Kültür Exp", 5, 3000, 3,
            new List<eStat> { eStat.CultureExp}, new List<int> { 5, 10, 15});
        SkillNode sn4 = new SkillNode(3, "Özel Sergiler", "Özel sergiler düzenleyerek daha fazla giriþ ücreti alýn.", "Müze Giriþ Ücreti", 7, 5000, 5,
            new List<eStat> { eStat.MuseumEnterPrice }, new List<int> { 10, 20, 30, 40, 50 });
        SkillNode sn5 = new SkillNode(4, "Etkileþimli Eðitim", "Ziyaretçilere interaktif eðitim deneyimleri sunarak mutluluk kazançlarýný artýrýn.", "Mutluluk Artýþ Oraný", 10, 7000, 3,
            new List<eStat> { eStat.HappinessIncreaseRatio }, new List<int> { 5, 10, 15 });
        SkillNode sn6 = new SkillNode(5, "Sanatçý Konuklar", "Ünlü sanatçýlarý müzenize davet edin ve ziyaretçi çekmeyi artýrýn.", "Pozitif Yorum", 5, 10000, 3,
            new List<eStat> { eStat.VisitorPossitiveComment }, new List<int> { 5, 10, 15});
        SkillNode sn7 = new SkillNode(6, "Müze Restorasyonu", "Müzenizin fiziksel durumunu iyileþtirerek ziyaretçi memnuniyetini artýrýn.", "Ziyaret Etme Ýsteði", 50, 15000, 5,
            new List<eStat> { eStat.WantVisittingRatio }, new List<int> { 5, 8, 10, 12, 14 });
        SkillNode sn8 = new SkillNode(7, "Ziyaretçi Memnuniyeti", "Ziyaretçilerin memnuniyetini arttýrýn.", "Mutluluk Artýþ Oraný", 13, 13000, 3,
            new List<eStat> {  eStat.HappinessIncreaseRatio }, new List<int> { 15,20,25 });
        SkillNode sn9 = new SkillNode(8, "Temizlik Önemlidir", "Temizlikçilerin aldýðý günlük maaþý düþürür.", "Temizlikçi Maaþ Ýndirimi", 6, 10000, 3,
            new List<eStat> { eStat.CleanerSalaryDiscount}, new List<int> { 5, 10, 15});
        SkillNode sn10 = new SkillNode(9, "Sanat Atölyeleri", "Sanat atölyeleri düzenleyerek öðretici deneyimler sunun.", "Ziyaretçi Hýzý Artýþý", 5, 30000, 3,
            new List<eStat> { eStat.VisitorsSpeedIncrease}, new List<int> { 5, 10, 15});
        SkillNode sn11 = new SkillNode(10, "Kavga Çýkarma!", "Güvenlik hýzý artýþý saðlar.", "Güvenlik Hýzý Artýþý", 5, 12000, 5,
            new List<eStat> { eStat.SecuritiesSpeedIncrease }, new List<int> { 5, 10, 15, 18, 20});
        SkillNode sn12 = new SkillNode(11, "Kültür Festivali", "Kültür festivalleri yaklaþýyor, daha hýzlý temizlemeliyiz!", "Temizlikçi Hýzý Artýþý", 5, 8000, 5,
            new List<eStat> { eStat.CleanersSpeedIncrease }, new List<int> { 5, 10, 15, 18, 20 });
        SkillNode sn13 = new SkillNode(12, "VR Sanat Deneyimi", "Sanal gerçeklik sanat deneyimleri sunarak teknolojik olarak öne çýkýn.", "Kültür Exp", 7, 15000, 3,
            new List<eStat> { eStat.CultureExp }, new List<int> { 20, 25, 30});
        SkillNode sn14 = new SkillNode(13, "Müze Uzantýsý", "Müzenizin alanýný geniþleterek daha fazla eser sergileyin.", "Ziyaretçi Kapasitesi", 20, 100000, 2,
            new List<eStat> { eStat.VisitorCapacity}, new List<int> { 20,30});

        SkillNode sn15 = new SkillNode(14, "Gece Açýlýþlar", "Gece açýlýþlarý düzenleyerek daha fazla ziyaretçiye ulaþýn.", "Ziyaretçi Kapasitesi", 5, 5000, 10, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5, 10, 15, 20, 25, 30 });
        SkillNode sn16 = new SkillNode(15, "Sanat Konferanslarý", "Sanat konferanslarý düzenleyerek kültürel etkinlikler sunun.", "Mutluluk", 5, 5000, 10, new List<eStat> { eStat.BaseHappiness }, new List<int> { 5, 10, 15, 20, 25, 30 });

        SkillNode sn17 = new SkillNode(16, "Eser Baðýþlarý", "Baðýþlarla müzenizin koleksiyonunu geniþletin.", "Pozitif Yorum", 12, 40000, 3,
            new List<eStat> { eStat.HappinessIncreaseRatio }, new List<int> { 12, 15, 17}) ;
        SkillNode sn18 = new SkillNode(17, "Malzemeleri Yenile", "Temizlikçiler kaliteli temizlik yapmayý sever.", "Temizlikçi Hýzý Artýþý", 8, 45000, 3,
            new List<eStat> { eStat.CleanersSpeedIncrease }, new List<int> { 18, 19, 20 });
        SkillNode sn19 = new SkillNode(18, "Ziyaretçiler Uçsun!", "Ziyaretçiler paten ayakkabý giyer.", "Ziyaretçi Hýzý Artýþý", 8, 25000, 3,
            new List<eStat> { eStat.VisitorsSpeedIncrease}, new List<int> { 18, 22, 24});
        SkillNode sn20 = new SkillNode(19, "Reklam Afiþleri", "Ziyaretçiler müzeyi daha çok merak eder.", "Ziyaret Etme Ýsteði", 8, 80000, 2,
            new List<eStat> { eStat.WantVisittingRatio }, new List<int> { 20, 30 });

        SkillNode sn21 = new SkillNode(20, "Yeni Sergi Salonlarý", "Yeni sergi salonlarý açarak müzenizin kapasitesini artýrýn.", "Ziyaretçi Kapasitesi, +5 Ziyaret Etme Ýsteði", 140, 60000, 3,
            new List<eStat> { eStat.VisitorCapacity, eStat.WantVisittingRatio }, new List<int> { 5, 10, 15, 5, 10, 15 });
        
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
        skillNodes.Add(sn20);
        skillNodes.Add(sn21);
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

    public void AddSkillObject(GameObject _skillObject)
    {
        skillObjects.Add(_skillObject);
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
                    //skill.SkillRequiredPoint = _currentAmount;
                }
            }
        }
    }
    public void SetSkillTextProcess(SkillNode skill)
    {
        SkillNode currentSkill = skillNodes.Where(x => x.ID == skill.ID).SingleOrDefault();
        GameObject baseSkiilObj = skillObjects.Where(x => x.GetComponent<BaseSkillOptions>().SkillID == currentSkill.ID).SingleOrDefault();

        if (currentSkill != null)
        {
            currentSkill = skill;
            int lenght = baseSkiilObj.transform.childCount;
            for (int i = 0; i < lenght; i++)
            {
                Debug.Log(baseSkiilObj.transform.GetChild(i).gameObject.name);
                Transform childTransform = baseSkiilObj.transform.GetChild(i);
                if (childTransform.TryGetComponent(out SkillAbilityAmountController skillAmountController))
                {
                    Debug.Log(currentSkill.SkillCurrentLevel);
                    Debug.Log(currentSkill.SkillMaxLevel);
                    skillAmountController.SetSkillCurrentLevelUI(currentSkill.SkillCurrentLevel);
                }
                if (childTransform.TryGetComponent(out SkillAbilityMaxAmountController skillMaxController))
                {
                    skillMaxController.SetSkillMaxLevelUI(currentSkill.SkillMaxLevel);
                    Debug.Log("Mevcut Skill'in Max Leveli => " + currentSkill.SkillMaxLevel);
                }
            }
        }
    }
}
