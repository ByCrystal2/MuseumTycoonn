using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class WorkerBehaviour : MonoBehaviour
{

    [SerializeField] private NavMeshAgent Agent;
    [SerializeField] private float NpcSpeed;
    [SerializeField] private float NpcCurrentSpeed;

    [SerializeField] private WorkerType workerType;
    public WorkerData MyDatas;
    public Worker MyScript;
    private void Awake()
    {
        NpcCurrentSpeed = NpcSpeed;
    }
    private void Start()
    {
        switch (workerType)
        {
            case WorkerType.Security:
                MyScript = new Security(NpcCurrentSpeed,100);
                break;
            case WorkerType.Housekeeper:
                MyScript = new Housekeeper(NpcCurrentSpeed, 100);
                break;
            case WorkerType.Musician:
                MyScript = new Musician(NpcCurrentSpeed, 100);
                break;
            case WorkerType.Receptionist:
                MyScript = new Receptionist(NpcCurrentSpeed, 100);
                break;
            case WorkerType.BrochureSeller:
                MyScript = new BrochureSeller(NpcCurrentSpeed, 100);
                break;
            default:
                break;
        }
        Debug.Log($"MyWork is => {workerType} || MySpeed => {MyScript.Speed} || MyEnergt => {MyScript.Energy}");
        float s = (int)Random.Range(-100, 51) * 0.01f;
        Agent.speed = NpcCurrentSpeed + s;

        Security security = new Security(10,10);
        // Görev oluþturma
        
        Debug.Log("Görev iþçiye atandý.");
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
