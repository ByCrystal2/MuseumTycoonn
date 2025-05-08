using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    [SerializeField] GameObject LoadingPanel;
    [SerializeField]
    string[] loadingCanvasIgnoringTags;

    public static AuthManager instance { get; private set; }
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        CreateNewLoading();
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