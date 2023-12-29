using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MuseumManager : MonoBehaviour
{
    public static MuseumManager instance { get; private set; }
    public List<PictureElementData> PictureDatabase = new List<PictureElementData>();
    public List<PictureElement> CurrentActivePictures = new List<PictureElement>();
    public List<PictureData> InventoryPictures = new List<PictureData>();
    public List<ItemData> PurchasedItems = new List<ItemData>();
    public List<NPCBehaviour> CurrentNpcs = new List<NPCBehaviour>();

    public Sprite EmptyPictureSprite;

    protected float Gold, Culture, Gem, SkillPoint;
    protected int CurrentCultureLevel;
    protected float SmootherCultureExp;
    public int CurrentNPCCountInMuseum; // Müzede'ki mevcut NPC sayýsý.
    public int DailyNPCCountInMuseum; // Müzede'ki günlük NPC sayýsý.
    public int TotalVisitorCommentCount; // Toplam ziyaretçi yorum sayýsý.
    public float TotalVisitorHappiness; // Toplam ziyaretçi mutluluðu.
    public float DailyEarning; // Günlük kazanç.

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
        CatchTheColorForAll();        
        CurrentCultureLevel = 1;
        //InventoryPictures.Add(new PictureElement() 
        //{ 
        //    data = new PictureData()
        //    {
        //        id = 1,
        //        MostCommonColors = CatchTheColors.instance.FindMostUsedColors(MyPictures[0].texture),
        //        texture = MyPictures[0].texture // NULL REFERANCE ALIYORUZ.
        //    },
            
        //    id = 1,
        //    isActive = true,
        //    isLocked = false,
        //    isFirst = true,
        //    RequiredGold = 300,
        //    painterData = new PainterData(1,"Leonardo Da Vinci", "1755'te resmedilen ünlü tablo.",1,CatchTheColors.instance.TextureToSprite(MyPictures[0].texture))    
            
        
        //});
    }

    public void AddNewItemToInventory(PictureData _newPicture)
    {
        InventoryPictures.Add(_newPicture);
        PicturesMenuController.instance.UpdatePicture();
    }

    public (float _gold, float _Culture, float _Gem, float _SkillPoint, int _CurrentCultureLevel) GetSaveData()
    {

        return (Gold, Culture, Gem, SkillPoint, CurrentCultureLevel);
    }

    public PictureElementData GetPictureElementData(int _id)
    {
        return PictureDatabase.Where(x => x.id == _id).SingleOrDefault();
    }

    public void SetSaveData(float _gold, float _Culture, float _Gem, float _SkillPoint, int _CurrentCultureLevel)
    {
        Gold = _gold;
        Culture = _Culture;
        Gem = _Gem;
        SkillPoint = _SkillPoint;
        CurrentCultureLevel = _CurrentCultureLevel;        
    }

    private void FixedUpdate()
    {
        if (SmootherCultureExp < Culture)
        {
            SmoothTheExp(true);
        }
        else if (SmootherCultureExp > Culture)
        {
            SmoothTheExp(false);
        }
    }

    void SmoothTheExp(bool _isIncrease)
    {
        if (_isIncrease)
        {
            SmootherCultureExp += Time.deltaTime * (0.05f * GetRequiredCultureExp());
            if (SmootherCultureExp > Culture)
                SmootherCultureExp = Culture;
        }
        else
        {
            SmootherCultureExp -= Time.deltaTime * (0.05f * GetRequiredCultureExp());
            if (SmootherCultureExp < Culture)
                SmootherCultureExp = Culture;
        }

        if(UIController.instance != null)
            UIController.instance.CultureFillBar.fillAmount = SmootherCultureExp / GetRequiredCultureExp();
    }

    void CatchTheColorForAll()
    {
        int length = PictureDatabase.Count;
        for (int i = 0; i < length; i++)
        {
            PictureDatabase[i].id = (i + 1);
            PictureDatabase[i].MostCommonColors = CatchTheColors.instance.FindMostUsedColors(PictureDatabase[i].texture);
        }
    }

    public void OnNpcEnteredMuseum(NPCBehaviour _newNpc)
    {
        if(!CurrentNpcs.Contains(_newNpc))
            CurrentNpcs.Add(_newNpc);
        
        //Kac adet npc var buraya guncellenicek.
    }

    public void OnNpcPaid()
    {
        Gold += GetTicketPrice();
        UIController.instance.GoldText.text = "" + Gold;
        Debug.Log("An npc entered Museum. New gold: " + Gold);
        DailyEarning = Gold;
        UIController.instance.InMuseumCurrentNPCCountChanged(GetInMuseumVisitorCount());
        UIController.instance.InMuseumDailyEarningChanged(DailyEarning);
        UIController.instance.DailyVisitorCountChanged(AddAndGetDailyNPCCount());
        UIController.instance.UIChangesControl();
        if (PicturesMenuController.instance.CurrentPicture != null)
        {
            Debug.Log("Picture Control: " + PicturesMenuController.instance.CurrentPicture.name);
            PicturesMenuController.instance.GoldControledButtonShape();
        }
        List<AudioSource> Sources = AudioManager.instance.GetSoundEffects(SoundEffectType.EarnGold).Select(x=> x.AudioSource).ToList();
        Sources[Random.Range(0, Sources.Count)].Play();
    }
    
    public void AddGold(float _gold)
    {
        Gold += _gold;
        UIController.instance.GoldText.text = "" + Gold;
        Debug.Log(" New gold: " + Gold);
        DailyEarning += Gold;
        UIController.instance.InMuseumDailyEarningChanged(DailyEarning);
    }
    public void AddGem(float _gem)
    {
        Gem += _gem;
        UIController.instance.GemText.text = "" + Gem;
        Debug.Log("New gold: " + Gem);
    }
    public void SpendingSkillPoint(float _point)
    {
        SkillPoint -= _point;
        UIController.instance.SkillPointCountChanged(SkillPoint);
        Debug.Log("Spending Successful. New Point: " + SkillPoint);
    }
    public void SpendingGold(float _gold)
    {
        Gold -= _gold;
        UIController.instance.GoldText.text = "" + Gold;
        Debug.Log("Spending Successful. New gold: " + Gold);
    }

    public void SpendingGem(float _gem)
    {
        Gem -= _gem;
        UIController.instance.GemText.text = "" + Gem;
        Debug.Log("Spending Successful. New gem: " + Gem);
    }
    public void OnNpcExitedMuseum(NPCBehaviour _oldNpc)
    {
        if (CurrentNpcs.Contains(_oldNpc))
            CurrentNpcs.Remove(_oldNpc);
        UIController.instance.InMuseumCurrentNPCCountChanged(GetInMuseumVisitorCount());
        CalculateTotalVisitorHappiness();
        Debug.Log("An npc left Museum.");
        
    }
    public void OnNpcCommentedPicture(NPCBehaviour _npc, PictureElement _pictureElement, float starCount)
    {
        if (starCount > 5)
        {
            starCount = 5;
        }
        else if(starCount < 1)
        {
            starCount = 1;
        }
            
        List<TableCommentEvaluationData> randomDatas = TableCommentEvaluationManager.instance.GetComment(starCount);
        TableCommentEvaluationData randomData = randomDatas[Random.Range(0, randomDatas.Count)];
        AddTotalVisitorCommentCount(1);
        CalculateTotalVisitorHappiness();
        UIController.instance.AddCommentInGlobalTab(EmptyPictureSprite, _npc.name, randomData.Message, System.DateTime.Now.Hour + ":" + System.DateTime.Now.Minute + ":" + System.DateTime.Now.Second);
    }
    public void AddTotalVisitorCommentCount(int count)
    {
        TotalVisitorCommentCount += count;
        UIController.instance.TotalVisitorsCommentCountChanged(TotalVisitorCommentCount);
    }
    public bool IsMuseumFull()
    {
        return (CurrentNpcs.Count == GetMuseumCurrentCapacity());
    }

    public void AddCultureExp(float _xp)
    {
        int newXP = (int)((float)_xp + (float)_xp * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.CultureExp] / 100f));
        Culture += newXP;
        if (Culture > GetRequiredCultureExp())
        {
            Culture -= GetRequiredCultureExp();
            CurrentCultureLevel++;
            SkillPoint++;
            CultureLevelUP();
        }
        UIController.instance.CultureLevelCountChanged(CurrentCultureLevel);
        UIController.instance.SkillPointCountChanged(SkillPoint);
        UIController.instance.UIChangesControl();      
    }

    public void AddPictureTable(PictureElement PE)
    {
        InventoryPictures.Add(PE._pictureData);
    }

    public int GetInMuseumVisitorCount()
    {
        return CurrentNpcs.Count;        
    }

    public float GetCurrentGold()
    {
        return Gold;
    }
    public float GetCurrentGem()
    {
        return Gem;
    }
    public int GetCurrentCultureLevel()
    {
        return CurrentCultureLevel;
    }
    public float GetCurrentSkillPoint()
    {
        return SkillPoint;
    }
    public void CultureLevelUP()
    {
        UIController.instance.CultureLevelText.text = "" + CurrentCultureLevel;        
        Debug.Log("Yeni levelde kazanilan bonuslari buraya islicez.");
    }
    public int AddAndGetDailyNPCCount()
    {
       return DailyNPCCountInMuseum++;
    }
    public void CalculateTotalVisitorHappiness()
    {
        float totalHappiness = 0;

        foreach (NPCBehaviour npc in CurrentNpcs)
        {
            totalHappiness += npc.Happiness;
        }
        
        if (CurrentNpcs.Count > 0)
        {
            TotalVisitorHappiness = totalHappiness / CurrentNpcs.Count;
        }
        else
        {
            TotalVisitorHappiness = 0; // Eðer hiç NPC yoksa.
        }
        
        UIController.instance.CurrentTotalHappinessChanged(Mathf.Round(GetFinalVisitorHappiness()));
      
    }
    public void CalculateAndAddTextAllInfos()
    {
        UIController.instance.SkillPointCountChanged(SkillPoint);
        UIController.instance.InMuseumCurrentNPCCountChanged(CurrentNPCCountInMuseum);
        UIController.instance.DailyVisitorCountChanged(DailyNPCCountInMuseum);
        UIController.instance.CultureLevelCountChanged(CurrentCultureLevel);
        UIController.instance.InMuseumDailyEarningChanged(DailyEarning);
        UIController.instance.TotalVisitorsCommentCountChanged(TotalVisitorCommentCount);
        UIController.instance.CurrentTotalHappinessChanged(TotalVisitorHappiness);

    }
    public float GetFinalVisitorHappiness()
    {
        float value = TotalVisitorHappiness + SkillTreeManager.instance.CurrentBuffs[(int)eStat.BaseHappiness];
        if (value <= 100)
        {
             return value;
        }
        else
        {
            return 100;
        }
    }


    public int GetRequiredCultureExp()
    {
        return CultureLevel[CurrentCultureLevel];
    }

    public float GetMuseumCurrentCapacity()
    {
        return SkillTreeManager.instance.CurrentBuffs[(int)eStat.VisitorCapacity] + MaxVisitorPerCultureLevel[CurrentCultureLevel];
    }

    public float GetTicketPrice()
    {
        return SkillTreeManager.instance.CurrentBuffs[(int)eStat.MuseumEnterPrice] + TicketPricePerCultureLevel[CurrentCultureLevel];
    }

    public int GetRequiredSkillPointExp()
    {
        return RequiredSkillPointExp[CurrentCultureLevel];
    }

    public List<Texture2D> GetPicturesTexture()
    {
        return PictureDatabase.Select(x=> x.texture).ToList();
    }
    public PictureElement GetPictureElement(int _id)
    {
        Debug.Log("get itemid: " + _id);
        PictureElement[] pictureElements = FindObjectsOfType<PictureElement>();
        CurrentActivePictures = pictureElements.Where(x=> x._pictureData.isLocked == false).ToList();
        return CurrentActivePictures.Find(x => x._pictureData.id == _id);
    }

    public List<int> CultureLevel = new List<int>() 
    { 0, 100, 225, 450, 600, 800, 1050, 1350, 1700, 2100, 2650,
        3200, 3750, 4600, 5300, 6000, 999999
    };

    public List<int> MaxVisitorPerCultureLevel = new List<int>()
    {
        0, 5,  6,  7,  8,  9,  10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20
    };

    public List<int> TicketPricePerCultureLevel = new List<int>()
    {
        0, 300,  305,  310,  315,  320,  325, 330, 335, 340, 345, 350, 358, 366, 374, 382, 390
    };

    public List<int> RequiredSkillPointExp = new List<int>()
    {
        0, 100,  150,  200,  250,  300,  350, 400, 450, 500, 550, 600, 650, 700, 750, 800, 850
    };
}
