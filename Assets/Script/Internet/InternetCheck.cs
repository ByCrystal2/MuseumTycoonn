using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class InternetCheck : MonoBehaviour
{
    private bool isInternetAvailable = true;

    private InternetCheck instance;
    private void Awake()
    {
        if (instance)
            return;
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Update()
    {
        bool previousState = isInternetAvailable;
        isInternetAvailable = Application.internetReachability != NetworkReachability.NotReachable;

        if (!isInternetAvailable && previousState)
        {
            // Internet is lost
            NoInternet();
        }
        else if (isInternetAvailable && !previousState)
        {
            // Internet is back
            InternetRestored();
        }
    }

    void NoInternet()
    {// Pause the game
        Debug.Log("Internet connection is not available. The game is PAUSED.");
        UIInteractHandler.instance.AskQuestion("Internet Connection", "Your internet connection has been lost.\nPlease check your connection.");
        Time.timeScale = 0.1f;
    }

    void InternetRestored()
    {// Resume the game
        Debug.Log("Internet connection available. The game is RESUME.");
        UIInteractHandler.instance.AskQuestion("Internet Connection", "Your internet connection has been restored.", null, null, (x) => { Time.timeScale = 1; CleanupDontDestroyOnLoad(); SceneManager.LoadScene("Auth"); });
    }

    void CleanupDontDestroyOnLoad()
    {
        // Find all objects marked as DontDestroyOnLoad and destroy them
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.scene.name == "DontDestroyOnLoad") // Objects in the DontDestroyOnLoad context have a null scene name
            {
                Destroy(obj);
            }
        }
    }
}

