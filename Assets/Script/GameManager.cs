using Firebase.Extensions;
using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    public List<AudioSource> allAudioSources = new List<AudioSource>();
    private bool uiControl;
    public bool UIControl { get { return uiControl; } set { uiControl = value; } }

    public PlayerSaveData CurrentSaveData;

    string _GameSave = "GameSave";
    public string GameLanguage = "Turkish";
    float AutoSaveTimer;
    GameMode CurrentGameMode;
    public RewardManager rewardManager;
    public int PictureChangeRequiredAmount = 250;
    public float ActiveRoomsRequiredMoney;
    public float BaseWorkerHiringPrice;
    public bool IsFirstGame = true, IsWatchTutorial;
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
        AutoSaveTimer = Time.time + 300;
    }

    public void Start()
    {
        StartCoroutine(WaitForInstance());
    }
    public IEnumerator WaitForInstance()
    {
        while (TimeManager.instance == null)
        {
            yield return null;
        }
        TimeManager.instance.InvokeRepeating(nameof(TimeManager.instance.UpdateCurrentTime), 1, 1f);
        DateTime dateControl = new DateTime(0001,01,01,01,01,01);
        //Debug.Log("TimeManager.instance.CurrentDateTime <= dateControl => " + TimeManager.instance.CurrentDateTime + " | " + dateControl);
        while (TimeManager.instance.CurrentDateTime.Second <= dateControl.Second)
        {
            yield return null;
        }
        Init();
        //Load();
        System.Threading.Tasks.Task task1 = LoadIsFirstGame();
        yield return new WaitUntil(() => task1.IsCompleted);
        if (IsFirstGame)
            ItemManager.instance.SetCalculatedDailyRewardItems();
        LoadIsWatchTutorial();
        LoadDailyRewardItems();
        LoadGooglePlayAchievements();
        LoadLastDailyRewardTime();
        LoadMuseumNumeralDatas();
        LanguageControlInDatabase();
        LoadInventoryPictures();
        LoadPurchasedItems();

        Debug.Log("CurrentSaveData.ActiveRoomsRequiredMoney => " + CurrentSaveData.ActiveRoomsRequiredMoney);
    }
    public void Init()
    {
        FirebaseAuthManager.instance.CreateNewLoading();
        TableCommentEvaluationManager.instance.AddAllNPCComments();
        SkillTreeManager.instance.AddSkillsForSkillTree();
        ItemManager.instance.AddItems();
        AudioManager.instance.AllAudioSourcesOptions();    
    }

    private void FixedUpdate()
    {
        if (AutoSaveTimer < Time.time)
        {
            Save();
            AutoSaveTimer = Time.time + 300;
        }

    }
    public async void LanguageControlInDatabase()
    {
        string databaseLanguage = "";

#if UNITY_EDITOR
        await FirestoreManager.instance.GetGameDataInDatabase("ahmet123").ContinueWithOnMainThread(getTask =>
        {
            if (getTask.IsCompleted)
            {
                Dictionary<string, object> gameDatas = getTask.Result;
                databaseLanguage = gameDatas["GameLanguage"].ToString();
            }
        });
#else
        await FirestoreManager.instance.GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUser().UserId).ContinueWithOnMainThread(getTask =>
                {
                    if (getTask.IsCompleted)
                    {
                        Dictionary<string, object> gameDatas = getTask.Result;
                        databaseLanguage = gameDatas["GameLanguage"].ToString();
                    }
                });
