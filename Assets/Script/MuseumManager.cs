using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class MuseumManager : MonoBehaviour
{
    public static MuseumManager instance { get; private set; }
    public List<PictureElementData> PictureDatabase = new List<PictureElementData>();
    public List<PictureElement> AllPictureElements = new List<PictureElement>();
    public List<PictureData> InventoryPictures = new List<PictureData>();
    public List<ItemData> PurchasedItems = new List<ItemData>();
    public List<NPCBehaviour> CurrentNpcs = new List<NPCBehaviour>();

    //workers
    [SerializeField] public List<WorkerBehaviour> WorkersInInventory = new List<WorkerBehaviour>();
    [SerializeField] public List<WorkerBehaviour> CurrentActiveWorkers = new List<WorkerBehaviour>();
    //workers

    
    //reward
    public System.DateTime lastDailyRewardTime;
    //reward
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
            if(UIController.instance.CultureFillBar != null)
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

    public void OnNpcPaid(float _score)
    {
        float targetGold = _score * GetTicketPrice();
        UIController.instance.GoldText.GetComponent<GoldStackHandler>().AddTempGold(targetGold);
        AddGold(targetGold);
        Debug.Log("Earned: " + targetGold + " by npc paid for an image. New gold: " + Gold + " _score => " + _score);
        UIController.instance.InMuseumCurrentNPCCountChanged(GetInMuseumVisitorCount());
        UIController.instance.DailyVisitorCountChanged(AddAndGetDailyNPCCount());
        UIController.instance.UIChangesControl();
        if (PicturesMenuController.instance.CurrentPicture != null)
        {
            Debug.Log("Picture Control: " + PicturesMenuController.instance.CurrentPicture.name);
            PicturesMenuController.instance.GoldControledButtonShape();
        }
        
        AudioManager.instance.PlayGoldPaidSound();
    }
    
    public void AddGold(float _gold)
    {
        Gold += _gold;
        UIController.instance.GoldText.text = "" + Gold;
        Debug.Log(" New gold: " + Gold);
        DailyEarning += Gold;
        UIController.instance.InMuseumDailyEarningChanged(DailyEarning);
//#if UNITY_EDITOR
//        FirestoreManager.instance.UpdateGameData("ahmet123");
//#else
//        FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUser().UserId);
//#endif
    }
    public void AddGem(float _gem)
    {
        Gem += _gem;
        UIController.instance.GemText.text = "" + Gem;
        Debug.Log("New Gem: " + Gem);
//#if UNITY_EDITOR
//        FirestoreManager.instance.UpdateGameData("ahmet123");
//#else
//        FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUser().UserId);
//#endif
    }
    public void AddSkillPoint(int _point) // TEHLIKELI KOD! TESTTEN SONRA SILINMELIDIR!
    {
        SkillPoint += _point;
    }
    public void SpendingSkillPoint(float _point)
    {
        if (SkillPoint - _point < 0)
        {
            Debug.LogWarning("Skill Point miktari 0'dan kucuk olamaz!");
            return;
        }
        SkillPoint -= _point;
        UIController.instance.SkillPointCountChanged(SkillPoint);
        Debug.Log("Spending Successful. New Point: " + SkillPoint);
//#if UNITY_EDITOR
//        FirestoreManager.instance.UpdateGameData("ahmet123");
//#else
//        FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUser().UserId);
//#endif
    }
    public void SpendingGold(float _gold)
    {
        if (Gold - _gold < 0)
        {
            Debug.LogWarning("Para miktari 0'dan kucuk olamaz!");
            return;
        }
        Gold -= _gold;
        UIController.instance.GoldText.text = "" + Gold;
        Debug.Log("Spending Successful. New gold: " + Gold);
//#if UNITY_EDITOR
//        FirestoreManager.instance.UpdateGameData("ahmet123");
//#else
//        FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUser().UserId);
//#endif
    }

    public void SpendingGem(float _gem)
    {
        if (Gem - _gem < 0)
        {
            Debug.LogWarning("Gem miktari 0'dan kucuk olamaz!");
            return;
        }
        Gem -= _gem;
        UIController.instance.GemText.text = "" + Gem;
        Debug.Log("Spending Successful. New gem: " + Gem);
//#if UNITY_EDITOR
//        FirestoreManager.instance.UpdateGameData("ahmet123");
//#else
//        FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUser().UserId);
//#endif
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
        else if (starCount < 1)
        {
            starCount = 1;
        }

        List<TableCommentEvaluationData> randomDatas = TableCommentEvaluationManager.instance.GetComment(starCount);
        TableCommentEvaluationData randomData = randomDatas[Random.Range(0, randomDatas.Count)];
        AddTotalVisitorCommentCount(1);
        CalculateTotalVisitorHappiness();
        string likedArtist = "";
        foreach (var artist in _npc.GetLikedArtists())
        {
            if (artist == _pictureElement._pictureData.painterData.Description)
            {
                likedArtist = artist.Split(" ")[0] + " Bu Ressama bayýlýyorum!";
                break;
            }
        }
        UIController.instance.AddCommentInGlobalTab(EmptyPictureSprite, _npc.name, $"{randomData.Message} {likedArtist}", System.DateTime.Now.Hour + ":" + System.DateTime.Now.Minute + ":" + System.DateTime.Now.Second);
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
            GameManager.instance.BaseWorkerHiringPrice += 200;
            SkillPoint++;
            CultureLevelUP();
        }
        UIController.instance.CultureLevelCountChanged(CurrentCultureLevel);
        UIController.instance.SkillPointCountChanged(SkillPoint);
        UIController.instance.UIChangesControl();
//#if UNITY_EDITOR
//        FirestoreManager.instance.UpdateGameData("ahmet123");
//#else
//        FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUser().UserId);
//#endif
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
            totalHappiness += npc.GetNpcHappiness();
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
        float capasity = SkillTreeManager.instance.CurrentBuffs[(int)eStat.VisitorCapacity] + MaxVisitorPerCultureLevel[CurrentCultureLevel];
        Debug.Log("GetMuseumCurrentCapacity => " + capasity);
        return capasity;
    }

    public float GetTicketPrice()
    {
        Debug.Log("Ticket price: " + (SkillTreeManager.instance.CurrentBuffs[(int)eStat.MuseumEnterPrice] + TicketPricePerCultureLevel[CurrentCultureLevel]));
        int result = SkillTreeManager.instance.CurrentBuffs[(int)eStat.MuseumEnterPrice] + TicketPricePerCultureLevel[CurrentCultureLevel];
        if (result <= 0)
            return 1;
        else
            return result;
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
        Debug.Log("Onemli! get itemid: " + _id);
        PictureElement[] pictureElements = FindObjectsOfType<PictureElement>();
        AllPictureElements = pictureElements.Where(x=> x._pictureData.isLocked == false).ToList();
        Debug.Log("AllPictureElements.count: " + AllPictureElements.Count);
        PictureElement pe = AllPictureElements.Find(x => x._pictureData.id == _id);
        Debug.Log(pe == null ? "Pe null" : "pe id: " + (pe._pictureData != null ? pe._pictureData.id.ToString() : "Picture data is null"));
        return pe;
    }

    private List<int> CultureLevel = new List<int>() 
    { 0, 100, 225, 450, 600, 800, 1050, 1350, 1700, 2100, 2650,
        3200, 3750, 4600, 5300, 6000, 999999
    };

    private List<int> MaxVisitorPerCultureLevel = new List<int>()
    {
        0, 5,  6,  7,  8,  9,  10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20
    };

    private List<int> TicketPricePerCultureLevel = new List<int>()
    {
        0, 5,  6,  7,  8,  9,  10, 12, 14, 16, 18, 20, 23, 26, 29, 32, 35
    };

    private List<int> RequiredSkillPointExp = new List<int>()
    {
        0, 100,  125,  150,  175,  200,  225, 250, 275, 300, 335, 370, 405, 440, 475, 510, 550
    };
}
