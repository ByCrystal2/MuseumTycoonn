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
        Load();
        if (GameLanguage == "" || GameLanguage == null || GameLanguage == string.Empty)
            SetGameLanguage("English");
        else
            SetGameLanguage(CurrentSaveData.GameLanguage);

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
    
    public string GetGameLanguage()
    {
        return GameLanguage;
    }
    public void SetGameLanguage(string _language)
    {
        GameLanguage = _language;
        LocalizationManager.CurrentLanguage = _language;
        CurrentSaveData.GameLanguage = _language;
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
        if (CurrentSaveData.SaveName == "")
            CurrentSaveData.SaveName = _GameSave;

        CurrentSaveData.GameLanguage = GameLanguage;

        var a = MuseumManager.instance.GetSaveData();
        CurrentSaveData.Gold = a._gold;
        CurrentSaveData.Culture = a._Culture;
        CurrentSaveData.Gem = a._Gem;
        CurrentSaveData.SkillPoint = a._SkillPoint;
        CurrentSaveData.CurrentCultureLevel = a._CurrentCultureLevel;

        if (NpcManager.instance != null)
        {
            CurrentSaveData.IsFirstGame = NpcManager.instance.IsFirstGame;
        }

        if (rewardManager != null)
        {
            CurrentSaveData.LastDailyRewardTime =  rewardManager.lastDailyRewardTime.ToString("yyyy-MM-dd HH:mm:ss");
            CurrentSaveData.WhatDay = TimeManager.instance.WhatDay;
        }
        

        Debug.Log("CurrentSaveData.LastDailyRewardTime => " + CurrentSaveData.LastDailyRewardTime);
        Debug.Log("CurrentSaveData.WhatDay => " + CurrentSaveData.WhatDay);
        List<RoomData> currentRooms = new List<RoomData>();
        if (RoomManager.instance != null)
        {
            List<RoomData> AllActiveRooms = RoomManager.instance.RoomDatas.Where(x=> (x.isActive && !x.isLock) || x.isActive).ToList();
            foreach (var room in AllActiveRooms)
                currentRooms.Add(room);
            Debug.Log("currentRooms.count: " + currentRooms.Count);
        }

        List<PictureData> currentActivePictures = new List<PictureData>();
        foreach (var item in MuseumManager.instance.CurrentActivePictures)
            currentActivePictures.Add(item._pictureData);
        Debug.Log("currentActivePictures.count: " + currentActivePictures.Count);

        List<PictureData> inventoryPictures = new List<PictureData>();
        foreach (var item in MuseumManager.instance.InventoryPictures)
            inventoryPictures.Add(item);
        Debug.Log("inventoryPictures.count: " + inventoryPictures.Count);

        List<ItemData> inventoryItems = new List<ItemData>();
        foreach (var item in MuseumManager.instance.PurchasedItems)
        {
            inventoryItems.Add(item);
        }
        Debug.Log("inventoryItems.count: " + inventoryItems.Count);

        List<ItemData> dailyRewardItems = new List<ItemData>();
        foreach (var item in ItemManager.instance.CurrentDailyRewardItems)
        {
            dailyRewardItems.Add(item);
        }
        Debug.Log("dailyRewardItems.count: " + dailyRewardItems.Count);

        List<SkillNode> skillNodes = new List<SkillNode>();
        foreach (var skill in SkillTreeManager.instance.skillNodes)
        {
            skillNodes.Add(skill);
        }

        List<EditObjData> statueDatas = new List<EditObjData>();
        if (RoomManager.instance != null)
        {
            foreach (var statueData in RoomManager.instance.statuesHandler.activeEditObjs)
            {
                statueDatas.Add(statueData);
            }
        }
        List<WorkerData> currentWorkerDatas = new List<WorkerData>();
        List<WorkerData> inventoryWorkerDatas = new List<WorkerData>();
        Debug.Log("skillNodes.count: " + skillNodes.Count);
        if (WorkerManager.instance != null)
        {
            
            foreach (var currentWorker in WorkerManager.instance.GetCurrentWorkers())
            {
                currentWorkerDatas.Add(currentWorker.MyDatas);
            }
            Debug.Log("currentWorkerDatas.count: " + currentWorkerDatas.Count);
            
            foreach (var inventoryWorker in WorkerManager.instance.GetWorkersInInventory())
            {
                inventoryWorkerDatas.Add(inventoryWorker.MyDatas);
            }
            Debug.Log("inventoryWorkerDatas.count: " + inventoryWorkerDatas.Count);
        }

        HashSet<string> savedRoomCells = new HashSet<string>(CurrentSaveData.Rooms.Select(r => r.availableRoomCell));
        foreach (var room in currentRooms)
        {
            string roomCell = room.availableRoomCell.CellLetter.ToString() + room.availableRoomCell.CellNumber.ToString();
            if (!savedRoomCells.Contains(roomCell))//
            {
                RoomSaveData newSaveData = new RoomSaveData();
                newSaveData.availableRoomCell = roomCell;
                newSaveData.isLock = room.isLock;
                newSaveData.isActive = room.isActive;
                newSaveData.RequiredMoney = room.RequiredMoney;
                newSaveData.IsHasStatue = room.isHasStatue;
                newSaveData.MyStatue = room.GetMyStatueInTheMyRoom();

                newSaveData.MyRoomWorkersIDs.Clear();
                int length = room.MyRoomWorkersIDs.Count;
                for (int i = 0; i < length; i++)
                {
                    newSaveData.MyRoomWorkersIDs.Add(room.MyRoomWorkersIDs[i]);
                }
                CurrentSaveData.Rooms.Add(newSaveData);
            }
            else
            {
                RoomSaveData currentSavedRoom = CurrentSaveData.Rooms.Where(x=> x.availableRoomCell == roomCell).SingleOrDefault();
                currentSavedRoom.isLock = room.isLock;
                currentSavedRoom.isActive = room.isActive;
                currentSavedRoom.RequiredMoney = room.RequiredMoney;
                currentSavedRoom.IsHasStatue = room.isHasStatue;
                currentSavedRoom.MyStatue = room.GetMyStatueInTheMyRoom();

                currentSavedRoom.MyRoomWorkersIDs.Clear();
                int length = room.MyRoomWorkersIDs.Count;
                for (int i = 0; i < length; i++)
                {
                    currentSavedRoom.MyRoomWorkersIDs.Add(room.MyRoomWorkersIDs[i]);
                }
            }
        }

        CurrentSaveData.CurrentPictures = currentActivePictures;
        CurrentSaveData.InventoryPictures = inventoryPictures;
        CurrentSaveData.PurchasedItems = inventoryItems;
        CurrentSaveData.DailyRewardItems = dailyRewardItems;
        CurrentSaveData.SkillNodes = skillNodes;
        CurrentSaveData.CurrentWorkerDatas = currentWorkerDatas;
        CurrentSaveData.InventoryWorkerDatas = inventoryWorkerDatas;
        CurrentSaveData.StatueDatas = statueDatas;

        if(GoogleAdsManager.instance != null)
            CurrentSaveData.adData = GoogleAdsManager.instance.adsData;

        if (RoomManager.instance != null)
            CurrentSaveData.ActiveRoomsRequiredMoney = RoomManager.instance.activeRoomsRequiredMoney;

        string jsonString = JsonUtility.ToJson(CurrentSaveData);
        File.WriteAllText(Application.persistentDataPath + "/" + CurrentSaveData.SaveName + ".json", jsonString); // this will write the json to the specified path

        Debug.Log("Game Save Location: " + Application.persistentDataPath + "/" + CurrentSaveData.SaveName + ".json");
    }

    public void LoadRooms()
    {
        if (CurrentSaveData.ActiveRoomsRequiredMoney > 0)
            RoomManager.instance.activeRoomsRequiredMoney = CurrentSaveData.ActiveRoomsRequiredMoney;
        

        List<RoomData> AllRooms = GameObject.FindObjectsOfType<RoomData>().ToList();
        //RoomLoad
        foreach (var room in AllRooms)
        {
            RoomSaveData currentRoomData = CurrentSaveData.Rooms.Where(x=> x.availableRoomCell == room.availableRoomCell.CellLetter.ToString() + room.availableRoomCell.CellNumber.ToString()).SingleOrDefault();
            if (currentRoomData != null)
            {
                Debug.Log("Current checkID: " + currentRoomData.availableRoomCell);
                room.isActive = currentRoomData.isActive;
                room.isLock = currentRoomData.isLock;
                room.RequiredMoney = currentRoomData.RequiredMoney;
                room.isHasStatue = currentRoomData.IsHasStatue;
                room.SetMyStatue(currentRoomData.MyStatue);

                room.MyRoomWorkersIDs.Clear();
                int length = currentRoomData.MyRoomWorkersIDs.Count;
                for (int i = 0; i < length; i++)
                {
                    room.MyRoomWorkersIDs.Add(currentRoomData.MyRoomWorkersIDs[i]);
                }
            }
            //Debug.Log("Room name: " + room.transform.name + " / room.pictureDirections Count: " + room.pictureDirections.Count);
            //PictureLoad

            room.LoadThisRoom();
        }
        MuseumManager.instance.InventoryPictures = CurrentSaveData.InventoryPictures;

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
            
            //MuseumManager.instance.CurrentActivePictures = CurrentSaveData.CurrentPictures;
            //MuseumManager.instance.InventoryPictures = CurrentSaveData.InventoryPictures;
            LoadGameLanguage();
            MuseumManager.instance.SetSaveData(CurrentSaveData.Gold, CurrentSaveData.Culture, CurrentSaveData.Gem, CurrentSaveData.SkillPoint, CurrentSaveData.CurrentCultureLevel);
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
    }
    public void LoadGameLanguage()
    {
        GameLanguage = CurrentSaveData.GameLanguage;
    }
    public void LoadRemoveAds()
    {
        if (GoogleAdsManager.instance != null)
            GoogleAdsManager.instance.adsData = CurrentSaveData.adData;        
    }
    public void LoadIsFirstGame()
    {
        if (NpcManager.instance != null)
        {
            NpcManager.instance.IsFirstGame = CurrentSaveData.IsFirstGame;
        }
    }
    public void LoadPurchasedItems()
    {
        MuseumManager.instance.PurchasedItems = CurrentSaveData.PurchasedItems;

        //for (int i = 0; i < MuseumManager.instance.PurchasedItems.Count; i++) => Fatmagul'un kodu
        //{
        //    for (int k = 0; i < ItemManager.instance.GetAllItemDatas().Count; k++)
        //    {
        //        Debug.Log($"Silinecek Item: {MuseumManager.instance.PurchasedItems[i].ID}:{MuseumManager.instance.PurchasedItems[i].Name}");
        //        if (ItemManager.instance.GetAllItemDatas()[k].ID == MuseumManager.instance.PurchasedItems[i].ID)
        //        {
        //            if (ItemManager.instance.GetAllItemDatas()[k].CurrentItemType == ItemType.Table)
        //            {
        //                ItemManager.instance.GetAllItemDatas().Remove(ItemManager.instance.GetAllItemDatas()[k]);
        //                break;
        //            }
        //        }
        //    }
        //}
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
    public void LoadSkills()
    {
        SkillTreeManager.instance.skillNodes = CurrentSaveData.SkillNodes;
        foreach (SkillNode skill in SkillTreeManager.instance.skillNodes)
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

    public void LoadWorkers()
    {
        foreach (WorkerData worker in CurrentSaveData.CurrentWorkerDatas)
        {
            Worker w = WorkerManager.instance.GetWorkerToWorkerType(worker);
            WorkerBehaviour wb = WorkerManager.instance.GetAllWorkers().Where(x=> x.ID == w.ID).SingleOrDefault();
            w.IWorkRoomsIDs.Clear();
            wb.MyDatas.WorkRoomsIDs.Clear();
            int length = worker.WorkRoomsIDs.Count;
            Debug.Log("worker.WorkRoomsIDs.Count => " + worker.WorkRoomsIDs.Count);
            for (int i = 0; i < length; i++)
            {
                Debug.Log("worker.WorkRoomsIDs[i] => " + worker.WorkRoomsIDs[i]);
                w.IWorkRoomsIDs.Add(worker.WorkRoomsIDs[i]);
                wb.MyDatas.WorkRoomsIDs.Add(worker.WorkRoomsIDs[i]);
            }
           WorkerManager.instance.GetCurrentWorkers().Add(WorkerManager.instance.GetAllWorkers().Where(x=> x.ID == w.ID).SingleOrDefault());

            if (w.IWorkRoomsIDs.Count > 0)
            {
                wb.gameObject.SetActive(true);
                Debug.Log("wb.gameObject.SetActive(true) => " + (w.IWorkRoomsIDs.Count > 0) + "Worker Name => " + wb.name);
            }
        }
        foreach (WorkerData worker in CurrentSaveData.InventoryWorkerDatas)
        {
            Worker w = WorkerManager.instance.GetWorkerToWorkerType(worker);
            WorkerManager.instance.GetWorkersInInventory().Add(WorkerManager.instance.GetAllWorkers().Where(x => x.ID == w.ID).SingleOrDefault());
        }
    }
    public void LoadDailyRewardItems()
    {
        ItemManager.instance.CurrentDailyRewardItems = CurrentSaveData.DailyRewardItems;        
    }
    public void LoadLastDailyRewardTime()
    {
        Debug.Log("CurrentSaveData.LastDailyRewardTime => " + CurrentSaveData.LastDailyRewardTime);
        if (CurrentSaveData.LastDailyRewardTime != "")
        {
            rewardManager.lastDailyRewardTime = DateTime.Parse(CurrentSaveData.LastDailyRewardTime);
            TimeManager.instance.WhatDay = CurrentSaveData.WhatDay;
        }
    }

    public void LoadStatues()
    {
        RoomManager.instance.statuesHandler.activeEditObjs = CurrentSaveData.StatueDatas;

        foreach (var statue in RoomManager.instance.statuesHandler.activeEditObjs)
        {
            RoomManager.instance.AddSavedStatues(statue);
        }
    }

    private void OnApplicationQuit()
    {
        TimeManager.instance.FirstOpen = true;
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
        //RoomManager
        public float ActiveRoomsRequiredMoney = 1000;

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
}
public enum GameMode
{
    None,
    MuseumEditing,
    FPS,
    Ghost,
    RoomEditing
}
