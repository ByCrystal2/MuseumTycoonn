using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCBehaviour : MonoBehaviour
{
    [SerializeField] private Transform TargetPosition;
    [SerializeField] private NPCState CurrentState;
    [SerializeField] private NpcTargets CurrentTarget;
    [SerializeField] private NavMeshAgent Agent;
    [SerializeField] private float NpcSpeed;
    [SerializeField] private Vector3 OutsidePosition;
    public float IdleLength;
    private float IdleTimer;

    Animator anim;

    public int VisitableArtAmount;

    public List<MyColors> LikedColors = new List<MyColors>(); //3renk
    public MyColors DislikedColor;
    public List<string> LikedArtist;
    public float Happiness; //0-100; Mutlu npc daha fazla sanat gezmek isteyecek. Ve npc hizini etkileyecek.
    public float Stress; //0-100; Stressli npc daha az sanat gormek isteyecek. Ve npc hizini etkileyecek.
    public float Toilet; //100-0; Tuvaleti zaten biliyoruz.
    public float Education; //0-10; Egitimli insanlar daha cok kultur kazandirir.

    bool DecidedToEnter;

    bool isGidis;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        CurrentTarget = NpcTargets.Outside;
        float x = (int)Random.Range(-100, 51)*0.01f;
        Agent.speed = NpcSpeed + x;
        OutsidePosition = Vector3.zero;
    }
    void Start()
    {
        IdleBack();

        Vector3 spawnPoint = Vector3.zero;
        int x = Random.Range(0, 2);
        if (x == 0)
        {
            TargetPosition = NpcManager.instance.GidisListe[Random.Range(0, NpcManager.instance.GidisListe.Count)];
            spawnPoint = TargetPosition.position + new Vector3(Random.Range(-29, 30) * 0.1f, 0, Random.Range(-29, 30) * 0.1f);
            isGidis = true;
        }
        else
        {
            TargetPosition = NpcManager.instance.GelisListe[Random.Range(0, NpcManager.instance.GelisListe.Count)];
            spawnPoint = TargetPosition.position + new Vector3(Random.Range(-29, 30) * 0.1f, 0, Random.Range(-29, 30) * 0.1f);
            isGidis = false;
        }

        NavMeshHit hit;
        if (NavMesh.SamplePosition(spawnPoint, out hit, 10, NavMesh.AllAreas))
            spawnPoint = hit.position;

        transform.position = spawnPoint;
    }

    // Update is called once per frame
    void Update()
    {
        if (TargetPosition == null)
        {
            CreateTarget();
            return;
        }

        if (CurrentState == NPCState.Idle)
        {
            if (IdleTimer < Time.time)
                CreateTarget();
            anim.SetBool("Walk", false);
        }
        else if (CurrentState == NPCState.Move)
            Move();
    }

    public void MoveOpt(Vector3 target)
    {
        float distance = Vector3.Distance(transform.position, target);
        if (distance < 1f)
            IdleBack();
        if (TargetPosition != null)
        {
            Agent.SetDestination(target);
            anim.SetBool("Walk", true);
        }
    }

    public void Move()
    {
        if (CurrentTarget == NpcTargets.Inside)
            MoveOpt(TargetPosition.position);
        else if (CurrentTarget == NpcTargets.Outside)
            MoveOpt(OutsidePosition);
        else if (CurrentTarget == NpcTargets.EnterWay)
            MoveOpt(TargetPosition.position);
    }

    public void CreateTarget()
    {
        if (CurrentTarget == NpcTargets.Outside)
        {
            if (isGidis)
            {
                int index = TargetPosition.GetSiblingIndex();
                index += 1;
                if (NpcManager.instance.GidisListe.Count == index)
                {
                    int x = Random.Range(0, 101);
                    if (x <= 20)
                    {
                        if (!MuseumManager.instance.IsMuseumFull())
                        {
                            Debug.Log("Npc decided to enter museum.");
                            OnDecidedEnterMuseum();
                            return;
                        }
                    }
                    index = 0;
                }
                TargetPosition = NpcManager.instance.GidisListe[index];
            }
            else
            {
                int index = TargetPosition.GetSiblingIndex();
                index += 1;
                if (NpcManager.instance.GelisListe.Count == index)
                {
                    int x = Random.Range(0, 101);
                    if (x <= 20)
                    {
                        if (!MuseumManager.instance.IsMuseumFull())
                        {
                            Debug.Log("Npc decided to enter museum.");
                            OnDecidedEnterMuseum();
                            return;
                        }
                    }
                    index = 0;
                }
                TargetPosition = NpcManager.instance.GelisListe[index];
            }
            OutsidePosition = TargetPosition.position + new Vector3(Random.Range(-29, 30) * 0.1f, 0, Random.Range(-29, 30) * 0.1f);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(OutsidePosition, out hit, 10, NavMesh.AllAreas))
                OutsidePosition = hit.position;
            
            CurrentState = NPCState.Move;
        }
        else if (CurrentTarget == NpcTargets.Inside)
        {
            OutsidePosition = Vector3.zero;
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
        else if (CurrentTarget == NpcTargets.EnterWay)
        {
            NpcArrivedTheEnterGate();
        }
    }

    public void OnDecidedEnterMuseum()
    {
        CurrentTarget = NpcTargets.EnterWay;
        CurrentState = NPCState.Move;
        DecidedToEnter = true;

        if (isGidis)
            TargetPosition = NpcManager.instance.Enter2Point;
        else
            TargetPosition = NpcManager.instance.Enter1Point;
        MuseumManager.instance.OnNpcEnteredMuseum(this);
    }

    public void IdleBack()
    {
        CurrentState = NPCState.Idle;        
        IdleTimer = Time.time + IdleLength;
    }

    public void NpcArrivedTheEnterGate()
    {
        CurrentTarget = NpcTargets.Inside;
        CurrentState = NPCState.Move;
        CreateTarget();

        MuseumManager.instance.OnNpcPaid();
    }

    public void OnFootstep()
    {

    }
}

public enum NPCState
{
    Idle,
    Move,
    Investigate,
    Discuss,
    Escape,
}

public enum NpcTargets
{
    Outside,
    EnterWay,
    Inside,
}