#endif

        if (databaseLanguage == "" || databaseLanguage == null || databaseLanguage == string.Empty)
        {
            GameLanguage = "English";
            LocalizationManager.CurrentLanguage = "English";
        }
        else
        {
            GameLanguage = databaseLanguage;
            LocalizationManager.CurrentLanguage = databaseLanguage;
        }
    }
    public string GetGameLanguage()
    {
        return GameLanguage;
    }
    public async void SetGameLanguage(string _language)
    {
        GameLanguage = _language;
        LocalizationManager.CurrentLanguage = _language;
#if UNITY_EDITOR
        FirestoreManager.instance.UpdateGameLanguageInGameDatas("ahmet123");
#else
                    FirestoreManager.instance.UpdateGameLanguageInGameDatas(FirebaseAuthManager.instance.GetCurrentUser().UserId);
#endif
    }
    public GameMode GetCurrentGameMode()
    {
        return CurrentGameMode;
    }
    public void SetCurrenGameMode(GameMode _gameMode)
    {
        CurrentGameMode = _gameMode;
    }
    public void Save()
    {
        //if (CurrentSaveData.SaveName == "")
        //    CurrentSaveData.SaveName = _GameSave;

        //CurrentSaveData.GameLanguage = GameLanguage;

        //var a = MuseumManager.instance.GetSaveData();
        //CurrentSaveData.Gold = a._gold;
        //CurrentSaveData.Culture = a._Culture;
        //CurrentSaveData.Gem = a._Gem;
        //CurrentSaveData.SkillPoint = a._SkillPoint;
        //CurrentSaveData.CurrentCultureLevel = a._CurrentCultureLevel;

        //if (GameManager.instance != null)
        //{
        //    CurrentSaveData.IsFirstGame = GameManager.instance.IsFirstGame;
        //}

        //if (GameManager.instance != null)
        //{
        //    CurrentSaveData.IsWatchTutorial = GameManager.instance.IsWatchTutorial;
        //}

        ////if (rewardManager != null)
        ////{
        ////    CurrentSaveData.LastDailyRewardTime =  rewardManager.lastDailyRewardTime.ToString("yyyy-MM-dd HH:mm:ss");
        ////    CurrentSaveData.WhatDay = TimeManager.instance.WhatDay;
        ////}
        

        //Debug.Log("CurrentSaveData.LastDailyRewardTime => " + CurrentSaveData.LastDailyRewardTime);
        //Debug.Log("CurrentSaveData.WhatDay => " + CurrentSaveData.WhatDay);
        //List<RoomData> currentRooms = new List<RoomData>();
        //if (RoomManager.instance != null)
        //{
        //    List<RoomData> AllActiveRooms = RoomManager.instance.RoomDatas.Where(x=> (x.isActive && !x.isLock) || x.isActive).ToList();
        //    foreach (var room in AllActiveRooms)
        //        currentRooms.Add(room);
        //    Debug.Log("currentRooms.count: " + currentRooms.Count);
        //}

        //List<PictureData> currentActivePictures = new List<PictureData>();
        //foreach (var item in MuseumManager.instance.AllPictureElements)
        //    currentActivePictures.Add(item._pictureData);
        //Debug.Log("currentActivePictures.count: " + currentActivePictures.Count);

        //List<PictureData> inventoryPictures = new List<PictureData>();
        //foreach (var item in MuseumManager.instance.InventoryPictures)
        //    inventoryPictures.Add(item);
        //Debug.Log("inventoryPictures.count: " + inventoryPictures.Count);

        //List<ItemData> inventoryItems = new List<ItemData>();
        //foreach (var item in MuseumManager.instance.PurchasedItems)
        //{
        //    inventoryItems.Add(item);
        //}
        //Debug.Log("inventoryItems.count: " + inventoryItems.Count);

        //List<ItemData> dailyRewardItems = new List<ItemData>();
        //foreach (var item in ItemManager.instance.CurrentDailyRewardItems)
        //{
        //    dailyRewardItems.Add(item);
        //}
        //Debug.Log("dailyRewardItems.count: " + dailyRewardItems.Count);

        //List<SkillNode> skillNodes = new List<SkillNode>();
        //foreach (var skill in SkillTreeManager.instance.skillNodes)
        //{
        //    skillNodes.Add(skill);
        //}

        //List<EditObjData> statueDatas = new List<EditObjData>();
        //if (RoomManager.instance != null)
        //{
        //    foreach (var statueData in RoomManager.instance.statuesHandler.activeEditObjs)
        //    {
        //        statueDatas.Add(statueData);
        //    }
        //}
        //List<WorkerData> currentWorkerDatas = new List<WorkerData>();
        //List<WorkerData> inventoryWorkerDatas = new List<WorkerData>();
        //Debug.Log("skillNodes.count: " + skillNodes.Count);
        //if (WorkerManager.instance != null)
        //{
            
        //    foreach (var currentWorker in MuseumManager.instance.CurrentActiveWorkers)
        //    {
        //        currentWorkerDatas.Add(currentWorker.MyDatas);
        //    }
        //    Debug.Log("currentWorkerDatas.count: " + currentWorkerDatas.Count);
            
        //    //foreach (var inventoryWorker in WorkerManager.instance.GetWorkersInInventory())
        //    //{
        //    //    inventoryWorkerDatas.Add(inventoryWorker.MyDatas);
        //    //}
        //    //Debug.Log("inventoryWorkerDatas.count: " + inventoryWorkerDatas.Count);
        //}

        //HashSet<string> savedRoomCells = new HashSet<string>(CurrentSaveData.Rooms.Select(r => r.availableRoomCell));
        //foreach (var room in currentRooms)
        //{
        //    string roomCell = room.availableRoomCell.CellLetter.ToString() + room.availableRoomCell.CellNumber.ToString();
        //    if (!savedRoomCells.Contains(roomCell))//
        //    {
        //        RoomSaveData newSaveData = new RoomSaveData();
        //        newSaveData.availableRoomCell = roomCell;
        //        newSaveData.isLock = room.isLock;
        //        newSaveData.isActive = room.isActive;
        //        newSaveData.RequiredMoney = room.RequiredMoney;
        //        newSaveData.IsHasStatue = room.isHasStatue;
        //        newSaveData.MyStatue = room.GetMyStatueInTheMyRoom();

        //        newSaveData.MyRoomWorkersIDs.Clear();
        //        int length = room.MyRoomWorkersIDs.Count;
        //        for (int i = 0; i < length; i++)
        //        {
        //            newSaveData.MyRoomWorkersIDs.Add(room.MyRoomWorkersIDs[i]);
        //        }
        //        CurrentSaveData.Rooms.Add(newSaveData);
        //    }
        //    else
        //    {
        //        RoomSaveData currentSavedRoom = CurrentSaveData.Rooms.Where(x=> x.availableRoomCell == roomCell).SingleOrDefault();
        //        currentSavedRoom.isLock = room.isLock;
        //        currentSavedRoom.isActive = room.isActive;
        //        currentSavedRoom.RequiredMoney = room.RequiredMoney;
        //        currentSavedRoom.IsHasStatue = room.isHasStatue;
        //        currentSavedRoom.MyStatue = room.GetMyStatueInTheMyRoom();

        //        currentSavedRoom.MyRoomWorkersIDs.Clear();
        //        int length = room.MyRoomWorkersIDs.Count;
        //        for (int i = 0; i < length; i++)
        //        {
        //            currentSavedRoom.MyRoomWorkersIDs.Add(room.MyRoomWorkersIDs[i]);
        //        }
        //    }
        //}

        //CurrentSaveData.CurrentPictures = currentActivePictures;
        ////CurrentSaveData.InventoryPictures = inventoryPictures;
        //CurrentSaveData.PurchasedItems = inventoryItems;
        //CurrentSaveData.DailyRewardItems = dailyRewardItems;
        //CurrentSaveData.SkillNodes = skillNodes;
        //CurrentSaveData.CurrentWorkerDatas = currentWorkerDatas;
        //CurrentSaveData.InventoryWorkerDatas = inventoryWorkerDatas;
        //CurrentSaveData.StatueDatas = statueDatas;

        //if(GoogleAdsManager.instance != null)
        //    CurrentSaveData.adData = GoogleAdsManager.instance.adsData;

        ////if (RoomManager.instance != null)
        //    //CurrentSaveData.ActiveRoomsRequiredMoney = RoomManager.instance.activeRoomsRequiredMoney;

        ////if (WorkerManager.instance != null)
        ////    CurrentSaveData.baseWorkerHiringPrice = WorkerManager.instance.BaseWorkerHiringPrice;

        //string jsonString = JsonUtility.ToJson(CurrentSaveData);
        //File.WriteAllText(Application.persistentDataPath + "/" + CurrentSaveData.SaveName + ".json", jsonString); // this will write the json to the specified path

        //Debug.Log("Game Save Location: " + Application.persistentDataPath + "/" + CurrentSaveData.SaveName + ".json");
    }

    public async System.Threading.Tasks.Task LoadRooms()
    {
        if (!IsFirstGame)
        {
            Dictionary<string, object> gameDatas = new Dictionary<string, object>();                

#if UNITY_EDITOR
            gameDatas = await FirestoreManager.instance.GetGameDataInDatabase("ahmet123");
#else
            gameDatas = await FirestoreManager.instance.GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUser().UserId);
#endif
            float activeRoomsRequiredMoney = Convert.ToSingle(gameDatas["ActiveRoomsRequiredMoney"]);
            ActiveRoomsRequiredMoney = activeRoomsRequiredMoney; 
        }
        

        //MuseumManager.instance.AllPictureElements = FindObjectsOfType<PictureElement>().ToList();
        List<RoomData> AllRooms = GameObject.FindObjectsOfType<RoomData>().ToList();
        //RoomLoad
        foreach (var room in AllRooms)
        {
              room.LoadThisRoom();
        }
        //MuseumManager.instance.InventoryPictures = CurrentSaveData.InventoryPictures;

        //int adet = 0;
        //int length = RoomManager.instance.transform.childCount;
        //int length2 = CurrentSaveData.Rooms.Count;
        //for (int x = 0; x < length; x++) //Odalarin listesi... 20...
        //{
        //    if (RoomManager.instance.transform.GetChild(x).TryGetComponent(out RoomData currentRoom))
        //    {
        //        for (int y = 0; y < length2; y++) //Save datalarimizin listesi... 20
        //        {
        //            RoomSaveData currentRoomSave = CurrentSaveData.Rooms[y];
        //            string currentID = currentRoom.availableRoomCell.CellLetter.ToString() + currentRoom.availableRoomCell.CellNumber.ToString();
        //            Debug.Log("Current checkID: " + currentID);
        //            Debug.Log("currentRoomSave.availableRoomCell: " + currentRoomSave.availableRoomCell);
        //            if (currentID == currentRoomSave.availableRoomCell)
        //            {
        //                currentRoom.isActive = currentRoomSave.isActive;
        //                currentRoom.isLock = currentRoomSave.isLock;
        //                currentRoom.RequiredMoney = currentRoomSave.RequiredMoney;
                        
        //                Debug.Log("kac adet oda bulundu: " + adet);
        //                adet++;
        //                break;
        //            }
        //        }
        //    }
        //}
    }

    public void Load()
    {
        if (CurrentSaveData.SaveName == "")
            CurrentSaveData.SaveName = _GameSave;   
        

        if (File.Exists(Application.persistentDataPath + "/" + CurrentSaveData.SaveName + ".json"))
        {
            string jsonString = File.ReadAllText(Application.persistentDataPath + "/" + CurrentSaveData.SaveName + ".json"); // read the json file from the file system
            CurrentSaveData = JsonUtility.FromJson<PlayerSaveData>(jsonString); // de-serialize the data to your myData object           
            LoadGameLanguage();

        }
        else
        {
            Debug.Log("Save dosyasi yok..");
            // su an saat 08:00  => lastDailyRewardTime = 08:00 
            // => AfterDailyRewardTime => lastDailyRewardTime + dailyRewardInterval
            // => WhichDay++ => if dailyRewardInterval <= (lastDailyRewardTime + dailyRewardInterval) - CurrentTime;
            //08:00                //24               09:00
            CurrentSaveData.ActiveRoomsRequiredMoney = 1000;                     
            Save();
        }
        PictureChangesReqiuredAmountCalculater();
        CurrentSaveData.baseWorkerHiringPrice = 500;
    }
    public async void LoadGooglePlayAchievements()
    {
        Dictionary<string, object> gameDatas = new Dictionary<string, object>();
#if UNITY_EDITOR
        gameDatas = await FirestoreManager.instance.GetGameDataInDatabase("ahmet123");
#else
            gameDatas = await FirestoreManager.instance.GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUser().UserId);
