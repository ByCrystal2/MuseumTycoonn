using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using UnityEngine.SceneManagement;
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
        auth = FirebaseAuth.DefaultInstance;

#if UNITY_EDITOR
        if (System.IO.File.Exists(Application.persistentDataPath + "/" + "Admin/" + "LoggedIn_EditorUser" + ".json"))
        {
            EditorSingInController.gameObject.SetActive(false);
            string jsonString = System.IO.File.ReadAllText(Application.persistentDataPath + "/" + "Admin/" + "LoggedIn_EditorUser" + ".json");
            DatabaseUser user = JsonUtility.FromJson<DatabaseUser>(jsonString);
            SetDatabaseUser(user);
            StartCoroutine(FirestoreManager.instance.CheckIfUserExists(databaseUser.UserID, databaseUser.Email, databaseUser.PhoneNumber, databaseUser.Name));
            CreateNewLoading();
        }
        else
        {
            EditorSingInController.gameObject.SetActive(true);
        }
#endif
#if !UNITY_EDITOR
        SignInWithEmailAndPassword("test@gmail.com", "testuser123");
#endif
    }

    public void SignInWithEmailAndPassword(string email, string password)
    {
        StartCoroutine(SignInCoroutine(email, password));
    }

    private IEnumerator SignInCoroutine(string email, string password)
    {
        var task = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsCanceled || task.IsFaulted)
        {
            HandleException(task.Exception);
        }
        else
        {
            currentUser = task.Result.User;
            databaseUser = new DatabaseUser(currentUser.DisplayName, currentUser.Email, currentUser.PhoneNumber, currentUser.UserId);
            StartCoroutine(FirestoreManager.instance.CheckIfUserExists());
            CreateNewLoading();
        }
    }

    public void RegisterWithEmailAndPassword(string email, string password, string name)
    {
        StartCoroutine(RegisterCoroutine(email, password, name));
    }

    private IEnumerator RegisterCoroutine(string email, string password, string name)
    {
        var task = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsCanceled || task.IsFaulted)
        {
            HandleException(task.Exception);
        }
        else
        {
            currentUser = task.Result.User;
            UserProfile profile = new UserProfile { DisplayName = name };
            currentUser.UpdateUserProfileAsync(profile);

            databaseUser = new DatabaseUser(name, currentUser.Email, currentUser.PhoneNumber, currentUser.UserId);
            StartCoroutine(FirestoreManager.instance.CheckIfUserExists());
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
            if (!loadingCanvasIgnoringTags.Contains(c.tag)) { canvas = c.transform; break; }
        if (canvas != null)
        {
            GameObject obj = Instantiate(LoadingPanel, canvas);
            obj.GetComponent<LoadingScene>().LoadNextScene();
        }
        else
            Debug.Log("Mevcut sahnede canvas bulunmamaktadir veya tum canvaslar engellenenler listesindedir. Mevcut sahne:" + SceneManager.GetActiveScene().name);
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
        databaseUser = new DatabaseUser(user.Name, user.Email, user.PhoneNumber, user.UserID);
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
