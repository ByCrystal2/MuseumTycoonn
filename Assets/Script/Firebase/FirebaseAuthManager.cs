using System;
using UnityEngine;

#if !UNITY_WEBGL
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
#endif

#if UNITY_WEBGL
using FirebaseWebGL.Scripts.FirebaseBridge;
using FirebaseWebGL.Scripts.Objects;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
#endif

public class FirebaseAuthManager : MonoBehaviour
{
    [SerializeField] GameObject LoadingPanel;
    [SerializeField] string[] loadingCanvasIgnoringTags;
    [Header("Editor")]
    [SerializeField] EditorUserSignCanvasController EditorSingInController;
    public List<DatabaseUser> editorUsers = new List<DatabaseUser>();
    DatabaseUser databaseUser;
    public static FirebaseAuthManager instance { get; private set; }
#if !UNITY_WEBGL
    // Android ve di�er platformlarda Firebase Auth C# SDK kullan�l�r
    private FirebaseAuth auth;

    void Awake()
    {
        if (instance)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);        
    }
        private void Start()
    {
#if UNITY_EDITOR
        if (System.IO.File.Exists(Application.persistentDataPath + "/Admin/LoggedIn_EditorUser.json"))
        {
            EditorSingInController.gameObject.SetActive(false);
            string jsonString = System.IO.File.ReadAllText(Application.persistentDataPath + "/Admin/LoggedIn_EditorUser.json");
            DatabaseUser user = JsonUtility.FromJson<DatabaseUser>(jsonString);
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
SignInWithEmail("test@gmail.com", "testuser123");
        
    }

    public void RegisterWithEmail(string email, string password)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Kay�t i�lemi iptal edildi.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("Kay�t hatas�: " + task.Exception);
                return;
            }
            // Kay�t ba�ar�l�
            FirebaseUser newUser = task.Result;
            Debug.Log("Kay�t ba�ar�l�. UserId: " + newUser.UserId);
            // �rne�in burada DatabaseUser s�n�f�na bilgileri aktarabilirsiniz:
            // DatabaseUser dbUser = new DatabaseUser { userId = newUser.UserId, email = newUser.Email };
        });
    }

    public void SignInWithEmail(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Giri� i�lemi iptal edildi.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("Giri� hatas�: " + task.Exception);
                return;
            }
            // Giri� ba�ar�l�
            FirebaseUser user = task.Result;
            Debug.Log("Giri� ba�ar�l�. UserId: " + user.UserId);
            // DatabaseUser bilgilerini g�ncelleyin:
            // DatabaseUser dbUser = new DatabaseUser { userId = user.UserId, email = user.Email };
        });
    }
#else
    // WebGL platformunda rotolonico/FirebaseWebGL paketi kullan�l�r
    // Kay�t i�lemi i�in fonksiyon
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
    private void Start()
    {
#if UNITY_EDITOR
        if (System.IO.File.Exists(Application.persistentDataPath + "/Admin/LoggedIn_EditorUser.json"))
        {
            EditorSingInController.gameObject.SetActive(false);
            string jsonString = System.IO.File.ReadAllText(Application.persistentDataPath + "/Admin/LoggedIn_EditorUser.json");
            DatabaseUser user = JsonUtility.FromJson<DatabaseUser>(jsonString);
            SetDatabaseUser(user);            
            CreateNewLoading();
        }
        else
        {
            EditorSingInController.gameObject.SetActive(true);
        }
        return;
#endif
        SignInWithEmail("test@gmail.com", "testuser123");
    }
    public void RegisterWithEmail(string email, string password)
    {
        FirebaseAuth.CreateUserWithEmailAndPassword(
            email,
            password,
            gameObject.name,
            nameof(OnRegisterSuccess),
            nameof(OnRegisterError)
        );
    }

    // Giri� i�lemi i�in fonksiyon
    public void SignInWithEmail(string email, string password)
    {
        FirebaseAuth.SignInWithEmailAndPassword(
            email,
            password,
            gameObject.name,
            nameof(OnSignInSuccess),
            nameof(OnSignInError)
        );
    }

    // Kay�t ba�ar�l� callback (JSON sonucu user bilgisi i�erir)
    private void OnRegisterSuccess(string jsonResult)
    {
        try
        {
            var user = JsonUtility.FromJson<FirebaseUser>(jsonResult);
            Debug.Log("WebGL Kay�t ba�ar�l�. UserId: " + user.uid + ", Email: " + user.email);
            // �rnek: DatabaseUser s�n�f�na bilgileri aktar
            // DatabaseUser dbUser = new DatabaseUser { userId = user.uid, email = user.email };
        }
        catch (Exception e)
        {
            Debug.LogError("OnRegisterSuccess parsing hatas�: " + e.Message);
        }
    }

    // Kay�t hatas� callback
    private void OnRegisterError(string errorJson)
    {
        try
        {
            var error = JsonUtility.FromJson<FirebaseError>(errorJson);
            Debug.LogError($"WebGL Kay�t hatas� ({error.code}): {error.message}");
        }
        catch (Exception e)
        {
            Debug.LogError("OnRegisterError parsing hatas�: " + e.Message);
        }
    }

    // Giri� ba�ar�l� callback (JSON sonucu user bilgisi i�erir)
    private void OnSignInSuccess(string jsonResult)
    {
        try
        {
            var user = JsonUtility.FromJson<FirebaseUser>(jsonResult);
            Debug.Log("WebGL Giri� ba�ar�l�. UserId: " + user.uid + ", Email: " + user.email);
            // �rnek: DatabaseUser s�n�f�na bilgileri aktar
             databaseUser = new DatabaseUser(user.displayName, user.email, user.phoneNumber, user.uid);
             CreateNewLoading();
        }
        catch (Exception e)
        {
            Debug.LogError("OnSignInSuccess parsing hatas�: " + e.Message);
        }
    }

    // Giri� hatas� callback
    private void OnSignInError(string errorJson)
    {
        try
        {
            var error = JsonUtility.FromJson<FirebaseError>(errorJson);
            Debug.LogError($"WebGL Giri� hatas� ({error.code}): {error.message}");
        }
        catch (Exception e)
        {
            Debug.LogError("OnSignInError parsing hatas�: " + e.Message);
        }
    }
#endif
    public void CreateNewLoading()
    {
        Debug.Log("CreateNewLoading method is starting...");
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
            Debug.LogWarning("No suitable canvas found in scene: " + SceneManager.GetActiveScene().name);
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
            Debug.LogWarning("No suitable canvas found in scene: " + SceneManager.GetActiveScene().name);
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