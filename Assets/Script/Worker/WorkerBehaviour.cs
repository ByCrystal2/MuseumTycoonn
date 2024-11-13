using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

[System.Serializable]
public class WorkerBehaviour : MonoBehaviour
{
    [SerializeField] public int ID;
    [SerializeField] public NavMeshAgent Agent;
    [SerializeField] public Animator Anim;
    [SerializeField] public float CurrentXp;
    [SerializeField] public int StarRank;
    [SerializeField] public float NpcSpeed;
    [SerializeField] public float NpcCurrentSpeed;
    [SerializeField] public float PatrolRadius;
    [SerializeField] public float Energy;
    [SerializeField] public float MaxEnergy;
    [SerializeField] public int NotPaidCounter;

    [SerializeField] public WorkerType workerType;
    [SerializeField] public bool IsMale;

    //UI
    public Camera forWorkerInfoPanelCam;
    //UI
    public WorkerData MyDatas;
    public Worker MyScript;
    public Transform EffectParent;
    public Transform LevelUpEffectParent;
    public GameObject Weapon;

    private bool isPassedOut;

    public GameObject RestIcon; 
    public AudioSource m_AudioSource;
    public AudioClip WorkerSoundClipsWhileWorking;
    public float stuckThreshold = 0.1f; // Adjust as needed
    public float stuckTimeThreshold = 1.5f; // Adjust as needed
    private bool isStuck = false;
    private float timeStuck = 0f;
    private void Awake()
    {
        if(StarRank == 0)
            StarRank = 1;
    }

    private void Start()
    {
        RefreshEnergy();
    }

