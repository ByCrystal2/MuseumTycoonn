using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Unity.VisualScripting;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    [SerializeField] public MissionCollectionHandler collectionHandler;
    List<GameMission> gameMissions = new List<GameMission>();
    public List<GameMission> ActiveMissions = new List<GameMission>();
    public List<LanguageData> missionLanguageDatas = new List<LanguageData>();
    public static MissionManager instance { get; private set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        InitGameMissions();
    }
    private void OnEnable()
    {
        StartCoroutine(WaitForTimerManager());
        
    }
    private void OnDisable()
    {
        TimeManager.instance.OnFiveMinutePassed -= StartRandomGameMission;
    }
    IEnumerator WaitForTimerManager()
    {
        yield return new WaitUntil(() => TimeManager.instance != null);
        Debug.Log("in WaitForTimerManager method.");
        TimeManager.instance.OnFiveMinutePassed += StartRandomGameMission;
    }
    private void StartRandomGameMission()
    {
        if (NpcManager.instance != null && NpcManager.instance.databaseProcessComplated)
        {
            Debug.Log("In StartRandomGameMission method.");
            GameMission randomMission = gameMissions[Random.Range(0, gameMissions.Count)];
            bool anyHasCurrentMission = NotificationManager.instance.notificationMissionHandlers.Any(x => x.TargetNotificationID == randomMission.TargetNotificationID);
            if (anyHasCurrentMission)
            {
                Debug.LogWarning("Boyle bir gorev zaten gonderilmis. Mevcut gorev bitene kadar ayni turde gorev gelemez!");
                return;
            }
            randomMission.GetMissionCollection().missionRequirements = randomMission.GetMissionCollection().CreateRequirement();
            randomMission.UpdateDesc();
            NotificationManager.instance.SendNotification(NotificationManager.instance.GetNotificationWithID(100000), new SenderHelper(WhoSends.System, 9999), 2, null, null, null/*,randomMission.Description*/);
            NotificationHandler handler = NotificationManager.instance.SendNotification(NotificationManager.instance.GetNotificationWithID(randomMission.TargetNotificationID), new SenderHelper(WhoSends.System, 9999), 1, null, new NotificationMissionHandler(randomMission.TargetNotificationID, () =>
            {
                Debug.Log("Gorev basladi!");
                GameMission flag = new GameMission(randomMission);
                AddMission(flag);
                StartGameMission(flag);
            }), randomMission, randomMission.Description);
        }
        
    }
    public void ComplateActiveMissionWithID(int _id)
    {
        GameMission targetMission = ActiveMissions.Where(x=> x.ID == _id).SingleOrDefault();
        if (targetMission == null) { Debug.LogError("Gonderilen gorev, aktif gorevlerde bulunmamaktadir!"); return; }
        targetMission.TriggerReward();
        if (targetMission.GetMissionCollection() != null && targetMission.GetMissionCollection().missionCollectionType == MissionCollectionType.NpcInteraction)
        {
            SpawnHandler.instance.OnEndMission();
        }
        Debug.Log(targetMission.ID + " ID'li gorev tamamlandi!");
        RemoveMissionWithId(targetMission.ID);
    }
    public void MissionTimeEnding(int _id)
    {
        GameMission targetMission = ActiveMissions.Where(x => x.ID == _id).SingleOrDefault();
        if (targetMission == null) { Debug.LogError("Suresi biten gorev, aktif gorevlerde bulunmamaktadir!"); return; }
        Debug.Log(targetMission.ID + " ID'li gorev tamamlanamadan suresi bitti!");
        RemoveMissionWithId(targetMission.ID);
        NotificationManager.instance.SendNotification(NotificationManager.instance.GetNotificationWithID(7), new SenderHelper(WhoSends.System, 9999), 2);
    }
    void AddMission(GameMission _newMission)
    {
        ActiveMissions.Add(_newMission);
    }
    public void RemoveMissionWithId(int _missionID)
    {
        GameMission currentMission = ActiveMissions.Where(x=> x.ID == _missionID).SingleOrDefault();
        if (currentMission == null) { Debug.LogError("Gonderilen gorev, aktif gorevlerde bulunmamaktadir!"); return; }
        ActiveMissions.Remove(currentMission);
    }
    void StartGameMission(GameMission _gameMission)
    {
        if (_gameMission.missionType == MissionType.Collection)
        {
            UIController.instance.missionUIHandler.MissionUIActivation(_gameMission.missionType,true);
            CollectionHelper collectionHelper = _gameMission.GetMissionCollection();
            UIController.instance.missionUIHandler.collectionUIHandler.SetDatas(collectionHelper);
            UIController.instance.missionUIHandler.collectionUIHandler.StartMissionLifeTime(_gameMission.MissionComplationTime);
            UIController.instance.missionUIHandler.collectionUIHandler.infoPanelController.SetInfoText(_gameMission.Description);
            collectionHandler.SetMission(_gameMission);
            collectionHandler.CollectionProcess();
        }
        else if (_gameMission.missionType == MissionType.NPC)
        {
            // Npc etkilesimi islemleri koleksiyon enumu bunyesinde yapilmalidir
            // Npc ile farkli gorevler icin bura kullanilabilinir.
        }
    }
    public readonly string npcInteractionDefaultString = "{%now}/{%tot} {%t}. {%loc1}: {%cl} {%loc2} {%st}";
    void InitGameMissions()
    {
        gameMissions.Clear();
        GameMission gm1 = new GameMission(1, 100000, 100001, "Jewelry Mission", "Collect 10 gems!", 210, 120, MissionType.Collection, new CollectionHelper(0, 10, MissionCollectionType.Gem));
        gm1.SetRewardEvent(() =>
        {
            NotificationManager.instance.SendNotification(NotificationManager.instance.GetNotificationWithID(6), new SenderHelper(WhoSends.System, 9999), 2);
            NotificationManager.instance.SendNotification(NotificationManager.instance.GetNotificationWithID(10001), new SenderHelper(WhoSends.System, 9999), 1, new NotificationRewardHandler(10001, () =>
            {
                MuseumManager.instance.AddGem(20);
            }), null, null, LanguageDatabase.instance.Language.NotificationRewardMessages.Where(x=> x.TargetID == (gm1.ID + 10000)).SingleOrDefault().ActiveLanguage);
        });

        GameMission gm2 = new GameMission(2, 100000, 100002, "Gold Görevi", "8 adet altýn topla!", 210, 60, MissionType.Collection, new CollectionHelper(0, 8, MissionCollectionType.Gold));
        gm2.SetRewardEvent(() =>
        {
            NotificationManager.instance.SendNotification(NotificationManager.instance.GetNotificationWithID(6), new SenderHelper(WhoSends.System, 9999), 2);
            NotificationManager.instance.SendNotification(NotificationManager.instance.GetNotificationWithID(10002), new SenderHelper(WhoSends.System, 9999), 1, new NotificationRewardHandler(10002, () =>
            {
                MuseumManager.instance.AddGold(2500);
            }), null, null, LanguageDatabase.instance.Language.NotificationRewardMessages.Where(x => x.TargetID == (gm2.ID + 10000)).SingleOrDefault().ActiveLanguage);
        });

        GameMission gm3 = new GameMission(3, 100000, 100003, "Interact With Visitors", npcInteractionDefaultString, 300, 60, MissionType.Collection, new CollectionHelper(0, 5, MissionCollectionType.NpcInteraction));
        gm3.SetRewardEvent(() =>
        {
            NotificationManager.instance.SendNotification(NotificationManager.instance.GetNotificationWithID(6), new SenderHelper(WhoSends.System, 9999), 2);
            NotificationManager.instance.SendNotification(NotificationManager.instance.GetNotificationWithID(10003), new SenderHelper(WhoSends.System, 9999), 1, new NotificationRewardHandler(10003, () =>
            {
                MuseumManager.instance.AddGold(5000);
            }), null, null, LanguageDatabase.instance.Language.NotificationRewardMessages.Where(x => x.TargetID == (gm3.ID + 10000)).SingleOrDefault().ActiveLanguage);
        });
        gameMissions.Add(gm1);
        gameMissions.Add(gm2); // bu yorum satirlari testlerden sonra acilacak!
        gameMissions.Add(gm3);
#if UNITY_EDITOR
        for (int i = 0; i < gameMissions.Count; i++)
        {
            //gameMissions[i].MissionComplationTime = 60; //istenildigi zaman acilabilir. sisteme bir zarar vermez.
        }
#endif        
    }
    public List<GameMission> GetAllMissionDatas()
    {
        return gameMissions;
    }
    public GameMission GetMissionWithInfoId(int id)
    {
        return gameMissions.Where(x=> x.InfoNotificationID == id).SingleOrDefault();
    }
    public GameMission GetMissionWithTargetId(int id)
    {
        return gameMissions.Where(x => x.TargetNotificationID == id).SingleOrDefault();
    }
    public GameMission GetMissionWithId(int id)
    {
        return gameMissions.Where(x => x.ID == id).SingleOrDefault();
    }
    public bool AnyActiveMission()
    {
        return ActiveMissions.Any(x => x.isActive);
    }
}
[System.Serializable]
public class GameMission
{
    public int ID;
    public int InfoNotificationID;
    public int TargetNotificationID;
    public string Header;
    public string Description;
    public float MissionComplationTime;
    public float ValidityPeriodMission;
    public bool isActive;
    public MissionType missionType;
    CollectionHelper collection;
    event System.Action onMissionReward;
    public GameMission(int iD, int infoNotificationID, int targetNotificationID, string header, string description, float missionComplationTime, float validityPeriodMission, MissionType missionType, CollectionHelper collection = null)
    {
        LanguageData targetHeaderLanguageData = LanguageDatabase.instance.Language.MissionHeaderMessages.Where(x=> x.TargetID  == iD).SingleOrDefault();
        LanguageData targetDescLanguageData = null;
        if (collection.missionCollectionType != MissionCollectionType.NpcInteraction)
            targetDescLanguageData = LanguageDatabase.instance.Language.MissionDescriptionMessages.Where(x=> x.TargetID  == iD).SingleOrDefault();
        else
            targetDescLanguageData = LanguageDatabase.instance.Language.MissionDescriptionMessages.Where(x => x.TargetID == 999).SingleOrDefault();

        ID = iD;
        InfoNotificationID = infoNotificationID;
        TargetNotificationID = targetNotificationID;
        if (targetHeaderLanguageData != null)
            Header = targetHeaderLanguageData.ActiveLanguage;
        else
            Header = header;

        if (targetDescLanguageData != null)
            Description = targetDescLanguageData.ActiveLanguage;
        else { Description = description; }
        MissionComplationTime = missionComplationTime;
        ValidityPeriodMission = validityPeriodMission;
        isActive = false;
        this.missionType = missionType;
        this.collection = collection;
        UpdateDesc();
    }
    public void UpdateDesc()
    {
        if (collection.missionCollectionType != MissionCollectionType.NpcInteraction) return;
        this.Description = MissionManager.instance.npcInteractionDefaultString;

        LanguageData targetHeaderLanguageData = LanguageDatabase.instance.Language.MissionHeaderMessages.Where(x => x.TargetID == ID).SingleOrDefault();
        LanguageData targetDescLanguageData = LanguageDatabase.instance.Language.MissionDescriptionMessages.Where(x => x.TargetID == 999).SingleOrDefault();

        if (targetHeaderLanguageData != null)
            Header = targetHeaderLanguageData.ActiveLanguage;

        if (targetDescLanguageData != null)
            Description = Description.Replace("{%loc1}", targetDescLanguageData.ActiveLanguage);

        LanguageData targetNpcInteractionColorLanguageData = LanguageDatabase.instance.Language.MissionNpcInteractionColorMessages.Where(x => x.TargetID == (int)collection.missionRequirements.targetColor).SingleOrDefault();
        LanguageData targetNpcInteractionStateLanguageData = LanguageDatabase.instance.Language.MissionNpcInteractionStateMessages.Where(x => x.TargetID == (int)collection.missionRequirements.targetState).SingleOrDefault();
        LanguageData targetNpcInteractionHelperLanguageData = LanguageDatabase.instance.Language.MissionNpcInteractionHelperMessages.Where(x => x.TargetID == 0).SingleOrDefault();
        LanguageData targetNpcInteractionTypeLanguageData = LanguageDatabase.instance.Language.MissionNpcInteractionTypeMessages.Where(x => x.TargetID == (int)collection.missionRequirements.targetType).SingleOrDefault();
        if (collection.missionCollectionType == MissionCollectionType.NpcInteraction)
            if (LanguageDatabase.instance.TranslationWillBeProcessed)
            {
                string newDesc = new string(Description);
                newDesc = newDesc.Replace("{%now}/{%tot}", collection.StartValue + "/" + collection.EndValue);

                if (targetNpcInteractionTypeLanguageData != null)
                    newDesc = newDesc.Replace("{%t}", targetNpcInteractionTypeLanguageData.ActiveLanguage);

                if (collection.missionRequirements.missionColorType != MissionColorType.None)
                    newDesc = newDesc.Replace("{%cl}", targetNpcInteractionColorLanguageData.ActiveLanguage);
                else
                    newDesc = newDesc.Replace("{%cl}", "");

                if (collection.missionRequirements.targetState != MissionTargetState.None)
                    newDesc = newDesc.Replace("{%st}", targetNpcInteractionStateLanguageData.ActiveLanguage);
                else
                    newDesc = newDesc.Replace("{%st}", "");

                if (collection.missionRequirements.missionColorType != MissionColorType.None && collection.missionRequirements.targetState != MissionTargetState.None)
                    newDesc = newDesc.Replace("{%loc2}", targetNpcInteractionHelperLanguageData.ActiveLanguage);
                else
                    newDesc = newDesc.Replace("{%loc2}", "");
                Debug.Log("Game mission (int)collection.missionRequirements.targetColor: " + collection.missionRequirements.targetColor);
                Debug.Log("Game mission (int)collection.missionRequirements.targetState: " + collection.missionRequirements.targetState);
                Debug.Log("Game mission (int)collection.missionRequirements.targetType: " + collection.missionRequirements.targetType);
                Debug.Log("Game mission newDesc: " + newDesc);

                Description = newDesc;
            }
    }
    public GameMission(GameMission _copy)
    {
        ID = _copy.ID;
        InfoNotificationID = _copy.InfoNotificationID;
        TargetNotificationID = _copy.TargetNotificationID;
        Header = _copy.Header;
        Description = _copy.Description;
        MissionComplationTime= _copy.MissionComplationTime;
        ValidityPeriodMission= _copy.ValidityPeriodMission;
        isActive = _copy.isActive;
        missionType = _copy.missionType;
        collection = new CollectionHelper(_copy.collection);
        onMissionReward = new System.Action(_copy.onMissionReward);
    }

