using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        AudioManager.instance.PlayMusicOfMenu();
        UnityAdsManager.instance.Initialize();
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
        CurrentSaveData.CurrentPictures = currentActivePictures;
        CurrentSaveData.InventoryPictures = inventoryPictures;

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
            Save();
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

        public AdverstingData adData;
    }
}
