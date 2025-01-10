using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    [SerializeField] public MissionCollectionHandler collectionHandler;
    List<GameMission> gameMissions = new List<GameMission>();
    public List<GameMission> ActiveMissions = new List<GameMission>();
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
            collectionHandler.SetMission(_gameMission);
            collectionHandler.CollectionProcess();
        }
        else if (_gameMission.missionType == MissionType.NPC)
        {
            // Npc etkilesimi islemleri koleksiyon enumu bunyesinde yapilmalidir
            // Npc ile farkli gorevler icin bura kullanilabilinir.
        }
    }
    void InitGameMissions()
    {
        gameMissions.Clear();
        GameMission gm1 = new GameMission(1, 100000, 100001, "Mücevher Görevi", "10 adet mücevher topla!", 210, 120, MissionType.Collection, new CollectionHelper(0, 10, MissionCollectionType.Gem));
        gm1.SetRewardEvent(() =>
        {
            NotificationManager.instance.SendNotification(NotificationManager.instance.GetNotificationWithID(6), new SenderHelper(WhoSends.System, 9999), 2);
            NotificationManager.instance.SendNotification(NotificationManager.instance.GetNotificationWithID(9999), new SenderHelper(WhoSends.System, 9999), 1, new NotificationRewardHandler(9999, () =>
            {
                MuseumManager.instance.AddGem(50);
            }), null, null, "+50 Gem kazandýnýz!");
        });
        GameMission gm2 = new GameMission(2, 100000, 100002, "Gold Görevi", "8 adet altýn topla!", 210, 60, MissionType.Collection, new CollectionHelper(0, 8, MissionCollectionType.Gold));
        gm2.SetRewardEvent(() =>
        {
            NotificationManager.instance.SendNotification(NotificationManager.instance.GetNotificationWithID(6), new SenderHelper(WhoSends.System, 9999), 2);
            NotificationManager.instance.SendNotification(NotificationManager.instance.GetNotificationWithID(10001), new SenderHelper(WhoSends.System, 9999), 1, new NotificationRewardHandler(10001, () =>
            {
                MuseumManager.instance.AddGold(1000);
            }), null, null, "+1000 Gold kazandýnýz!");
        });

        gameMissions.Add(gm1);
        gameMissions.Add(gm2);
#if UNITY_EDITOR
        for (int i = 0; i < gameMissions.Count; i++)
        {
            gameMissions[i].MissionComplationTime = 60;
        }
#endif        
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
public class CollectionHelper
{
    public MissionCollectionType missionCollectionType;
    public int StartValue;
    public int EndValue;
    public CollectionHelper(int startValue, int endValue, MissionCollectionType _type)
    {
        StartValue = startValue;
        EndValue = endValue;
        missionCollectionType = _type;
    }
    public CollectionHelper(CollectionHelper _copy) 
    {
        missionCollectionType = _copy.missionCollectionType;
        StartValue = _copy.StartValue;
        EndValue = _copy.EndValue;
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
}
public enum MissionType
{
    Collection,
    NPC
}
public enum MissionCollectionType // Koleksyion gorevlerinin icerik enumu. (MissionCollectionUIHandler.iconSprites kod yolunda ki iconlarin sirasi baz alinmistir)
{
    Gem,
    Gold, 
    Grass,
    NpcInteraction,
    None
}