    public void SetRewardEvent(System.Action @event)
    {
        if (onMissionReward != null)
        {
            Debug.LogError("Bu goreve zaten bir odul event'i eklenmis!");
            return;
        }
        onMissionReward += @event;
    }
    public void TriggerReward()
    {
        isActive = false;
        onMissionReward?.Invoke();
    }
    public CollectionHelper GetMissionCollection()
    {
        return collection;
    }
}

[System.Serializable]
public struct MissionRequirements
{
    public MissionColorType missionColorType;
    public MyColors targetColor; //Target Color for the quest
    public MissionTargetState targetState;
    public MissionTargetType targetType; //Incele ya da Dov.
}

[System.Serializable]
public class CollectionHelper
{
    public MissionRequirements missionRequirements;
    public MissionCollectionType missionCollectionType;
    public int StartValue;
    public int EndValue;
    public CollectionHelper(int startValue, int endValue, MissionCollectionType _type)
    {
        StartValue = startValue;
        EndValue = endValue;
        missionCollectionType = _type;
        if (_type == MissionCollectionType.NpcInteraction)
        missionRequirements = CreateRequirement();
    }
    public CollectionHelper(CollectionHelper _copy) 
    {
        missionCollectionType = _copy.missionCollectionType;
        StartValue = _copy.StartValue;
        EndValue = _copy.EndValue;

        missionRequirements.missionColorType = _copy.missionRequirements.missionColorType;
        missionRequirements.targetColor = _copy.missionRequirements.targetColor;
        missionRequirements.targetState = _copy.missionRequirements.targetState;
        missionRequirements.targetType = _copy.missionRequirements.targetType;
    }
    public bool IsFull()
    {
        if (StartValue >= EndValue)
            return true;
        else
            return false;
    }
    public bool IsSpawnedMission()
    {
        bool result = missionCollectionType == MissionCollectionType.Gem || missionCollectionType == MissionCollectionType.Gold || missionCollectionType == MissionCollectionType.Grass;

        return result;
    }

