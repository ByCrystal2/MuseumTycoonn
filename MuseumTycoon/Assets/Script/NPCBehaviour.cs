using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class NPCBehaviour : MonoBehaviour
{
    [SerializeField] private Transform TargetPosition;
    [SerializeField] public List<DialogData> MySources;
    [SerializeField] private AudioSource CurrentAudioSource;
    [SerializeField] private NPCState CurrentState;
    [SerializeField] private NpcTargets CurrentTarget;
    [SerializeField] private InvestigateState CurrentInvestigate;
    [SerializeField] private NavMeshAgent Agent;
    [SerializeField] private float NpcSpeed;
    [SerializeField] private float NpcCurrentSpeed;
    [SerializeField] private float NpcRotationSpeed;
    [SerializeField] private Vector3 OutsidePosition;
    [SerializeField] private NPCBehaviour DialogTarget;

    public float IdleLength;
    private float IdleTimer;

    public WalkEnum NpcWalkType;

    Animator anim;

    public int VisitableArtAmount;

    public List<MyColors> LikedColors = new List<MyColors>(); //3renk
    public MyColors DislikedColor;
    public List<string> LikedArtist;
    public float Happiness; //0-100; Mutlu npc daha fazla sanat gezmek isteyecek. Ve npc hizini etkileyecek.
    public float Stress; //0-100; Stressli npc daha az sanat gormek isteyecek. Ve npc hizini etkileyecek.
    public float Toilet; //100-0; Tuvaleti zaten biliyoruz.
    public float Education; //0-10; Egitimli insanlar daha cok kultur kazandirir.
    public float AdditionalLuck; //sans oranini buradan arttirabiliriz.

    public int NpcVisittingArtAmount;

    bool DecidedToEnter;

    bool isGidis;
    public bool IsBusy; //Mesgul mu?
    public bool IsMale; //Erkek mi?

    
    private void Awake()
    {
        
    }
    void Start()
    {
        MySources = AudioManager.instance.GetDialogs(IsMale);
        CurrentAudioSource = transform.GetChild(0).GetComponent<AudioSource>();
        AdditionalLuck = 1000;
        NpcCurrentSpeed = NpcSpeed + NpcSpeed * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.VisitorsSpeedIncrease] / 100f); ;
        NpcRotationSpeed = 5;
        anim = GetComponent<Animator>();
        CurrentTarget = NpcTargets.Outside;
        float s = (int)Random.Range(-100, 51) * 0.01f;
        Agent.speed = NpcCurrentSpeed + s;
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
            SetCurrentAnimationState("Walk", 0);
        }
        else if (CurrentState == NPCState.Move)
        {
            if (IdleTimer > Time.time)
                return;
            Move();
        }
        else if (CurrentState == NPCState.Investigate)
        {
            SetCurrentAnimationState("Walk", 0);
            if (CurrentInvestigate == InvestigateState.Dialog)
            {
                if (IdleTimer < Time.time)
                {
                    Debug.Log("Dialog end with npc: " + name + " /dialogTarget: " + DialogTarget.name);
                    OnDialogEnd();
                    CurrentAudioSource.Stop();
                    //AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcByeBye, CurrentAudioSource);
                    return;
                }
                if(DialogTarget != null)
                    LookAtOptimal(DialogTarget.transform);
                return;
            }
            LookAtOptimal(TargetPosition.parent);
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
            if(DialogTarget != null)
                Debug.Log("dialog targeti olmasina ragmen Move a gecti.");
            Agent.SetDestination(target);
            SetCurrentAnimationState("Walk", (int)NpcWalkType, "Dialog", 0);
        }
    }

    public void InvestigateStart()
    {
        PictureElement PE = TargetPosition.GetComponentInParent<PictureElement>();
         
        if (PE._pictureData.TextureID == 0)
        {
            CurrentState = NPCState.Move;
            CreateTarget();
            return;
        }
        Debug.Log("Investigate Start");
        Agent.isStopped = true;
        Agent.enabled = false;
        CurrentState = NPCState.Investigate;

        CurrentInvestigate = InvestigateState.Look;
        SetCurrentAnimationState("Look", 1);
        //IdleTimer = Time.time + IdleLength;
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
                    x = (int)(x + (float)x * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.WantVisittingRatio] / 100f));
                    if (x >= 80)
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
                    x = (int) (x + (float)x * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.WantVisittingRatio] / 100f));
                    if (x >= 80)
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
            {
                int length2 = MuseumManager.instance.CurrentNpcs.Count;
                for (int u = 0; u < length2; u++)
                {
                    if (!NpcManager.instance.Locations[i].GetComponent<LocationData>().isLocked && NpcManager.instance.Locations[i] != TargetPosition &&
                        MuseumManager.instance.CurrentNpcs[u].TargetPosition != NpcManager.instance.Locations[i].transform)
                        possible.Add(NpcManager.instance.Locations[i]);
                }
            }

            if (possible.Count == 0)
            {
                Debug.LogError("Gidilecek hic bir yer yok!.");
                Happiness -= Time.deltaTime * (Stress * 0.1f);
                Stress += Time.deltaTime * 2;
                CheckStats();
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

    void CheckStats()
    {
        OnHappinessChange();

        if (Stress > 100)
        {
            Stress = 100;
            OnStressFull();
        }
        if (Stress < 0)
        {
            Stress = 0;
        }
    }

    void OnHappinessChange()
    {
        if (Happiness <= 25)
        {
            NpcWalkType = WalkEnum.SadWalk;
            float sadSpeed = NpcSpeed * 0.35f;
            NpcCurrentSpeed = sadSpeed + sadSpeed * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.VisitorsSpeedIncrease] / 100f);
            Agent.speed = NpcCurrentSpeed;
            AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcSad, CurrentAudioSource);
            if (Happiness < 0)
            {
                Happiness = 0;
                OnHappinessEnd();
            }
        }
        else if (Happiness > 25 && Happiness < 75)
        {
            NpcWalkType = WalkEnum.NormalWalk;
            NpcCurrentSpeed = NpcSpeed + NpcSpeed * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.VisitorsSpeedIncrease] / 100f); ;
            Agent.speed = NpcCurrentSpeed;
        }
        else
        {
            NpcWalkType = WalkEnum.HappyWalk;
            float happySpeed = NpcSpeed * 1.25f;
            NpcCurrentSpeed = happySpeed + happySpeed * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.VisitorsSpeedIncrease] / 100f);
            Agent.speed = NpcCurrentSpeed;
            AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcHappiness, CurrentAudioSource);
            if (Happiness > 100)
            {
                Happiness = 100;
            }
        }
    }

    void OnStressFull()
    {
        Debug.Log("Npc Cok Stressli;");
    }

    void OnHappinessEnd()
    {
        Debug.Log("Npc Cok Mutsuz;");
        
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

        if (PE == null)
        {
            if (NpcVisittingArtAmount == 0)
            {
                return false;
            }

            return true;
        }
        if (PE._pictureData.TextureID == 0)
        {
            if (NpcVisittingArtAmount == 0)
            {
                return false;
            }

            return true;
        }
        int NPCMaxScore = 10;
        int NPCMinScore = 0;
        int NPCCurrentScore = 5;
        PictureElementData ped = MuseumManager.instance.GetPictureElementData(PE._pictureData.TextureID);
        List<MyColors> ArtColors = ped.MostCommonColors;
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
        // MuseumManager.instance.AddTotalVisitorCommentCount(1);
        MuseumManager.instance.OnNpcCommentedPicture(this,PE, Mathf.Round(NPCCurrentScore)); // Þimdilik Current Score Göre Yýldýz Yorumu Yapýldý.

        int center = 5;
        if (NPCCurrentScore < center)
        {
            AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcDisLike, CurrentAudioSource);
            Stress += (5 - NPCCurrentScore) * 2;
            Happiness -= Mathf.Round(Stress * 0.2f);
        }
        else
        {
            AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcLike, CurrentAudioSource);
            Stress -= (NPCCurrentScore - 5) * 2;
            int _happiness = (int)Mathf.Round((100 - Stress) * 0.05f);
            _happiness = (int)(_happiness + (float)_happiness * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.HappinessIncreaseRatio] / 100f));
            Happiness += _happiness;
        }

        CheckStats();

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
        Happiness = 100;
        Stress = 20;
        NpcWalkType = WalkEnum.NormalWalk;
        NpcCurrentSpeed = NpcSpeed + NpcSpeed * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.VisitorsSpeedIncrease] / 100f); //3.3
        Agent.speed = NpcCurrentSpeed;
        TargetPosition = NpcManager.instance.GidisListe[Random.Range(0, NpcManager.instance.GidisListe.Count)];

        OutsidePosition = TargetPosition.position + new Vector3(Random.Range(-29, 30) * 0.1f, 0, Random.Range(-29, 30) * 0.1f);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(OutsidePosition, out hit, 10, NavMesh.AllAreas))
            OutsidePosition = hit.position;

        SetCurrentAnimationState("Walk", (int)NpcWalkType, "Dialog", 0);
        MuseumManager.instance.OnNpcExitedMuseum(this);
    }

    public void SetCurrentAnimationState(string firstState, int firstInt, string secondState = "", int secondInt = 0, string thirdState = "", int thirdInt = 0)
    {
        //anim.SetInteger("Liked", 0);
        //anim.SetInteger("NoLiked", 0);
        //anim.SetInteger("Dialog", 0);
        //anim.SetInteger("Walk", 0);
        //anim.SetInteger("Hit", 0);
        //anim.SetInteger("GetHit", 0);
        //anim.SetInteger("Fall", 0);
        //anim.SetInteger("inFight", 0);
        //anim.SetInteger("Look", 0);

        anim.SetInteger(firstState, firstInt);
        if (secondState != "")
            anim.SetInteger(secondState , secondInt);
        if (thirdState != "")
            anim.SetInteger(thirdState, thirdInt);
    }

    public void OnEndInvestigatePicture()
    {
        Debug.Log("Looking end.");
        SetCurrentAnimationState("Look", 0);
        if (DialogTarget != null)
            return;
        
        IdleTimer = Time.time + 2;

        var dialogTarget = CheckTheCurrentLookingNpcs();
        float delay = dialogTarget._delayTime;
        NPCBehaviour npcTarget = dialogTarget._dialogTarget;
        if (npcTarget != null)
        {
            IdleTimer = delay;
            CurrentInvestigate = InvestigateState.Dialog;
            DialogTarget = npcTarget;
            IsBusy = true;
            Debug.Log("IdleTimer: " + IdleTimer + " /Dialog end with npc: " + name + " /dialogTarget: " + DialogTarget.name);
            SetCurrentAnimationState("Dialog", Random.Range(1, 4));            
            AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcTalking,CurrentAudioSource);            
            return;
        }

        Agent.isStopped = false;
        Agent.enabled = true;
        if (OnNPCInvestigatedArt())
            CreateTarget();
        else
            OnExitWayMuseum();
    }

    (float _delayTime, NPCBehaviour _dialogTarget)  CheckTheCurrentLookingNpcs()
    {
        NPCBehaviour theDialogPartner = null;

        int length = MuseumManager.instance.CurrentNpcs.Count;
        for (int i = 0; i < length; i++)
        {
            NPCBehaviour currentNpc = MuseumManager.instance.CurrentNpcs[i];
            if (currentNpc.TargetPosition.parent == TargetPosition.parent && currentNpc.CurrentState == NPCState.Investigate &&
                currentNpc.DialogTarget == null && currentNpc != this && !currentNpc.IsBusy)
            {
                float luck = Random.Range(0, 101);
                luck += (Happiness * 0.2f);
                luck += (AdditionalLuck * luck * 0.01f);
                if (luck > 70)
                {
                    theDialogPartner = MuseumManager.instance.CurrentNpcs[i];
                    break;
                }
            }
        }

        float timer = 0;
        if (theDialogPartner != null)
        {
            theDialogPartner.DialogTarget = this;
            theDialogPartner.CurrentInvestigate = InvestigateState.Dialog;
            theDialogPartner.CurrentState = NPCState.Investigate;
            theDialogPartner.IsBusy = true;
            theDialogPartner.SetCurrentAnimationState("Dialog", Random.Range(1, 4), "Look", 0);

            timer = Time.time + Random.Range(5,15);
            theDialogPartner.IdleTimer = timer;
            Debug.Log("theDialogPartner.IdleTimer: " + theDialogPartner.IdleTimer + " /Dialog end with npc: " + theDialogPartner.name + " /dialogTarget: " + theDialogPartner.DialogTarget.name);
        }

        return (timer, theDialogPartner);
    }

    public void OnDialogEnd()
    {
        IdleTimer = Time.time + 1;
        Debug.Log("Dialog end.");
        SetCurrentAnimationState("Dialog",-1);
        StopCoroutine(FarewellDelay());        
        StartCoroutine(FarewellDelay());
    }

    IEnumerator FarewellDelay()
    {
        while (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Farewell")
            yield return new WaitForEndOfFrame();

        float timer = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcByeBye, CurrentAudioSource);
        while (timer > 0)
        {
            if (DialogTarget != null)
                LookAtOptimal(DialogTarget.transform);
            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Agent.isStopped = false;
        Agent.enabled = true;
        DialogTarget = null;
        CurrentInvestigate = InvestigateState.Look;
        SetCurrentAnimationState("Dialog", 0);
        IsBusy = false;
        if (OnNPCInvestigatedArt())
            CreateTarget();
        else
            OnExitWayMuseum();
    }

    void LookAtOptimal(Transform target)
    {
        Vector3 directionToTarget = target.position - transform.position;

        // Calculate the angle in degrees to rotate towards the target
        float angle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;

        // Create a rotation that only affects the Y axis
        Quaternion targetRotation = Quaternion.Euler(0, angle, 0);

        // Apply the rotation to the NPC's transform
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, NpcRotationSpeed * Time.deltaTime);
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

public enum InvestigateState
{
    Look,
    Dialog,
    Fight
}

public enum NpcTargets
{
    Outside,
    EnterWay,
    Inside,
}

public enum WalkEnum
{
    Idle,
    NormalWalk,
    HappyWalk,
    SadWalk,
    DrunkWalk,
    QueenWalk,
    KingWalk,
}
