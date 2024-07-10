using System.Collections.Generic;
using UnityEngine;

public class NpcManager : MonoBehaviour
{
    public static NpcManager instance { get; private set; }
    public NPCBehaviour CurrentNPC; // npc information panelde gorunen npc! (uzerine tiklanan npc)
    public List<LocationData> Locations = new List<LocationData>();

    public List<Transform> GidisListe = new List<Transform>();
    public List<Transform> GelisListe = new List<Transform>();
    public List<Transform> HorseCartList = new List<Transform>();

    public Transform Enter1Point, Enter2Point;

    public Transform RoomsParent;
    public float NpcMaxMoveDistance = 90;

    [Header("NPC UI Visual")]
    public Color StartColor;
    public Color EndColor;
    public List<Sprite> StressEmojis;

    [SerializeField] private Transform NPCMessParent;
    [SerializeField] private List<NPCBehaviour> GuiltyNpcs = new List<NPCBehaviour>();
    [SerializeField] private List<NPCBehaviour> TargetGuiltyNpcs = new List<NPCBehaviour>();
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
        GameManager.instance.rewardManager = FindObjectOfType<RewardManager>();

        AwakeLoadingProcesses();
    }
    public async void AwakeLoadingProcesses()
    {
        if (GameManager.instance.IsFirstGame)
        {
            GameManager.instance.ActiveRoomsRequiredMoney = 1000;
            GameManager.instance.BaseWorkerHiringPrice = 500;

            await FirestoreManager.instance.UpdateGameData("ahmet123",true);
        }
        UIController.instance.roomUISPanelController.InitializeRoomUIS();
        RoomManager.instance.AddRooms(); // in app baglantisi kurulmadan once odalar yuklendi.
        GameManager.instance.LoadRooms();
        //GameManager.instance.LoadInventoryPictures();
        GameManager.instance.LoadStatues();
        WorkerManager.instance.BaseAllWorkerOptions();
        GameManager.instance.LoadWorkers();
        WorkerManager.instance.CreateWorkersToMarket();
        GameManager.instance.LoadDailyRewardItems();
        //Gaming Services Activation

        BuyingConsumables.instance.InitializePurchasing();

        //Start Method
        AudioManager.instance.PlayMusicOfGame();
        Transform skillsContentTransform = UIController.instance.skillsContent.transform;
        int length = skillsContentTransform.childCount;
        for (int i = 0; i < length; i++)
        {
            if (skillsContentTransform.GetChild(i).TryGetComponent(out BaseSkillOptions component))
            {
                SkillTreeManager.instance.AddSkillObject(skillsContentTransform.GetChild(i).gameObject);

            }
        }
        GameManager.instance.LoadSkills();
        GameManager.instance.LoadLastDailyRewardTime();

        if (GameManager.instance != null)
        {
            if (!GameManager.instance.IsWatchTutorial)
            {
                DialogueTrigger firstDialog = GameObject.FindWithTag("TutorialNPC").GetComponent<DialogueTrigger>();
                if (firstDialog != null)
                    firstDialog.TriggerDialog(Steps.Step1);
            }
            else
            {
                DialogueManager.instance.SetActivationDialoguePanel(false);
                GameObject.FindWithTag("TutorialNPC").SetActive(false); // Destroyda edilebilirdi fakat lazim olabilir ilerde.
                UIController.instance.tutorialUISPanel.gameObject.SetActive(false);
            }
        }
        ItemData firstTableForPlayer = new ItemData(99999, "Vincent van Gogh", "Hediye Tablo", 1, 0, null, ItemType.Table, ShoppingType.Gold, 1, 3);
        if (GameManager.instance.IsFirstGame)
        {
            Debug.Log("GameManager IsFirstGame True. And First Game Process Starting...");
            ItemManager.instance.SetCalculatedDailyRewardItems();

            GameManager.instance.rewardManager.lastDailyRewardTime = TimeManager.instance.CurrentDateTime;
            MuseumManager.instance.OnNpcPaid(2500);

            PictureData newInventoryItem = new PictureData();
            newInventoryItem.TextureID = firstTableForPlayer.textureID;
            newInventoryItem.RequiredGold = GameManager.instance.PictureChangeRequiredAmount;
            newInventoryItem.painterData = new PainterData(firstTableForPlayer.ID, firstTableForPlayer.Description, firstTableForPlayer.Name, firstTableForPlayer.StarCount);
            MuseumManager.instance.AddNewItemToInventory(newInventoryItem);
            
            int index = TimeManager.instance.WhatDay;
            // E�er bulunduysa
            if (index != -1)
            {
                // Orijinal listedeki ��eyi al
                var originalItem = ItemManager.instance.CurrentDailyRewardItems[index];

                // Orijinal ��enin bir kopyas�n� olu�tur
                var updatedItem = originalItem;

                // Kopyan�n �zerinde de�i�iklik yap
                updatedItem.IsLocked = false;

                // Kopyay� orijinal listeye geri yerle�tir
                ItemManager.instance.CurrentDailyRewardItems[index] = updatedItem;
            }
            //GameManager.instance.rewardManager.CheckRewards(true);
            GPGamesManager.instance.achievementController.FirstGameAchievement();
        }
        else
        {
        }
        UIController.instance.SetUpdateWeeklyRewards();
        GameManager.instance.LoadRemoveAds();
        if (GoogleAdsManager.instance.adsData == null)
            GoogleAdsManager.instance.adsData = new AdverstingData();

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
        //Start Method
    }

    private void Start()
    {
       
    }

    #region Npc Mess
    public void AddMessIntoMessParent(Transform _newMess)
    {
        _newMess.SetParent(NPCMessParent.GetChild(0));
    }

    public void SetMessCleaning(Transform _newMess)
    {
        _newMess.SetParent(NPCMessParent.GetChild(1));
    }

    public void DestroyMess(GameObject _newMess)
    {
        Destroy(_newMess);
    }

    public NpcMess GetNearestMess(Transform _pos,List<int> _myRooms)
    {
        List<RoomData> AllMessedRooms = new List<RoomData>();
        int length = NPCMessParent.GetChild(0).childCount;
        for (int i = 0; i < length; i++)
        {
            NpcMess messData = NPCMessParent.GetChild(0).GetChild(i).GetComponent<NpcMess>();
            if (!AllMessedRooms.Contains(messData.inRoom))
                AllMessedRooms.Add(messData.inRoom);
        }

        List<RoomData> myRooms = RoomManager.instance.GetRoomWithIDs(_myRooms);

        List<RoomData> MyMessedRooms = new List<RoomData>();
        int allMessedCount = AllMessedRooms.Count;
        int myRoomsCount = myRooms.Count;
        for (int a = 0; a < allMessedCount; a++)
            for (int b = 0; b < myRoomsCount; b++)
                if (AllMessedRooms[a] == myRooms[b])
                    MyMessedRooms.Add(myRooms[a]);

        NpcMess NearestMess = null;
        float nearest = 100000;
        int myMessedRoomsCount = MyMessedRooms.Count;
        if (myMessedRoomsCount > 0)
        {
            nearest = 999999;
            for (int i = 0; i < myMessedRoomsCount; i++)
            {
                float currentDistance = Vector3.Distance(_pos.position, MyMessedRooms[i].CenterPoint.position);
                if (currentDistance < nearest)
                {
                    nearest = currentDistance;
                    NearestMess = NPCMessParent.GetChild(0).GetChild(i).GetComponent<NpcMess>();
                }
            }
        }
        else
        {
            List<RoomData> nearRoomsToMyArea = new List<RoomData>();
            foreach (var room in myRooms)
            {
                List<RoomData> neighborRooms = RoomManager.instance.GetDesiredNeighborRooms(room);
                //string x = "My room id: " + room.ID + "\n";
                foreach (var neighborRoom in neighborRooms)
                {
                    //x += "Neighbor room ID => " + neighborRoom.ID + "\n";
                    nearRoomsToMyArea.Add(neighborRoom);
                }   
                //Debug.Log(x);
            }
            int length2 = NPCMessParent.GetChild(0).childCount;
            for (int i = 0; i < length2; i++)
            {
                bool contains = false;
                foreach (var item in nearRoomsToMyArea)
                {
                    if (NPCMessParent.GetChild(0).GetChild(i).GetComponent<NpcMess>().inRoom.ID == item.ID)
                    {
                        contains = true;
                        break;
                    }
                }
                if (!contains)
                    continue;

                float currentDistance = Vector3.Distance(_pos.position, NPCMessParent.GetChild(0).GetChild(i).position);
                if (currentDistance < nearest)
                {
                    nearest = currentDistance;
                    NearestMess = NPCMessParent.GetChild(0).GetChild(i).GetComponent<NpcMess>();
                }
            }
        }
        return NearestMess;
    }
    #endregion

    #region Npc Guilty

    public void AddGuiltyNPC(NPCBehaviour _guiltyNPC)
    {
        if (!GuiltyNpcs.Contains(_guiltyNPC))
            GuiltyNpcs.Add(_guiltyNPC);
    }

    public void RemoveGuiltyNPC(NPCBehaviour _guiltyNPC)
    {
        if (GuiltyNpcs.Contains(_guiltyNPC))
            GuiltyNpcs.Remove(_guiltyNPC);

        if (TargetGuiltyNpcs.Contains(_guiltyNPC))
            TargetGuiltyNpcs.Remove(_guiltyNPC);
    }

    public void SetGuiltyNPCAsTarget(NPCBehaviour _guiltyNPC)
    {
        if (_guiltyNPC != null)
        {
            if (GuiltyNpcs.Contains(_guiltyNPC))
                GuiltyNpcs.Remove(_guiltyNPC);

            if (!TargetGuiltyNpcs.Contains(_guiltyNPC))
                TargetGuiltyNpcs.Add(_guiltyNPC);
        }
    }

    public NPCBehaviour GetNearestGuiltyNPC(Vector3 _currentSecurityLocation, List<int> _myRooms)
    {
        List<RoomData> myRooms = RoomManager.instance.GetRoomWithIDs(_myRooms);

        List<NPCBehaviour> firstPrioritNPCs = new List<NPCBehaviour>();
        foreach (var npc in GuiltyNpcs)
            foreach (var room in myRooms)
                if (npc.CurrentVisitedRoom.ID == room.ID)
                    firstPrioritNPCs.Add(npc);

        NPCBehaviour NearestNPC = null;
        float nearest = 100;
        int myFirstPriorityRoomsCount = firstPrioritNPCs.Count;
        if (myFirstPriorityRoomsCount > 0)
        {
            nearest = 999999;
            for (int i = 0; i < myFirstPriorityRoomsCount; i++)
            {
                float currentDistance = Vector3.Distance(_currentSecurityLocation, firstPrioritNPCs[i].transform.position);
                if (currentDistance < nearest)
                {
                    nearest = currentDistance;
                    NearestNPC = firstPrioritNPCs[i];
                }
            }
        }
        else
        {
            List<RoomData> nearRoomsToMyArea = new List<RoomData>();
            foreach (var room in myRooms)
            {
                List<RoomData> neighborRooms = RoomManager.instance.GetDesiredNeighborRooms(room);
                string x = "My room id: " + room.ID + "\n";
                foreach (var neighborRoom in neighborRooms)
                {
                    x += "Neighbor room ID => " + neighborRoom.ID + "\n";
                    nearRoomsToMyArea.Add(neighborRoom);
                }
                Debug.Log(x);
            }
            int length2 = GuiltyNpcs.Count;
            for (int i = 0; i < length2; i++)
            {
                bool contains = false;
                foreach (var item in nearRoomsToMyArea)
                {
                    if (GuiltyNpcs[i].CurrentVisitedRoom.ID == item.ID)
                    {
                        contains = true;
                        break;
                    }
                }
                if (!contains)
                    continue;

                float currentDistance = Vector3.Distance(_currentSecurityLocation, GuiltyNpcs[i].transform.position);
                if (currentDistance < nearest)
                {
                    nearest = currentDistance;
                    NearestNPC = GuiltyNpcs[i];
                }
            }
        }

        SetGuiltyNPCAsTarget(NearestNPC);
        return NearestNPC;
    }

    #endregion
}

public enum NpcEmotionEffect
{
    Happiness,
    Sadness,
}
