using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class WorkerManager : MonoBehaviour
{
    [SerializeField] public List<WorkerBehaviour> AllWorkers = new List<WorkerBehaviour>();
    [SerializeField] public List<WorkerBehaviour> WorkersInMarket = new List<WorkerBehaviour>();
    [SerializeField] public List<WorkerBehaviour> WorkersInInventory = new List<WorkerBehaviour>();
    [SerializeField] public List<WorkerBehaviour> CurrentActiveWorkers = new List<WorkerBehaviour>();

    [SerializeField] public List<WorkerAndTasks> WorkersAndTasks = new List<WorkerAndTasks>();

    [SerializeField] Transform WorkersContent;
    public static WorkerManager instance { get; set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {

    }

    public void BaseAllWorkerOptions()
    {
        int length = WorkersContent.childCount;
        for (int i = 0; i < length; i++)
        {
            if (WorkersContent.GetChild(i).TryGetComponent(out WorkerBehaviour workerMono))
            {
                AllWorkers.Add(WorkersContent.GetChild(i).GetComponent<WorkerBehaviour>());
            }
            WorkersContent.GetChild(i).gameObject.SetActive(false);
        }
        AddAllWorkersSubWork();
       
    }
    public void CreateWorkersToMarket()
    {
        int length = WorkersInInventory.Count;
        int length1 = CurrentActiveWorkers.Count;
        foreach (var worker in AllWorkers)
        {
            bool isInventory = false;
            bool isActiveWorker = false;
            for (int j = 0; j < length; j++)
            {
                if (worker.ID == WorkersInInventory[j].ID)
                {
                    isInventory = true;
                    break;
                }
            }
            for (int k = 0; k < length1; k++)
            {
                if (worker.ID == CurrentActiveWorkers[k].ID)
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
    public void AddAllWorkersSubWork()
    {
        foreach (var worker in AllWorkers)
        {
            worker.Agent = worker.gameObject.GetComponent<NavMeshAgent>();
            worker.NpcCurrentSpeed = worker.NpcSpeed;
           
            switch (worker.workerType)
            {
                case WorkerType.Security:
                    worker.MyScript = new Security(worker.ID, worker.NpcCurrentSpeed, 100,worker.workerType,0 , worker);
                    worker.MyDatas = new WorkerData(worker.ID, worker.MyScript.Level, worker.MyScript.Exp, worker.MyScript.IWorkRoomsIDs,worker.workerType);
                    break;
                case WorkerType.Housekeeper:
                    worker.MyScript = new Housekeeper(worker.ID,worker.NpcCurrentSpeed, 100, worker.workerType,0 , worker);
                    worker.MyDatas = new WorkerData(worker.ID, worker.MyScript.Level, worker.MyScript.Exp, worker.MyScript.IWorkRoomsIDs, worker.workerType);
                    break;
                case WorkerType.Musician:
                    worker.MyScript = new Musician(worker.ID, worker.NpcCurrentSpeed, 100, worker.workerType, 0, worker);
                    worker.MyDatas = new WorkerData(worker.ID, worker.MyScript.Level, worker.MyScript.Exp, worker.MyScript.IWorkRoomsIDs, worker.workerType);
                    break;
                case WorkerType.Receptionist:
                    worker.MyScript = new Receptionist(worker.ID, worker.NpcCurrentSpeed, 100, worker.workerType, 0, worker);
                    worker.MyDatas = new WorkerData(worker.ID, worker.MyScript.Level, worker.MyScript.Exp, worker.MyScript.IWorkRoomsIDs, worker.workerType);
                    break;
                case WorkerType.BrochureSeller:
                    worker.MyScript = new BrochureSeller(worker.ID, worker.NpcCurrentSpeed, 100, worker.workerType, 0, worker);
                    worker.MyDatas = new WorkerData(worker.ID, worker.MyScript.Level, worker.MyScript.Exp, worker.MyScript.IWorkRoomsIDs, worker.workerType);
                    break;
                default:
                    break;
            }
            float s = (int)Random.Range(-100, 51) * 0.01f;
            worker.Agent.speed = worker.NpcCurrentSpeed + s;
        }
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
        foreach (WorkerBehaviour worker in CurrentActiveWorkers)
        {
            WorkersAndTasks.Add(new WorkerAndTasks(worker));
        }
    }
    public List<WorkerBehaviour> GetAllWorkers(){ return AllWorkers; }
    public List<WorkerBehaviour> GetWorkersInMarket() {  return WorkersInMarket; }
    public List<WorkerBehaviour> GetWorkersInInventory() { return WorkersInInventory; }
    public List<WorkerBehaviour> GetCurrentWorkers() { return CurrentActiveWorkers; }
    public List<WorkerAndTasks> GetWorkersAndWorkersTasksy() { return WorkersAndTasks; }

    public void AddWorkerToInventory(WorkerBehaviour _newWorker)
    {
        WorkersInInventory.Add(_newWorker);
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