#endif
        int purchasedRoomCount = gameDatas.ContainsKey("PurchasedRoomCount") ? Convert.ToInt32(gameDatas["PurchasedRoomCount"]) : 0;
        int numberOfTablesPlaced = gameDatas.ContainsKey("NumberOfTablesPlaced") ? Convert.ToInt32(gameDatas["NumberOfTablesPlaced"]) : 0;
        int numberOfVisitors = gameDatas.ContainsKey("NumberOfVisitors") ? Convert.ToInt32(gameDatas["NumberOfVisitors"]) : 0;
        int numberOfStatuesPlaced = gameDatas.ContainsKey("NumberOfStatuesPlaced") ? Convert.ToInt32(gameDatas["NumberOfStatuesPlaced"]) : 0;
        int totalNumberOfMuseumVisitors = gameDatas.ContainsKey("TotalNumberOfMuseumVisitors") ? Convert.ToInt32(gameDatas["TotalNumberOfMuseumVisitors"]) : 0;
        int totalWorkerHiringCount = gameDatas.ContainsKey("TotalWorkerHiringCount") ? Convert.ToInt32(gameDatas["TotalWorkerHiringCount"]) : 0;
        int totalWorkerAssignCount = gameDatas.ContainsKey("TotalWorkerAssignCount") ? Convert.ToInt32(gameDatas["TotalWorkerAssignCount"]) : 0;

        GPGamesManager.instance.achievementController.SetDatas(purchasedRoomCount, numberOfTablesPlaced, numberOfVisitors, numberOfStatuesPlaced, totalNumberOfMuseumVisitors,totalWorkerHiringCount, totalWorkerAssignCount);
    }
    public void PictureChangesReqiuredAmountCalculater()
    {        
        int museumLevel = MuseumManager.instance.GetCurrentCultureLevel();

        if (museumLevel <= 5)
            PictureChangeRequiredAmount = 0;
        else
            PictureChangeRequiredAmount = 250;
    }
    public void LoadGameLanguage()
    {
        GameLanguage = CurrentSaveData.GameLanguage;
    }
    public async System.Threading.Tasks.Task LoadRemoveAds()
    {
        Dictionary<string, object> gameDatas = new Dictionary<string, object>();
#if UNITY_EDITOR
        gameDatas = await FirestoreManager.instance.GetGameDataInDatabase("ahmet123");
#else
            gameDatas = await FirestoreManager.instance.GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUser().UserId);
