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
        CreateNewLoading();
        //try
        //{
        //    PlayGamesPlatform.instance.Authenticate(status =>
        //    {

        //        if (status == SignInStatus.Success)
        //        {
        //            try
        //            {
        //                PlayGamesPlatform.instance.RequestServerSideAccess(true, code =>
        //                {
        //                    FirebaseAuth auth = FirebaseAuth.DefaultInstance;

        //                    Credential credential = PlayGamesAuthProvider.GetCredential(code);

        //                    StartCoroutine(AuthGet());

        //                    IEnumerator AuthGet()
        //                    {
        //                        System.Threading.Tasks.Task<FirebaseUser> task = auth.SignInWithCredentialAsync(credential);

        //                        while (task.IsCompleted)
        //                        {
        //                            yield return null;
        //                        }

        //                        if (task.IsCanceled)
        //                        {
        //                            //Cancel
        //                            textMeshPro.text += "Auth Cancelled";
        //                        }
        //                        else if (task.IsFaulted)
        //                        {
        //                            //task.exeption
        //                            textMeshPro.text += "Fauled => " + task.Exception.ToString();
        //                        }
        //                        else
        //                        {
        //                            FirebaseUser newUser = task.Result;
        //                            textMeshPro.text += newUser.ToString();
        //                            CreateNewLoading();
        //                        }
        //                    }
        //                });
        //            }
        //            catch (System.Exception e)
        //            {

        //                textMeshPro.text += "Exception => " + e.Message;
        //            }
                    

        //        }
        //        else
        //        {
        //            textMeshPro.text = "Auth Failed!";

        //            // <Testlerden sonra silineccek kodlar>
        //            CreateNewLoading();
        //            // </Testlerden sonra silineccek kodlar>
        //        }
        //    });
        //}
        //catch (System.Exception e)
        //{
        //    textMeshPro.text += e.Message;
        //    throw;
        //}
    }

    public void CreateNewLoading()
    {       
        Transform canvas = FindObjectOfType<Canvas>().gameObject.transform;
        Instantiate(LoadingPanel,canvas);        
    }
}
