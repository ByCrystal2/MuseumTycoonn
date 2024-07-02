using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class FirebaseAuthManager : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI textMeshPro;
    [SerializeField] GameObject LoadingPanel;
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
            textMeshPro.text = "Auth Failed!";
            Debug.Log("Auth Failed! result => " + status.ToString());
        }
    }

    private IEnumerator AuthGet(Credential credential)
    {
        var task = auth.SignInWithCredentialAsync(credential);

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsCanceled)
        {
            textMeshPro.text += "Auth Cancelled";
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
            textMeshPro.text += currentUser.ToString();
            Debug.Log("Auth Success => " + currentUser.UserId);
            CreateNewLoading();
        }
    }

    private void HandleException(System.Exception e)
    {
        textMeshPro.text += "Exception => " + e.Message;
        Debug.Log("Exception => " + e.Message);
    }

    public void CreateNewLoading()
    {
        Transform canvas = FindObjectOfType<Canvas>().transform;
        Instantiate(LoadingPanel, canvas);
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