#endif
        AdverstingData databaseAdversting = new AdverstingData();
        bool removeAds = gameDatas.ContainsKey("RemoveAllAds") ? Convert.ToBoolean(gameDatas["RemoveAllAds"]) : false;
        databaseAdversting.RemovedAllAds = removeAds;
        if (GoogleAdsManager.instance != null)
            GoogleAdsManager.instance.adsData = databaseAdversting;        
    }
    public async void LoadMuseumNumeralDatas()
    {
        Dictionary<string, object> gameDatas = new Dictionary<string, object>();
#if UNITY_EDITOR
        gameDatas = await FirestoreManager.instance.GetGameDataInDatabase("ahmet123");
#else
            gameDatas = await FirestoreManager.instance.GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUser().UserId);
#endif
        float _gold, _culture, _gem, _skillPoint;
        int _currentCultureLevel;
        _gold = Convert.ToSingle(gameDatas["Gold"]);
        _culture = Convert.ToSingle(gameDatas["Culture"]);
        _gem = Convert.ToSingle(gameDatas["Gem"]);
        _skillPoint = Convert.ToSingle(gameDatas["SkillPoint"]);
        _currentCultureLevel = Convert.ToInt32(gameDatas["CurrentCultureLevel"]);
        Debug.Log($"GameData in database values: DataCount:{gameDatas.Count} Gold:{_gold} - Culture:{_culture} - Gem:{_gem} - SkillPoint:{_skillPoint} - Culture Level:{_currentCultureLevel}");
        MuseumManager.instance.SetSaveData(_gold, _culture, _gem, _skillPoint, _currentCultureLevel);
    }
    public async System.Threading.Tasks.Task LoadIsFirstGame()
    {
        bool firstGameResult = false;

#if UNITY_EDITOR
        await FirestoreManager.instance.GetGameDataInDatabase("ahmet123").ContinueWithOnMainThread(getTask =>
        {
            if (getTask.IsCompleted)
            {
                Dictionary<string, object> gameDatas = getTask.Result;
                firstGameResult = gameDatas.ContainsKey("IsFirstGame") ? Convert.ToBoolean(gameDatas["IsFirstGame"]) : false;
            }
        });
#else
            await FirestoreManager.instance.GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUser().UserId).ContinueWithOnMainThread(getTask =>
        {
            if (getTask.IsCompleted)
            {
                Dictionary<string, object> gameDatas = getTask.Result;
                firstGameResult = gameDatas.ContainsKey("IsFirstGame") ? Convert.ToBoolean(gameDatas["IsFirstGame"]) : false;
            }
        });
