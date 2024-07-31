using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class WorkerManager : MonoBehaviour
{
    [SerializeField] public List<WorkerBehaviour> AllWorkers = new List<WorkerBehaviour>();
    [SerializeField] public List<WorkerBehaviour> WorkersInMarket = new List<WorkerBehaviour>();
    //[SerializeField] public List<WorkerBehaviour> WorkersInInventory = new List<WorkerBehaviour>();
    [SerializeField] public List<WorkerBehaviour> CurrentActiveWorkers = new List<WorkerBehaviour>();

    [SerializeField] public List<WorkerAndTasks> WorkersAndTasks = new List<WorkerAndTasks>();

    [SerializeField] public Transform WorkersContent;
    public static WorkerManager instance { get; set; }

    private List<int> RequiredExpsPerLevel = new List<int>() {
        0, 110, 145, 180, 235, //1Star 5LV
        470, 561, 643, 756, 904, //2Star 10lv
        1808, 2104, 2208, 2315, 2531, //3Star 15lv
        5062, 5433, 5761, 6062, 6437, //4Star 20lv
        12874, 14643, 17001, 19362, 24099, //5Star 25lv
    };

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }


    public int CalculateCurrentLevel(float _myExp)
    {
        int level = 1;
        int length = RequiredExpsPerLevel.Count;
        for (int i = length - 1; i >= 0; i--)
            if (_myExp >= RequiredExpsPerLevel[i])
                level = i + 1;

        return level;
    }

    public bool CanEarnExp(float _myExp, int _rank)
    {
        int level = CalculateCurrentLevel(_myExp);
        bool can = true;
        if (_rank == 1 && level >= 5)
            can = false;
        if (_rank == 2 && level >= 10)
            can = false;
        if (_rank == 3 && level >= 15)
            can = false;
        if (_rank == 4 && level >= 20)
            can = false;

        return can;
    }

    public void CreateWorkersToMarket()
    {
        int length = MuseumManager.instance.WorkersInInventory.Count;
        int length1 = MuseumManager.instance.CurrentActiveWorkers.Count;
        foreach (var worker in AllWorkers)
        {
            bool isInventory = false;
            bool isActiveWorker = false;
            for (int j = 0; j < length; j++)
            {
                if (worker.ID == MuseumManager.instance.WorkersInInventory[j].ID)
                {
                    isInventory = true;
                    break;
                }
            }
            for (int k = 0; k < length1; k++)
            {
                if (worker.ID == MuseumManager.instance.CurrentActiveWorkers[k].ID)
                {
                    isActiveWorker = true;
                    break;
                }
            }
            if (!isInventory && !isActiveWorker)
            {
                WorkersInMarket.Add(worker);
            }
        }
    }
    
    public void SetDatabaseDatas(WorkerBehaviour worker, WorkerData databaseWorker, string noneDatabaseName)
    {
        switch (worker.workerType)
        {
            case WorkerType.Security:
                if (databaseWorker != null)
                    worker.MyScript = new Security(worker.ID, databaseWorker.Name, databaseWorker.Level, worker.NpcCurrentSpeed, 100, Random.Range(20, 61), Random.Range(150, 200), worker.IsMale, databaseWorker.WorkRoomsIDs, worker.workerType, 0, worker);
                else
                    worker.MyScript = new Security(worker.ID, noneDatabaseName, 1, worker.NpcCurrentSpeed, 100, Random.Range(20, 61), Random.Range(150, 200), worker.IsMale, new List<int>(), worker.workerType, 0, worker);

                if (databaseWorker != null)
                    worker.MyDatas = new WorkerData(databaseWorker.ID, databaseWorker.Name, databaseWorker.Level, worker.MyScript.Exp, databaseWorker.WorkRoomsIDs, worker.workerType);
                else
                    worker.MyDatas = new WorkerData(worker.ID, worker.MyScript.Name, worker.MyScript.Level, worker.MyScript.Exp, worker.MyScript.IWorkRoomsIDs, worker.workerType);
                break;
            case WorkerType.Housekeeper:
                if (databaseWorker != null)
                    worker.MyScript = new Housekeeper(worker.ID, databaseWorker.Name, databaseWorker.Level, worker.NpcCurrentSpeed, 100, Random.Range(20, 61), Random.Range(150, 200), worker.IsMale, databaseWorker.WorkRoomsIDs, worker.workerType, 0, worker);
                else
                    worker.MyScript = new Housekeeper(worker.ID, noneDatabaseName, 1, worker.NpcCurrentSpeed, 100, Random.Range(20, 61), Random.Range(150, 200), worker.IsMale, new List<int>(), worker.workerType, 0, worker);
                if (databaseWorker != null)
                    worker.MyDatas = new WorkerData(databaseWorker.ID, databaseWorker.Name, databaseWorker.Level, worker.MyScript.Exp, databaseWorker.WorkRoomsIDs, worker.workerType);
                else
                    worker.MyDatas = new WorkerData(worker.ID, worker.MyScript.Name, worker.MyScript.Level, worker.MyScript.Exp, worker.MyScript.IWorkRoomsIDs, worker.workerType);
                break;
            case WorkerType.Musician:
                if (databaseWorker != null)
                    worker.MyScript = new Musician(worker.ID, databaseWorker.Name, databaseWorker.Level, worker.NpcCurrentSpeed, 100, Random.Range(20, 61), Random.Range(150, 200), worker.IsMale, databaseWorker.WorkRoomsIDs, worker.workerType, 0, worker);
                else
                    worker.MyScript = new Musician(worker.ID, noneDatabaseName, 1, worker.NpcCurrentSpeed, 100, Random.Range(20, 61), Random.Range(150, 200), worker.IsMale, new List<int>(), worker.workerType, 0, worker);
                if (databaseWorker != null)
                    worker.MyDatas = new WorkerData(databaseWorker.ID, databaseWorker.Name, databaseWorker.Level, worker.MyScript.Exp, databaseWorker.WorkRoomsIDs, worker.workerType);
                else
                    worker.MyDatas = new WorkerData(worker.ID, worker.MyScript.Name, worker.MyScript.Level, worker.MyScript.Exp, worker.MyScript.IWorkRoomsIDs, worker.workerType);
                break;
            case WorkerType.Receptionist:
                if (databaseWorker != null)
                    worker.MyScript = new Receptionist(worker.ID, databaseWorker.Name, databaseWorker.Level, worker.NpcCurrentSpeed, 100, Random.Range(20, 61), Random.Range(150, 200), worker.IsMale, databaseWorker.WorkRoomsIDs, worker.workerType, 0, worker);
                else
                    worker.MyScript = new Receptionist(worker.ID, noneDatabaseName, 1, worker.NpcCurrentSpeed, 100, Random.Range(20, 61), Random.Range(150, 200), worker.IsMale, new List<int>(), worker.workerType, 0, worker);
                if (databaseWorker != null)
                    worker.MyDatas = new WorkerData(databaseWorker.ID, databaseWorker.Name, databaseWorker.Level, worker.MyScript.Exp, databaseWorker.WorkRoomsIDs, worker.workerType);
                else
                    worker.MyDatas = new WorkerData(worker.ID, worker.MyScript.Name, worker.MyScript.Level, worker.MyScript.Exp, worker.MyScript.IWorkRoomsIDs, worker.workerType);
                break;
            case WorkerType.BrochureSeller:
                if (databaseWorker != null)
                    worker.MyScript = new BrochureSeller(worker.ID, databaseWorker.Name, databaseWorker.Level, worker.NpcCurrentSpeed, 100, Random.Range(20, 61), Random.Range(150, 200), worker.IsMale, databaseWorker.WorkRoomsIDs, worker.workerType, 0, worker);
                else
                    worker.MyScript = new BrochureSeller(worker.ID, noneDatabaseName, 1, worker.NpcCurrentSpeed, 100, Random.Range(20, 61), Random.Range(150, 200), worker.IsMale, new List<int>(), worker.workerType, 0, worker);
                if (databaseWorker != null)
                    worker.MyDatas = new WorkerData(databaseWorker.ID, databaseWorker.Name, databaseWorker.Level, worker.MyScript.Exp, databaseWorker.WorkRoomsIDs, worker.workerType);
                else
                    worker.MyDatas = new WorkerData(worker.ID, worker.MyScript.Name, worker.MyScript.Level, worker.MyScript.Exp, worker.MyScript.IWorkRoomsIDs, worker.workerType);
                break;
            default:
                break;
        }
        float s = (int)Random.Range(-100, 51) * 0.01f;
        worker.Agent.speed = worker.NpcCurrentSpeed + s;

        int securityCount, housekeeperCount, musicianCount, receptionistCount, brochureSellerCount;
        securityCount = AllWorkers.Where(x=> x.workerType == WorkerType.Security).Count();
        housekeeperCount = AllWorkers.Where(x=> x.workerType == WorkerType.Housekeeper).Count();
        musicianCount = AllWorkers.Where(x=> x.workerType == WorkerType.Musician).Count();
        receptionistCount = AllWorkers.Where(x=> x.workerType == WorkerType.Receptionist).Count();
        brochureSellerCount = AllWorkers.Where(x=> x.workerType == WorkerType.BrochureSeller).Count();

        Debug.Log($"Security Sayisi => {securityCount} || Housekeeper Sayisi => {housekeeperCount} || Musician Sayisi => {musicianCount} || Receptionist Sayisi => {receptionistCount} || BrochureSeller Sayisi => {brochureSellerCount}");
    }
    public Worker GetWorkerToWorkerType(WorkerData _workerData)
    {
        switch (_workerData.WorkerType)
        {
            case WorkerType.None:
                break;
            case WorkerType.Security:
                if (GetAllWorkers().Where(x => x.ID == _workerData.ID).SingleOrDefault().MyScript is Security security )
                {
                    WorkerDataTransferToWorkerOptions(security,_workerData);
                    return security;
                }
                break;
            case WorkerType.Housekeeper:
                if (GetAllWorkers().Where(x => x.ID == _workerData.ID).SingleOrDefault().MyScript is Housekeeper houseKeeper)
                {
                    WorkerDataTransferToWorkerOptions(houseKeeper, _workerData);
                    return houseKeeper;
                }
                break;
            case WorkerType.Musician:
                if (GetAllWorkers().Where(x => x.ID == _workerData.ID).SingleOrDefault().MyScript is Musician musician)
                {
                    WorkerDataTransferToWorkerOptions(musician, _workerData);
                    return musician;
                }
                break;
            case WorkerType.Receptionist:
                if (GetAllWorkers().Where(x => x.ID == _workerData.ID).SingleOrDefault().MyScript is Receptionist receptionist)
                {
                    WorkerDataTransferToWorkerOptions(receptionist, _workerData);
                    return receptionist;
                }
                break;
            case WorkerType.BrochureSeller:
                if (GetAllWorkers().Where(x => x.ID == _workerData.ID).SingleOrDefault().MyScript is BrochureSeller brochureSeller)
                {
                    WorkerDataTransferToWorkerOptions(brochureSeller, _workerData);
                    return brochureSeller;
                }
                break;
            default:
                break;            
        }
        return default;
    }
    public void WorkerDataTransferToWorkerOptions(Worker worker, WorkerData workerData)
    {
        worker.Level = workerData.Level;
        worker.IWorkRoomsIDs.Clear();
        foreach (int roomID in workerData.WorkRoomsIDs)
        {
            worker.IWorkRoomsIDs.Add(roomID);
        }
    }
    public void AddWorkersTasks()
    {
        foreach (WorkerBehaviour worker in MuseumManager.instance.CurrentActiveWorkers)
        {
            WorkersAndTasks.Add(new WorkerAndTasks(worker));
        }
    }
    public List<WorkerBehaviour> GetAllWorkers(){ return AllWorkers; }
    public List<WorkerBehaviour> GetWorkersInMarket() {  return WorkersInMarket; }
    //public List<WorkerBehaviour> GetWorkersInInventory() { return WorkersInInventory; }
    public List<WorkerBehaviour> GetCurrentWorkers() { return CurrentActiveWorkers; }
    public List<WorkerAndTasks> GetWorkersAndWorkersTasksy() { return WorkersAndTasks; }

    public void AddWorkerToInventory(WorkerBehaviour _newWorker)
    {
        MuseumManager.instance.WorkersInInventory.Add(_newWorker);
#if UNITY_EDITOR
        FirestoreManager.instance.UpdateGameData("ahmet123");
#else
             FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUser().UserId);
#endif
    }
    public int GetBaseHiringWorkerWithMuseumLevel()
    {
        int museumLevel = MuseumManager.instance.GetCurrentCultureLevel();

        if (museumLevel <= 5 && museumLevel < 10)
        {
            return (int)(1000 + GameManager.instance.BaseWorkerHiringPrice);
        }
        else if (museumLevel <= 10 && museumLevel < 15)
        {
            return (int)(2000 + GameManager.instance.BaseWorkerHiringPrice);
        }
        else if (museumLevel <= 15 && museumLevel < 20)
        {
            return (int)(3000 + GameManager.instance.BaseWorkerHiringPrice);
        }
        else
        {
            return (int)(4000 + GameManager.instance.BaseWorkerHiringPrice);
        }
    }
}
[System.Serializable]
public class WorkerAndTasks
{
    public List<Task> tasks;
    public WorkerBehaviour worker;

    public WorkerAndTasks(WorkerBehaviour worker)
    {
        this.worker = worker;
        this.tasks = new List<Task>(worker.MyScript.MyTasks);
    }
}
