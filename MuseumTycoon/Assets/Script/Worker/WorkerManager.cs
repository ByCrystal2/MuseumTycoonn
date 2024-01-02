using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerManager : MonoBehaviour
{
    [SerializeField] public List<WorkerBehaviour> AllWorkers = new List<WorkerBehaviour>();
    [SerializeField] public List<WorkerBehaviour> CurrentWorkers = new List<WorkerBehaviour>();
    [SerializeField] public List<WorkerBehaviour> WorkersInMarket = new List<WorkerBehaviour>();
    [SerializeField] public List<WorkerAndTasks> WorkersAndTasks = new List<WorkerAndTasks>();
    private void Start()
    {
        foreach (WorkerBehaviour worker in CurrentWorkers)
        {
            WorkersAndTasks.Add(new WorkerAndTasks(worker));
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