#endif
        Debug.Log("Is First Game ? = " + firstGameResult);
        IsFirstGame = firstGameResult;
    }
    public async void LoadIsWatchTutorial()
    {
        bool watchTutorialResult = false;
#if UNITY_EDITOR
        await FirestoreManager.instance.GetGameDataInDatabase("ahmet123").ContinueWithOnMainThread(getTask =>
        {
            if (getTask.IsCompleted)
            {
                Dictionary<string, object> gameDatas = getTask.Result;
                watchTutorialResult = gameDatas.ContainsKey("IsWatchTutorial") ? Convert.ToBoolean(gameDatas["IsWatchTutorial"]) : false;
            }
        });
#else
            await FirestoreManager.instance.GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUser().UserId).ContinueWithOnMainThread(getTask =>
        {
            if (getTask.IsCompleted)
            {
                Dictionary<string, object> gameDatas = getTask.Result;
                watchTutorialResult = gameDatas.ContainsKey("IsWatchTutorial") ? Convert.ToBoolean(gameDatas["IsWatchTutorial"]) : false;
            }
        });
#endif

        Debug.Log("Is Watch Tutorial ? = " + watchTutorialResult);
        IsWatchTutorial = watchTutorialResult;
    }
    public void LoadInventoryPictures()
    {
        List<PictureData> pictureDatas = new List<PictureData>();
#if UNITY_EDITOR
        FirestoreManager.instance.pictureDatasHandler.GetAllPictureInDatabase("ahmet123").ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                List<PictureData> databasePictures = task.Result;
                foreach (PictureData picture in databasePictures)
                {
                    if (!picture.isActive)
                    {
                        pictureDatas.Add(picture);
                    }
                }
            }
            else
            {
                Debug.LogError("Hata olustu: " + task.Exception);
            }
        });
