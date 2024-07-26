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
#if UNITY_EDITOR
            StartCoroutine(FirestoreManager.instance.CheckIfUserExists("ahmet123", "ahmetburak04.ab@gmail.com","+905456984055","ByCrystal"));
            CreateNewLoading();
#endif
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
    public FirebaseUser GetCurrentUser()
    {
        return currentUser;
    }
}
