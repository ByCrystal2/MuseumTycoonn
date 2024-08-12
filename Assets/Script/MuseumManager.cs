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
            SpawnHandler.instance.StartSpawnProcess();
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
{
    0, 100, 225, 450, 600, 800, 1050, 1350, 1700, 2100, 2650,
    3200, 3750, 4600, 5300, 6000, 6700, 7400, 8100, 8800, 9500,
    10200, 10900, 11600, 12300, 13000, 13700, 14400, 15100, 15800,
    16500, 17200, 17900, 18600, 19300, 20000, 20700, 21400, 22100,
    22800, 23500, 24200, 24900, 25600, 26300, 27000, 27700, 28400,
    29100, 29800, 30500, 31200, 31900, 32600, 33300, 34000, 34700,
    35400, 36100, 36800, 37500, 38200, 38900, 39600, 40300, 41000,
    41700, 42400, 43100, 43800, 44500, 45200, 45900, 46600, 47300,
    48000, 48700, 49400, 50100, 50800, 51500, 52200, 52900, 53600,
    54300, 55000, 55700, 56400, 57100, 57800, 58500, 59200, 59900,
    60600, 61300, 62000, 62700, 63400, 64100, 64800
};

    private List<int> MaxVisitorPerCultureLevel = new List<int>()
{
    0, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
    21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35,
    36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50,
    51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65,
    66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80,
    81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95,
    96, 97, 98, 99, 100, 101, 102, 103
};

    private List<int> TicketPricePerCultureLevel = new List<int>()
{
    0, 5, 6, 7, 8, 9, 10, 12, 14, 16, 18, 20, 23, 26, 29, 32, 35,
    38, 41, 44, 47, 50, 53, 56, 59, 62, 65, 68, 71, 74, 77, 80,
    83, 86, 89, 92, 95, 98, 101, 104, 107, 110, 113, 116, 119,
    122, 125, 128, 131, 134, 137, 140, 143, 146, 149, 152, 155,
    158, 161, 164, 167, 170, 173, 176, 179, 182, 185, 188, 191,
    194, 197, 200, 203, 206, 209, 212, 215, 218, 221, 224, 227,
    230, 233, 236, 239, 242, 245, 248, 251, 254, 257, 260, 263,
    266, 269, 272, 275, 278, 281, 284
};

    private List<int> RequiredSkillPointExp = new List<int>()
{
    0, 100, 125, 150, 175, 200, 225, 250, 275, 300, 335, 370, 405,
    440, 475, 510, 550, 586, 622, 658, 695, 731, 767, 803, 840,
    876, 912, 948, 985, 1021, 1057, 1093, 1130, 1166, 1202, 1238,
    1275, 1311, 1347, 1383, 1420, 1456, 1492, 1528, 1565, 1601,
    1637, 1673, 1710, 1746, 1782, 1818, 1855, 1891, 1927, 1963,
    2000, 2036, 2072, 2108, 2145, 2181, 2217, 2253, 2290, 2326,
    2362, 2398, 2435, 2471, 2507, 2543, 2580, 2616, 2652, 2688,
    2725, 2761, 2797, 2833, 2870, 2906, 2942, 2978, 3015, 3051,
    3087, 3123, 3160, 3196, 3232, 3268, 3305, 3341, 3377, 3413,
    3450, 3486, 3522, 3558
};
}
