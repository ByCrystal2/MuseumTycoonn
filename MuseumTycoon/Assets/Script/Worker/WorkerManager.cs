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
        int length2 = WorkersInInventory.Count;
        foreach (var worker in AllWorkers)
        {
            bool isInventory = false;
            for (int j = 0; j < length2; j++)
            {
                if (worker.ID == WorkersInInventory[j].ID)
                {
                    isInventory = true;
                    break;
                }
            }
            if (!isInventory)
            {
                WorkersInMarket.Add(worker);
            }
        }
        
        Task task1 = new Task("Hirsizi Yakala");
        foreach (var worker in AllWorkers)
        {
            if (worker.workerType == WorkerType.Security)
            {
                if (worker.MyScript is Security security)
                {
                    //if (security.CanPerformTask(task1))
                    //{
                    //    security.AssignTask(task1);                    
                    //}
                }
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
                    worker.MyScript = new Security(worker.ID, worker.NpcCurrentSpeed, 100);
                    break;
                case WorkerType.Housekeeper:
                    worker.MyScript = new Housekeeper(worker.ID,worker.NpcCurrentSpeed, 100);
                    break;
                case WorkerType.Musician:
                    worker.MyScript = new Musician(worker.ID, worker.NpcCurrentSpeed, 100);
                    break;
                case WorkerType.Receptionist:
                    worker.MyScript = new Receptionist(worker.ID, worker.NpcCurrentSpeed, 100);
                    break;
                case WorkerType.BrochureSeller:
                    worker.MyScript = new BrochureSeller(worker.ID, worker.NpcCurrentSpeed, 100);
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
