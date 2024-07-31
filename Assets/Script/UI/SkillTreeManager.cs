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
        SkillNode sn1 = new SkillNode(0, "Ziyaret�i Rehberi", "Oyuncuyu bilgilendirecek rehber.", "Mutluluk", 0, 0, 1,
            new List<eStat> { eStat.BaseHappiness }, new List<int> { 5 });
        SkillNode sn2 = new SkillNode(1, "Reklam Kampanyalar�", "M�zenizin tan�t�m�n� yapmak i�in reklam kampanyalar� olu�turun.", "Ziyaret�i Kapasitesi", 0, 0, 1,
            new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn3 = new SkillNode(2, "Sanat Okulu ��birli�i", "Yerel sanat okullar�yla i�birli�i yaparak ��rencileri m�zenize �ekin.", "K�lt�r Exp", 5, 3000, 3,
            new List<eStat> { eStat.CultureExp}, new List<int> { 5, 10, 15});
        SkillNode sn4 = new SkillNode(3, "�zel Sergiler", "�zel sergiler d�zenleyerek daha fazla giri� �creti al�n.", "M�ze Giri� �creti", 7, 5000, 5,
            new List<eStat> { eStat.MuseumEnterPrice }, new List<int> { 10, 20, 30, 40, 50 });
        SkillNode sn5 = new SkillNode(4, "Etkile�imli E�itim", "Ziyaret�ilere interaktif e�itim deneyimleri sunarak mutluluk kazan�lar�n� art�r�n.", "Mutluluk Art�� Oran�", 10, 7000, 3,
            new List<eStat> { eStat.HappinessIncreaseRatio }, new List<int> { 5, 10, 15 });
        SkillNode sn6 = new SkillNode(5, "Sanat�� Konuklar", "�nl� sanat��lar� m�zenize davet edin ve ziyaret�i �ekmeyi art�r�n.", "Pozitif Yorum", 5, 10000, 3,
            new List<eStat> { eStat.VisitorPossitiveComment }, new List<int> { 5, 10, 15});
        SkillNode sn7 = new SkillNode(6, "M�ze Restorasyonu", "M�zenizin fiziksel durumunu iyile�tirerek ziyaret�i memnuniyetini art�r�n.", "Ziyaret Etme �ste�i", 50, 15000, 5,
            new List<eStat> { eStat.WantVisittingRatio }, new List<int> { 5, 8, 10, 12, 14 });
        SkillNode sn8 = new SkillNode(7, "Ziyaret�i Memnuniyeti", "Ziyaret�ilerin memnuniyetini artt�r�n.", "Mutluluk Art�� Oran�", 13, 13000, 3,
            new List<eStat> {  eStat.HappinessIncreaseRatio }, new List<int> { 15,20,25 });
        SkillNode sn9 = new SkillNode(8, "Temizlik �nemlidir", "Temizlik�ilerin ald��� g�nl�k maa�� d���r�r.", "Temizlik�i Maa� �ndirimi", 6, 10000, 3,
            new List<eStat> { eStat.CleanerSalaryDiscount}, new List<int> { 5, 10, 15});
        SkillNode sn10 = new SkillNode(9, "Sanat At�lyeleri", "Sanat at�lyeleri d�zenleyerek ��retici deneyimler sunun.", "Ziyaret�i H�z� Art���", 5, 30000, 3,
            new List<eStat> { eStat.VisitorsSpeedIncrease}, new List<int> { 5, 10, 15});
        SkillNode sn11 = new SkillNode(10, "Kavga ��karma!", "G�venlik h�z� art��� sa�lar.", "G�venlik H�z� Art���", 5, 12000, 5,
            new List<eStat> { eStat.SecuritiesSpeedIncrease }, new List<int> { 5, 10, 15, 18, 20});
        SkillNode sn12 = new SkillNode(11, "K�lt�r Festivali", "K�lt�r festivalleri yakla��yor, daha h�zl� temizlemeliyiz!", "Temizlik�i H�z� Art���", 5, 8000, 5,
            new List<eStat> { eStat.CleanersSpeedIncrease }, new List<int> { 5, 10, 15, 18, 20 });
        SkillNode sn13 = new SkillNode(12, "VR Sanat Deneyimi", "Sanal ger�eklik sanat deneyimleri sunarak teknolojik olarak �ne ��k�n.", "K�lt�r Exp", 7, 15000, 3,
            new List<eStat> { eStat.CultureExp }, new List<int> { 20, 25, 30});
        SkillNode sn14 = new SkillNode(13, "M�ze Uzant�s�", "M�zenizin alan�n� geni�leterek daha fazla eser sergileyin.", "Ziyaret�i Kapasitesi", 20, 100000, 2,
            new List<eStat> { eStat.VisitorCapacity}, new List<int> { 20,30});

        SkillNode sn15 = new SkillNode(14, "Gece A��l��lar", "Gece a��l��lar� d�zenleyerek daha fazla ziyaret�iye ula��n.", "Ziyaret�i Kapasitesi", 5, 5000, 10, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5, 10, 15, 20, 25, 30 });
        SkillNode sn16 = new SkillNode(15, "Sanat Konferanslar�", "Sanat konferanslar� d�zenleyerek k�lt�rel etkinlikler sunun.", "Mutluluk", 5, 5000, 10, new List<eStat> { eStat.BaseHappiness }, new List<int> { 5, 10, 15, 20, 25, 30 });

        SkillNode sn17 = new SkillNode(16, "Eser Ba���lar�", "Ba���larla m�zenizin koleksiyonunu geni�letin.", "Pozitif Yorum", 12, 40000, 3,
            new List<eStat> { eStat.HappinessIncreaseRatio }, new List<int> { 12, 15, 17}) ;
        SkillNode sn18 = new SkillNode(17, "Malzemeleri Yenile", "Temizlik�iler kaliteli temizlik yapmay� sever.", "Temizlik�i H�z� Art���", 8, 45000, 3,
            new List<eStat> { eStat.CleanersSpeedIncrease }, new List<int> { 18, 19, 20 });
        SkillNode sn19 = new SkillNode(18, "Ziyaret�iler U�sun!", "Ziyaret�iler paten ayakkab� giyer.", "Ziyaret�i H�z� Art���", 8, 25000, 3,
            new List<eStat> { eStat.VisitorsSpeedIncrease}, new List<int> { 18, 22, 24});
        SkillNode sn20 = new SkillNode(19, "Reklam Afi�leri", "Ziyaret�iler m�zeyi daha �ok merak eder.", "Ziyaret Etme �ste�i", 8, 80000, 2,
            new List<eStat> { eStat.WantVisittingRatio }, new List<int> { 20, 30 });

        SkillNode sn21 = new SkillNode(20, "Yeni Sergi Salonlar�", "Yeni sergi salonlar� a�arak m�zenizin kapasitesini art�r�n.", "Ziyaret�i Kapasitesi, +5 Ziyaret Etme �ste�i", 140, 60000, 3,
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