#else
        FirestoreManager.instance.pictureDatasHandler.GetAllPictureInDatabase(FirebaseAuthManager.instance.GetCurrentUser().UserId).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                List<PictureData> databasePictures = task.Result;
                foreach (PictureData picture in databasePictures)
                {
                    if (!picture.isActive)
                    {
                        pictureDatas.Add(picture);
                    }
                }
            }
            else
            {
                Debug.LogError("Hata olustu: " + task.Exception);
            }
        });
#endif
        MuseumManager.instance.InventoryPictures = pictureDatas;
    }
    public async void LoadPurchasedItems()
    {
        List<ItemData> purchasedItems = new List<ItemData>();
        Dictionary<string, object> gameDatas = new Dictionary<string, object>();
#if UNITY_EDITOR
        gameDatas = await FirestoreManager.instance.GetGameDataInDatabase("ahmet123");
#else
        gameDatas = await FirestoreManager.instance.GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUser().UserId);
#endif
        List<int> itemIDs = ((List<object>)gameDatas["PurchasedItemIDs"]).Select(x => Convert.ToInt32(x)).ToList();
        foreach (int id in itemIDs)
        {
            ItemData databaseItem = ItemManager.instance.GetItemDataWithID(id);
            purchasedItems.Add(databaseItem);
        }
        MuseumManager.instance.PurchasedItems = purchasedItems;

        foreach (ItemData item in MuseumManager.instance.PurchasedItems)
        {
            ItemData removeItem = ItemManager.instance.GetAllItemDatas().Where(x => x.ID == item.ID).SingleOrDefault();
            Debug.Log($"Silinecek Item: {removeItem.ID}:{removeItem.Name}");
            ItemManager.instance.ShopItemDatas.Remove(removeItem);
        }

        foreach (ItemData item in MuseumManager.instance.PurchasedItems)
        {
            ItemData removeItem = ItemManager.instance.RItems.Where(x => x.ID == item.ID).SingleOrDefault();
            Debug.Log($"Silinecek Item: {removeItem.ID}:{removeItem.Name}");
            ItemManager.instance.RItems.Remove(removeItem);
        }

    }
    public async System.Threading.Tasks.Task LoadSkills()
    {
        List<SkillNode> afterDatabaseSkills = new List<SkillNode>();
        foreach (var skill in SkillTreeManager.instance.skillNodes)
        {
            SkillNode s = null;
#if UNITY_EDITOR
            await FirestoreManager.instance.skillDatasHandler.GetSkillInDatabase("ahmet123", skill.ID);
#else
            await FirestoreManager.instance.skillDatasHandler.GetSkillInDatabase(FirebaseAuthManager.instance.GetCurrentUser().UserId, skill.ID);
#endif
            if (s != null)
                afterDatabaseSkills.Add(s);
            else
                afterDatabaseSkills.Add(skill);
            
        }
        foreach (SkillNode skill in afterDatabaseSkills)
        {
            GameObject baseSkiilObj = SkillTreeManager.instance.skillObjects.Where(x=> x.GetComponent<BaseSkillOptions>().SkillID == skill.ID).SingleOrDefault();
            if (baseSkiilObj != null)
            {
                int lenght = baseSkiilObj.transform.childCount;
                for (int i = 0; i < lenght; i++)
                {
                    Debug.Log(baseSkiilObj.transform.GetChild(i).gameObject.name);
                    Transform childTransform = baseSkiilObj.transform.GetChild(i);
                    if (childTransform.TryGetComponent(out SkillAbilityAmountController skillAmountController))
                    {
                        Debug.Log(skill.SkillCurrentLevel);
                        Debug.Log(skill.SkillMaxLevel);                        
                        skillAmountController.SetSkillCurrentLevelUI(skill.SkillCurrentLevel);
                    }
                    if (childTransform.TryGetComponent(out SkillAbilityMaxAmountController skillMaxController))
                    {
                        skillMaxController.SetSkillMaxLevelUI(skill.SkillMaxLevel);
                        Debug.Log("Mevcut Skill'in Max Leveli => " + skill.SkillMaxLevel);
                    }
                }
            }
        }
    }

    public async System.Threading.Tasks.Task LoadWorkers()
    {
        Dictionary<string, object> gameDatas = new Dictionary<string, object>();
#if UNITY_EDITOR
        gameDatas = await FirestoreManager.instance.GetGameDataInDatabase("ahmet123");
#else
        gameDatas = await FirestoreManager.instance.GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUser().UserId);
