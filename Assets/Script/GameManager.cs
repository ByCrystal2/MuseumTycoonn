using Firebase.Extensions;
using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TaskExtensions;
using Translator;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

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
    public RewardManager _rewardManager;
    public int PictureChangeRequiredAmount = 250;
    public float ActiveRoomsRequiredMoney;
    public float BaseWorkerHiringPrice;
    public bool IsFirstGame = true, IsWatchTutorial;
    [SerializeField] bool enableRuntimeDebugger;

    private System.Threading.CancellationTokenSource cts;
    DynamicTranslation translation;
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
#if UNITY_EDITOR
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = enableRuntimeDebugger;
#endif
        cts = new System.Threading.CancellationTokenSource();
        translation = new DynamicTranslation();
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
        System.Threading.Tasks.Task task = LoadLastDailyRewardTime();
        yield return new WaitUntil(() => task.IsCompleted);
        //TimeManager.instance.InvokeRepeating(nameof(TimeManager.instance.UpdateCurrentTime), 1, 1f);
        TimeManager.instance.UpdateCurrentTime();
        TimeManager.instance.StartProgressCoroutine();
        DateTime dateControl = new DateTime(0001,01,01,01,01,01);
        yield return new WaitUntil(() => TimeManager.instance.CurrentDateTime.Second > dateControl.Second);
        Init();
        //Load();
        System.Threading.Tasks.Task task1 = LoadIsFirstGame();
        yield return new WaitUntil(() => task1.IsCompleted);
        if (IsFirstGame)
            ItemManager.instance.SetCalculatedDailyRewardItems();
        LoadIsWatchTutorialAsync();
        LoadDailyRewardItems();
        LoadGooglePlayAchievements();
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
        NotificationManager.instance.NotificationInit();
    }

    private void FixedUpdate()
    {
        if (AutoSaveTimer < Time.time)
        {
            Save();
            AutoSaveTimer = Time.time + 300;
            FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID);
            Debug.Log("Auto Save!");
        }

    }
    public async System.Threading.Tasks.Task BulkTranslateAndAssignAsync(List<string> textsToTranslate, Action<List<string>> onTranslationComplete)
    {
        // 1. Adým: Tüm metinleri birleþtir
        string combinedText = string.Join("|", textsToTranslate);

        // 2. Adým: Birleþik metni çevir
        string translatedText = await translation.TranslateTextAsync(combinedText, GetLanguageShortString(GetGameLanguage()));

        // 3. Adým: Çevirilen metni parçalara ayýr
        List<string> translatedParts = new List<string>(translatedText.Split('|'));

        // Çeviri tamamlandýðýnda callback fonksiyonunu çaðýrýn
        onTranslationComplete?.Invoke(translatedParts);
    }
    public async System.Threading.Tasks.Task BulkTranslateAndAssignAsync(string language, List<string> textsToTranslate, Action<List<string>> onTranslationComplete)
    {
        // 1. Adým: Tüm metinleri birleþtir
        string combinedText = string.Join("|", textsToTranslate);

        // 2. Adým: Birleþik metni çevir
        string translatedText = await translation.TranslateTextAsync(combinedText, GetLanguageShortString(language));

        // 3. Adým: Çevirilen metni parçalara ayýr
        List<string> translatedParts = new List<string>(translatedText.Split('|'));

        // Çeviri tamamlandýðýnda callback fonksiyonunu çaðýrýn
        onTranslationComplete?.Invoke(translatedParts);
    }
    public string GetLanguageShortString(string language)
    {
        return (language) switch
        {
            "English" => "en",
            "Turkish" => "tr",
            "Thai" => "th",
            "Spanish" => "es",
            "Chinese (Traditional)" => "zh-TW",
            "Chinese (Simplified)" => "zh-CN",
            "Kurdish" => "ku",
            "Russian" => "ru",
            "German" => "de",
            "French" => "fr",
            "Japanese" => "ja",
            "Korean" => "ko",
            "Arabic" => "ar",
            _ => "en"
        };
    }
    public bool DatabaseLanguageProgressComplated;
    public async void LanguageControlInDatabase()
    {
        DatabaseLanguageProgressComplated = false;
        string databaseLanguage = string.Empty;

        try
        {
            // Get user ID based on environment
            string userId = FirebaseAuthManager.instance.GetCurrentUserWithID().UserID;

            // Fetch game data from Firestore
            Dictionary<string, object> gameDatas = await FirestoreManager.instance.GetGameDataInDatabase(userId)
                .WithCancellation(GameManager.instance.GetFirebaseToken().Token)
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.LogError($"Error fetching game data: {task.Exception}");
                        return new Dictionary<string, object>();
                    }
                    return task.Result;
                });

            // Retrieve the language from the fetched data
            if (gameDatas != null && gameDatas.ContainsKey("GameLanguage"))
            {
                databaseLanguage = gameDatas["GameLanguage"].ToString();
            }

            // Set the game language based on the retrieved data
            if (string.IsNullOrEmpty(databaseLanguage))
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
        catch (System.Threading.Tasks.TaskCanceledException)
        {
            Debug.Log("LoadGameLanguageAsync operation was canceled.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading game language: {ex.Message}");
        }
        DatabaseLanguageProgressComplated = true;
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
        FirestoreManager.instance.UpdateGameLanguageInGameDatas(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID);
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

        ////if (_rewardManager != null)
        ////{
        ////    CurrentSaveData.LastDailyRewardTime =  _rewardManager.lastDailyRewardTime.ToString("yyyy-MM-dd HH:mm:ss");
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
        string userID = FirebaseAuthManager.instance.GetCurrentUserWithID().UserID;

        Debug.Log("LoadRooms userID => " + userID);

        if (!IsFirstGame)
        {
            try
            {
                Dictionary<string, object> gameDatas = await FirestoreManager.instance
                    .GetGameDataInDatabase(userID)
                    .WithCancellation(GetFirebaseToken().Token);

                float activeRoomsRequiredMoney = gameDatas.ContainsKey("ActiveRoomsRequiredMoney")
                    ? Convert.ToSingle(gameDatas["ActiveRoomsRequiredMoney"])
                    : 1000;

                Debug.Log("LoadRooms activeRoomsRequiredMoney => " + activeRoomsRequiredMoney);
                ActiveRoomsRequiredMoney = activeRoomsRequiredMoney;
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                Debug.Log("LoadRooms operation was canceled.");
                return; // Ýþlem iptal edildiðinde geri dön.
            }
            catch (Exception ex)
            {
                Debug.LogError("Error loading game data: " + ex.Message);
                return; // Hata durumunda geri dön.
            }
        }

        List<RoomData> allRooms = RoomManager.instance.RoomDatas;
        List<int> AllRoomIDs = allRooms.Select(x => x.ID).ToList();

        try
        {
            List<RoomData> databaseRooms = await FirestoreManager.instance
                .roomDatasHandler
                .GetRoomsInDatabase(userID, AllRoomIDs)
                .WithCancellation(GetFirebaseToken().Token);

            Debug.Log("Database room data retrieval completed.");

            foreach (RoomData databaseRoom in databaseRooms)
            {
                if (databaseRoom != null)
                {
                    Debug.Log($"databaseRoom.ID => {databaseRoom.ID} databaseRoom.isActive => {databaseRoom.isActive} databaseRoom.StatueID => {databaseRoom.GetMyStatueInTheMyRoom()?.ID}");

                    RoomData myRoom = allRooms.SingleOrDefault(x => x.ID == databaseRoom.ID);
                    if (myRoom != null)
                    {
                        myRoom = databaseRoom;
                        Debug.Log("myRoom.ID is " + myRoom.ID + " databaseRoom.ID is " + databaseRoom.ID);

                        if (myRoom.GetMyStatueInTheMyRoom() != null)
                        {
                            Debug.Log($"before myRoom.GetMyStatueInTheMyRoom().IsPurchased => {myRoom.GetMyStatueInTheMyRoom().IsPurchased}");
                            var myStatue = myRoom.GetMyStatueInTheMyRoom();
                            Debug.Log($"myStatue Infos: ID:{myStatue.ID} + Name:{myStatue.Name} + RoomCell:{myStatue._currentRoomCell.CellLetter.ToString() + myStatue._currentRoomCell.CellNumber} + Bonusses.Count:{myStatue.Bonusses.Count}");
                            myStatue.SetIsPurchased();
                            Debug.Log($"myStatue Infos: FocusedLevel:{myStatue.FocusedLevel}");
                            RoomManager.instance.AddSavedStatues(myStatue);
                            RoomManager.instance.statuesHandler.activeEditObjs.Add(myStatue);
                            Debug.Log($"after myRoom.GetMyStatueInTheMyRoom().IsPurchased => {myRoom.GetMyStatueInTheMyRoom().IsPurchased}");
                        }
                    }
                }
            }
        }
        catch (System.Threading.Tasks.TaskCanceledException)
        {
            Debug.Log("LoadRooms operation was canceled.");
            return; // Ýþlem iptal edildiðinde geri dön.
        }
        catch (Exception ex)
        {
            Debug.LogError("Room Database Loading Error:" + ex.Message);
        }

        Debug.Log("allRooms.Count => " + allRooms.Count);
        foreach (var room in allRooms)
        {
            room.LoadThisRoom();
        }
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

        gameDatas = await FirestoreManager.instance.GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID).WithCancellation(GetFirebaseToken().Token);

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
        try
        {
            Debug.Log("LoadRemoveAds test 1 complated;");
            Dictionary<string, object> gameDatas = new Dictionary<string, object>();

            gameDatas = await FirestoreManager.instance.GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID).WithCancellation(GetFirebaseToken().Token);

            AdverstingData databaseAdversting = new AdverstingData();
            bool removeAds = gameDatas.ContainsKey("RemoveAllAds") ? Convert.ToBoolean(gameDatas["RemoveAllAds"]) : false;
            databaseAdversting.RemovedAllAds = removeAds;
            if (GoogleAdsManager.instance != null)
                GoogleAdsManager.instance.adsData = databaseAdversting;

            Debug.Log("LoadRemoveAds test 2 complated;");
            if (GoogleAdsManager.instance.adsData.RemovedAllAds)
            {
                GoogleAdsManager.instance.StartInterstitialAdBool(false);
                GoogleAdsManager.instance.StartBannerAdBool(false);
            }
            else
            {
                GoogleAdsManager.instance.StartInterstitialAdBool(true);
                GoogleAdsManager.instance.StartBannerAdBool(true);
            }
            GoogleAdsManager.instance.StartRewardAdBool(true);
            Debug.Log("LoadRemoveAds test 3 complated;");
        }
        catch (Exception _Ex)
        {
            Debug.Log("LoadRemoveAds method caught an error => " + _Ex.Message);
        }
       

    }
    public async void LoadMuseumNumeralDatas()
    {
        Dictionary<string, object> gameDatas = new Dictionary<string, object>();

        gameDatas = await FirestoreManager.instance.GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID).WithCancellation(GetFirebaseToken().Token);

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

        try
        {
            Dictionary<string, object> gameDatas = await FirestoreManager.instance
                .GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID)
                .WithCancellation(GetFirebaseToken().Token);

            firstGameResult = gameDatas.ContainsKey("IsFirstGame")
                ? Convert.ToBoolean(gameDatas["IsFirstGame"])
                : false;

            Debug.Log("Is First Game ? = " + firstGameResult);
            IsFirstGame = firstGameResult;
        }
        catch (System.Threading.Tasks.TaskCanceledException)
        {
            Debug.Log("LoadIsFirstGame was canceled.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error loading IsFirstGame status: " + ex.Message);
        }
    }
    public async void LoadIsWatchTutorialAsync()
    {
        bool watchTutorialResult = false;

        try
        {
            Dictionary<string, object> gameDatas = await FirestoreManager.instance
                .GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID)
                .WithCancellation(GetFirebaseToken().Token);

            watchTutorialResult = gameDatas.ContainsKey("IsWatchTutorial")
                ? Convert.ToBoolean(gameDatas["IsWatchTutorial"])
                : false;

            Debug.Log("Is Watch Tutorial ? = " + watchTutorialResult);
            IsWatchTutorial = watchTutorialResult;
        }
        catch (System.Threading.Tasks.TaskCanceledException)
        {
            Debug.Log("LoadIsWatchTutorial was canceled.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error loading watch tutorial status: " + ex.Message);
        }
    }
    public async void LoadInventoryPictures()
    {
        List<PictureData> pictureDatas = new List<PictureData>();

        try
        {
            List<PictureData> databasePictures;

            databasePictures = await FirestoreManager.instance.pictureDatasHandler
            .GetAllPictureInDatabase(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID)
                .WithCancellation(GetFirebaseToken().Token);

            foreach (PictureData picture in databasePictures)
            {
                if (!picture.isActive)
                {
                    pictureDatas.Add(picture);
                }
            }

            MuseumManager.instance.InventoryPictures = pictureDatas;
        }
        catch (System.Threading.Tasks.TaskCanceledException)
        {
            Debug.Log("LoadInventoryPictures was canceled.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error loading inventory pictures: " + ex.Message);
        }
    }
    public async void LoadPurchasedItems()
    {
        List<ItemData> purchasedItems = new List<ItemData>();
        Dictionary<string, object> gameDatas = new Dictionary<string, object>();

        gameDatas = await FirestoreManager.instance.GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID).WithCancellation(GetFirebaseToken().Token);

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
    
    public void TranslateAllItems()
    {        
        List<LanguageData> databaseItemDescLanguages = LanguageDatabase.instance.Language.ItemDescriptions;
        List<ItemData> allItems = ItemManager.instance.AllItems;
        int allItemLength = allItems.Count;
        List<ItemData> translatedAllItems = ItemManager.instance.AllItems;
        for (int i = 0; i < allItemLength; i++)
        {
            ItemData item = translatedAllItems[i]; //begin
            LanguageData targetLanguageData = databaseItemDescLanguages.Where(x => x.TargetID == item.ID).SingleOrDefault();
            //process
            if (MuseumManager.instance.PurchasedItems.Any(x => x.ID == item.ID))
            {
                Debug.Log($"this item {item.Name} is contain in the PurchasedItems list.");
                ItemData purchItem = MuseumManager.instance.PurchasedItems.Where(x => x.ID == item.ID).SingleOrDefault();
                if (item.Description != string.Empty)
                {
                    if (targetLanguageData is not null)
                        purchItem.Description = targetLanguageData.ActiveLanguage;
                    else
                        Debug.Log($"{purchItem.ID} idli item null (Birden fazla item olabilir mi => {databaseItemDescLanguages.Where(x => x.TargetID == item.ID).Count() > 1})");
                }
                int index = MuseumManager.instance.PurchasedItems.IndexOf(item);
                MuseumManager.instance.PurchasedItems[index] = purchItem;
            }
            if (ItemManager.instance.ShopItemDatas.Any(x => x.ID == item.ID))
            {
                Debug.Log($"this item {item.Name} is contain in the ShopItemDatas list.");
                ItemData purchItem = ItemManager.instance.ShopItemDatas.Where(x => x.ID == item.ID).SingleOrDefault();
                if (item.Description != string.Empty)
                {
                    if (targetLanguageData is not null)
                        purchItem.Description = targetLanguageData.ActiveLanguage;
                    else
                        Debug.Log($"{purchItem.ID} idli item null (Birden fazla item olabilir mi => {databaseItemDescLanguages.Where(x => x.TargetID == item.ID).Count() > 1})");
                }
                int index = ItemManager.instance.ShopItemDatas.IndexOf(item);
                ItemManager.instance.ShopItemDatas[index] = purchItem;
            }
            if (ItemManager.instance.IAPItems.Any(x => x.ID == item.ID))
            {
                Debug.Log($"this item {item.Name} is contain in the IAPItems list.");
                ItemData purchItem = ItemManager.instance.IAPItems.Where(x => x.ID == item.ID).SingleOrDefault();
                if (item.Description != string.Empty)
                {
                    if (targetLanguageData is not null)
                        purchItem.Description = targetLanguageData.ActiveLanguage;
                    else
                        Debug.Log($"{purchItem.ID} idli item null (Birden fazla item olabilir mi => {databaseItemDescLanguages.Where(x => x.TargetID == item.ID).Count() > 1})");
                }
                int index = ItemManager.instance.IAPItems.IndexOf(item);
                ItemManager.instance.IAPItems[index] = purchItem;
            }
            if (item.Description != string.Empty)
            {
                if (targetLanguageData is not null)
                    item.Description = targetLanguageData.ActiveLanguage;
                else
                    Debug.Log($"{item.ID} idli item null (Birden fazla item olabilir mi => {databaseItemDescLanguages.Where(x => x.TargetID == item.ID).Count() > 1})");
            }
            //process
            translatedAllItems[i] = item;//end
        }
        Debug.Log("All Items Localized.");
        ItemManager.instance.AllItems = translatedAllItems;
    }
    public async System.Threading.Tasks.Task LoadSkills()
    {
        List<SkillNode> afterDatabaseSkills = new List<SkillNode>();
        List<SkillNode> allSkills = SkillTreeManager.instance.skillNodes.ToList();
        List<int> skillNodeIDs = allSkills.Select(x => x.ID).ToList();

        afterDatabaseSkills = await FirestoreManager.instance.skillDatasHandler.GetSkillsInDatabase(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID, skillNodeIDs).WithCancellation(GetFirebaseToken().Token);

        int length = allSkills.Count; 

        SkillTreeManager.instance.RefreshSkillBonuses();

        for (int i = 0; i < length; i++)
        {
            if (i < afterDatabaseSkills.Count)
            {
                SkillTreeManager.instance.SetSkillTextProcess(afterDatabaseSkills[i]);
                Debug.Log(afterDatabaseSkills[i].ID + " li skill UI guncellenmistir. (database)");
            }
            else
            {
                allSkills[i].SkillEffect = $"+{allSkills[i].Amounts[allSkills[i].SkillCurrentLevel > 0 ? allSkills[i].SkillCurrentLevel - 1 : 0]} {allSkills[i].defaultEffectString}";
                SkillTreeManager.instance.SetSkillTextProcess(allSkills[i]);
                Debug.Log(allSkills[i].ID + " li skill UI guncellenmistir.");
            }
        }
    }
    public void TranslateAllSkills()
    {
        //Çeviri iþlemlerini baþlat
        List<SkillNode> allSkills = SkillTreeManager.instance.skillNodes;

        List<LanguageData> skillNamesDatas = LanguageDatabase.instance.Language.SkillNames;
        List<LanguageData> skillDescDatas = LanguageDatabase.instance.Language.SkillDescriptions;
        List<LanguageData> skillEffectDatas = LanguageDatabase.instance.Language.SkillEffects;
        List<LanguageData> skillDefaultEffectStringDatas = LanguageDatabase.instance.Language.SkillDefaultStrings;
        int length = allSkills.Count;
        for (int i = 0; i < length; i++)
        {
            SkillNode currentSkill = allSkills[i];
            currentSkill.SkillName = skillNamesDatas.Where(x => x.TargetID == currentSkill.ID).SingleOrDefault().ActiveLanguage;
            currentSkill.SkillDescription = skillDescDatas.Where(x => x.TargetID == currentSkill.ID).SingleOrDefault().ActiveLanguage;
            currentSkill.SkillEffect = skillEffectDatas.Where(x => x.TargetID == currentSkill.ID).SingleOrDefault().ActiveLanguage;
            currentSkill.defaultEffectString = skillDefaultEffectStringDatas.Where(x => x.TargetID == currentSkill.ID).SingleOrDefault().ActiveLanguage;
        }
    }
    public void TranslateCommendsEvulations()
    {
        List<TableCommentEvaluationData> allDatas = TableCommentEvaluationManager.instance.datas;
        List<LanguageData> commendLanguageDatas = LanguageDatabase.instance.Language.CommendEvulations;
        int length = allDatas.Count;
        for (int i = 0; i < length; i++)
        {
            allDatas[i].Message = commendLanguageDatas[i].ActiveLanguage;
        }
    }
    public void TranslateSkillInfos()
    {
        List<LanguageData> skillInfoLanguageDatas = LanguageDatabase.instance.Language.SkillInfoStrings;
        int length = skillInfoLanguageDatas.Count;
        for (int i = 0; i < length; i++)
        {
            UIController.instance.languageStrings.Add(skillInfoLanguageDatas[i].ActiveLanguage);
        }
    }
    public void TranslatePictureInfos()
    {
        List<LanguageData> pictureInfoLanguageDatas = LanguageDatabase.instance.Language.PictureInfoStrings;
        int length = pictureInfoLanguageDatas.Count;
        for (int i = 0; i < length; i++)
        {
            PicturesMenuController.instance.PictureStrings.Add(pictureInfoLanguageDatas[i].ActiveLanguage);
        }
    }

    public void TranslateShopQuestionInfos()
    {
        List<LanguageData> questionInfoLanguageDatas = LanguageDatabase.instance.Language.ShopQuestionInfoStrings;
        int length = questionInfoLanguageDatas.Count;
        for (int i = 0; i < length; i++)
        {
            UIController.instance.SkillQuestionInfos.Add(questionInfoLanguageDatas[i].ActiveLanguage);
        }
    }
    public void TranslateNotificationMessages()
    {
        List<Notification> allDatas = NotificationManager.instance.Notifications;
        List<LanguageData> notificationLanguageDatas = LanguageDatabase.instance.Language.NotificationMessages;
        int length = allDatas.Count;
        for (int i = 0; i < length; i++)
        {
            allDatas[i].Message = notificationLanguageDatas[i].ActiveLanguage;
        }
    }
    // Ceviri islemlerinin tamamlanip tamamlanmadigini kontrol eder.
    private void CheckAndSetCompletion(ref int completedProcesses, System.Threading.Tasks.TaskCompletionSource<bool> tcs)
    {
        completedProcesses++;
        if (completedProcesses >= 4)
        {
            tcs.SetResult(true);
        }
    }
    public async System.Threading.Tasks.Task LoadWorkers()
    {
        int length = WorkerManager.instance.WorkersContent.childCount;
        for (int i = 0; i < length; i++)
        {
            if (WorkerManager.instance.WorkersContent.GetChild(i).TryGetComponent(out WorkerBehaviour workerMono))
            {
                WorkerManager.instance.AllWorkers.Add(workerMono);
                workerMono.Agent = workerMono.gameObject.GetComponent<NavMeshAgent>();
                workerMono.NpcCurrentSpeed = workerMono.NpcSpeed;
                string noneDatabaseName = Constant.GetNPCName(workerMono.IsMale);
                WorkerData databaseWorker = null;
                WorkerManager.instance.SetDatabaseDatas(workerMono, databaseWorker, noneDatabaseName);
                workerMono.gameObject.SetActive(false);
            }
        }
        Debug.Log("WorkerManager.instance.AllWorkers.Count => " + WorkerManager.instance.AllWorkers.Count);
        Dictionary<string, object> gameDatas = new Dictionary<string, object>();
        List<WorkerBehaviour> allWorkers = WorkerManager.instance.GetAllWorkers();

        gameDatas = await FirestoreManager.instance.GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID).WithCancellation(GetFirebaseToken().Token);

        BaseWorkerHiringPrice = Convert.ToSingle(gameDatas["BaseWorkerHiringPrice"]);

        List<WorkerData> currentActiveWorkers = new List<WorkerData>();

        List<int> workerIds = allWorkers.Select(x => x.ID).ToList();
        Debug.Log("workerIds.Count => " + workerIds.Count);

        currentActiveWorkers = await FirestoreManager.instance.workerDatasHandler.GetWorkersInDatabase(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID, workerIds).WithCancellation(GetFirebaseToken().Token);

        foreach (WorkerData databaseWorker in currentActiveWorkers)
        {
            List<int> iWorkRoomIds = new List<int>();
            int length1 = databaseWorker.WorkRoomsIDs.Count;
            for (int i = 0; i < length1; i++)
            {
                iWorkRoomIds.Add(databaseWorker.WorkRoomsIDs[i]);
            }
            Worker w = WorkerManager.instance.GetWorkerToWorkerType(databaseWorker);
            WorkerBehaviour wb = allWorkers.Where(x => x.ID == w.ID).SingleOrDefault();
            Debug.Log($"Database active worker infos: id:{databaseWorker.ID}, name:{databaseWorker.Name}, level:{databaseWorker.Level}, Work Room Count: {databaseWorker.WorkRoomsIDs.Count}");
            w.Name = databaseWorker.Name;
            w.Level = databaseWorker.Level;
            wb.StarRank = WorkerManager.instance.GetRankWithLevel(databaseWorker.Level);
            wb.MyDatas = new WorkerData(databaseWorker);
            w.IWorkRoomsIDs.Clear();
            wb.MyDatas.WorkRoomsIDs.Clear();
            int length2 = iWorkRoomIds.Count;
            for (int i = 0; i < length2; i++)
            {
                w.IWorkRoomsIDs.Add(iWorkRoomIds[i]);
                wb.MyDatas.WorkRoomsIDs.Add(iWorkRoomIds[i]);
            }
            MuseumManager.instance.CurrentActiveWorkers.Add(wb);           
        }

        int length3 = allWorkers.Count;
        for (int i = 0; i < length3; i++)
        {
            WorkerBehaviour currentWorker = allWorkers[i];
            currentWorker.Agent = currentWorker.gameObject.GetComponent<NavMeshAgent>();
            currentWorker.NpcCurrentSpeed = currentWorker.NpcSpeed;
            string noneDatabaseName = Constant.GetNPCName(currentWorker.IsMale);
            WorkerData databaseWorker = null;
            if (currentActiveWorkers.Any(x => x.ID == currentWorker.ID))
            {
                databaseWorker = currentActiveWorkers.FirstOrDefault(x => x.ID == currentWorker.ID);
                Debug.Log("currentActiveWorkers.Any(x => x.ID == currentWorker.ID) => " + currentWorker.ID);
            }
            WorkerManager.instance.SetDatabaseDatas(currentWorker, databaseWorker, noneDatabaseName);
        }
        List<WorkerData> inventoryWorkers = new List<WorkerData>();
        List<int> inventoryWorkerIDs = ((List<object>)gameDatas["WorkersInInventoryIDs"]).Select(x => Convert.ToInt32(x)).ToList();
        foreach (int id in inventoryWorkerIDs)
        {
            WorkerData databaseItem = allWorkers.Where(x => x.ID == id).SingleOrDefault().MyDatas;
            inventoryWorkers.Add(databaseItem);
        }


        
        foreach (WorkerData worker in inventoryWorkers)
        {
            Worker w = WorkerManager.instance.GetWorkerToWorkerType(worker);

            if (w != null)
                MuseumManager.instance.WorkersInInventory.Add(allWorkers.Where(x => x.ID == w.ID).SingleOrDefault());
            else
                Debug.Log($"Veritabanindan gelen {worker.Name} adli, {worker.ID} ID'li npc iscilerimiz arasinda bulunmamaktadir.");
        }
        
        WorkerManager.instance.CreateWorkersToMarket();
        int length4 = MuseumManager.instance.CurrentActiveWorkers.Count;
        for (int i = 0; i < length4; i++)
            MuseumManager.instance.CurrentActiveWorkers[i].gameObject.SetActive(true);
    }
    public async System.Threading.Tasks.Task LoadDailyRewardItems()
    {
        List<ItemData> dailyRewardItems = new List<ItemData>();
        Dictionary<string, object> gameDatas = new Dictionary<string, object>();

        gameDatas = await FirestoreManager.instance.GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID).WithCancellation(GetFirebaseToken().Token);

        List<int> itemIDs = ((List<object>)gameDatas["DailyRewardItemIDs"]).Select(x => Convert.ToInt32(x)).ToList();
        List<bool> itemsPurchased = ((List<object>)gameDatas["DailyRewardItemsPurchased"]).Select(x => Convert.ToBoolean(x)).ToList();
        List<bool> itemsLocked = ((List<object>)gameDatas["DailyRewardItemsLocked"]).Select(x => Convert.ToBoolean(x)).ToList();
        int length = itemIDs.Count;
        for (int i = 0; i < length; i++)
        {
            ItemData databaseItem = ItemManager.instance.GetAllDailyRewardItemDatas().Where(x => x.ID == itemIDs[i]).SingleOrDefault();
            databaseItem.IsPurchased = itemsPurchased[i];
            databaseItem.IsLocked = itemsLocked[i];
            Debug.Log($"DailyRewardDatabase id:{itemIDs[i]} and ItemManager item id {databaseItem.ID}");
            dailyRewardItems.Add(databaseItem);
        }
        if (dailyRewardItems.Count > 0)
            ItemManager.instance.CurrentDailyRewardItems = dailyRewardItems;              
    }
    public async System.Threading.Tasks.Task LoadLastDailyRewardTime()
    {
        Dictionary<string, object> gameDatas = new Dictionary<string, object>();

        gameDatas = await FirestoreManager.instance.GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID).WithCancellation(GetFirebaseToken().Token);

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
    public System.Threading.CancellationTokenSource GetFirebaseToken()
    {
        return cts;
    }
    private void CancelFirebaseOperations()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = new System.Threading.CancellationTokenSource(); // Yeni bir token oluþtur.
        }
    }

    private void OnApplicationQuit()
    {
        AudioManager.instance.SaveAudioSettings();
        FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID);
        CancelFirebaseOperations();
    }
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            AudioManager.instance.SaveAudioSettings();
            FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID);

            CancelFirebaseOperations();
            TimeManager.instance.StopProgressCoroutine();
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
            TimeManager.instance.UpdateCurrentTime();
            TimeManager.instance.StartProgressCoroutine();
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
namespace TaskExtensions
{
    using System.Threading.Tasks;
    using System.Threading;
    public static class TaskHelper
    {
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();

            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task))
                {
                    throw new TaskCanceledException(task);
                }
            }

            return await task; // task baþarýyla tamamlandý, sonucu dön.
        }
    }
}

public enum GameMode
{
    None,
    MuseumEditing,
    FPS,
    Ghost,
    RoomEditing
}
namespace Translator
{
    using UnityEngine;
    using I2.Loc;
    using System.Collections;
    using System.Threading.Tasks;

    public class DynamicTranslation : MonoBehaviour
    {
        public async Task<string> TranslateTextAsync(string text, string targetLanguage)
        {
            // I2 Localization kullanarak çeviri iþlemi
            // ForceTranslate fonksiyonunu doðrudan çaðýrýyoruz.
            string localizedText = GoogleTranslation.ForceTranslate(text, "auto", targetLanguage);

            Debug.Log("LocalizedText => " + localizedText);

            // Çevirinin baþarýlý olup olmadýðýný kontrol edin
            if (!string.IsNullOrEmpty(localizedText))
            {
                return localizedText;
            }
            else
            {
                Debug.LogError("Çeviri baþarýsýz oldu.");
                return text; // Çeviri baþarýsýzsa, orijinal metni döndür
            }
        }
    }
}