using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
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
    float AutoSaveTimer;

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
        Init();
        Load();
    }

    public void Init()
    {
        SceneManager.LoadScene("Menu");        
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
    
    public void Save()
    {
        if (CurrentSaveData.SaveName == "")
            CurrentSaveData.SaveName = _GameSave;

        var a = MuseumManager.instance.GetSaveData();
        CurrentSaveData.Gold = a._gold;
        CurrentSaveData.Culture = a._Culture;
        CurrentSaveData.Gem = a._Gem;
        CurrentSaveData.SkillPoint = a._SkillPoint;
        CurrentSaveData.CurrentCultureLevel = a._CurrentCultureLevel;

        List<PictureData> currentActivePictures = new List<PictureData>();
        foreach (var item in MuseumManager.instance.CurrentActivePictures)
            currentActivePictures.Add(item._pictureData);

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

        List<SkillNode> skillNodes = new List<SkillNode>();
        foreach (var skill in SkillTreeManager.instance.skillNodes)
        {
            skillNodes.Add(skill);
        }
        Debug.Log("skillNodes.count: " + skillNodes.Count);

        CurrentSaveData.CurrentPictures = currentActivePictures;
        CurrentSaveData.InventoryPictures = inventoryPictures;
        CurrentSaveData.PurchasedItems = inventoryItems;
        CurrentSaveData.SkillNodes = skillNodes;


        CurrentSaveData.Rooms = new List<RoomSaveData>();
        if (RoomManager.instance != null)
        {
            int length = RoomManager.instance.transform.childCount;
            for (int i = 0; i < length; i++)
            {
                if (RoomManager.instance.transform.GetChild(i).TryGetComponent(out RoomData currentRoomData))
                {
                    RoomSaveData newRoomSaveData = new RoomSaveData()
                    {
                        availableRoomCell = currentRoomData.availableRoomCell.CellLetter.ToString() + currentRoomData.availableRoomCell.CellNumber.ToString(),
                        isLock = currentRoomData.isLock,
                        isActive = currentRoomData.isActive,
                        RequiredMoney = currentRoomData.RequiredMoney
                    };
                    CurrentSaveData.Rooms.Add(newRoomSaveData);
                }
            }
        }

        if(UnityAdsManager.instance != null)
            CurrentSaveData.adData = UnityAdsManager.instance.adsData;

        string jsonString = JsonUtility.ToJson(CurrentSaveData);
        File.WriteAllText(Application.persistentDataPath + "/" + CurrentSaveData.SaveName + ".json", jsonString); // this will write the json to the specified path

        Debug.Log("Game Save Location: " + Application.persistentDataPath + "/" + CurrentSaveData.SaveName + ".json");
    }

    public void LoadRooms()
    {
        int adet = 0;
        int length = RoomManager.instance.transform.childCount;
        int length2 = CurrentSaveData.Rooms.Count;
        for (int x = 0; x < length; x++) //Odalarin listesi... 20...
        {
            if (RoomManager.instance.transform.GetChild(x).TryGetComponent(out RoomData currentRoom))
            {
                for (int y = 0; y < length2; y++) //Save datalarimizin listesi... 20
                {
                    RoomSaveData currentRoomSave = CurrentSaveData.Rooms[y];
                    string currentID = currentRoom.availableRoomCell.CellLetter.ToString() + currentRoom.availableRoomCell.CellNumber.ToString();
                    Debug.Log("Current checkID: " + currentID);
                    Debug.Log("currentRoomSave.availableRoomCell: " + currentRoomSave.availableRoomCell);
                    if (currentID == currentRoomSave.availableRoomCell)
                    {
                        currentRoom.isActive = currentRoomSave.isActive;
                        currentRoom.isLock = currentRoomSave.isLock;
                        currentRoom.RequiredMoney = currentRoomSave.RequiredMoney;
                        
                        Debug.Log("kac adet oda bulundu: " + adet);
                        adet++;
                        break;
                    }
                }
            }
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
            UnityAdsManager.instance.adsData = CurrentSaveData.adData;
            //MuseumManager.instance.CurrentActivePictures = CurrentSaveData.CurrentPictures;
            //MuseumManager.instance.InventoryPictures = CurrentSaveData.InventoryPictures;
            MuseumManager.instance.SetSaveData(CurrentSaveData.Gold, CurrentSaveData.Culture, CurrentSaveData.Gem, CurrentSaveData.SkillPoint, CurrentSaveData.CurrentCultureLevel);
        }
        else
        {
            Debug.Log("Save dosyasi yok..");
            Save();
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
    public void LoadPictures(Transform _roomsParent, bool _firstload)
    {
        foreach (var item in CurrentSaveData.CurrentPictures)
        {
            PictureElement pe = _roomsParent.GetChild(item.RoomID).GetChild(item.id).GetComponent<PictureElement>();
            Debug.Log("Before => item textureid: " + item.TextureID + " / pe._pictureData.texture ID: " + pe._pictureData.TextureID);
            pe._pictureData = item;
            Debug.Log("After => item textureid: " + item.TextureID + " / pe._pictureData.texture ID: " + pe._pictureData.TextureID);
            pe.UpdateVisual(_firstload);
        }

        MuseumManager.instance.InventoryPictures = CurrentSaveData.InventoryPictures;
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

    [System.Serializable]
    public class PlayerSaveData
    {
        public string SaveName;

        public float Gold;
        public float Culture;
        public float Gem;
        public float SkillPoint;
        public int CurrentCultureLevel;

        public List<PictureData> CurrentPictures = new List<PictureData>();
        public List<PictureData> InventoryPictures = new List<PictureData>();
        public List<RoomSaveData> Rooms = new List<RoomSaveData>();
        public List<ItemData> PurchasedItems = new List<ItemData>();
        public List<SkillNode> SkillNodes = new List<SkillNode>();

        public AdverstingData adData; //ADS SISTEMI KURULDUKTAN SONRA EKLENECEK.
    }
}
