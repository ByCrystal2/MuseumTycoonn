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

    private void OnApplicationQuit()
    {
        Save();
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

        CurrentSaveData.CurrentPictures = currentActivePictures;
        CurrentSaveData.InventoryPictures = inventoryPictures;

        string jsonString = JsonUtility.ToJson(CurrentSaveData);
        File.WriteAllText(Application.persistentDataPath + "/" + CurrentSaveData.SaveName + ".json", jsonString); // this will write the json to the specified path

        Debug.Log("Game Save Location: " + Application.persistentDataPath + "/" + CurrentSaveData.SaveName + ".json");
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
            MuseumManager.instance.SetSaveData(CurrentSaveData.Gold, CurrentSaveData.Culture, CurrentSaveData.Gem, CurrentSaveData.SkillPoint, CurrentSaveData.CurrentCultureLevel);
        }
        else
        {
            Save();
        }
    }

    public void LoadPictures(Transform _roomsParent)
    {
        foreach (var item in CurrentSaveData.CurrentPictures)
        {
            PictureElement pe = _roomsParent.GetChild(item.RoomID).GetChild(item.id).GetComponent<PictureElement>();
            pe.UpdateVisual();
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
    }
}
