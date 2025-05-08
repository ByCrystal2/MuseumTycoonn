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
}