    private void OnEnable()
    {
        //if (Weapon != null)
        //    Weapon.SetActive(false);
        
        if (MyScript is Housekeeper hk)
        {
            List<RoomData> myRooms = RoomManager.instance.GetRoomWithIDs(MyDatas.WorkRoomsIDs);
            if(myRooms.Count > 0)
            {
                Vector3 startPoint = hk.PatrolToRandomPoint(myRooms[0].CenterPoint.position, 7);
                Agent.enabled = false;
                Debug.Log("set start position for worker: " + startPoint);
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
        else if (MyScript is Security sc)
        {
            Debug.Log("MyDatas.WorkRoomsIDs => " + " [" + name + "] "+ MyDatas.WorkRoomsIDs.Count);
            Debug.Log("All room count => " + RoomManager.instance.RoomDatas.Count);
            List<RoomData> myRooms = RoomManager.instance.GetRoomWithIDs(MyDatas.WorkRoomsIDs);
            if (myRooms.Count > 0)
            {
                Vector3 startPoint = sc.PatrolToRandomPoint(myRooms[0].CenterPoint.position, 7);
                Agent.enabled = false;
                Debug.Log("set start position for worker: " + startPoint);
                transform.position = startPoint;
                Agent.enabled = true;
                sc.CreateNewTarget();
            }
            else
            {
                Debug.LogError("No Room data on the current worker. Name: " + transform.name);
                gameObject.SetActive(false);
            }
        }
        else if (MyScript is Musician m)
        {
            List<RoomData> myRooms = RoomManager.instance.GetRoomWithIDs(MyDatas.WorkRoomsIDs);
            if (myRooms.Count > 0)
            {
                Vector3 startPoint = m.PatrolToRandomPoint(myRooms[0].CenterPoint.position, 7);
                Agent.enabled = false;
                Debug.Log("set start position for worker: " + startPoint);
                transform.position = startPoint;
                Agent.enabled = true;
                m.CreateNewTarget();
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
        else if (MyScript is Security sc)
        {
            sc.CurrentTarget = Vector3.zero;
            sc.CurrentActiveTask = null;
            sc.MyCurrentNPCTarget = null;
        }
        else if (MyScript is Musician m)
        {
            m.CurrentTarget = Vector3.zero;
            m.CurrentActiveTask = null;
        }
    }

    private void FixedUpdate()
    {
        if (isPassedOut)
        {
            return;
        }

        if (MyScript is Housekeeper hk)
            if (hk.CurrentTarget != Vector3.zero)
            {
                if (CheckStuck())
                {
                    hk.CreateNewTarget();
                    return;
                }
                hk.Move(hk.CurrentTarget, hk.MyTasks.Count > 0);
                Energy -= Time.deltaTime;
                if (Energy < 0)
                    OnEnergyRunOut();
            }

        if (MyScript is Security sc)
            if (sc.CurrentTarget != Vector3.zero)
            {
                if (CheckStuck())
                {
                    sc.CreateNewTarget();
                    return;
                }
                sc.Move(sc.CurrentTarget, sc.MyTasks.Count > 0);
                Energy -= Time.deltaTime;
                if (Energy < 0)
                    OnEnergyRunOut();
            }

        if (MyScript is Musician m)
            if (m.CurrentTarget != Vector3.zero)
            {
                if (CheckStuck())
                {
                    m.CreateNewTarget();
                    return;
                }
                m.Move(m.CurrentTarget, m.MyTasks.Count > 0);
                Energy -= Time.deltaTime;
                if (Energy < 0)
                    OnEnergyRunOut();
            }
    }

    public bool CheckStuck()
    {
        if (Agent.velocity.magnitude < stuckThreshold)
        {
            timeStuck += Time.deltaTime;
            if (timeStuck >= stuckTimeThreshold)
            {
                Debug.Log("NPC is stuck!");
                return true;
                // You can handle what to do when the NPC is stuck here
            }
            else
                return false;
        }
        else
        {
            // Reset the stuck timer if the NPC is moving
            timeStuck = 0f;
            return false;
        }
    }

    public bool IsPositionAccessible(Vector3 position)
    {
        NavMeshPath path = new NavMeshPath();
        if (Agent.CalculatePath(position, path))
            return true;
        return false;
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
        else if (MyScript is Security sc)
            sc.CreateNewTarget();
        else if (MyScript is Musician m)
            m.CreateNewTarget();
    }

    private IEnumerator EndTaskDelayed(float _delay)
    {
        if (MyScript is Musician m)
        {
            for (int i = 0; i < _delay; i++)
            {
                if (i % 3 == 0)
                {
                    foreach (var item in MuseumManager.instance.CurrentNpcs)
                    {
                        if (Vector3.Distance(item.transform.position, transform.position) < m.GetHappinessRange())
                        {
                            item.ChangeHappinessValue((4 * (m.Level + 1)));
                        }
                    }
                }
                yield return new WaitForSeconds(1);
            }

            m.EndMusic();
            if (WorkerManager.instance.CanEarnExp(CurrentXp, StarRank,this))
            {
                byte multiplier = 1;
#if UNITY_EDITOR
                multiplier = 10;
#endif
                CurrentXp += Random.Range(7, 14) * multiplier;
                int newLevel = WorkerManager.instance.CalculateCurrentLevel(CurrentXp);
                Debug.Log("newLevel => " + newLevel);
                MyDatas.Level = newLevel;
                MyDatas.Xp = CurrentXp;
                if (newLevel > m.Level)
                    OnLevelUP();
                m.Level = newLevel;
            }
        }
        else
        {
            yield return new WaitForSeconds(_delay);
            if (MyScript is Housekeeper hk)
            {
                hk.EndClean();
                if (WorkerManager.instance.CanEarnExp(CurrentXp, StarRank, this))
                {
                    CurrentXp += Random.Range(7, 14);
                    int newLevel = WorkerManager.instance.CalculateCurrentLevel(CurrentXp);
                    MyDatas.Level = newLevel;
                    MyDatas.Xp = CurrentXp;
                    if (newLevel > hk.Level)
                        OnLevelUP();
                    hk.Level = newLevel;
                }
            }
            else if (MyScript is Security sc)
            {
                sc.CaughtGuilty();
                if (WorkerManager.instance.CanEarnExp(CurrentXp, StarRank, this))
                {
                    CurrentXp += Random.Range(7, 14);
                    int newLevel = WorkerManager.instance.CalculateCurrentLevel(CurrentXp);
                    MyDatas.Level = newLevel;
                    MyDatas.Xp = CurrentXp;
                    if (newLevel > sc.Level)
                        OnLevelUP();
                    sc.Level = newLevel;
                }
            }
        }
    }

    public void PlayWorkSounds()
    {
        m_AudioSource.PlayOneShot(WorkerSoundClipsWhileWorking);
    }

    private void OnLevelUP()
    {
        Debug.Log("Worker with name: " + transform.name + " has leveled up.");

        ParticleSystem[] beatParticle = LevelUpEffectParent.GetComponentsInChildren<ParticleSystem>();
        foreach (var item in beatParticle)
        {
            var mainModule = item.main;
            mainModule.duration = 5;
            item.Stop();
            item.Play();
        }
        FirestoreManager.instance.workerDatasHandler.UpdateWorkerData(FirebaseAuthManager.instance.GetCurrentUserWithID().UserID, MyDatas);
    }

    private void OnEnergyRunOut()
    {
        isPassedOut = true;
        Agent.isStopped = true;
        Agent.enabled = false;
        Debug.Log("Worker passed out.");
        Anim.SetInteger("Walk", 0);
        Anim.SetInteger("WorkerHaveRest", 1);
        CancelInvoke();
        Invoke(nameof(StartStandUP), 8);
        Invoke(nameof(RefreshEnergy), 10);
        RestIcon.SetActive(true);
    }

    public void StartStandUP()
    {
        Anim.SetInteger("WorkerHaveRest", -1);
    }

    void RefreshEnergy()
    {
        Anim.SetInteger("WorkerHaveRest", 0);
        isPassedOut = false;
        Agent.isStopped = false;
        Agent.enabled = true;
        MaxEnergy = 200 + MyScript.Level * 20; 
        Energy = MaxEnergy;
        RestIcon.SetActive(false);
    }

    public void SetSalaryAsPaid()
    {
        NotPaidCounter = 0;
    }

    public void SetSalaryAsNotPaid()
    {
        NotPaidCounter++;
        Debug.LogError("Isci maasini " + NotPaidCounter + " seferdir alamadi. Burada uyari verilebilir.");
        Notification notPaidNotification = NotificationManager.instance.GetNotificationWithID(2);
        NotificationManager.instance.SendNotification(notPaidNotification, new SenderHelper(WhoSends.Worker, ID));
        if (NotPaidCounter >= 3)
        {
            NotPaidCounter = 0;
            WorkerManager.instance.TransferCurrentWorkerToInventory(ID);
            NotificationManager.instance.SendNotification(NotificationManager.instance.GetNotificationWithID(3), new SenderHelper(WhoSends.System, 9999));
            Debug.LogError("isci maaisini 3 seferdir alamadi, envantere kaldirildi", transform);
        }
    }
    public static float BaseSalary = 10;

    private void OnMouseDown()
    {
        if (UIController.instance.IsPointerOverAnyUI()) return;
        if (GameManager.instance.GetCurrentGameMode() == GameMode.FPS)
            PlayerManager.instance.LockPlayer();
        UIController.instance.workerInfoPanelController.SetWorker(this);
        UIController.instance.workerInfoPanelController.gameObject.SetActive(true);
    }
}

[System.Serializable]
public class WorkerData
{
    public int ID;
    public string Name;
    public int Age;
    public float Height;
    public int Level;
    public float Xp;
    public float baseSalary;
    public List<int> WorkRoomsIDs = new List<int>();
    [HideInInspector]public WorkerType WorkerType;
    [HideInInspector]public WorkerIn WorkerIn;
    public WorkerData(int _id,string _name, int _age, float _height, int level, float _Xp, List<int> _workRoomsIDs, WorkerType workerType, WorkerIn _workerIn, float baseSalary = -1)
    {
        this.ID = _id;
        this.Name = _name;
        this.Age = _age;
        this.Height = _height;
        this.Level = level;
        this.Xp = _Xp;
        WorkRoomsIDs.Clear();
        foreach (int i in _workRoomsIDs) { WorkRoomsIDs.Add(i); }
        this.WorkerType = workerType;
        this.WorkerIn = _workerIn;
        this.baseSalary = baseSalary;
        if(this.baseSalary == -1)
            this.baseSalary = WorkerBehaviour.BaseSalary;
    }
    public WorkerData(WorkerData _copyData)
    {
        ID = _copyData.ID;
        Name = _copyData.Name;
        Age = _copyData.Age;
        Height = _copyData.Height;
        Level = _copyData.Level;
        Xp = _copyData.Xp;
        WorkRoomsIDs.Clear();
        int length = _copyData.WorkRoomsIDs.Count;
        for (int i = 0; i < length; i++)
            WorkRoomsIDs.Add(_copyData.WorkRoomsIDs[i]);
        this.WorkerType = _copyData.WorkerType;
        this.WorkerIn = _copyData.WorkerIn;
        this.baseSalary = _copyData.baseSalary;
        if (this.baseSalary == -1)
            this.baseSalary = WorkerBehaviour.BaseSalary;
    }
}
public enum WorkerIn
{
    Active,
    Inventory,
    Shop
}