#endif
        BaseWorkerHiringPrice = Convert.ToSingle(gameDatas["BaseWorkerHiringPrice"]);

        List<WorkerData> inventoryWorkers = new List<WorkerData>();
        List<int> workerIDs = ((List<object>)gameDatas["WorkersInInventoryIDs"]).Select(x => Convert.ToInt32(x)).ToList();
        foreach (int id in workerIDs)
        {
            WorkerData databaseItem = WorkerManager.instance.GetAllWorkers().Where(x => x.ID == id).SingleOrDefault().MyDatas;
            inventoryWorkers.Add(databaseItem);
        }

        List<WorkerData> currentActiveWorkers = new List<WorkerData>();

        List<int> workerIds = WorkerManager.instance.GetAllWorkers().Select(x => x.MyDatas.ID).ToList();
        foreach (var id in workerIds)
        {
            WorkerData databaseWorker = null;
#if UNITY_EDITOR
            databaseWorker = await FirestoreManager.instance.workerDatasHandler.GetWorkerInDatabase("ahmet123", id);
#else
            databaseWorker = await FirestoreManager.instance.workerDatasHandler.GetWorkerInDatabase(FirebaseAuthManager.instance.GetCurrentUser().UserId, id);
#endif
            if (databaseWorker != null)
            {
                currentActiveWorkers.Add(databaseWorker);
            }
        }

        foreach (WorkerData databaseWorker in currentActiveWorkers)
        {
            List<int> iWorkRoomIds = new List<int>();
            int length1 = databaseWorker.WorkRoomsIDs.Count;
            for (int i = 0; i < length1; i++)
            {
                iWorkRoomIds.Add(databaseWorker.WorkRoomsIDs[i]);
            }
            Worker w = WorkerManager.instance.GetWorkerToWorkerType(databaseWorker);
            WorkerBehaviour wb = WorkerManager.instance.GetAllWorkers().Where(x=> x.ID == w.ID).SingleOrDefault();
            w.Name = databaseWorker.Name;
            w.Level = databaseWorker.Level;
            w.IWorkRoomsIDs.Clear();
            wb.MyDatas.WorkRoomsIDs.Clear();
            int length = iWorkRoomIds.Count;
            Debug.Log("worker.WorkRoomsIDs.Count => " + iWorkRoomIds.Count);
            for (int i = 0; i < length; i++)
            {
                w.IWorkRoomsIDs.Add(iWorkRoomIds[i]);
                wb.MyDatas.WorkRoomsIDs.Add(iWorkRoomIds[i]);
            }
            MuseumManager.instance.CurrentActiveWorkers.Add(WorkerManager.instance.GetAllWorkers().Where(x=> x.ID == w.ID).SingleOrDefault());

            if (w.IWorkRoomsIDs.Count > 0)
            {
                wb.gameObject.SetActive(true);
                Debug.Log("wb.gameObject.SetActive(true) => " + (w.IWorkRoomsIDs.Count > 0) + "Worker Name => " + wb.name);
            }
        }
        foreach (WorkerData worker in inventoryWorkers)
        {
            Worker w = WorkerManager.instance.GetWorkerToWorkerType(worker);
            MuseumManager.instance.WorkersInInventory.Add(WorkerManager.instance.GetAllWorkers().Where(x => x.ID == w.ID).SingleOrDefault());
        }
    }
    public async System.Threading.Tasks.Task LoadDailyRewardItems()
    {
        List<ItemData> dailyRewardItems = new List<ItemData>();
        Dictionary<string, object> gameDatas = new Dictionary<string, object>();
#if UNITY_EDITOR
        gameDatas = await FirestoreManager.instance.GetGameDataInDatabase("ahmet123");
#else
        gameDatas = await FirestoreManager.instance.GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUser().UserId);
