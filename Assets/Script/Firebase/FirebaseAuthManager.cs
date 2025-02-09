using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Linq;

public class FirebaseAuthManager : MonoBehaviour
{
    [SerializeField] GameObject LoadingPanel;
    [SerializeField] string[] loadingCanvasIgnoringTags;
    FirebaseAuth auth;
    FirebaseUser currentUser;

    [Header("Editor")]
    [SerializeField] EditorUserSignCanvasController EditorSingInController;
    public List<DatabaseUser> editorUsers = new List<DatabaseUser>();
    DatabaseUser databaseUser;

    public static FirebaseAuthManager instance { get; private set; }

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

    }

    void Start()
    {
#if UNITY_EDITOR
        if (System.IO.File.Exists(Application.persistentDataPath + "/" + "Admin/" + "LoggedIn_EditorUser" + ".json"))
        {
            EditorSingInController.gameObject.SetActive(false);
            string jsonString = System.IO.File.ReadAllText(Application.persistentDataPath + "/" + "Admin/" + "LoggedIn_EditorUser" + ".json"); // read the json file from the file system
            DatabaseUser user = JsonUtility.FromJson<DatabaseUser>(jsonString); // de-serialize the data to your myData object
            SetDatabaseUser(user);
            StartCoroutine(FirestoreManager.instance.CheckIfUserExists(databaseUser.UserID, databaseUser.Email, databaseUser.PhoneNumber, databaseUser.Name));
            CreateNewLoading();
        }
        else
        {
            EditorSingInController.gameObject.SetActive(true);
        }
        return;
#endif
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }

    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            Debug.Log("ProcessAuthentication(SignInStatus status) Status success!");
            try
            {
                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    auth = FirebaseAuth.DefaultInstance;
                    Credential credential = PlayGamesAuthProvider.GetCredential(code);
                    StartCoroutine(AuthGet(credential));
                });
            }
            catch (System.Exception e)
            {
                HandleException(e);
            }
        }
        else
        {

            Debug.Log("Auth Failed! result => " + status.ToString());
        }
    }

    private IEnumerator AuthGet(Credential credential)
    {
        var task = auth.SignInWithCredentialAsync(credential);

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsCanceled)
        {
            Debug.Log("Auth Cancelled!");
        }
        else if (task.IsFaulted)
        {
            HandleException(task.Exception);
            Debug.Log("Auth Faulted!");
        }
        else
        {
            currentUser = task.Result;
            databaseUser = new DatabaseUser(currentUser.DisplayName,currentUser.Email,currentUser.PhoneNumber,currentUser.UserId);
            StartCoroutine(FirestoreManager.instance.CheckIfUserExists());
            Debug.Log("Auth Success => " + currentUser.UserId);
            CreateNewLoading();
        }
    }

    private void HandleException(System.Exception e)
    {
        Debug.Log("Exception => " + e.Message);
    }

    public void CreateNewLoading()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        Transform canvas = transform;
        foreach (Canvas c in canvases)
            if (!loadingCanvasIgnoringTags.Contains(c.tag)){canvas = c.transform; break;}
        if (canvas != null)
        {
            GameObject obj = Instantiate(LoadingPanel, canvas);
            obj.GetComponent<LoadingScene>().LoadNextScene();
        }
        else
            Debug.Log("Mevcut sahnede canvas bulunmamaktadir veya tum canvaslar engellenenler listesindedir. Mevcut sahne:"+SceneManager.GetActiveScene().name);
    }
    public void ForFireBaseLoading()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        Transform canvas = transform;
        foreach (Canvas c in canvases)
            if (!loadingCanvasIgnoringTags.Contains(c.tag)) { canvas = c.transform; break; }
        if (canvas != null)
        {
            GameObject obj = Instantiate(LoadingPanel, canvas);
            StartCoroutine(obj.GetComponent<LoadingScene>().LoadFirebaseData());
        }
        else
            Debug.Log("Mevcut sahnede canvas bulunmamaktadir veya tum canvaslar engellenenler listesindedir. Mevcut sahne:" + SceneManager.GetActiveScene().name);
    }
    public FirebaseAuth GetAuth()
    {
        return auth;
    }
    public DatabaseUser GetCurrentUserWithID()
    {
        return databaseUser;
    }
    public void SetDatabaseUser(DatabaseUser user)
    {
        databaseUser = new DatabaseUser(user.Name,user.Email,user.PhoneNumber,user.UserID);
    }
}
[System.Serializable]
public class DatabaseUser
{
    public string Name;
    public string Email;
    public string PhoneNumber;
    public string UserID;

    public DatabaseUser(string _name, string _eMail, string _phoneNumber, string _UserID)
    {
        Name = _name;
        Email = _eMail;
        PhoneNumber = _phoneNumber;
        UserID = _UserID;
    }
}
