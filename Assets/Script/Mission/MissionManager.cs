using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    [SerializeField] MissionCollectionHandler collectionHandler;
    List<GameMission> gameMissions = new List<GameMission>();

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
        TimeManager.instance.OnOneMinutePassed -= StartRandomGameMission;
    }
    IEnumerator WaitForTimerManager()
    {
        yield return new WaitUntil(() => TimeManager.instance != null);
        Debug.Log("in WaitForTimerManager method.");
        TimeManager.instance.OnOneMinutePassed += StartRandomGameMission;
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

            NotificationManager.instance.SendNotification(NotificationManager.instance.GetNotificationWithID(100000), new SenderHelper(WhoSends.System, 9999), 2, null, null, null/*,randomMission.Description*/);
            NotificationHandler handler = NotificationManager.instance.SendNotification(NotificationManager.instance.GetNotificationWithID(100001), new SenderHelper(WhoSends.System, 9999), 1, null, new NotificationMissionHandler(100001, () =>
            {
                Debug.Log("Gorev basladi!");
                StartGameMission(randomMission);
            }), randomMission, randomMission.Description);
        }
        
    }
    void StartGameMission(GameMission _gameMission)
    {
        if (_gameMission.missionType == MissionType.Collection)
        {
            UIController.instance.missionUIHandler.MissionUIActivation(_gameMission.missionType);
            CollectionHalper collectionHelper = _gameMission.GetMissionCollection();
            UIController.instance.missionUIHandler.collectionUIHandler.SetDatas(collectionHelper.StartValue, collectionHelper.EndValue, collectionHelper.missionCollectionType);
            collectionHandler.SpawnCollection(_gameMission);
        }
    }
    void InitGameMissions()
    {
        gameMissions.Clear();
        GameMission gm1 = new GameMission(1,100000,100001,"Mücevher Görevi", "10 adet mücevher topla!",300,120,MissionType.Collection, new CollectionHalper(0,10,MissionCollectionType.Gem));
        gm1.SetRewardEvent(() =>
        {
            MuseumManager.instance.AddGem(10);
        });
        gameMissions.Add(gm1);
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
        return gameMissions.Any(x => x.isActive);
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
    CollectionHalper collection;
    event System.Action onMissionReward;
    public GameMission(int iD, int infoNotificationID, int targetNotificationID, string header, string description, float missionComplationTime, float validityPeriodMission, MissionType missionType, CollectionHalper collection = null)
    {
        ID = iD;
        InfoNotificationID = infoNotificationID;
        TargetNotificationID = targetNotificationID;
        Header = header;
        Description = description;
        MissionComplationTime = missionComplationTime;
        ValidityPeriodMission = validityPeriodMission;
        isActive = false;
        this.missionType = missionType;
        this.collection = collection;
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
    public CollectionHalper GetMissionCollection()
    {
        return collection;
    }
}
public class CollectionHalper
{
    public MissionCollectionType missionCollectionType;
    public int StartValue;
    public int EndValue;
    public CollectionHalper(int startValue, int endValue, MissionCollectionType _type)
    {
        StartValue = startValue;
        EndValue = endValue;
        missionCollectionType = _type;
    }
}
public enum MissionType
{
    Collection,
}
public enum MissionCollectionType // Koleksyion gorevlerinin icerik enumu. (MissionCollectionUIHandler.iconSprites kod yolunda ki iconlarin sirasi baz alinmistir)
{
    Gem,
    Gold, 
    Grass,
    None
}