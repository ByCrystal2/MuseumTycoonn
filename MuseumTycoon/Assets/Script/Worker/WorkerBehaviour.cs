using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class WorkerBehaviour : MonoBehaviour
{
    [SerializeField] public int ID;
    [SerializeField] public NavMeshAgent Agent;
    [SerializeField] public float NpcSpeed;
    [SerializeField] public float NpcCurrentSpeed;

    [SerializeField] public WorkerType workerType;
    public WorkerData MyDatas;
    public Worker MyScript;
    private void Awake()
    {
                
    }
    private void Start()
    {        
        
    }

    public WorkerType GetMyWorkerType()
    {
        return workerType;
    }
}
[System.Serializable]
public sealed class WorkerData
{
    public int ID;
    public int Level;
    public List<int> WorkRoomsIDs = new List<int>();
    [HideInInspector]public WorkerType WorkerType;
    public WorkerData(int _id, int level, List<int> _workRoomsIDs, WorkerType workerType)
    {
        this.ID = _id;
        this.Level = level;
        WorkRoomsIDs.Clear();
        foreach (int i in _workRoomsIDs) { WorkRoomsIDs.Add(i); }
        this.WorkerType = workerType;

    }
}
