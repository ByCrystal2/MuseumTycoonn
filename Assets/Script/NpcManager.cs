using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
    [SerializeField] private List<DialogReadyNpc> DialogReadyNPCs = new List<DialogReadyNpc>();
    [SerializeField] private List<NPCBehaviour> GuiltyNpcs = new List<NPCBehaviour>();
    [SerializeField] private List<NPCBehaviour> TargetGuiltyNpcs = new List<NPCBehaviour>();

    //ForTutorial
    [SerializeField] private Transform museumDoor1;
    [SerializeField] private Transform museumDoor2;

    public static string AnimResetParam = "Reset";
    public static string AnimMoveParam = "Walk";
    public static string AnimLookParam = "Look";
    public static string AnimDialogParam = "Dialog";
    public static string AnimLikedParam = "Liked";
    public static string AnimNoLikedParam = "NoLiked";
    public static string AnimMakeMessParam = "MakeMess";

    public Transform TPSCamera;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);

        GameManager.instance._rewardManager = FindObjectOfType<RewardManager>();
        AudioManager.instance.buttonSoundHandlers = FindObjectsOfType<ButtonSoundHandler>().ToList();
        FirebaseAuthManager.instance.ForFireBaseLoading();
        AwakeLoadingProcesses();
    }
    public bool databaseProcessComplated = false;
    public async void AwakeLoadingProcesses()
    {
        Debug.Log("Database load test 1 complated.");
        databaseProcessComplated = false;
        LoadingScene.ComplateLoadingStep();
        if (GameManager.instance.IsFirstGame)
        {
            GameManager.instance.ActiveRoomsRequiredMoney = 1000;
            GameManager.instance.BaseWorkerHiringPrice = 500;

            await FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID);
        }
        LoadingScene.ComplateLoadingStep();
        Debug.Log("Database load test 2 complated.");
        UIController.instance.roomUISPanelController.InitializeRoomUIS();
        RoomManager.instance.AddRooms(); // in app baglantisi kurulmadan once odalar yuklendi.        
        //GameManager.instance.LoadInventoryPictures();
        //GameManager.instance.LoadStatues();
        await GameManager.instance.LoadCustomizationData();        
        
        Debug.Log("Database load test 3 complated.");
        GameManager.instance.LoadWorkers();
        LoadingScene.ComplateLoadingStep();
        Debug.Log("Database load test 4 complated.");
        //Gaming Services Activation

        BuyingConsumables.instance.InitializePurchasing();
        Debug.Log("Database load test 5 complated.");
        LoadingScene.ComplateLoadingStep();
        //Start Method
        AudioManager.instance.PlayMusicOfGame();
        try
        {
            int length = UIController.instance.skillsContents.Count;
            for (int i = 0; i < length; i++)
            {
                Transform skillsContentTransform = UIController.instance.skillsContents[i];
                int length1 = skillsContentTransform.childCount;
                for (int k = 0; k < length1; k++)
                {
                    if (skillsContentTransform.GetChild(k).TryGetComponent(out BaseSkillOptions component))
                    {
                        SkillTreeManager.instance.AddSkillObject(skillsContentTransform.GetChild(k).gameObject);

                    }
                }
            }
        }
        catch (System.Exception _ex)
        {
            Debug.LogError("Skill Contents for processes are caugth an error! => " + _ex.Message);
        }
        Debug.Log("Database load test 6 complated.");
        LoadingScene.ComplateLoadingStep();

        ItemData firstTableForPlayer = new ItemData(99999, "Vincent van Gogh", "Hediye Tablo", 1, 0, null, ItemType.Table, ShoppingType.Gold, 1, 3);
        EditObjData objDataForTutorial = new EditObjData(9999, "Statue1", 1000, (Texture2D)RoomManager.instance.statuesHandler.statueResults[0], EditObjType.Statue, RoomManager.instance.statuesHandler.bonuss, 0, 0);
        RoomManager.instance.statuesHandler.editObjs[0] = objDataForTutorial; //en son burda kalmistin...

        //TRANSLATE PROCESSES
        if (LanguageDatabase.instance.TranslationWillBeProcessed)
        {            
            GameManager.instance.TranslateAllItems();
            GameManager.instance.TranslateAllSkills();
            GameManager.instance.TranslateCommendsEvulations();
            GameManager.instance.TranslateSkillInfos();
            GameManager.instance.TranslatePictureInfos();
            GameManager.instance.TranslateShopQuestionInfos();
            GameManager.instance.TranslateNotificationMessages();
            GameManager.instance.TranslateCustomizationStrings();
        }
        //TRANSLATE PROCESSES
        GameManager.instance.LoadSkills();
        //GameManager.instance.LoadLastDailyRewardTime();
        Debug.Log("Database load test 7 complated.");
        LoadingScene.ComplateLoadingStep();
        if (GameManager.instance.IsFirstGame)
        {
            Debug.Log("GameManager IsFirstGame True. And First Game Process Starting...");

            MuseumManager.instance.lastDailyRewardTime = TimeManager.instance.CurrentDateTime;

            StartCoroutine(FirstGameController());

            ItemData firstTableForPlayerSub = ItemManager.instance.AllItems.Where(x => x.ID == firstTableForPlayer.ID).SingleOrDefault();
            PictureData newInventoryItem = new PictureData();
            newInventoryItem.TextureID = firstTableForPlayerSub.textureID;
            newInventoryItem.RequiredGold = GameManager.instance.PictureChangeRequiredAmount;
            newInventoryItem.painterData = new PainterData(firstTableForPlayerSub.ID, firstTableForPlayerSub.Description, firstTableForPlayerSub.Name, firstTableForPlayerSub.StarCount);
            MuseumManager.instance.AddNewItemToInventory(newInventoryItem);

            //RoomManager.instance.statuesHandler.editObjs.Add(null);

            //for (int i = RoomManager.instance.statuesHandler.editObjs.Count - 1; i > 0; i--)
            //{
            //    RoomManager.instance.statuesHandler.editObjs[i] = RoomManager.instance.statuesHandler.editObjs[i - 1];
            //}

             //Tutorial
            

            int index = TimeManager.instance.WhatDay;
            // Eðer bulunduysa
            if (index != -1)
            {
                // Orijinal listedeki öðeyi al
                var originalItem = ItemManager.instance.CurrentDailyRewardItems[index];

                // Orijinal öðenin bir kopyasýný oluþtur
                var updatedItem = originalItem;

                // Kopyanýn üzerinde deðiþiklik yap
                updatedItem.IsLocked = false;

                // Kopyayý orijinal listeye geri yerleþtir
                ItemManager.instance.CurrentDailyRewardItems[index] = updatedItem;
            }
            //GameManager.instance._rewardManager.CheckRewards(true);
            GPGamesManager.instance.achievementController.FirstGameAchievement();
        }
        Debug.Log("Database load test 8 complated.");
        LoadingScene.ComplateLoadingStep();
        
        LoadingScene.ComplateLoadingStep();
        Debug.Log("Database load test 9 complated.");
        UIController.instance.SetUpdateWeeklyRewards();
        Debug.Log("Database load test 10 complated.");
        await GameManager.instance.LoadRemoveAds();
        LoadingScene.ComplateLoadingStep();
        Debug.Log("Database load test 11 complated.");
        await GameManager.instance.LoadRooms();
        LoadingScene.ComplateLoadingStep();
        Debug.Log("Database load test 12 complated.");
        RoomManager.instance.statuesHandler.ActiveStatuesControl();
        //Start Method
        databaseProcessComplated = true;
        TimeManager.instance.OnOneMinutePassed += OnGettingSalary;
        Debug.Log("Database Loading Process Complated.");
    }
    public void MuseumDoorsProcess(bool _isOpen)
    {
        if (_isOpen)
        {
            museumDoor1.GetComponent<DoorBehaviour>().DoorProcess(-105, 2f);
            museumDoor2.GetComponent<DoorBehaviour>().DoorProcess(105, 2f);
        }
        else
        {
            museumDoor1.GetComponent<DoorBehaviour>().DoorProcess(0, 2f);
            museumDoor2.GetComponent<DoorBehaviour>().DoorProcess(0, 2f);
        }
    }

    private void Start()
    {
        PlayerManager.instance.LockPlayer();
    }
    private void OnDisable()
    {
        TimeManager.instance.OnOneMinutePassed -= OnGettingSalary;
    }
    public void SetNpcPairsInDialog(NPCBehaviour npc1, NPCBehaviour npc2)
    {

    }

    IEnumerator FirstGameController()
    {
        yield return new WaitForSeconds(2);
        if (GameManager.instance.IsFirstGame)
        {
            UIController.instance.goldStackHandler.AddTempGold(2500);
            MuseumManager.instance.AddGold(2500);
        }
    }
    public void OnGettingSalary() // NPCs salary
    {
        if (!GameManager.instance.IsWatchTutorial) return;
        List<WorkerBehaviour> priority = new();
        List<WorkerBehaviour> main = MuseumManager.instance.CurrentActiveWorkers;
        foreach (var item in main)
        {
            if (item.NotPaidCounter == 2)
                priority.Insert(0, item);
            if (item.NotPaidCounter == 1)
                priority.Add(item);
        }

        foreach (var item in main)
            if(!priority.Contains(item))
                priority.Add(item);

        string debug = "Salary Report for npcs;\n";
        foreach (var item in priority)
        {
            int id = item.ID;            
            float currentSalary = (item.MyDatas.baseSalary * ((item.StarRank + 1) / 0.5f) * (item.NotPaidCounter > 0 ? item.NotPaidCounter : 1)); //Eger 2 sefer maasini alamamissa (2 + 1 = 3) su anki maasida dahil 3 maas odenmeli. Yani geriye donuk alinmamis maaslari silmiyoruz. SIlmek istersen ''* (item.NotPaidCounter + 1)'' bu kismi silebilirsin;
#if UNITY_EDITOR
            currentSalary *= 10;
#endif
            float currentGold = MuseumManager.instance.GetCurrentGold();

            bool success = MuseumManager.instance.SpendingGold(currentSalary);
            if (success)
                item.SetSalaryAsPaid();
            else
                item.SetSalaryAsNotPaid();

            string debugAdd = "WorkerID: <color=cyan>" + id + "</color> - Salary: <color=blue>" + currentSalary + "</color> / Paid: " + (success ? "<color=green>Paid</color>" : "<color=red>Could not PAID!</color>");
            debug += debugAdd + "\n";
        }
        debug += "<color=magenta>End of Report!</color>";
        Debug.Log(debug);
    }

    public int GetTotalMessCount()
    {
        return NPCMessParent.childCount;
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
    public int GetMessCount()
    {
        return NPCMessParent.GetChild(0).childCount;
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
                    MyMessedRooms.Add(myRooms[b]);

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

    public void AddDialogReadyNpc(Transform _target, NPCBehaviour _npc)
    {
        bool contains = false;
        foreach (var item in DialogReadyNPCs)
        {
            if (item.NPC == _npc)
            {
                contains = true;
                break;
            }
        }

        if (!contains)
            DialogReadyNPCs.Add(new() { Target = _target, NPC = _npc });
    }

    public void RemoveDialogReadyNPC(NPCBehaviour _npc)
    {
        int length = DialogReadyNPCs.Count;
        for (int i = length - 1; i >= 0; i--)
            if (DialogReadyNPCs[i].NPC == _npc)
                DialogReadyNPCs.RemoveAt(i);
    }

    public NPCBehaviour FindDialogPartner(NPCBehaviour _npc)
    {
        NPCBehaviour partner = null;
        int length = DialogReadyNPCs.Count;
        int myIndex = -1;
        int partnerIndex = -1;
        for (int i = 0; i < length; i++)
        {
            if (DialogReadyNPCs[i].NPC == _npc)
            {
                myIndex = i;
                break;
            }
        }

        for (int i = 0; i < length; i++)
        {
            if (DialogReadyNPCs[myIndex].Target == DialogReadyNPCs[i].Target && DialogReadyNPCs[i].NPC != DialogReadyNPCs[myIndex].NPC)
            {
                partnerIndex = i;
                partner = DialogReadyNPCs[i].NPC;
                break;
            }
        }

        if (partner != null)
        {
            Debug.Log(DialogReadyNPCs[myIndex].NPC.name + " ve " + DialogReadyNPCs[partnerIndex].NPC.name + " dialog baslattilar.");
            DialogReadyNPCs[myIndex].NPC.SetDialogTarget(DialogReadyNPCs[partnerIndex].NPC);
            DialogReadyNPCs[partnerIndex].NPC.SetDialogTarget(DialogReadyNPCs[myIndex].NPC);

            DialogReadyNPCs[myIndex].NPC.SetNPCState(NPCState.Dialog, true);
            DialogReadyNPCs[partnerIndex].NPC.SetNPCState(NPCState.Dialog, true);

            if (myIndex != partnerIndex)
            {
                if (myIndex > partnerIndex)
                {
                    DialogReadyNPCs.RemoveAt(myIndex);
                    DialogReadyNPCs.RemoveAt(partnerIndex);
                }
                else
                {
                    DialogReadyNPCs.RemoveAt(partnerIndex);
                    DialogReadyNPCs.RemoveAt(myIndex);
                }
            }
            else
            {
                DialogReadyNPCs.RemoveAt(myIndex);
            }
        }

        return partner;
    }

    public NPCBehaviour GetNearestGuiltyNPC(Vector3 _currentSecurityLocation, List<int> _myRooms)
    {
        List<RoomData> myRooms = RoomManager.instance.GetRoomWithIDs(_myRooms);

        List<NPCBehaviour> firstPrioritNPCs = new List<NPCBehaviour>();
        foreach (var npc in GuiltyNpcs)
            foreach (var room in myRooms)
                if (npc.GetNpcCurrentRoom().ID == room.ID)
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
                    if (GuiltyNpcs[i].GetNpcCurrentRoom().ID == item.ID)
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

#if UNITY_EDITOR
    [Header("Set Component Shortcut")]
    [SerializeField] private bool SetNPCsRequiredComponents;
    [SerializeField] private bool SetLocationDataIDs;

    private void OnDrawGizmos()
    {
        if (SetNPCsRequiredComponents)
        {
            SetNPCsRequiredComponents = false;
            NPCBehaviour[] npcBehaviours = FindObjectsByType<NPCBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
            foreach (var item in npcBehaviours)
            {
                item.SetComponents();
            EditorUtility.SetDirty(item);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Listedeki tum npclerin componentleri guncellendi.");
        }
        if (SetLocationDataIDs)
        {
            SetLocationDataIDs = false;
            LocationData[] locationDatas = FindObjectsByType<LocationData>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
            int id = 1001;
            foreach (var item in locationDatas)
            {
                if (item.transform.parent.TryGetComponent(out PictureElement PD))
                {
                    item.id = PD._pictureData.id;
                }
                else
                {
                    item.id = id;
                    id++;
                }
                EditorUtility.SetDirty(item);
            }
            AssetDatabase.SaveAssets();
        }
    }
#endif

    public static float EscapeSpeedMultiplier = 2.5f;

    public static List<float> delaysPerState = new List<float>() 
    {
        2, //Idle
        0.5f, //Move
        5, //Investigate
        5, //DialogFree
        10, //Dialog
        6, //Farewell
        3, //CombatBeat
        15, //Combatbeaten
        10, //VictoryDance
    };
}

[System.Serializable]
public struct DialogReadyNpc
{
    public Transform Target;
    public NPCBehaviour NPC;
}

[System.Serializable]
public struct NPCGeneralData
{
    public string NpcName;
    public bool isMale; 
}

[System.Serializable]
public struct NPCStatData
{
    public float NpcSpeed;
    public float NpcCurrentSpeed;
    public float NpcRotationSpeed;

    public List<MyColors> LikedColors;
    public MyColors DislikedColor;
    public List<string> LikedArtist;
    public float Happiness;
    public float HappinessBuff;

    public float Stress;
    public float StressBuff;

    public float toilet;
    public float education;
    public float educationBuff;
    public float additionalLuck;
}

[System.Serializable]
public struct NPCStateData
{
    public NPCState _mainState;
    public NpcLocationState _location;
}

public enum NpcEmotionEffect
{
    Happiness,
    Sadness,
}

public enum WalkEnum
{
    Idle,
    NormalWalk,
    HappyWalk,
    SadWalk,
    DrunkWalk,
    QueenWalk,
    KingWalk,
}

public enum NpcLocationState
{
    Outside,
    EnterWay,
    Inside,
}

public enum NPCState
{
    Idle,
    Move,
    Investigate,
    DialogFree,
    Dialog,
    Farewell,
    CombatBeat,
    CombatBeaten,
    VictoryDance,
}