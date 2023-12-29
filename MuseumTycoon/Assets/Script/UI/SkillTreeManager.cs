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
        SkillNode sn1 = new SkillNode(0, "Ziyaret�i Rehberi", "Oyuncuyu bilgilendirecek rehber.", "+5 Mutluluk", 0, 0, DefaultMaxSkillLevel,
            new List<eStat> { eStat.BaseHappiness }, new List<int> { 1,2,3,4,5,6,7,8,9,10 });
        SkillNode sn2 = new SkillNode(1, "Reklam Kampanyalar�", "M�zenizin tan�t�m�n� yapmak i�in reklam kampanyalar� olu�turun.", "+5 Ziyaret�i Kapasitesi", 0, 0, DefaultMaxSkillLevel,
            new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 });
        SkillNode sn3 = new SkillNode(2, "Test", "M�zenizin tan�t�m�n�", "+5 Ziyaret�i Kapasitesi", 2, 3000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn4 = new SkillNode(3, "Sanat Okulu ��birli�i", "Yerel sanat okullar�yla i�birli�i yaparak ��rencileri m�zenize �ekin.", "+5 Mutluluk, +5 Ziyaret�i Kapasitesi", 10, 5000,
            DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn5 = new SkillNode(4, "�zel Sergiler", "�zel sergiler d�zenleyerek daha fazla ziyaret�i �ekin.", "+10 Ziyaret�i Kapasitesi", 20, 10000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 }
        );
        SkillNode sn6 = new SkillNode(5, "Etkile�imli E�itim", "Ziyaret�ilere interaktif e�itim deneyimleri sunarak k�lt�r kazan�lar�n� art�r�n.", "+10 K�lt�r Exp Kazan�m�", 30, 15000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn7 = new SkillNode(6, "Sanat�� Konuklar", "�nl� sanat��lar� m�zenize davet edin ve ziyaret�i �ekmeyi art�r�n.", "+15 Ziyaret�i Kapasitesi, +5 Mutluluk", 40, 20000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn8 = new SkillNode(7, "M�ze Restorasyonu", "M�zenizin fiziksel durumunu iyile�tirerek ziyaret�i memnuniyetini art�r�n.", "+10 Mutluluk, +10 Ziyaret�i Kapasitesi", 50, 25000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn9 = new SkillNode(8, "Gece A��l��lar", "Gece a��l��lar� d�zenleyerek daha fazla ziyaret�iye ula��n.", "+15 Ziyaret�i Kapasitesi", 60, 30000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 }
        );
        SkillNode sn10 = new SkillNode(9, "VIP Rehberler", "VIP rehberlerle i�birli�i yaparak premium turlar sunun.", "+10 Mutluluk, +15 Ziyaret�i Kapasitesi", 70, 35000, DefaultMaxSkillLevel,
            new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn11 = new SkillNode(10, "Sanat At�lyeleri", "Sanat at�lyeleri d�zenleyerek ��retici deneyimler sunun.", "+15 K�lt�r Exp Kazan�m�", 80, 40000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 }
        );
        SkillNode sn12 = new SkillNode(11, "Uzay Sergisi", "Uzay temal� sergiler d�zenleyerek daha geni� bir kitleye ula��n.", "+20 Ziyaret�i Kapasitesi, +5 Mutluluk", 90, 45000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn13 = new SkillNode(12, "K�lt�r Festivali", "K�lt�r festivalleri d�zenleyerek m�zenizin pop�laritesini art�r�n.", "+20 Ziyaret�i Kapasitesi", 100, 50000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 }
        );
        SkillNode sn14 = new SkillNode(13, "VR Sanat Deneyimi", "Sanal ger�eklik sanat deneyimleri sunarak teknolojik olarak �ne ��k�n.", "+20 K�lt�r Exp Kazan�m�", 110, 55000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn15 = new SkillNode(14, "M�ze Uzant�s�", "M�zenizin alan�n� geni�leterek daha fazla eser sergileyin.", "+15 Ziyaret�i Kapasitesi, +10 Mutluluk", 120, 60000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 }
        );
        SkillNode sn16 = new SkillNode(15, "Arkeoloji Kaz�lar�", "Arkeoloji kaz�lar�na kat�larak benzersiz eserler bulun.", "+25 K�lt�r Exp Kazan�m�", 130, 65000, DefaultMaxSkillLevel,
            new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn17 = new SkillNode(16, "Sanat�� ��birli�i", "�nl� sanat��larla i�birli�i yaparak �zel koleksiyonlar olu�turun.", "+30 Ziyaret�i Kapasitesi, +10 Mutluluk", 140, 70000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn18 = new SkillNode(17, "Dijital Sanat G�sterileri", "Dijital sanat g�sterileriyle modern sanat deneyimleri sunun.", "+25 K�lt�r Exp Kazan�m�", 150, 75000, DefaultMaxSkillLevel,
            new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn19 = new SkillNode(18, "K�lt�rel Etkinlikler", "K�lt�rel etkinlikler d�zenleyerek m�zenizi canland�r�n.", "+40 Ziyaret�i Kapasitesi, +15 Mutluluk", 160, 80000, DefaultMaxSkillLevel, new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });
        SkillNode sn20 = new SkillNode(19, "M�ze Hava K�r�", "M�zenizi her t�rl� hava ko�ulunda a��k tutun.", "+50 K�lt�r Exp Kazan�m�", 170, 85000, DefaultMaxSkillLevel,
            new List<eStat> { eStat.VisitorCapacity }, new List<int> { 5 });

        // T�m yetenekleri ekleyin
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
