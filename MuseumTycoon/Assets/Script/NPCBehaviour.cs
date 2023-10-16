using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCBehaviour : MonoBehaviour
{
    [SerializeField] private Transform TargetPosition;
    [SerializeField] private NPCState CurrentState;
    [SerializeField] private NavMeshAgent Agent;


    public float IdleLength;
    private float IdleTimer;

    Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    void Start()
    {
        IdleBack();
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentState == NPCState.Idle)
        {
            if (IdleTimer < Time.time)
                CreateTarget();
            anim.SetBool("Walk", false);
        }
        else if (CurrentState == NPCState.Move)
        {
            float distance = Vector3.Distance(transform.position, TargetPosition.position);
            Debug.Log("Distance with target: " + distance);
            if (distance < 0.5f)
                IdleBack();
            else
                Move();
        }
    }

    public void Move()
    {
        if (TargetPosition != null)
        {
            Agent.SetDestination(TargetPosition.position);
            anim.SetBool("Walk", true);
        }
    }

    public void CreateTarget()
    {
        int length = NpcManager.instance.Locations.Count;
        List<LocationData> possible = new List<LocationData>();

        for (int i = 0; i < length; i++)
            if (!NpcManager.instance.Locations[i].GetComponent<LocationData>().isLocked && NpcManager.instance.Locations[i] != TargetPosition)
                possible.Add(NpcManager.instance.Locations[i]);

        if (possible.Count == 0)
        {
            Debug.LogError("Gidilecek hic bir yer yok!.");
        }
        else
        {
            TargetPosition = NpcManager.instance.Locations[Random.Range(0, length)].transform;
            CurrentState = NPCState.Move;            
        }
    }

    public void IdleBack()
    {
        CurrentState = NPCState.Idle;        
        IdleTimer = Time.time + IdleLength;
        TargetPosition = null;
    }
}

public enum NPCState
{
    Idle,
    Move,
    LookStart,
    LookEnd,
}