    public MissionRequirements CreateRequirement()
    {
        MissionRequirements _missionRequirements = new();

        int r = Random.Range(0, 101);
        bool hasColor = false;
        if(r <= 49)
        {
            //Color quest
            //_missionRequirements.missionColorType = Random.Range(0, 101) <= 49 ? MissionColorType.Liked : MissionColorType.Disliked;
            _missionRequirements.missionColorType = MissionColorType.Liked;
            _missionRequirements.targetColor = (MyColors)Random.Range(0, (int)MyColors.Length);
            hasColor = true;
        }
        else
        {
            //No color
            _missionRequirements.missionColorType = MissionColorType.None; //Renk onemsiz.
            _missionRequirements.targetColor = MyColors.Black; //Not important.
        }

        r = Random.Range(0, 101);

        if (!hasColor)
            r = 0;

        if (r <= 49)
        {
            //State Quest
            _missionRequirements.targetState = (MissionTargetState)Random.Range(1, (int)MissionTargetState.Length);
        }
        else
        {
            //No State
            _missionRequirements.targetState = MissionTargetState.None;
        }

        _missionRequirements.targetType = Random.Range(0, 101) <= 49 ? MissionTargetType.Investigate : MissionTargetType.Beat;
        return _missionRequirements;
    }
}

public enum MissionType
{
    Collection,
    NPC
}

public enum MissionColorType
{
    None,
    Liked,
    Disliked,
}

public enum MissionTargetType
{
    Investigate,
    Beat,
}

public enum MissionTargetState
{
    None,
    Happy,
    Sad,
    Stressed,
    Relaxed,
    NeedToilet,
    Length,
}

public enum MissionCollectionType // Koleksyion gorevlerinin icerik enumu. (MissionCollectionUIHandler.iconSprites kod yolunda ki iconlarin sirasi baz alinmistir)
{
    Gem,
    Gold, 
    Grass,
    NpcInteraction,
    None
}