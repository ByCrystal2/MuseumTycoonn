using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class NPCBetterBehaviour : MonoBehaviour
{
    [SerializeField] private Transform TargetObject;
    [SerializeField] private List<NavigationHandler.WayPointRuntime> TargetPositions;
    [SerializeField] private AudioSource CurrentAudioSource;
    [SerializeField] private NPCState CurrentState;
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
                //DoToilet();
            }
        }
    }
    public float Education; //0-10; Egitimli insanlar daha cok kultur kazandirir.
    public float AdditionalLuck; //sans oranini buradan arttirabiliriz.

    public int NpcVisittingArtAmount;

    bool DecidedToEnter;

    bool isGidis;
    public float IsBusy; //Mesgul mu?
    public bool IsMale; //Erkek mi?
    public List<Renderer> npcRenderers = new();
    public List<Canvas> myCanvasses = new();

    bool isStopped;
    LODGroup myLOD;

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
        if (!_exceptWalk)
            anim.SetInteger("Walk", 0);
        anim.SetInteger("Hit", 0);
        anim.SetInteger("GetHit", 0);
        anim.SetInteger("Fall", 0);
        anim.SetInteger("inFight", 0);
        anim.SetInteger("Look", 0);
        anim.SetInteger("MakeMess", 0);
    }

    //IEnumerator StartFight(bool _IWillBeat)
    //{
    //    if (_IWillBeat)
    //    {
    //        int kickid = Random.Range(1, 3);
    //        SetCurrentAnimationState("Dialog", -100 - kickid);
    //        while (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Kick" + kickid)
    //        {
    //            Debug.Log("Anim.GetCurrentAnimatorClipInfo(0)[0].clip.name: " + anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
    //            if (!AnimActive())
    //                break;

    //            yield return new WaitForEndOfFrame();
    //        }
    //        AudioManager.instance.PlayPunchSound(CurrentAudioSource);

    //        float timer = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
    //        //AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcByeBye, CurrentAudioSource);
    //        while (timer > 0)
    //        {
    //            if (!AnimActive())
    //                break;
    //            if (DialogTarget != null)
    //                LookAtOptimal(DialogTarget.transform.position);
    //            timer -= Time.deltaTime;
    //            yield return new WaitForEndOfFrame();
    //        }

    //        //NpcManager.instance.AddGuiltyNPC(this);
    //        OnBeginGuilty();
    //        SetCurrentAnimationState("Dialog", -1);
    //        while (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "AfterKick")
    //        {
    //            if (!AnimActive())
    //                break;
    //            Debug.Log("Anim.GetCurrentAnimatorClipInfo(0)[0].clip.name: " + anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
    //            yield return new WaitForEndOfFrame();
    //        }

    //        timer = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
    //        //AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcByeBye, CurrentAudioSource);
    //        while (timer > 0)
    //        {
    //            if (!AnimActive())
    //                break;
    //            if (DialogTarget != null)
    //                LookAtOptimal(DialogTarget.transform.position);
    //            timer -= Time.deltaTime;
    //            yield return new WaitForEndOfFrame();
    //        }

    //        Stress /= Stress;
    //        isStopped = false;
    //        DialogTarget = null;
    //        SetCurrentAnimationState("Dialog", 0);
    //        IsBusy = Time.time - 1;
    //        if (OnNPCInvestigatedArt())
    //            CreateTarget();
    //        else
    //            OnExitWayMuseum();
    //    }
    //    else
    //    {
    //        yield return new WaitForSeconds(0.6f);

    //        SetCurrentAnimationState("Dialog", -2);
    //        while (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "FallAnimation")
    //        {
    //            if (!AnimActive())
    //                break;
    //            Debug.Log("Anim.GetCurrentAnimatorClipInfo(0)[0].clip.name: " + anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
    //            yield return new WaitForEndOfFrame();
    //        }

    //        float timer = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
    //        //AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcByeBye, CurrentAudioSource);
    //        while (timer > 0)
    //        {
    //            if (!AnimActive())
    //                break;
    //            if (DialogTarget != null)
    //                LookAtOptimal(DialogTarget.transform.position);
    //            timer -= Time.deltaTime;
    //            yield return new WaitForEndOfFrame();
    //        }

    //        Stress += 50;
    //        int x = Random.Range(4, 11);
    //        PlayBeatenEffect(true);
    //        yield return new WaitForSeconds(x);
    //        SetCurrentAnimationState("Dialog", -1);

    //        while (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "StandUp")
    //        {
    //            if (!AnimActive())
    //                break;
    //            Debug.Log("Anim.GetCurrentAnimatorClipInfo(0)[0].clip.name: " + anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
    //            yield return new WaitForEndOfFrame();
    //        }

    //        timer = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
    //        //AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcByeBye, CurrentAudioSource);
    //        while (timer > 0)
    //        {
    //            if (!AnimActive())
    //                break;
    //            if (DialogTarget != null)
    //                LookAtOptimal(DialogTarget.transform.position);
    //            timer -= Time.deltaTime;
    //            yield return new WaitForEndOfFrame();
    //        }

    //        isStopped = false;
    //        DialogTarget = null;
    //        SetCurrentAnimationState("Dialog", 0);
    //        IsBusy = Time.time - 1;
    //        PlayBeatenEffect(false);
    //        if (OnNPCInvestigatedArt())
    //            CreateTarget();
    //        else
    //            OnExitWayMuseum();
    //    }
    //}

    //public void Beaten()
    //{
    //    //StopAllCoroutines();
    //    StartCoroutine(BeatenNumerator());
    //    OnEndGuilty();
    //}

    //public IEnumerator BeatenNumerator()
    //{
    //    isStopped = true;
    //    IsBusy = Time.time + 10;
    //    SetCurrentAnimationState("Dialog", 0, "Walk", 0);
    //    yield return new WaitForSeconds(0.6f);

    //    SetCurrentAnimationState("Dialog", -2, "Walk", 0);
    //    while (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "FallAnimation")
    //    {
    //        if (!AnimActive())
    //            break;
    //        Debug.Log("Anim.GetCurrentAnimatorClipInfo(0)[0].clip.name: " + anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
    //        yield return new WaitForEndOfFrame();
    //    }

    //    float timer = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
    //    //AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcByeBye, CurrentAudioSource);
    //    while (timer > 0)
    //    {
    //        if (!AnimActive())
    //            break;
    //        if (DialogTarget != null)
    //            LookAtOptimal(DialogTarget.transform.position);
    //        timer -= Time.deltaTime;
    //        yield return new WaitForEndOfFrame();
    //    }

    //    Stress += 50;
    //    int x = Random.Range(4, 11);
    //    PlayBeatenEffect(true);
    //    yield return new WaitForSeconds(x);
    //    SetCurrentAnimationState("Dialog", -1);

    //    while (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "StandUp")
    //    {
    //        if (!AnimActive())
    //            break;
    //        Debug.Log("Anim.GetCurrentAnimatorClipInfo(0)[0].clip.name: " + anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
    //        yield return new WaitForEndOfFrame();
    //    }

    //    timer = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
    //    //AudioManager.instance.GetDialogAudios(MySources, DialogType.NpcByeBye, CurrentAudioSource);
    //    while (timer > 0)
    //    {
    //        if (!AnimActive())
    //            break;
    //        if (DialogTarget != null)
    //            LookAtOptimal(DialogTarget.transform.position);
    //        timer -= Time.deltaTime;
    //        yield return new WaitForEndOfFrame();
    //    }

    //    isStopped = false;
    //    DialogTarget = null;
    //    SetCurrentAnimationState("Dialog", 0);
    //    IsBusy = Time.time - 1;
    //    PlayBeatenEffect(false);
    //    OnExitWayMuseum(2f);
    //}


    //IEnumerator WaitingForIsPointerOver()
    //{
    //    //for (int i = 0; i < 3; i++)
    //    yield return new WaitForEndOfFrame();

    //    if (!UIController.instance.IsPointerOverAnyUI())
    //    {
    //        Debug.Log("NPC'ye tiklandi/dokunuldu.");
    //        if (GameManager.instance.GetCurrentGameMode() == GameMode.FPS)
    //        {
    //            PlayerManager.instance.LockPlayer();
    //        }
    //        //NpcManager.instance.CurrentNPC = this;
    //        UIController.instance.NpcInformationPanel.SetActive(true);
    //        UIController.instance.SetNPCInfoPanelUIs(MyName, Happiness, Stress, Toilet, Education, LikedColors, LikedArtist);
    //        //if (!(CurrentInvestigate == InvestigateState.Fight || CurrentInvestigate == InvestigateState.Dialog || CurrentInvestigate == InvestigateState.Look)) // InvestiagetState sistemi yanlis calisiyor. Bundan dolayi if icerisinde donen bool degerin tersi alindi (Orn: npc tabloya bakmamasina ragmen, InvestiagetState Look oluyor.)
    //        //{
    //        //    SetMyCamerasActivation(false, true);
    //        //}
    //        //else
    //        //{
    //        //    SetMyCamerasActivation(true, false);
    //        //}
    //    }
    //}
    //private void OnMouseDown()
    //{
    //    //StopAllCoroutines();
    //    StartCoroutine(WaitingForIsPointerOver());
    //}

    //public void DoToilet()
    //{
    //    if (IsBusy > Time.time)
    //        return;

    //    int MaxCultureFactorEffect = 30;

    //    int cultureFactor = Mathf.Clamp(MuseumManager.instance.GetCurrentCultureLevel(), 0, MaxCultureFactorEffect);
    //    float ToiletChance = 30 + (Stress * 0.5f) - cultureFactor;
    //    if (ToiletChance < 0)
    //        ToiletChance = 0;

    //    float skillFactor = 0; //skillden (-) bonus gelmesi lazim. 

    //    float change = Random.Range(0, 101);
    //    Debug.Log("ToiletChance: " + ToiletChance + " / Current chance: " + change);
    //    if (change < ToiletChance)
    //    {
    //        SetCurrentAnimationState("MakeMess", 1);
    //        IsBusy = Time.time + 3;
    //        isStopped = true;
    //        Invoke(nameof(CreateMess), 1f);
    //        Toilet = 0;
    //        Stress = Stress / 2;
    //    }
    //    Toilet = 0;

    //}

    //public void CreateMess()
    //{
    //    if (CurrentVisitedRoom == null)
    //        return;

    //    GameObject npc_Mess;
    //    int messChance = Random.Range(0, 3);
    //    if (messChance == 0)
    //        npc_Mess = Instantiate(Resources.Load<GameObject>("NPC/NPC_Poop"), transform.position, Quaternion.identity);
    //    else if (messChance == 1)
    //        npc_Mess = Instantiate(Resources.Load<GameObject>("NPC/NPC_Gas"), transform.position, Quaternion.identity);
    //    else
    //        npc_Mess = Instantiate(Resources.Load<GameObject>("NPC/NPC_Pee"), transform.position, Quaternion.identity);

    //    npc_Mess.GetComponent<NpcMess>().SetCurrentRoom(CurrentVisitedRoom);
    //    NpcManager.instance.AddMessIntoMessParent(npc_Mess.transform);
    //    IsBusy = Time.time - 1;
    //    isStopped = false;
    //    SetCurrentAnimationState("MakeMess", 0);
    //}
}