#endif
        List<int> itemIDs = ((List<object>)gameDatas["DailyRewardItemIDs"]).Select(x => Convert.ToInt32(x)).ToList();
        foreach (int id in itemIDs)
        {
            ItemData databaseItem = ItemManager.instance.GetAllDailyRewardItemDatas().Where(x=> x.ID == id).SingleOrDefault();
            dailyRewardItems.Add(databaseItem);
        }
        if (dailyRewardItems.Count > 0)
            ItemManager.instance.CurrentDailyRewardItems = dailyRewardItems;              
    }
    public async void LoadLastDailyRewardTime()
    {
        Dictionary<string, object> gameDatas = new Dictionary<string, object>();
#if UNITY_EDITOR
        gameDatas = await FirestoreManager.instance.GetGameDataInDatabase("ahmet123");
#else
        gameDatas = await FirestoreManager.instance.GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUser().UserId);
#endif
        string lastDateTimeString = gameDatas.ContainsKey("LastDailyRewardTime")? gameDatas["LastDailyRewardTime"].ToString(): "";
        byte whatDay = gameDatas.ContainsKey("WhatDay") ? Convert.ToByte(gameDatas["WhatDay"]): (byte)0;
        Debug.Log("Loading LastDailyRewardTime => " + lastDateTimeString);
        Debug.Log("Loading WhatDay => " + whatDay);
        if (lastDateTimeString != "")
        {
            MuseumManager.instance.lastDailyRewardTime = DateTime.Parse(lastDateTimeString);
            TimeManager.instance.WhatDay = whatDay;
        }
    }

    public void LoadStatues()
    {
        //RoomManager.instance.statuesHandler.activeEditObjs = CurrentSaveData.StatueDatas;

        //foreach (var statue in RoomManager.instance.statuesHandler.activeEditObjs)
        //{
        //    RoomManager.instance.AddSavedStatues(statue);
        //}
    }

    private void OnApplicationQuit()
    {
        //TimeManager.instance.FirstOpen = true;
        //Save();
    }
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            //Save();
        }
    }
}

[System.Serializable]
public class PlayerSaveData
{
    public string SaveName;
    public string GameLanguage;

    public float Gold;
    public float Culture;
    public float Gem;
    public float SkillPoint;
    public int CurrentCultureLevel;

    //IsFirstGame
    public bool IsFirstGame = true;
    //isWatchTutorial
    public bool IsWatchTutorial = false;
    //RoomManager
    public float ActiveRoomsRequiredMoney = 1000;
    //WorkerManager
    public float baseWorkerHiringPrice;

    public List<PictureData> CurrentPictures = new List<PictureData>();
    public List<PictureData> InventoryPictures = new List<PictureData>();
    public List<RoomSaveData> Rooms = new List<RoomSaveData>();
    public List<ItemData> PurchasedItems = new List<ItemData>();
    public List<ItemData> DailyRewardItems = new List<ItemData>();
    public List<SkillNode> SkillNodes = new List<SkillNode>();
    public List<WorkerData> CurrentWorkerDatas = new List<WorkerData>();
    public List<WorkerData> InventoryWorkerDatas = new List<WorkerData>();
    public AdverstingData adData; //ADS SISTEMI KURULDUKTAN SONRA EKLENECEK.

    public List<EditObjData> StatueDatas = new List<EditObjData>();

    //DailyReward
    public string LastDailyRewardTime = "";
    public byte WhatDay;
}    

public enum GameMode
{
    None,
    MuseumEditing,
    FPS,
    Ghost,
    RoomEditing
}
