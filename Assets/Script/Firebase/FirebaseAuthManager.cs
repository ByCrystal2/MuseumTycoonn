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
        //CreateNewLoading();
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);        
    }
    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            // Continue with Play Games Services
            try
            {
                PlayGamesPlatform.Instance.Authenticate(status =>
                {

                    if (status == SignInStatus.Success)
                    {
                        try
                        {
                            PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                            {
                                auth = FirebaseAuth.DefaultInstance;                                
                                Credential credential = PlayGamesAuthProvider.GetCredential(code);

                                StartCoroutine(AuthGet());

                                IEnumerator AuthGet()
                                {
                                    System.Threading.Tasks.Task<FirebaseUser> task = auth.SignInWithCredentialAsync(credential);

                                    while (task.IsCompleted)
                                    {
                                        yield return null;
                                    }

                                    if (task.IsCanceled)
                                    {
                                        //Cancel
                                        textMeshPro.text += "Auth Cancelled";
                                    }
                                    else if (task.IsFaulted)
                                    {
                                        //task.exeption
                                        textMeshPro.text += "Fauled => " + task.Exception.ToString();
                                    }
                                    else
                                    {
                                        currentUser = task.Result;
                                        textMeshPro.text += currentUser.ToString();
                                        CreateNewLoading();
                                    }
                                }
                            });
                        }
                        catch (System.Exception e)
                        {

                            textMeshPro.text += "Exception => " + e.Message;
                        }


                    }
                    else
                    {
                        textMeshPro.text = "Auth Failed!";                        
                    }
                });
            }
            catch (System.Exception e)
            {
                textMeshPro.text += e.Message;
                throw;
            }
        }
        else
        {
            // Disable your integration with Play Games Services or show a login button
            // to ask users to sign-in. Clicking it should call
            // PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication).
#if UNITY_EDITOR
            CreateNewLoading();
#endif
            Debug.Log("Auth Failed! result => " + status.ToString());
        }
    }
    public void CreateNewLoading()
    {       
        Transform canvas = FindObjectOfType<Canvas>().gameObject.transform;
        Instantiate(LoadingPanel,canvas);        
    }
}
