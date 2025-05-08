using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using TaskExtensions;
using Translator;
using Unity.VisualScripting;
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
    [SerializeField] bool isDemo; // Bu field true olursa, mevcut versiyon demo surumu olur.

    private System.Threading.CancellationTokenSource cts;
    DynamicTranslation translation;

    public bool isLoadedGame;
    private const string encryptionKey = "SavePassword";
    private const string fileExtension = ".art";
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
        yield return TimeManager.instance.UpdateCurrentTime();
        TimeManager.instance.StartProgressCoroutine();
        
        System.Threading.Tasks.Task task = LoadLastDailyRewardTime();
        yield return new WaitUntil(() => task.IsCompleted);
        //TimeManager.instance.InvokeRepeating(nameof(TimeManager.instance.UpdateCurrentTime), 1, 1f);
        Debug.Log("LoadLastDailyRewardTime task is complated.");
        DateTime dateControl = new DateTime(0001,01,01,01,01,01);
        yield return new WaitUntil(() => TimeManager.instance.CurrentDateTime.Second > dateControl.Second);
        Init();
        //Load();
        System.Threading.Tasks.Task task1 = LoadIsFirstGame();
        yield return new WaitUntil(() => task1.IsCompleted);
        if (IsFirstGame)
            ItemManager.instance.SetCalculatedDailyRewardItems();
        LoadGame();
        StartCoroutine(LateLoad());
        if (LanguageDatabase.instance.TranslationWillBeProcessed)
        LanguageDatabase.instance.LoadLanguageData();
        Debug.Log("CurrentSaveData.ActiveRoomsRequiredMoney => " + CurrentSaveData.ActiveRoomsRequiredMoney);
    }
    public void Init()
    {
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
            SaveGame();
            AutoSaveTimer = Time.time + 300;
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
    public async System.Threading.Tasks.Task BulkTranslateAndAssignAsync(string language,List<LanguageData> textsToTranslateDatas,Action<List<LanguageData>> onTranslationComplete) // DÖNÜÞ TÝPÝNÝ List<LanguageData> YAPTIK!
    {
        // 1. Adým: Çevrilecek metinleri ve TargetID'leri eþleþtiren bir liste oluþtur
        List<(int TargetID, string Key)> textsWithIds = textsToTranslateDatas
            .Select(data => (data.TargetID, data.Key))
            .ToList();

        // Çevrilecek metinleri al
        List<string> textsToTranslate = textsWithIds.Select(x => x.Key).ToList();

        // 2. Adým: Metinleri birleþtir
        string combinedText = string.Join("|", textsToTranslate);

        // 3. Adým: Birleþik metni çevir
        string translatedText = await translation.TranslateTextAsync(combinedText, GetLanguageShortString(language));

        // 4. Adým: Çevirilen metni parçala
        List<string> translatedParts = new List<string>(translatedText.Split('|'));

        // 5. Adým: Çevirilen metinleri tekrar TargetID ile eþleþtir
        List<LanguageData> translatedDataList = new List<LanguageData>();

        for (int i = 0; i < textsWithIds.Count && i < translatedParts.Count; i++)
        {
            translatedDataList.Add(new LanguageData(textsWithIds[i].TargetID, translatedParts[i]));
        }

        // 6. Adým: Çeviri tamamlandýðýnda callback fonksiyonunu çaðýr
        onTranslationComplete?.Invoke(translatedDataList);
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
    public string GetGameLanguage()
    {
        return GameLanguage;
    }
    public async void SetGameLanguage(string _language)
    {
        GameLanguage = _language;
        LocalizationManager.CurrentLanguage = _language;
    }
    public GameMode GetCurrentGameMode()
    {
        return CurrentGameMode;
    }
    public void SetCurrenGameMode(GameMode _gameMode)
    {
        CurrentGameMode = _gameMode;
    }
    public bool LoadCompleted = false;
    public void SaveGame(bool _newSave = false, string _newSaveName = "")
    {
        if (CurrentSaveData.SaveName == "")
        {
            Debug.LogError("Could not save the game, save data lost.");
            return;
        }

        if (!LoadCompleted)
        {
            Debug.LogError("Loading is not completed yet. Can not save.");
            return;
        }

        string FileName = CurrentSaveData.UniqueSaveFolderName + "/" + (_newSaveName == "" ? CurrentSaveData.SaveName : _newSaveName);
        if (File.Exists(Application.persistentDataPath + "/" + FileName + fileExtension)) //Check the save name is exist
            File.Delete(Application.persistentDataPath + "/" + FileName + fileExtension);

        long unixTimestamp = ((System.DateTimeOffset)System.DateTime.Now).ToUnixTimeSeconds();

        bool newSave = CurrentSaveData.CreatedUID == 0;
        if (_newSave)
            newSave = true;

        if (isDemo)
        _newSaveName = _newSaveName + "_Demo";


        PlayerSaveData savedata = new PlayerSaveData();
        savedata.SaveName = !_newSave ? CurrentSaveData.SaveName : _newSaveName;
        savedata.CreatedUID = !newSave ? CurrentSaveData.CreatedUID : unixTimestamp;
        savedata.UniqueSaveFolderName = newSave ? CurrentSaveData.UniqueSaveFolderName + "_" + savedata.CreatedUID : CurrentSaveData.UniqueSaveFolderName;
        savedata.LastSave = unixTimestamp;

        //NumeralDatas
        savedata.Gold = CurrentSaveData.Gold;
        savedata.Culture = CurrentSaveData.Culture;
        savedata.Gem = CurrentSaveData.Gem;
        savedata.SkillPoint = CurrentSaveData.SkillPoint;
        savedata.CurrentCultureLevel = CurrentSaveData.CurrentCultureLevel;

        //Language
        savedata.GameLanguage = CurrentSaveData.GameLanguage;

        //Bools
        savedata.IsWatchTutorial = CurrentSaveData.IsWatchTutorial;
        savedata.IsFirstGame = CurrentSaveData.IsFirstGame;

        //Pictures
        savedata.CurrentPictures = CurrentSaveData.CurrentPictures;
        savedata.InventoryPictures = CurrentSaveData.InventoryPictures;

        //Rooms
        savedata.Rooms = CurrentSaveData.Rooms;
        savedata.ActiveRoomsRequiredMoney = CurrentSaveData.ActiveRoomsRequiredMoney;

        //Items
        savedata.PurchasedItems = CurrentSaveData.PurchasedItems;
        savedata.DailyRewardItems = CurrentSaveData.DailyRewardItems;
        savedata.LastDailyRewardTime = CurrentSaveData.LastDailyRewardTime;
        savedata.WhatDay = CurrentSaveData.WhatDay;

        //Workers
        savedata.CurrentWorkerDatas = CurrentSaveData.CurrentWorkerDatas;
        savedata.InventoryWorkerDatas = CurrentSaveData.InventoryWorkerDatas;
        savedata.baseWorkerHiringPrice = CurrentSaveData.baseWorkerHiringPrice;

        //Statues
        savedata.StatueDatas = CurrentSaveData.StatueDatas;

        //Skills
        savedata.SkillNodes = CurrentSaveData.SkillNodes;

        //CustomizeData
        savedata.customizeData = CurrentSaveData.customizeData;

        if (!_newSave)
        {
            CurrentSaveData = savedata;
            string fullPath = Application.persistentDataPath + "/" + CurrentSaveData.UniqueSaveFolderName + "/" + CurrentSaveData.SaveName + fileExtension;
            string folderPath = Application.persistentDataPath + "/" + CurrentSaveData.UniqueSaveFolderName;
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string jsonString = JsonUtility.ToJson(savedata);
            byte[] encryptedData = EncryptStringToBytes(jsonString);
            File.WriteAllBytes(fullPath, encryptedData);

            Debug.Log("Save Game: " + CurrentSaveData.SaveName + " / Saved successfully.");
        }
        else
        {
            string fullPath = Application.persistentDataPath + "/" + savedata.UniqueSaveFolderName + "/" + savedata.SaveName + fileExtension;
            string folderPath = Application.persistentDataPath + "/" + savedata.UniqueSaveFolderName;
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string jsonString = JsonUtility.ToJson(savedata);
            byte[] encryptedData = EncryptStringToBytes(jsonString);
            File.WriteAllBytes(fullPath, encryptedData);

            Debug.Log("Save Game as new save: " + savedata.SaveName + " / Saved successfully.");
        }

    }
    public void LoadGame()
    {
        string path = CurrentSaveData.SaveName;
        Debug.Log("Load save file location: " + Application.persistentDataPath + "/" + path + fileExtension);
        if (File.Exists(Application.persistentDataPath + "/" + path + fileExtension))
        {
            byte[] loadBytes = File.ReadAllBytes(Application.persistentDataPath + "/" + path + fileExtension);
            string decryptedData = DecryptStringFromBytes(loadBytes);
            CurrentSaveData = JsonUtility.FromJson<PlayerSaveData>(decryptedData);
        }
        else
        {
            Debug.Log("Save could not be found.");
            CurrentSaveData.ActiveRoomsRequiredMoney = 1000;
            SaveGame(true, "testuser123");
        }
        
    }
    IEnumerator LateLoad()
    {
        LoadCompleted = false;
        yield return null;

        yield return null;
        LoadIsWatchTutorialAsync();
        LoadDailyRewardItems();
        LoadMuseumNumeralDatas();
        LoadInventoryPictures();
        LoadPurchasedItems();

        LoadCompleted = true;

        PictureChangesReqiuredAmountCalculater();
        CurrentSaveData.baseWorkerHiringPrice = 500;
    }
    private byte[] EncryptStringToBytes(string plainText)
    {
        byte[] encrypted;

        using (Aes aesAlg = Aes.Create())
        {
            // Ensure the key is valid for AES
            aesAlg.Key = GetValidKey(encryptionKey, aesAlg);
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                }
                encrypted = msEncrypt.ToArray();
            }
        }

        return encrypted;
    }
    // Decrypt bytes to string
    public string DecryptStringFromBytes(byte[] cipherText)
    {
        string plaintext = "";

        using (Aes aesAlg = Aes.Create())
        {
            byte[] iv = new byte[16];
            System.Array.Copy(cipherText, iv, iv.Length);

            // Ensure the key is valid for AES
            aesAlg.Key = GetValidKey(encryptionKey, aesAlg);
            aesAlg.IV = iv;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(cipherText, iv.Length, cipherText.Length - iv.Length))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        return plaintext;
    }
    private byte[] GetValidKey(string key, Aes aesAlg)
    {
        // Convert the key string to bytes
        byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);

        // AES key sizes: 128-bit (16 bytes), 192-bit (24 bytes), or 256-bit (32 bytes)
        int validKeySize = aesAlg.KeySize / 8;

        // If the key is too large, truncate it. If it's too small, pad it with zeros.
        byte[] validKey = new byte[validKeySize];
        if (keyBytes.Length >= validKeySize)
        {
            Array.Copy(keyBytes, validKey, validKeySize);
        }
        else
        {
            Array.Copy(keyBytes, validKey, keyBytes.Length);
        }

        return validKey;
    }
    public async System.Threading.Tasks.Task LoadRooms() //Bu metot firebase'den izole edildi fakat json formatina gecilmeli.
    {
        if (!IsFirstGame)
        {
            try
            {

                float activeRoomsRequiredMoney = CurrentSaveData.ActiveRoomsRequiredMoney;

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
            List<RoomSaveData> databaseRooms = CurrentSaveData.Rooms;

            Debug.Log("Database room data retrieval completed.");

            foreach (RoomSaveData databaseRoom in databaseRooms)
            {
                if (databaseRoom != null)
                {
                    //Debug.Log($"databaseRoom.ID => {databaseRoom.ID} databaseRoom.isActive => {databaseRoom.isActive} databaseRoom.StatueID => {databaseRoom.GetMyStatueInTheMyRoom()?.ID}");

                    RoomData myRoom = allRooms.SingleOrDefault(x => x.ID == databaseRoom.ID);
                    if (myRoom != null)
                    {
                        myRoom = new RoomData(databaseRoom);
                        Debug.Log("myRoom.ID is " + myRoom.ID + " databaseRoom.ID is " + databaseRoom.ID);

                        if (myRoom.GetMyStatueInTheMyRoom() != null)
                        {
                            Debug.Log($"before myRoom.GetMyStatueInTheMyRoom().IsPurchased => {myRoom.GetMyStatueInTheMyRoom().IsPurchased}");
                            var myStatue = myRoom.GetMyStatueInTheMyRoom();
                            Debug.Log($"myStatue Infos: ID:{myStatue.ID} + Name:{myStatue.Name} + RoomCell:{myStatue._currentRoomCell.CellLetter.ToString() + myStatue._currentRoomCell.CellNumber} + Bonusses.Count:{myStatue.Bonusses.Count}");
                            myStatue.SetIsPurchased();
                            Debug.Log($"myStatue Infos: FocusedLevel:{myStatue.FocusedLevel}");
                            RoomManager.instance.AddSavedStatues(myStatue);                            
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
    //public async System.Threading.Tasks.Task LoadRemoveAds()
    //{
    //    try
    //    {
    //        Debug.Log("LoadRemoveAds test 1 complated;");
    //        Dictionary<string, object> gameDatas = new Dictionary<string, object>();

    //        gameDatas = await FirestoreManager.instance.GetGameDataInDatabase(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID).WithCancellation(GetFirebaseToken().Token);

    //        AdverstingData databaseAdversting = new AdverstingData();
    //        bool removeAds = gameDatas.ContainsKey("RemoveAllAds") ? Convert.ToBoolean(gameDatas["RemoveAllAds"]) : false;
    //        databaseAdversting.RemovedAllAds = removeAds;
    //        if (GoogleAdsManager.instance != null)
    //            GoogleAdsManager.instance.adsData = databaseAdversting;

    //        Debug.Log("LoadRemoveAds test 2 complated;");
    //        if (GoogleAdsManager.instance.adsData.RemovedAllAds)
    //        {
    //            GoogleAdsManager.instance.StartInterstitialAdBool(false);
    //            GoogleAdsManager.instance.StartBannerAdBool(false);
    //        }
    //        else
    //        {
    //            GoogleAdsManager.instance.StartInterstitialAdBool(true);
    //            GoogleAdsManager.instance.StartBannerAdBool(true);
    //        }
    //        GoogleAdsManager.instance.StartRewardAdBool(true);
    //        Debug.Log("LoadRemoveAds test 3 complated;");
    //    }
    //    catch (Exception _Ex)
    //    {
    //        Debug.Log("LoadRemoveAds method caught an error => " + _Ex.Message);
    //    }
       

    //}
    public async void LoadMuseumNumeralDatas()
    {
        MuseumManager.instance.SetSaveData(CurrentSaveData.Gold, CurrentSaveData.Culture, CurrentSaveData.Gem, CurrentSaveData.SkillPoint, CurrentSaveData.CurrentCultureLevel);
    }
    public async System.Threading.Tasks.Task LoadIsFirstGame()
    {
        bool firstGameResult = false;

        try
        {
            firstGameResult = CurrentSaveData.IsFirstGame;

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


            watchTutorialResult = CurrentSaveData.IsWatchTutorial;

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

            databasePictures = CurrentSaveData.CurrentPictures;

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

        List<int> itemIDs = CurrentSaveData.PurchasedItems.Select(x=> x.ID).ToList();
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

        afterDatabaseSkills = CurrentSaveData.SkillNodes;

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
        UIController.instance.languageStrings.Clear();
        int length = skillInfoLanguageDatas.Count;
        for (int i = 0; i < length; i++)
        {
            UIController.instance.languageStrings.Add(skillInfoLanguageDatas[i].ActiveLanguage);
        }
    }
    public void TranslatePictureInfos()
    {
        List<LanguageData> pictureInfoLanguageDatas = LanguageDatabase.instance.Language.PictureInfoStrings;
        PicturesMenuController.instance.PictureStrings.Clear();
        int length = pictureInfoLanguageDatas.Count;
        for (int i = 0; i < length; i++)
        {
            PicturesMenuController.instance.PictureStrings.Add(pictureInfoLanguageDatas[i].ActiveLanguage);
        }
    }

    public void TranslateShopQuestionInfos()
    {
        List<LanguageData> questionInfoLanguageDatas = LanguageDatabase.instance.Language.ShopQuestionInfoStrings;
        UIController.instance.SkillQuestionInfos.Clear();
        int length = questionInfoLanguageDatas.Count;
        for (int i = 0; i < length; i++)
        {
            UIController.instance.SkillQuestionInfos.Add(questionInfoLanguageDatas[i].ActiveLanguage);
        }
    }
    public void TranslateInternetCheckInfoStrings()
    {
        List<LanguageData> checkInfoLanguageDatas = LanguageDatabase.instance.Language.InternetCheckInfoStrings;
        InternetCheck.instance.InfoStrings.Clear();
        int length = checkInfoLanguageDatas.Count;
        for (int i = 0; i < length; i++)
        {
            InternetCheck.instance.InfoStrings.Add(checkInfoLanguageDatas[i].ActiveLanguage);
        }
    }
    public void TranslateNotificationMessages()
    {
        List<Notification> allDatas = NotificationManager.instance.Notifications;
        List<LanguageData> notificationLanguageDatas = LanguageDatabase.instance.Language.NotificationMessages;
        int length = allDatas.Count;
        for (int i = 0; i < length; i++)
        {
            if (i < notificationLanguageDatas.Count)
            allDatas[i].Message = notificationLanguageDatas[i].ActiveLanguage;
        }
    }
    public void TranslateCustomizationStrings()
    {
        List<LanguageData> onSelectedAnEquipmentStrings = LanguageDatabase.instance.Language.customize_SelectedAnEquipmentStrings;
        List<LanguageData> onExitProcessStrings = LanguageDatabase.instance.Language.customize_OnExitProcessStrings;
        int length = onSelectedAnEquipmentStrings.Count;
        for (int i = 0; i < length; i++)
        {
            CustomizeHandler.instance.onSelectedAnEquipmentStrings.Add(onSelectedAnEquipmentStrings[i].ActiveLanguage);
        }
        int length1 = onExitProcessStrings.Count;
        for (int i = 0; i < length1; i++)
        {
            CustomizeHandler.instance.onExitProcessStrings.Add(onExitProcessStrings[i].ActiveLanguage);
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
        List<WorkerBehaviour> allWorkers = WorkerManager.instance.GetAllWorkers();

        BaseWorkerHiringPrice = CurrentSaveData.baseWorkerHiringPrice;

        List<WorkerData> currentActiveWorkers = new List<WorkerData>();

        List<int> workerIds = allWorkers.Select(x => x.ID).ToList();
        Debug.Log("workerIds.Count => " + workerIds.Count);

        currentActiveWorkers = CurrentSaveData.CurrentWorkerDatas;

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
            WorkerData databaseWorker = null;
            string Name = "None";
            if (currentActiveWorkers.Any(x => x.ID == currentWorker.ID))
            {
                databaseWorker = currentActiveWorkers.FirstOrDefault(x => x.ID == currentWorker.ID);
                Name = databaseWorker.Name;
                currentWorker.MyScript.Age = databaseWorker.Age;
                currentWorker.MyScript.Height = databaseWorker.Height;
                Debug.Log("currentActiveWorkers.Any(x => x.ID == currentWorker.ID) => " + currentWorker.ID + " age:" + currentWorker.MyScript.Age + " height:"+currentWorker.MyScript.Height, currentWorker.transform);
            }
            else
            {
                Name = Constant.GetNPCName(currentWorker.IsMale);
            }
            WorkerManager.instance.SetDatabaseDatas(currentWorker, databaseWorker, Name);
        }
        List<WorkerData> inventoryWorkers = new List<WorkerData>();
        List<int> inventoryWorkerIDs = CurrentSaveData.InventoryWorkerDatas.Select(x=> x.ID).ToList();
        Debug.Log("inventoryWorkerIDs.Count => " + inventoryWorkerIDs);
        foreach (int id in inventoryWorkerIDs)
        {
            //WorkerData databaseItem = allWorkers.Where(x => x.ID == id).SingleOrDefault().MyDatas;
            WorkerData databaseItem = allWorkers.Where(x => x.ID == id).SingleOrDefault().MyDatas;
            Debug.Log("database Inventory worker name:" + databaseItem.Name);
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
        List<ItemData> dailyRewardItems = CurrentSaveData.DailyRewardItems;
        //for (int i = 0; i < length; i++)
        //{
        //    ItemData databaseItem = ItemManager.instance.GetAllDailyRewardItemDatas().Where(x => x.ID == itemIDs[i]).SingleOrDefault();
        //    databaseItem.IsPurchased = itemsPurchased[i];
        //    databaseItem.IsLocked = itemsLocked[i];
        //    Debug.Log($"DailyRewardDatabase id:{itemIDs[i]} and ItemManager item id {databaseItem.ID}");
        //    dailyRewardItems.Add(databaseItem);
        //}
        if (dailyRewardItems.Count > 0)
            ItemManager.instance.CurrentDailyRewardItems = dailyRewardItems;              
    }
    public async System.Threading.Tasks.Task LoadLastDailyRewardTime()
    {
        string lastDateTimeString = CurrentSaveData.LastDailyRewardTime;
        byte whatDay = CurrentSaveData.WhatDay;
        DateTime lastDateTime = DateTime.Parse(lastDateTimeString);
        Debug.Log("before Loading LastDailyRewardTime => " + lastDateTimeString);
        Debug.Log("before Loading WhatDay => " + whatDay);
        if (lastDateTime.Year > 1)
        {
            MuseumManager.instance.lastDailyRewardTime = DateTime.Parse(lastDateTimeString);
            TimeManager.instance.WhatDay = whatDay;
        }
        else
        {
            MuseumManager.instance.lastDailyRewardTime = TimeManager.instance.CurrentDateTime;
            TimeManager.instance.WhatDay = whatDay;
        }
        Debug.Log("after Loading LastDailyRewardTime => " + lastDateTimeString);
        Debug.Log("after Loading WhatDay => " + whatDay);
    }
    public async System.Threading.Tasks.Task LoadCustomizationData()
    {
        CharacterCustomizeData characterCustomize = new CharacterCustomizeData();
        characterCustomize = CurrentSaveData.customizeData;

        CustomizeHandler.instance.characterCustomizeData = characterCustomize;
        Debug.Log("CustomizeHandler.instance.characterCustomizeData.LastSelectedCustomizeCategory => " + CustomizeHandler.instance.characterCustomizeData.LastSelectedCustomizeCategory);
        CustomizeHandler.instance.CustomizationInit();
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
        CancelFirebaseOperations();
    }
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            if (NpcManager.instance != null && !NpcManager.instance.databaseProcessComplated) return;
            AudioManager.instance.SaveAudioSettings();
            SaveGame();            
            TimeManager.instance.StopProgressCoroutine();
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
            StartCoroutine(TimeManager.instance.UpdateCurrentTime());
            TimeManager.instance.StartProgressCoroutine();
        }
    }
}

[System.Serializable]
public class PlayerSaveData
{
    public string SaveName;
    public string GameLanguage;
    public string UniqueSaveFolderName;
    public long CreatedUID;
    public long LastSave;
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
    public CharacterCustomizeData customizeData = new CharacterCustomizeData();

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