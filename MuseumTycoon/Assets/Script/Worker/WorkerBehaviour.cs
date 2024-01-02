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
public class WorkerData
{
    // Isci datalari burda tutulacak.
}
