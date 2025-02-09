using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class InternetCheck : MonoBehaviour
{
    private bool isInternetAvailable = true;
    public List<string> InfoStrings = new List<string>(); // 0 => Internet Connection | 1 => Your internet connection has been lost. | 2 => Please check your connection.| 3 => Your internet connection has been restored.
    public static InternetCheck instance { get; private set; }
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
        if (InfoStrings.Count <= 0) { UIInteractHandler.instance.AskQuestion("Internet connection", "Internet connection is not available. The game is PAUSED."); Time.timeScale = 0.1f; return; }
        UIInteractHandler.instance.AskQuestion(InfoStrings[0], $"{InfoStrings[1]}\n{InfoStrings[2]}");
        Time.timeScale = 0.1f;
    }

    void InternetRestored()
    {// Resume the game
        Debug.Log("Internet connection available. The game is RESUME.");
        if (InfoStrings.Count <= 0) { UIInteractHandler.instance.AskQuestion("Internet connection", "Your internet connection has been restored.", null, null, (x) => { Time.timeScale = 1; }); return; }
        UIInteractHandler.instance.AskQuestion(InfoStrings[0], InfoStrings[3], null, null, (x) => { Time.timeScale = 1; /*CleanupDontDestroyOnLoad(); SceneManager.LoadScene("Auth");*/ });
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

