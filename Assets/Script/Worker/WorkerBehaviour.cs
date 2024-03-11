using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class WorkerBehaviour : MonoBehaviour
{
    [SerializeField] public int ID;
    [SerializeField] public NavMeshAgent Agent;
    [SerializeField] public Animator Anim;
    [SerializeField] public float CurrentXp;
    [SerializeField] public float NpcSpeed;
    [SerializeField] public float NpcCurrentSpeed;
    [SerializeField] public float PatrolRadius;

    [SerializeField] public WorkerType workerType;
    public WorkerData MyDatas;
    public Worker MyScript;
    public Transform EffectParent;
    private void Awake()
    {
                
    }
    private void Start()
    {        
        
    }

    private void OnEnable()
    {
        if (MyScript is Housekeeper hk)
        {
            List<RoomData> myRooms = RoomManager.instance.GetRoomWithIDs(MyDatas.WorkRoomsIDs);
            if(myRooms.Count > 0)
            {
                Agent.enabled = false;
                Vector3 startPoint = hk.PatrolToRandomPoint(myRooms[0].CenterPoint.position, 7);
                transform.position = startPoint;
                Agent.enabled = true;
                hk.CreateNewTarget();
            }
            else
            {
                Debug.LogError("No Room data on the current worker. Name: " + transform.name);
                gameObject.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        if (MyScript is Housekeeper hk)
        {
            hk.CurrentTarget = Vector3.zero;
            hk.CurrentActiveTask = null;
            if (hk.MyCurrentMessTask != null)
                NpcManager.instance.AddMessIntoMessParent(hk.MyCurrentMessTask.transform);
        }
    }

    private void FixedUpdate()
    {
        if (MyScript is Housekeeper hk)
        {
            if (hk.CurrentTarget != Vector3.zero)
            {
                hk.Move(hk.CurrentTarget, hk.MyTasks.Count > 0);
            }
        }
    }

    public void CreateNewTargetDelay(float _delay)
    {
        StopAllCoroutines();
        StartCoroutine(CreateNewTargetDelayed(_delay));
    }

    public void CreateEndTask(float _delay)
    {
        StopAllCoroutines();
        StartCoroutine(EndTaskDelayed(_delay));
    }

    private IEnumerator CreateNewTargetDelayed(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        if (MyScript is Housekeeper hk)
            hk.CreateNewTarget();
    }

    private IEnumerator EndTaskDelayed(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        if (MyScript is Housekeeper hk)
            hk.EndClean();
    }
}
[System.Serializable]
public sealed class WorkerData
{
    public int ID;
    public int Level;
    public float Xp;
    public List<int> WorkRoomsIDs = new List<int>();
    [HideInInspector]public WorkerType WorkerType;
    public WorkerData(int _id, int level, float _Xp, List<int> _workRoomsIDs, WorkerType workerType)
    {
        this.ID = _id;
        this.Level = level;
        this.Xp = _Xp;
        WorkRoomsIDs.Clear();
        foreach (int i in _workRoomsIDs) { WorkRoomsIDs.Add(i); }
        this.WorkerType = workerType;
    }
}
