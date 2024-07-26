using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class NPCBehaviour : MonoBehaviour
{
    [SerializeField] private Transform TargetObject;
    [SerializeField] private List<NavigationHandler.WayPointRuntime> TargetPositions;
    [SerializeField] private AudioSource CurrentAudioSource;
    [SerializeField] private NPCState CurrentState;
    [SerializeField] private NpcTargets CurrentTarget;
    [SerializeField] private InvestigateState CurrentInvestigate;
    //[SerializeField] public NavMeshAgent Agent;
    [SerializeField] private float NpcSpeed;
    [SerializeField] private float NpcCurrentSpeed;
    [SerializeField] private float NpcRotationSpeed;
    [SerializeField] private Vector3 OutsidePosition;
    [SerializeField] private NPCBehaviour DialogTarget;
    [SerializeField] private NPCUI MyNPCUI;
    [SerializeField] private Transform BeatenEffectParent;
    public RoomData CurrentVisitedRoom;

    //UI
    [SerializeField] string MyName = "Ahmet Burak"; // Default olarak Ahmet Burak atildi.
    private RawImage myRawImage;
    private Camera myDefaultCamera;
    private Camera myWorkCamera;

    public float IdleLength;
    private float IdleTimer;

    public WalkEnum NpcWalkType;

    Animator anim;

    public int VisitableArtAmount;

    public List<MyColors> LikedColors = new List<MyColors>(); //3renk
    public MyColors DislikedColor;
    public List<string> LikedArtist;
    public float Happiness = 50; //0-100; Mutlu npc daha fazla sanat gezmek isteyecek. Ve npc hizini etkileyecek.

    public List<LocationData> InvestigatedAreas = new List<LocationData>();

    private float DialogThreshold;
    private float _stress;
    public float Stress
    {
        get { return _stress; }
        private set
        {
            _stress = value;
            MyNPCUI.UpdateStressBar(_stress);
        }
    }
    //0-100; Stressli npc daha az sanat gormek isteyecek. Ve npc hizini etkileyecek.
    private float _toilet;
    public float Toilet
    {
        get { return _toilet; }
        private set
        {
            //Debug.Log("new Toilet: " + value);
            _toilet = value;
            if (_toilet >= 100)
            {
                _toilet = 100;
                DoToilet();
            }
        }
    }
    public float Education; //0-10; Egitimli insanlar daha cok kultur kazandirir.
    public float AdditionalLuck; //sans oranini buradan arttirabiliriz.

    public int NpcVisittingArtAmount;

    bool DecidedToEnter;

    bool isGidis;
    public bool IsBusy; //Mesgul mu?
    public bool IsMale; //Erkek mi?
    public NpcDialogState npcDialogState;
    public List<Renderer> npcRenderers = new();
    public List<Canvas> myCanvasses = new();

    bool isStopped;
    LODGroup myLOD;
    private void Awake()
    {
        npcRenderers = GetComponentsInChildren<Renderer>().ToList();
        myCanvasses = GetComponentsInChildren<Canvas>().ToList();
        myLOD = GetComponent<LODGroup>();
        int length = transform.childCount;
        for (int i = 0; i < length; i++)
        {
            if (transform.GetChild(i).CompareTag("NPCDefaultCamera"))
                myDefaultCamera = transform.GetChild(i).GetComponent<Camera>();
            else if(transform.GetChild(i).CompareTag("NPCWorkCamera"))
                myWorkCamera = transform.GetChild(i).GetComponent<Camera>();
        }
    }

    void Start()
    {
        MyName = Constant.instance.GetNPCName(IsMale);
        LikedArtist = Constant.instance.GetRandomFamousPaintersWithDesiredCount(Random.Range(1, 4));
        CurrentAudioSource = transform.GetChild(0).GetComponent<AudioSource>();
        AdditionalLuck = 20;
        NpcCurrentSpeed = NpcSpeed + NpcSpeed * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.VisitorsSpeedIncrease] / 100f); ;
        NpcRotationSpeed = 5;
        anim = GetComponent<Animator>();
        CurrentTarget = NpcTargets.Outside;
        float s = (int)Random.Range(-100, 51) * 0.01f;
        OutsidePosition = Vector3.zero;
        IdleBack();

        Vector3 spawnPoint = Vector3.zero;
        int x = Random.Range(0, 2);
        if (x == 0)
        {
            Transform TargetPosition = NpcManager.instance.GidisListe[Random.Range(0, NpcManager.instance.GidisListe.Count)];
            //spawnPoint = TargetPosition.position + new Vector3(Random.Range(-29, 30) * 0.1f, 0, Random.Range(-29, 30) * 0.1f);
            spawnPoint = TargetPosition.position;
            isGidis = true;
        }
        else
        {
            Transform TargetPosition = NpcManager.instance.GelisListe[Random.Range(0, NpcManager.instance.GelisListe.Count)];
            //spawnPoint = TargetPosition.position + new Vector3(Random.Range(-29, 30) * 0.1f, 0, Random.Range(-29, 30) * 0.1f);
            spawnPoint = TargetPosition.position;
            isGidis = false;
        }

        //NavMeshHit hit;
        //if (NavMesh.SamplePosition(spawnPoint, out hit, 10, NavMesh.AllAreas))
        //    spawnPoint = hit.position;

        transform.position = spawnPoint;
    }

    private void FixedUpdate()
    {
        bool visible = npcRenderers[0].isVisible;
            foreach (var item in myCanvasses)
                item.gameObject.SetActive(visible);
        anim.enabled = visible;
        if (!visible)
            ResetAnimations(false);
    }
    private float timer = 0f;
    private float interval = 0.033f; // 2 frame beklemek icin 1/30 saniye (30 FPS)
    // Update is called once per frame
    void Update()
    {
        //timer += Time.deltaTime;
        //if (timer < interval) return;
        //timer = 0f;
        if (TargetPositions.Count == 0)
        {
            if (IsBusy)
                return;
            CreateTarget();
            return;
        }

        if (CurrentState == NPCState.Idle)
        {
            if (IsBusy)
                return;
            if (IdleTimer < Time.time)
            {
                CreateTarget();
            }
            SetCurrentAnimationState("Walk", 0);
        }
        else if (CurrentState == NPCState.Move)
        {
            if (IsBusy)
                return;
            if (IdleTimer > Time.time)
                return;
            Move();
        }
        else if (CurrentState == NPCState.Investigate)
        {
            SetCurrentAnimationState("Walk", 0);
            if (CurrentInvestigate == InvestigateState.Dialog)
            {
                if (DialogThreshold < Time.time)
                {
                    OnDialogEnd(true);
                    return;
                }
                Debug.Log("end Dialog");
                if (IdleTimer < Time.time)
                {
                    Debug.Log("Dialog end with npc: " + name + " /dialogTarget: " + DialogTarget.name);
                    OnDialogEnd();
                    CurrentAudioSource.Stop();
                    //AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcByeBye, CurrentAudioSource);
                    return;
                }
                if(DialogTarget != null)
                    LookAtOptimal(DialogTarget.transform.position);
                return;
            }
            LookAtOptimal(TargetPositions[0].Position);
        }
    }

    public void MoveOpt(Vector3 target)
    {
        float distance = Vector3.Distance(transform.position, TargetPositions.Count > 0 ? TargetPositions[0].Position : target);
        if (distance < 1f)
        {
            if (TargetPositions.Count > 1)
            {
                TargetPositions.RemoveAt(0);
                return;
            }
            Transform pic = TargetObject.parent;
            PictureElement PE;
            if(pic.TryGetComponent(out PE))
                InvestigateStart();
            else
                IdleBack();
        }
        if (TargetPositions.Count > 0)
        {
            if(DialogTarget != null)
                Debug.Log("dialog targeti olmasina ragmen Move a gecti.");
            MoveToPath();
            ResetAnimations(true);
            SetCurrentAnimationState("Walk", (int)NpcWalkType, "Dialog", 0);
        }
    }

    void MoveToPath()
    {
        if (TargetPositions.Count == 0)
            return;

        Vector3 targetWaypoint = TargetPositions[0].Position;
        Vector3 direction = (targetWaypoint - transform.position).normalized;
        float step = NpcCurrentSpeed * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, step);

        Vector3 lookDirection = new Vector3(direction.x, 0, direction.z);
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, step);
        }
    }

    public void InvestigateStart()
    {
        Debug.Log("Investigate Start.");
        PictureElement PE = TargetObject.GetComponentInParent<PictureElement>();
        if (PE._pictureData.TextureID == 0)
        {
            CurrentState = NPCState.Move;
            CreateTarget();
            return;
        }
        Debug.Log("Investigate Start");
        isStopped = true;
        CurrentState = NPCState.Investigate;

        CurrentInvestigate = InvestigateState.Look;
        SetCurrentAnimationState("Look", 1);
        Invoke(nameof(LookEndManualChecker), 3);
        //IdleTimer = Time.time + IdleLength;
    }

    void LookEndManualChecker()
    {
        OnEndInvestigatePicture();
    }

    public void Move()
    {
        if (CurrentTarget == NpcTargets.Inside)
        {
            Toilet += Time.deltaTime * 4f;
            MoveOpt(TargetPositions[0].Position);
        }
        else if (CurrentTarget == NpcTargets.Outside)
            MoveOpt(OutsidePosition);
        else if (CurrentTarget == NpcTargets.EnterWay)
            MoveOpt(TargetPositions[0].Position);
    }

    public void CreateTarget()
    {
        if (TargetPositions.Count > 1)
        {
            return;
        }
        if (CurrentTarget == NpcTargets.Outside)
        {
            if (isGidis)
            {
                int index = Random.Range(0, NpcManager.instance.GidisListe.Count -1);
                int x = Random.Range(0, 101);
                x = (int)(x + (float)x * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.WantVisittingRatio] / 100f));
                if (x >= 80)
                {
                    if (!MuseumManager.instance.IsMuseumFull())
                    {
                        Debug.Log("Npc decided to enter museum.");
                        OnDecidedEnterMuseum();
                        TargetPositions = NavigationHandler.instance.CreateNavigation(transform, TargetObject);
                        return;
                    }
                    else
                    {
                            
                    }
                }
                TargetObject = NpcManager.instance.GidisListe[index];
                TargetPositions = NavigationHandler.instance.CreateNavigation(transform, TargetObject);
            }
            else
            {
                int index = Random.Range(0, NpcManager.instance.GidisListe.Count - 1);
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
                    else
                    {
                        Debug.Log("Museum is full.");
                    }
                }                
                TargetObject = NpcManager.instance.GelisListe[index];
                TargetPositions = NavigationHandler.instance.CreateNavigation(transform, TargetObject);
            }
            //OutsidePosition = TargetPosition.position + new Vector3(Random.Range(-29, 30) * 0.1f, 0, Random.Range(-29, 30) * 0.1f);
            OutsidePosition = TargetObject.position;

            //NavMeshHit hit;
            //if (NavMesh.SamplePosition(OutsidePosition, out hit, 10, NavMesh.AllAreas))
            //    OutsidePosition = hit.position;
            
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
                    float distance = Vector3.Distance(transform.position, NpcManager.instance.Locations[i].transform.position);
                    if (distance <= NpcManager.instance.NpcMaxMoveDistance)
                    {
                        if (!NpcManager.instance.Locations[i].GetComponent<LocationData>().isLocked && NpcManager.instance.Locations[i] != TargetObject &&
                        MuseumManager.instance.CurrentNpcs[u].TargetObject != NpcManager.instance.Locations[i].transform)
                            possible.Add(NpcManager.instance.Locations[i]);
                    }
                }
            }

            int visittedCount = InvestigatedAreas.Count;
            for (int i = 0; i < visittedCount; i++)
                if (possible.Contains(InvestigatedAreas[i]))
                    possible.Remove(InvestigatedAreas[i]);
            
            if (possible.Count == 0)
            {
                Debug.LogError("Gidilecek hic bir yer yok!.");
                MyNPCUI.PlayEmotionEffect(NpcEmotionEffect.Sadness);
                Happiness -= Time.deltaTime * (Stress * 0.1f);
                Stress += Time.deltaTime * 2;
                CheckStats();
                OnExitWayMuseum();
            }
            else
            {
                
                LocationData newTargetLocation = NpcManager.instance.Locations[Random.Range(0, length)];
                if (!InvestigatedAreas.Contains(newTargetLocation))
                    InvestigatedAreas.Add(newTargetLocation);
                TargetObject = newTargetLocation.transform;
                UpdateNavigation();
                CurrentState = NPCState.Move;                
            }
        } 
        else if (CurrentTarget == NpcTargets.EnterWay)
        {
            NpcArrivedTheEnterGate();
        }
    }

    void UpdateNavigation()
    {
        if(TargetObject == null)
        {
            Debug.LogError(transform.name + " npc bir target objeye sahip degil. Target obje olmadan navigasyon ayarlanamaz.");
            return;
        }
        TargetPositions = NavigationHandler.instance.CreateNavigation(transform, TargetObject);
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
            AudioManager.instance.GetDialogAudios(DialogType.NpcSad, CurrentAudioSource, IsMale);
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
        }
        else
        {
            NpcWalkType = WalkEnum.HappyWalk;
            float happySpeed = NpcSpeed * 1.25f;
            NpcCurrentSpeed = happySpeed + happySpeed * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.VisitorsSpeedIncrease] / 100f);
            AudioManager.instance.GetDialogAudios(DialogType.NpcHappiness, CurrentAudioSource, IsMale);
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
            TargetObject = NpcManager.instance.Enter2Point;
        else
            TargetObject = NpcManager.instance.Enter1Point;
        MuseumManager.instance.OnNpcEnteredMuseum(this);
    }

    bool OnNPCInvestigatedArt()
    {
        Debug.Log("EndOf Investigate");
        PictureElement PE = TargetObject.GetComponentInParent<PictureElement>();

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

        int length3 = LikedArtist.Count;
        bool isIncludeMyArtist = false;
        for (int i = 0; i < length3; i++)
        {            
            if (PE._pictureData.painterData.Description == LikedArtist[i])
            {
                isIncludeMyArtist = true;
                Debug.Log("Tablonun ressami, Npcnin sevdigi bir ressam. => " + LikedArtist[i]);
                break;
            }
        }
        if (isIncludeMyArtist)
            NPCMinScore++;
        else
            NPCMaxScore -= Random.Range(1, 4);


        NPCCurrentScore = Random.Range(NPCMinScore, NPCMaxScore + 1);
        MuseumManager.instance.AddCultureExp(NPCCurrentScore * 3);
        int multiplyScore = 0;
        switch (PE._pictureData.painterData.StarCount)
        {
            case 0:
            case 1:
                multiplyScore = 2;
                break;
            case 2:
                multiplyScore = 4;
                break;
            case 3:
                multiplyScore = 6;
                break;
            case 4:
                multiplyScore = 8;
                break;
            case 5:
                multiplyScore = 10;
                break;
            default:
                break;
        }
        MuseumManager.instance.OnNpcPaid(NPCCurrentScore * multiplyScore);
        Debug.Log(name + " adli npc tablo yorumundan kazanilan para: " + "NPCCurrentScore * PE._pictureData.painterData.StarCount => " + NPCCurrentScore * multiplyScore);
        // MuseumManager.instance.AddTotalVisitorCommentCount(1);
        MuseumManager.instance.OnNpcCommentedPicture(this,PE, Mathf.Round(NPCCurrentScore)); // Þimdilik Current Score Göre Yýldýz Yorumu Yapýldý.

        int center = 5;
        if (NPCCurrentScore < center)
        {
            AudioManager.instance.GetDialogAudios(DialogType.NpcDisLike, CurrentAudioSource, IsMale);
            Stress += (5 - NPCCurrentScore) * 2;
            Happiness -= Mathf.Round(Stress * 0.2f);
            MyNPCUI.PlayEmotionEffect(NpcEmotionEffect.Sadness);
        }
        else
        {
            AudioManager.instance.GetDialogAudios(DialogType.NpcLike, CurrentAudioSource, IsMale);
            Stress -= (NPCCurrentScore - 5) * 2;
            int _happiness = (int)Mathf.Round((100 - Stress) * 0.05f);
            _happiness = (int)(_happiness + (float)_happiness * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.HappinessIncreaseRatio] / 100f));
            Happiness += _happiness;
            MyNPCUI.PlayEmotionEffect(NpcEmotionEffect.Happiness);
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
        
    }
    void OnExitWayMuseum(float _multiplier = 1)
    {
        OnEndGuilty();
        NpcManager.instance.RemoveGuiltyNPC(this);
        CurrentTarget = NpcTargets.Outside;
        CurrentState = NPCState.Move;
        Happiness = 100;
        Stress = 0;
        NpcWalkType = WalkEnum.NormalWalk;
        NpcCurrentSpeed = (NpcSpeed + NpcSpeed * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.VisitorsSpeedIncrease] / 100f) * _multiplier); //3.3
        TargetObject = NpcManager.instance.GidisListe[Random.Range(0, NpcManager.instance.GidisListe.Count)];

        //OutsidePosition = TargetPosition.position + new Vector3(Random.Range(-29, 30) * 0.1f, 0, Random.Range(-29, 30) * 0.1f);
        OutsidePosition = TargetObject.position;

        //NavMeshHit hit;
        //if (NavMesh.SamplePosition(OutsidePosition, out hit, 10, NavMesh.AllAreas))
        //    OutsidePosition = hit.position;

        SetCurrentAnimationState("Walk", _multiplier == 1 ? (int)NpcWalkType : 102, "Dialog", 0);
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

    void ResetAnimations(bool _exceptWalk)
    {
        anim.SetInteger("Liked", 0);
        anim.SetInteger("NoLiked", 0);
        int currentDialog = anim.GetInteger("Dialog");
        if (currentDialog == 3)
            anim.SetInteger("Dialog", -101);
        else if (currentDialog == 1 || currentDialog == 2)
            anim.SetInteger("Dialog", -1);
        else if (currentDialog == -1)
            anim.SetInteger("Dialog", 0);
        if(!_exceptWalk)
            anim.SetInteger("Walk", 0);
        anim.SetInteger("Hit", 0);
        anim.SetInteger("GetHit", 0);
        anim.SetInteger("Fall", 0);
        anim.SetInteger("inFight", 0);
        anim.SetInteger("Look", 0);
        anim.SetInteger("MakeMess", 0);
    }

    public void OnEndInvestigatePicture()
    { // Tabloya bakma animasyonu (Looking) bitisinde cagirilan method.
        Debug.Log("Looking end.");
        CancelInvoke(nameof(LookEndManualChecker));
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
            DialogThreshold = Time.time + 10;
            DialogTarget = npcTarget;
            IsBusy = true;
            Debug.Log("IdleTimer: " + IdleTimer + " /Dialog start with npc: " + name + " /dialogTarget: " + DialogTarget.name);
            SetCurrentAnimationState("Dialog", Random.Range(1, 4));            
            AudioManager.instance.GetDialogAudios(DialogType.NpcTalking, CurrentAudioSource, IsMale);            
            return;
        }

        isStopped = false;
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
            if (currentNpc.TargetObject.parent == TargetObject.parent && currentNpc.CurrentState == NPCState.Investigate &&
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
            DialogThreshold = Time.time + 10;
            theDialogPartner.CurrentState = NPCState.Investigate;
            theDialogPartner.IsBusy = true;
            theDialogPartner.SetCurrentAnimationState("Dialog", Random.Range(1, 4), "Look", 0);

            timer = Time.time + Random.Range(5,15);
            theDialogPartner.IdleTimer = timer;
            Debug.Log("theDialogPartner.IdleTimer: " + theDialogPartner.IdleTimer + " /Dialog end with npc: " + theDialogPartner.name + " /dialogTarget: " + theDialogPartner.DialogTarget.name);
        }

        return (timer, theDialogPartner);
    }

    public void OnDialogEnd(bool _quickEnd = false)
    {
        if (npcDialogState != NpcDialogState.None)
            return;
        IdleTimer = Time.time + 1;
        Debug.Log("Dialog end.");

        StopAllCoroutines();
        if (_quickEnd || DialogTarget == null)
        {
            IsBusy = false;
            SetFarewellMode();
            return;
        }
        if (transform.GetInstanceID() > DialogTarget.transform.GetInstanceID())
        {
            if (Stress >= 0)
            {
                int luck = Random.Range(0 + (int)Stress, 101);
                if (luck >= (70 - Stress))
                {
                    bool amIbeat;
                    if (DialogTarget._stress == _stress)
                    {
                        int otherPriority = DialogTarget.transform.GetInstanceID();
                        amIbeat = transform.GetInstanceID() > otherPriority;
                    }
                    else
                    {
                        amIbeat = _stress > DialogTarget._stress;
                    }
                    SetFightMode(amIbeat);
                    DialogTarget.SetFightMode(!amIbeat);
                    return;
                }
            }
            SetFarewellMode();
            DialogTarget.SetFarewellMode();
        }
    }

    public void SetFarewellMode()
    {
        npcDialogState = NpcDialogState.Farewell;
        SetCurrentAnimationState("Dialog", -1);
        StopCoroutine(FarewellDelay());
        StartCoroutine(FarewellDelay());
    }

    public void SetFightMode(bool _IWillBeat)
    {
        npcDialogState = NpcDialogState.Combat;
        StartCoroutine(StartFight(_IWillBeat));
    }

    bool AnimActive()
    {
        Debug.Log("Npc gorus alaninda degil, aniamsyon oynamayacak.");
        OnDialogEnd();
        ResetAnimations(false);
        return anim.enabled;
    }
    IEnumerator StartFight(bool _IWillBeat)
    {
        if (_IWillBeat)
        {
            int kickid = Random.Range(1, 3);
            SetCurrentAnimationState("Dialog", -100 - kickid);
            while (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Kick" + kickid)
            {
                Debug.Log("Anim.GetCurrentAnimatorClipInfo(0)[0].clip.name: " + anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
                if(!AnimActive())
                    break;
                
                yield return new WaitForEndOfFrame();
            }
            AudioManager.instance.PlayPunchSound(CurrentAudioSource);

            float timer = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
            //AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcByeBye, CurrentAudioSource);
            while (timer > 0)
            {
                if (!AnimActive())
                    break;
                if (DialogTarget != null)
                    LookAtOptimal(DialogTarget.transform.position);
                timer -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            NpcManager.instance.AddGuiltyNPC(this);
            OnBeginGuilty();
            SetCurrentAnimationState("Dialog", -1);
            while (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "AfterKick")
            {
                if (!AnimActive())
                    break;
                Debug.Log("Anim.GetCurrentAnimatorClipInfo(0)[0].clip.name: " + anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
                yield return new WaitForEndOfFrame();
            }

            timer = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
            //AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcByeBye, CurrentAudioSource);
            while (timer > 0)
            {
                if (!AnimActive())
                    break;
                if (DialogTarget != null)
                    LookAtOptimal(DialogTarget.transform.position);
                timer -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            Stress /= Stress;
            isStopped = false;
            DialogTarget = null;
            CurrentInvestigate = InvestigateState.Look;
            SetCurrentAnimationState("Dialog", 0);
            npcDialogState = NpcDialogState.None;
            IsBusy = false;
            if (OnNPCInvestigatedArt())
                CreateTarget();
            else
                OnExitWayMuseum();
        }
        else
        {
            yield return new WaitForSeconds(0.6f);

            SetCurrentAnimationState("Dialog", -2);
            while (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "FallAnimation")
            {
                if (!AnimActive())
                    break;
                Debug.Log("Anim.GetCurrentAnimatorClipInfo(0)[0].clip.name: " + anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
                yield return new WaitForEndOfFrame();
            }

            float timer = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
            //AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcByeBye, CurrentAudioSource);
            while (timer > 0)
            {
                if (!AnimActive())
                    break;
                if (DialogTarget != null)
                    LookAtOptimal(DialogTarget.transform.position);
                timer -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            Stress += 50;
            int x = Random.Range(4, 11);
            PlayBeatenEffect(true);
            yield return new WaitForSeconds(x);
            SetCurrentAnimationState("Dialog", -1);

            while (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "StandUp")
            {
                if (!AnimActive())
                    break;
                Debug.Log("Anim.GetCurrentAnimatorClipInfo(0)[0].clip.name: " + anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
                yield return new WaitForEndOfFrame();
            }

            timer = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
            //AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcByeBye, CurrentAudioSource);
            while (timer > 0)
            {
                if (!AnimActive())
                    break;
                if (DialogTarget != null)
                    LookAtOptimal(DialogTarget.transform.position);
                timer -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            isStopped = false;
            DialogTarget = null;
            CurrentInvestigate = InvestigateState.Look;
            SetCurrentAnimationState("Dialog", 0);
            npcDialogState = NpcDialogState.None;
            IsBusy = false;
            PlayBeatenEffect(false);
            if (OnNPCInvestigatedArt())
                CreateTarget();
            else
                OnExitWayMuseum();
        }
    }

    public void PlayBeatenEffect(bool _isPlay)
    {
        ParticleSystem[] cleanParticle = BeatenEffectParent.GetComponentsInChildren<ParticleSystem>();
        foreach (var item in cleanParticle)
        {
            if (_isPlay)
                item.Play();
            else
                item.Stop();
        } 
    }

    public void OnBeginGuilty()
    {
        MyNPCUI.SetNPCasGuilty(true);
    }

    public void OnEndGuilty()
    {
        MyNPCUI.SetNPCasGuilty(false);
    }

    public void Beaten()
    {
        StopAllCoroutines();
        StartCoroutine(BeatenNumerator());
        OnEndGuilty();
    }

    public IEnumerator BeatenNumerator()
    {
        isStopped = true;
        IsBusy = true;
        SetCurrentAnimationState("Dialog", 0, "Walk", 0);
        yield return new WaitForSeconds(0.6f);

        SetCurrentAnimationState("Dialog", -2, "Walk", 0);
        while (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "FallAnimation")
        {
            if (!AnimActive())
                break;
            Debug.Log("Anim.GetCurrentAnimatorClipInfo(0)[0].clip.name: " + anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
            yield return new WaitForEndOfFrame();
        }

        float timer = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        //AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcByeBye, CurrentAudioSource);
        while (timer > 0)
        {
            if (!AnimActive())
                break;
            if (DialogTarget != null)
                LookAtOptimal(DialogTarget.transform.position);
            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Stress += 50;
        int x = Random.Range(4, 11);
        PlayBeatenEffect(true);
        yield return new WaitForSeconds(x);
        SetCurrentAnimationState("Dialog", -1);

        while (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "StandUp")
        {
            if (!AnimActive())
                break;
            Debug.Log("Anim.GetCurrentAnimatorClipInfo(0)[0].clip.name: " + anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
            yield return new WaitForEndOfFrame();
        }

        timer = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        //AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcByeBye, CurrentAudioSource);
        while (timer > 0)
        {
            if (!AnimActive())
                break;
            if (DialogTarget != null)
                LookAtOptimal(DialogTarget.transform.position);
            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        isStopped = false;
        DialogTarget = null;
        CurrentInvestigate = InvestigateState.Look;
        SetCurrentAnimationState("Dialog", 0);
        npcDialogState = NpcDialogState.None;
        IsBusy = false;
        PlayBeatenEffect(false);
        OnExitWayMuseum(2f);
    }

    IEnumerator FarewellDelay()
    {
        float timerC = Time.time + 5;
        yield return new WaitForSeconds(1);
        Debug.Log("First enter => anim.GetCurrentAnimatorClipInfo(0)[0].clip.name: " + anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
        while (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Farewell")
        {
            if (timerC < Time.time)
                break;
            if (!AnimActive())
                break;
            Debug.Log("Anim.GetCurrentAnimatorClipInfo(0)[0].clip.name: " + anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
            yield return new WaitForEndOfFrame();
        }

        float timer = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        //AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcByeBye, CurrentAudioSource);
        while (timer > 0)
        {
            if (timerC < Time.time)
                break;
            if (!AnimActive())
                break;
            if (DialogTarget != null)
                LookAtOptimal(DialogTarget.transform.position);
            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        isStopped = false;
        DialogTarget = null;
        CurrentInvestigate = InvestigateState.Look;
        SetCurrentAnimationState("Dialog", 0);
        npcDialogState = NpcDialogState.None;
        IsBusy = false;
        if (OnNPCInvestigatedArt())
            CreateTarget();
        else
            OnExitWayMuseum();
    }

    void LookAtOptimal(Vector3 target)
    {
        Vector3 directionToTarget = target - transform.position;

        // Calculate the angle in degrees to rotate towards the target
        float angle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;

        // Create a rotation that only affects the Y axis
        Quaternion targetRotation = Quaternion.Euler(0, angle, 0);

        // Apply the rotation to the NPC's transform
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, NpcRotationSpeed * Time.deltaTime);
    }
    
    public void SetMyCamerasActivation(bool _DefaultCameraActivation, bool _WorkCameraActivation)
    {
        if (_DefaultCameraActivation)
            myDefaultCamera.gameObject.SetActive(true);
        else
            myDefaultCamera.gameObject.SetActive(false);

        if (_WorkCameraActivation)
            myWorkCamera.gameObject.SetActive(true);
        else 
            myWorkCamera.gameObject.SetActive(false);
    }

    public void IncreaseHappiness(int _amount)
    {
        Happiness += _amount;
        Stress -= _amount;

        MyNPCUI.PlayEmotionEffect(NpcEmotionEffect.Happiness);

        OnHappinessChange();
    }
    
    IEnumerator WaitingForIsPointerOver()
    {
        //for (int i = 0; i < 3; i++)
            yield return new WaitForEndOfFrame();

        if (!UIController.instance.IsPointerOverAnyUI())
        {
            Debug.Log("NPC'ye tiklandi/dokunuldu.");
            if (GameManager.instance.GetCurrentGameMode() == GameMode.FPS)
            {
                PlayerManager.instance.LockPlayer();
            }
            NpcManager.instance.CurrentNPC = this;
            UIController.instance.NpcInformationPanel.SetActive(true);
            UIController.instance.SetNPCInfoPanelUIs(MyName, Happiness, Stress, Toilet, Education, LikedColors, LikedArtist);
            if (!(CurrentInvestigate == InvestigateState.Fight || CurrentInvestigate == InvestigateState.Dialog || CurrentInvestigate == InvestigateState.Look)) // InvestiagetState sistemi yanlis calisiyor. Bundan dolayi if icerisinde donen bool degerin tersi alindi (Orn: npc tabloya bakmamasina ragmen, InvestiagetState Look oluyor.)
            {
                SetMyCamerasActivation(false, true);
            }
            else
            {
                SetMyCamerasActivation(true, false);
            }
        }
    }
    private void OnMouseDown()
    {
        StopAllCoroutines();
        StartCoroutine(WaitingForIsPointerOver());
    }

    public void DoToilet()
    {
        if (IsBusy)
            return;
        
        int MaxCultureFactorEffect = 30;

        int cultureFactor = Mathf.Clamp(MuseumManager.instance.GetCurrentCultureLevel(), 0, MaxCultureFactorEffect);
        float ToiletChance = 30 + (Stress * 0.5f) - cultureFactor;
        if (ToiletChance < 0)
            ToiletChance = 0;

        float skillFactor = 0; //skillden (-) bonus gelmesi lazim. 

        float change = Random.Range(0, 101);
        Debug.Log("ToiletChance: " + ToiletChance + " / Current chance: " + change);
        if (change < ToiletChance)
        {
            SetCurrentAnimationState("MakeMess", 1);
            IsBusy = true;
            isStopped = true;
            Invoke(nameof(CreateMess), 1f);
            Toilet = 0;
            Stress = Stress / 2;
        }
        Toilet = 0;

    }

    public void CreateMess()
    {
        GameObject npc_Mess;
        int messChance = Random.Range(0,3);
        if (messChance == 0)
            npc_Mess = Instantiate(Resources.Load<GameObject>("NPC/NPC_Poop"),transform.position,Quaternion.identity);
        else if (messChance == 1)
            npc_Mess = Instantiate(Resources.Load<GameObject>("NPC/NPC_Gas"), transform.position, Quaternion.identity);
        else
            npc_Mess = Instantiate(Resources.Load<GameObject>("NPC/NPC_Pee"), transform.position, Quaternion.identity);

        npc_Mess.GetComponent<NpcMess>().SetCurrentRoom(CurrentVisitedRoom);
        NpcManager.instance.AddMessIntoMessParent(npc_Mess.transform);
        IsBusy = false;
        isStopped = false;
        SetCurrentAnimationState("MakeMess", 0);
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

public enum NpcDialogState
{
    None,
    Farewell,
    Combat,
}
