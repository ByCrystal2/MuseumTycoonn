using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class NPCBehaviour : MonoBehaviour
{
    [Header("Main State")]
    [SerializeField] private NPCStateData npcState;

    [Header("Move System")]
    [SerializeField] private Transform TargetObject;
    [SerializeField] private List<NavigationHandler.WayPointRuntime> TargetPositions;

    [Header("Required Components")]
    [SerializeField] private Animator anim;
    [SerializeField] private AudioSource CurrentAudioSource;
    [SerializeField] private NPCUI MyNPCUI;
    [SerializeField] private Transform BeatenEffectParent;
    [SerializeField] private Camera myDefaultCamera;
    [SerializeField] private Camera myWorkCamera;
    [SerializeField] private List<Renderer> npcRenderers = new();
    [SerializeField] private List<Canvas> myCanvasses = new();

    [Header("NPC Data")]
    [SerializeField] private NPCGeneralData GeneralData;
    [SerializeField] private NPCStatData StatData;

    [Header("Runtime Datas (DEBUG)")]
    [SerializeField] private NPCBehaviour DialogTarget;
    [SerializeField] private RoomData CurrentVisitedRoom;
    [SerializeField] private float IdleTimer;
    [SerializeField] private int VisitableArtAmount;
    [SerializeField] private WalkEnum NpcWalkType;
    [SerializeField] private List<LocationData> InvestigatedAreas = new List<LocationData>();
    [SerializeField] private float BusyUntil;
    [SerializeField] private bool escapeAfterBeat;
    [SerializeField] private bool ReadyForDialog;

    void Start()
    {
        StatData.NpcCurrentSpeed = StatData.NpcSpeed + StatData.NpcSpeed * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.VisitorsSpeedIncrease] / 100f);

        transform.position = Random.Range(0,2) == 0 ? NpcManager.instance.GidisListe[Random.Range(0, NpcManager.instance.GidisListe.Count)].position : NpcManager.instance.GelisListe[Random.Range(0, NpcManager.instance.GelisListe.Count)].position;

        InvestigatedAreas.Clear();
    }

    private void FixedUpdate()
    {
        RefreshVisibility();

        if (BusyUntil > Time.time)
            return;

        if (npcState._mainState == NPCState.Idle)
        {
            if (IdleTimer < Time.time)
                Idle();
            else
            {
                CreateTarget();
                SetNPCState(NPCState.Move, true);
            }
        }
        else if (npcState._mainState == NPCState.Move)
        {
            Move();
        }
        else if (npcState._mainState == NPCState.Investigate)
        {
            if(TargetObject != null)
                LookAt(TargetObject.parent);
            if (IdleTimer < Time.time)
                Investigate();
            else if (IdleTimer < Time.time + 1)
                OnInvestigateEnd();
        }
        else if (npcState._mainState == NPCState.DialogFree)
        {
            if (IdleTimer < Time.time)
            {
                TargetObject?.gameObject.SetActive(true);
                NpcManager.instance.RemoveDialogReadyNPC(this);
                ReadyForDialog = false;
                ResetAnimator();
                anim.SetTrigger(NpcManager.AnimResetParam);
                SetNPCState(NPCState.Idle, true);
            }
            else
            {
                ReadyForDialog = true;
                DialogFree();
            }
        }
        else if (npcState._mainState == NPCState.Dialog)
        {
            if (DialogTarget == null)
            {
                TargetObject?.gameObject.SetActive(true);
                ResetAnimator();
                anim.SetTrigger(NpcManager.AnimResetParam);
                SetNPCState(NPCState.Idle, true);
                return;
            }

            Debug.Log(transform.name + " bir dialoga basladi.");
            if (IdleTimer < Time.time)
                Dialog();
            else if (IdleTimer < Time.time + 1)
                OnDialogEnd();
            LookAt(DialogTarget.transform);
        }
        else if (npcState._mainState == NPCState.Farewell)
        {
            if (IdleTimer < Time.time)
                Farewell();
            else if (IdleTimer < Time.time + 1)
                OnFarewellEnd();
        }
        else if (npcState._mainState == NPCState.CombatBeat)
        {
            if(IdleTimer < Time.time)
                CombatBeat();
        }
        else if (npcState._mainState == NPCState.CombatBeaten)
        {
            if(IdleTimer < Time.time)
                CombatBeaten();
        }
    }

    private void Idle()
    {
        IdleTimer = Time.time + NpcManager.delaysPerState[(int)NPCState.Idle];
        ResetAnimator();
        anim.SetTrigger(NpcManager.AnimResetParam);
        anim.SetInteger(NpcManager.AnimMoveParam, 0);
    }

    private void Move()
    {
        if (CurrentVisitedRoom != null)
            StatData.toilet += Time.deltaTime * 4f;

        if(IdleTimer < Time.time)
        {
            ResetAnimator();
            anim.SetTrigger(NpcManager.AnimResetParam);
            IdleTimer = Time.time + NpcManager.delaysPerState[(int)NPCState.Move] * 5;
        }

        anim.SetInteger(NpcManager.AnimMoveParam, escapeAfterBeat ? 102 : (int)NpcWalkType);

        if (TargetPositions.Count == 0)
        {
            Transform pic = TargetObject.parent;
            if (pic.TryGetComponent(out PictureElement PE))
            {
                SetNPCState(NPCState.Investigate, true);
                TargetObject.gameObject.SetActive(false);
            }
            else
            {
                ResetAnimator();
                anim.SetTrigger(NpcManager.AnimResetParam);
                SetNPCState(NPCState.Idle, true);
            }
        }
        else
        {
            float distance = Vector3.Distance(transform.position, TargetPositions[0].Position);
            if (distance < 1f)
            {
                if (TargetPositions.Count > 0)
                {
                    TargetPositions.RemoveAt(0);
                    return;
                }
            }
            else
            {
                if (DialogTarget != null)
                    Debug.Log("dialog targeti olmasina ragmen Move a gecti.");

                if (TargetPositions.Count == 0)
                    return;

                Vector3 targetWaypoint = TargetPositions[0].Position;
                Vector3 direction = (targetWaypoint - transform.position).normalized;
                float step = StatData.NpcCurrentSpeed * Time.deltaTime;

                transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, step);

                Vector3 lookDirection = new Vector3(direction.x, 0, direction.z);
                if (lookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, step);
                }
            }
        }
    }

    private void Investigate()
    {
        IdleTimer = Time.time + NpcManager.delaysPerState[(int)NPCState.Investigate];
        TargetPositions.Clear();
        PictureElement PE = TargetObject.GetComponentInParent<PictureElement>();
        if (PE._pictureData.TextureID == 0)
        {
            IdleTimer = Time.time - 1;
            CreateTarget();
            SetNPCState(NPCState.Move, true);
            return;
        }

        ResetAnimator();
        anim.SetTrigger(NpcManager.AnimResetParam);
        anim.SetInteger(NpcManager.AnimLookParam, 1);
    }

    private void OnInvestigateEnd()
    {
        TargetObject.gameObject.SetActive(false);
        PictureElement PE = TargetObject.GetComponentInParent<PictureElement>();
        if(PE != null && PE._pictureData.TextureID != 0)
        {
            CalculateInvestigateScore(PE);
        }

        float luck = Random.Range(0, 101);
        luck += (GetNpcHappiness() * 0.2f);
        luck += (StatData.additionalLuck * luck * 0.01f);
        Debug.Log("iletisim kurma istegi %" + luck);
        if (luck > 0)
        {
            NpcManager.instance.AddDialogReadyNpc(TargetObject.parent, this);
            SetNPCState(NPCState.DialogFree, true);
            IdleTimer = Time.time + NpcManager.delaysPerState[(int)NPCState.DialogFree];
        }
        else
        {
            TargetObject?.gameObject.SetActive(true);
            ResetAnimator();
            anim.SetTrigger(NpcManager.AnimResetParam);
            SetNPCState(NPCState.Idle, true);
        }
        VisitableArtAmount--;
    }

    void CalculateInvestigateScore(PictureElement PE)
    {
        int NPCMaxScore = 10;
        int NPCMinScore = 0;
        int NPCCurrentScore = 5;
        PictureElementData ped = MuseumManager.instance.GetPictureElementData(PE._pictureData.TextureID);
        List<MyColors> ArtColors = ped.MostCommonColors;
        int length = ArtColors.Count;
        int length2 = StatData.LikedColors.Count;
        for (int i = 0; i < length; i++)
        {
            for (int a = 0; a < length2; a++)
            {
                if (StatData.LikedColors[a] == ArtColors[i])
                    NPCMinScore++;
                if (ArtColors[i] == StatData.DislikedColor)
                    NPCMaxScore -= Random.Range(1, 4);
            }
        }

        int length3 = StatData.LikedArtist.Count;
        bool isIncludeMyArtist = false;
        for (int i = 0; i < length3; i++)
        {
            if (PE._pictureData.painterData.Description == StatData.LikedArtist[i])
            {
                isIncludeMyArtist = true;
                Debug.Log("Tablonun ressami, Npcnin sevdigi bir ressam. => " + StatData.LikedArtist[i]);
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

        int center = 5;
        if (NPCCurrentScore < center)
        {
            AudioManager.instance.GetDialogAudios(DialogType.NpcDisLike, CurrentAudioSource, GeneralData.isMale);
            ChangeCoreStressValue((5 - NPCCurrentScore) * 2);
            ChangeCoreHappinessValue(Mathf.Round(GetNpcStress() * 0.2f));
            MyNPCUI.PlayEmotionEffect(NpcEmotionEffect.Sadness);
        }
        else
        {
            AudioManager.instance.GetDialogAudios(DialogType.NpcLike, CurrentAudioSource, GeneralData.isMale);
            ChangeCoreStressValue((NPCCurrentScore - 5) * 2);
            int _happiness = (int)Mathf.Round((100 - GetNpcStress()) * 0.05f);
            _happiness = (int)(_happiness + (float)_happiness * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.HappinessIncreaseRatio] / 100f));
            ChangeCoreHappinessValue(_happiness);
            MyNPCUI.PlayEmotionEffect(NpcEmotionEffect.Happiness);
        }
    }

    private void DialogFree()
    {
        if (DialogTarget != null)
        {
            AudioManager.instance.GetDialogAudios(DialogType.NpcTalking, CurrentAudioSource, GeneralData.isMale);
            return;
        }

        NPCBehaviour theDialogPartner = NpcManager.instance.FindDialogPartner(this);
        if(theDialogPartner != null)
            AudioManager.instance.GetDialogAudios(DialogType.NpcTalking, CurrentAudioSource, GeneralData.isMale);
    }

    private void Dialog()
    {
        TargetObject.gameObject.SetActive(false);
        IdleTimer = Time.time + NpcManager.delaysPerState[(int)NPCState.Dialog];

        ResetAnimator();
        anim.SetTrigger(NpcManager.AnimResetParam);
        anim.SetInteger(NpcManager.AnimDialogParam, Random.Range(1,4));
    }

    private void OnDialogEnd()
    {
        Debug.Log(transform.name + " mevcut dialogunu sonlandirdi.");
        if (DialogTarget == null)
        {
            TargetObject?.gameObject.SetActive(true);
            ResetAnimator();
            anim.SetTrigger(NpcManager.AnimResetParam);
            SetNPCState(NPCState.Idle, true);
        }
        else
        {
            if (transform.GetInstanceID() > DialogTarget.transform.GetInstanceID())
            {
                int Stress = (int)GetNpcStress();
                int partnerStress = (int)DialogTarget.GetNpcStress();
                if (Stress >= 0)
                {
                    int luck = Random.Range(0 + (int)Stress, 101);
                    if (luck >= (70 - Stress))
                    {
                        bool amIbeat;
                        if (partnerStress == Stress)
                        {
                            int otherPriority = DialogTarget.transform.GetInstanceID();
                            amIbeat = transform.GetInstanceID() > otherPriority;
                        }
                        else
                        {
                            amIbeat = Stress > partnerStress;
                        }

                        SetNPCState(amIbeat ? NPCState.CombatBeat : NPCState.CombatBeaten, true);
                        DialogTarget.SetNPCState(amIbeat ? NPCState.CombatBeaten : NPCState.CombatBeat, true);
                        return;
                    }
                }
                SetNPCState(NPCState.Farewell, true);
                DialogTarget.SetNPCState(NPCState.Farewell, true);
            }
        }
    }
    
    private void Farewell()
    {
        if (DialogTarget == null)
        {
            TargetObject?.gameObject.SetActive(true);
            ResetAnimator();
            anim.SetTrigger(NpcManager.AnimResetParam);
            SetNPCState(NPCState.Idle, true);
        }
        else
        {
            LookAt(DialogTarget.transform);
            ResetAnimator();
            anim.SetTrigger(NpcManager.AnimResetParam);
            anim.SetInteger(NpcManager.AnimDialogParam, -1);
        }
    }

    private void OnFarewellEnd()
    {
        if (DialogTarget == null)
        {
            TargetObject?.gameObject.SetActive(true);
            SetNPCState(NPCState.Idle, true);
        }
        else
        {
            TargetObject?.gameObject.SetActive(true);
            DialogTarget = null;
            ResetAnimator();
            anim.SetTrigger(NpcManager.AnimResetParam);
            SetNPCState(NPCState.Idle, true);
        }
    }

    private void CombatBeat()
    {
        TargetObject?.gameObject.SetActive(true);
        ResetAnimator();
        anim.SetTrigger(NpcManager.AnimResetParam);
        SetNPCState(NPCState.Idle, true);
    }
    
    private void CombatBeaten()
    {
        TargetObject?.gameObject.SetActive(true);
        ResetAnimator();
        anim.SetTrigger(NpcManager.AnimResetParam);
        SetNPCState(NPCState.Idle, true);
    }

    private void ResetAnimator()
    {
        anim.SetInteger(NpcManager.AnimLookParam, 0);
        anim.SetInteger(NpcManager.AnimMoveParam, 0);
        anim.SetInteger(NpcManager.AnimDialogParam, 0);
    }

    public void SetNPCState(NPCState _newState, bool _force)
    {
        if (_force)
        {
            
            npcState._mainState = _newState;
            IdleTimer = Time.time - 1;
        }
        else
        {
            //Sartlar kontrol edilir. Eger state degismesi uygunsa degistirilir.
            //Kontrol --- -- --

            //Uygun
            npcState._mainState = _newState;
            IdleTimer = Time.time - 1;
        }
    }

    public void CreateTarget()
    {
        escapeAfterBeat = false;
        if (npcState._location == NpcLocationState.Outside)
        {
            bool gidis = Random.Range(0, 2) == 0;
            int index = Random.Range(0, gidis ? NpcManager.instance.GidisListe.Count - 1 : NpcManager.instance.GelisListe.Count - 1);

            int chance = Random.Range(0, 101);
            chance = (int)(chance + (float)chance * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.WantVisittingRatio] / 100f));
            if (chance >= 80)
            {
                if (!MuseumManager.instance.IsMuseumFull())
                {
                    Debug.Log("Npc decided to enter museum.");
                    TargetObject = gidis ? NpcManager.instance.Enter2Point : NpcManager.instance.Enter1Point;
                    TargetPositions = NavigationHandler.instance.CreateNavigation(transform, TargetObject);
                    npcState._location = NpcLocationState.EnterWay;
                    InvestigatedAreas.Clear();
                    MuseumManager.instance.OnNpcEnteredMuseum(this);
                    return;
                }
            }

            TargetObject = gidis ? NpcManager.instance.GidisListe[index] : NpcManager.instance.GelisListe[index];
            UpdateNavigation();
        }
        else if (npcState._location == NpcLocationState.Inside)
        {
            if (VisitableArtAmount <= 0)
            {
                OnExitWayMuseum(false);
                return;
            }

            int length = NpcManager.instance.Locations.Count;
            List<LocationData> possible = new List<LocationData>();
            for (int i = 0; i < length; i++)
            {
                float distance = Vector3.Distance(transform.position, NpcManager.instance.Locations[i].transform.position);
                if (distance <= NpcManager.instance.NpcMaxMoveDistance)
                {
                    if (NpcManager.instance.Locations[i].gameObject.activeSelf && !NpcManager.instance.Locations[i].GetComponent<LocationData>().isLocked)
                        possible.Add(NpcManager.instance.Locations[i]);
                }
            }

            int lengthVisit = possible.Count;
            int lengthVisit2 = InvestigatedAreas.Count;
            for (int i = lengthVisit - 1; i >= 0; i--)
            {
                bool found = false;
                for (int j = lengthVisit2 - 1; j >= 0; j--)
                {
                    if (possible[i].id == InvestigatedAreas[j].id)
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                    possible.RemoveAt(i);
            }

            if (possible.Count == 0)
            {
                Debug.LogError("Gidilecek hic bir yer yok!.");
                MyNPCUI.PlayEmotionEffect(NpcEmotionEffect.Sadness);
                ChangeHappinessValue(-1 * (Time.deltaTime * (GetNpcStress() * 0.1f)));
                ChangeCoreStressValue(Time.deltaTime * 2);
                OnExitWayMuseum(false);
            }
            else
            {

                LocationData newTargetLocation = possible[Random.Range(0, possible.Count)];
                if (!InvestigatedAreas.Contains(newTargetLocation))
                    InvestigatedAreas.Add(newTargetLocation);
                TargetObject = newTargetLocation.transform;
                UpdateNavigation();
            }
        }
        else if (npcState._location == NpcLocationState.EnterWay)
        {
            NpcArrivedTheEnterGate();
        }
    }

    void OnExitWayMuseum(bool _escape)
    {
        MuseumManager.instance.OnNpcExitedMuseum(this);
        escapeAfterBeat = _escape;
        CurrentVisitedRoom = null;
        OnEndGuilty();
        SetHappinessValue(100);
        SetStressValue(0);
        NpcWalkType = WalkEnum.NormalWalk;
        StatData.NpcCurrentSpeed = (StatData.NpcSpeed + StatData.NpcSpeed * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.VisitorsSpeedIncrease] / 100f) * (escapeAfterBeat ? 2f : 1f));
        TargetObject = NpcManager.instance.GidisListe[Random.Range(0, NpcManager.instance.GidisListe.Count)];
        npcState._location = NpcLocationState.Outside;
        UpdateNavigation();
    }

    public void NpcArrivedTheEnterGate()
    {
        VisitableArtAmount = Random.Range(2, 5);
        npcState._location = NpcLocationState.Inside;
        StatData.Happiness = 50;
        StatData.Stress = 0;
        StatData.toilet = Random.Range(0, 30);
        CreateTarget();
        SetNPCState(NPCState.Move, true);
    }

    void LookAt(Transform Target)
    {
        Vector3 directionToTarget = Target.position - transform.position;
        float angle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, StatData.NpcRotationSpeed * Time.deltaTime);
    }
    void UpdateNavigation()
    {
        if (TargetObject == null)
        {
            Debug.LogError(transform.name + " npc bir target objeye sahip degil. Target obje olmadan navigasyon ayarlanamaz.");
            return;
        }
        TargetPositions = NavigationHandler.instance.CreateNavigation(transform, TargetObject);
    }

    private void RefreshVisibility()
    {
        bool visible = npcRenderers[0].isVisible;
        foreach (var item in myCanvasses)
            item.gameObject.SetActive(visible);
        anim.enabled = visible;
        if (!visible)
        {
            ResetAnimator();
        }
    }

    public void OnBeginGuilty()
    {
        MyNPCUI.SetNPCasGuilty(true);
    }

    public void OnEndGuilty()
    {
        MyNPCUI.SetNPCasGuilty(false);
        NpcManager.instance.RemoveGuiltyNPC(this);
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

    void OnHappinessChange()
    {
        float happiness = GetNpcHappiness();
        if (happiness <= 25)
        {
            NpcWalkType = WalkEnum.SadWalk;
            float sadSpeed = StatData.NpcSpeed * 0.45f;
            StatData.NpcCurrentSpeed = sadSpeed + sadSpeed * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.VisitorsSpeedIncrease] / 100f);
            AudioManager.instance.GetDialogAudios(DialogType.NpcSad, CurrentAudioSource, GeneralData.isMale);
        }
        else if (happiness > 25 && happiness < 75)
        {
            NpcWalkType = WalkEnum.NormalWalk;
            StatData.NpcCurrentSpeed = StatData.NpcSpeed + StatData.NpcSpeed * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.VisitorsSpeedIncrease] / 100f); ;
        }
        else
        {
            NpcWalkType = WalkEnum.HappyWalk;
            float happySpeed = StatData.NpcSpeed * 1.25f;
            StatData.NpcCurrentSpeed = happySpeed + happySpeed * ((float)SkillTreeManager.instance.CurrentBuffs[(int)eStat.VisitorsSpeedIncrease] / 100f);
            AudioManager.instance.GetDialogAudios(DialogType.NpcHappiness, CurrentAudioSource, GeneralData.isMale);
        }
    }

    #region Editleme Fonksiyonlari
    public void SetEnteredRoom(RoomData _currentVisitedRoom)
    {
        CurrentVisitedRoom = _currentVisitedRoom;
    }

    public void SetHappinessValue(float _happiness)
    {
        StatData.Happiness = _happiness;
        StatData.Happiness = Mathf.Clamp(StatData.Happiness, 0, 100);
        OnHappinessChange();
    }
    
    public void SetStressValue(float _stress)
    {
        StatData.Stress = _stress;
        StatData.Stress = Mathf.Clamp(StatData.Stress, 0, 100);
        //OnHappinessChange();
    }

    //negatif olarak da gonderilebilir. Negatif olmasi durumunda dusurulecek.
    public void ChangeCoreHappinessValue(float _changeAmount)
    {
        StatData.Happiness += _changeAmount;
        StatData.Happiness = Mathf.Clamp(StatData.Happiness, 0, 100);
        OnHappinessChange();
    } 
    
    public void ChangeHappinessValue(float _changeAmount)
    {
        StatData.HappinessBuff += _changeAmount;
        OnHappinessChange();
    }
    
    public void ChangeCoreStressValue(float _changeAmount)
    {
        StatData.Stress += _changeAmount;
        StatData.Stress = Mathf.Clamp(StatData.Stress, 0, 100);
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

    public void SetDialogTarget(NPCBehaviour _target)
    {
        DialogTarget = _target;
    }

    #endregion

    #region Get Fonksiyonlari
    public float GetNpcHappiness()
    {
        return Mathf.Clamp(StatData.Happiness + StatData.HappinessBuff, 0, 100);
    }
    
    public float GetNpcStress()
    {
        return Mathf.Clamp(StatData.Stress + StatData.StressBuff ,0 ,100);
    }

    public List<string> GetLikedArtists()
    {
        return StatData.LikedArtist;
    }

    public RoomData GetNpcCurrentRoom()
    {
        return CurrentVisitedRoom;
    }

    public NPCState GetCurrentState()
    {
        return npcState._mainState;
    }
    #endregion

    public void SetComponents()
    {
        CurrentAudioSource = transform.GetChild(0).GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        MyNPCUI = GetComponent<NPCUI>();

        npcRenderers = GetComponentsInChildren<Renderer>().ToList();
        myCanvasses = GetComponentsInChildren<Canvas>().ToList();
        int length = transform.childCount;
        for (int i = 0; i < length; i++)
        {
            if (transform.GetChild(i).CompareTag("NPCDefaultCamera"))
                myDefaultCamera = transform.GetChild(i).GetComponent<Camera>();
            else if (transform.GetChild(i).CompareTag("NPCWorkCamera"))
                myWorkCamera = transform.GetChild(i).GetComponent<Camera>();
        }

        //Editorde oto isim yukleme
        GeneralData.NpcName = Constant.GetNPCName(GeneralData.isMale);

        //Editorde Statlari yukleme
        StatData.Happiness = 0;
        StatData.HappinessBuff = 0;
        StatData.toilet = 0;
        StatData.education = 0;
        StatData.additionalLuck = 20;
        StatData.NpcRotationSpeed = 5;
        StatData.NpcSpeed = Random.Range(2.750f, 3.500f);

        //Editorde Sevilen ve sevilmeyen renkler ve sanatci
        StatData.LikedArtist = Constant.GetRandomFamousPaintersWithDesiredCount(Random.Range(1, 4));
        StatData.LikedColors = new();
        StatData.DislikedColor = MyColors.Black;
        MyColors[] enumDegerleri = (MyColors[])System.Enum.GetValues(typeof(MyColors));
        for (int i = 0; i < 4; i++)
        {
            MyColors bulunanRenk = MyColors.Black;
            do
            {
                int arananRenk = Random.Range(0, enumDegerleri.Length);
                bulunanRenk = enumDegerleri.FirstOrDefault(r => (int)r == (int)arananRenk);
            } while (StatData.LikedColors.Contains(bulunanRenk));
            if (i == 3)
                StatData.DislikedColor = bulunanRenk;
            else
                StatData.LikedColors.Add(bulunanRenk);
        }
    }

}

