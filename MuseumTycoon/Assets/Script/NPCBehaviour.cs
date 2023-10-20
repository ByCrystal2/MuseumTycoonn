using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public int NpcVisittingArtAmount;

    bool DecidedToEnter;

    bool isGidis;
    private void Awake()
    {
        
    }
    void Start()
    {
        anim = GetComponent<Animator>();
        CurrentTarget = NpcTargets.Outside;
        float s = (int)Random.Range(-100, 51) * 0.01f;
        Agent.speed = NpcSpeed + s;
        OutsidePosition = Vector3.zero;
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
            {
                CreateTarget();
            }
            anim.SetBool("Walk", false);
        }
        else if (CurrentState == NPCState.Move)
        {
            Move();
        }
        else if (CurrentState == NPCState.Investigate)
        {
            Debug.Log("Farkli goruslu iki npc ayni tabloya investigate ediyorken kavga etme ihtimali burada islenicek.");
            if (IdleTimer < Time.time)
            {
                if (OnNPCInvestigatedArt())
                    CreateTarget();
                else
                    OnExitWayMuseum();
            }
            anim.SetBool("Walk", false);
        }
    }

    public void MoveOpt(Vector3 target)
    {
        float distance = Vector3.Distance(transform.position, target);
        if (distance < 1f)
        {
            Transform pic = TargetPosition.parent;
            PictureElement PE;
            if(pic.TryGetComponent(out PE))
                InvestigateStart();
            else
                IdleBack();
        }
        if (TargetPosition != null)
        {
            Agent.SetDestination(target);
            anim.SetBool("Walk", true);
        }
    }

    public void InvestigateStart()
    {
        CurrentState = NPCState.Investigate;
        IdleTimer = Time.time + IdleLength;
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

    bool OnNPCInvestigatedArt()
    {
        Debug.Log("EndOf Investigate");
        PictureElement PE = TargetPosition.GetComponentInParent<PictureElement>();

        int NPCMaxScore = 10;
        int NPCMinScore = 0;
        int NPCCurrentScore = 5;
        List<MyColors> ArtColors = PE.data.MostCommonColors;
        int length = ArtColors.Count;
        int length2 = LikedColors.Count;
        for (int i = 0; i < length; i++)
        {
            for (int a = 0; a < length2; a++)
            {
                if (LikedColors[a] == ArtColors[i])
                    NPCMinScore++;
                if (ArtColors[i] == DislikedColor)
                    NPCMaxScore -= Random.Range(1,4);
            }
        }

        NPCCurrentScore = Random.Range(NPCMinScore, NPCMaxScore + 1);
        MuseumManager.instance.AddCultureExp(NPCCurrentScore * 3);

        NpcVisittingArtAmount--;
        if (NpcVisittingArtAmount == 0)
        {
            return false;
        }

        return true;
    }

    public void IdleBack()
    {
        CurrentState = NPCState.Idle;        
        IdleTimer = Time.time + IdleLength;
    }

    public void NpcArrivedTheEnterGate()
    {
        NpcVisittingArtAmount = Random.Range(2, 5);
        int LikedColorsAmount = Random.Range(2, 5);
        LikedColors = new List<MyColors>();
        MyColors[] enumDegerleri = (MyColors[])System.Enum.GetValues(typeof(MyColors));
        for (int i = 0; i < LikedColorsAmount + 1; i++)
        {
            MyColors bulunanRenk = MyColors.Black;
            do
            {
                int arananRenk = Random.Range(0, enumDegerleri.Length);
                bulunanRenk = enumDegerleri.FirstOrDefault(r => (int)r == (int)arananRenk);
            } while (LikedColors.Contains(bulunanRenk));
            if (i == LikedColorsAmount)
                DislikedColor = bulunanRenk;
            else
                LikedColors.Add(bulunanRenk);
        }

        CurrentTarget = NpcTargets.Inside;
        CurrentState = NPCState.Move;
        CreateTarget();

        MuseumManager.instance.OnNpcPaid();
    }

    void OnExitWayMuseum()
    {
        CurrentTarget = NpcTargets.Outside;
        CurrentState = NPCState.Move;
        TargetPosition = NpcManager.instance.GidisListe[Random.Range(0, NpcManager.instance.GidisListe.Count)];

        OutsidePosition = TargetPosition.position + new Vector3(Random.Range(-29, 30) * 0.1f, 0, Random.Range(-29, 30) * 0.1f);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(OutsidePosition, out hit, 10, NavMesh.AllAreas))
            OutsidePosition = hit.position;

        MuseumManager.instance.OnNpcExitedMuseum(this);
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
