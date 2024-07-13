using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    public Slider loadingSlider; // Eðer yükleme çubuðu kullanacaksanýz
    public TextMeshProUGUI loadingText;
    bool isLoading = false; // Yüklenme durumunu takip eden bayrak
    public void LoadNextScene()
    {
        Scenes nextScene = GetNextScene(SceneManager.GetActiveScene());
        StartCoroutine(LoadAsyncOperation(nextScene));
    }
    private IEnumerator LoadAsyncOperation(Scenes targetScene)
    {
        isLoading = true; // Yüklenme baþladýðýnda bayraðý true olarak ayarla

        Debug.Log("Current Scene => " + targetScene);

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(targetScene.ToString());

        while (!asyncOperation.isDone)
        {
            float progress = Mathf.Lerp(loadingSlider.value, asyncOperation.progress, Time.deltaTime * 5f);
            progress = Mathf.Clamp01(progress); // Asla 0 ve 1 arasýndan çýkmasýn

            if (progress >= 0.99f) // %100'e çok yaklaþtýðýnda
            {
                progress = 1f; // Tamamlanmýþ olarak kabul et
            }
            if (loadingSlider != null)
                loadingSlider.value = progress;

            if (loadingText != null)
                loadingText.text = (int)(progress * 100) + "%";

            yield return null;
        }

        isLoading = false; // Yükleme tamamlandýðýnda bayraðý false olarak ayarla
        Debug.Log("Target Sahnesi Yuklendi => " + targetScene.ToString());
    }

    private Scenes GetNextScene(Scene currentScene)
    {
        Scenes nextScene;

        if (Enum.TryParse(currentScene.name, out nextScene))
        {
            // Örnek bir durum: Auth sahnesinden sonra Game sahnesine geç
            switch (nextScene)
            {
                case Scenes.Auth:
                    return Scenes.Init;
                case Scenes.Init:
                    return Scenes.Menu;
                case Scenes.Menu:
                    Debug.Log("GameManager.instance.IsWatchTutorial => " + GameManager.instance.IsWatchTutorial);
                    if (!GameManager.instance.IsWatchTutorial)
                        return Scenes.TutorialLevel;
                    else
                        return Scenes.Game;
                case Scenes.TutorialLevel:
                    return Scenes.Game;
                default:
                    return Scenes.None;
            }
        }
        else
        {
            Debug.LogError("Invalid scene name: " + currentScene.name);
            return Scenes.None;
        }
    }
    public IEnumerator LoadFirebaseData()
    {
        Debug.Log("LoadFirebaseData is starting...");
        yield return new WaitUntil(() => PlayerManager.instance != null);
        PlayerManager.instance.LockPlayer();
        UIController.instance.CloseJoystickObj(true);
        List<NPCBehaviour> nps = FindObjectsOfType<NPCBehaviour>().ToList();
        foreach (var npc in nps)
        {
            npc.enabled = false;
            npc.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
        }
        float progressHelper = 0;
        while (!NpcManager.instance.databaseProcessComplated)
        {
            // Yükleme ilerlemesini güncelle
            int randomValue = UnityEngine.Random.Range(5, 10);
            progressHelper += randomValue;
            float progress = Mathf.Clamp01(progressHelper / 100f);
            loadingSlider.value = progress;
            loadingText.text = (int)(progress * 100) + "%";
            yield return new WaitForEndOfFrame(); ;
        }

        // Yükleme tamamlandýðýnda iþlemleri yap
        OnDataLoaded();
    }

    private void OnDataLoaded()
    {
        // Yükleme ekranýný gizle
        Debug.Log("LoadFirebaseData is ending...");
        if (GameManager.instance.IsWatchTutorial)
        {
            List<NPCBehaviour> nps = FindObjectsOfType<NPCBehaviour>().ToList();
            foreach (var npc in nps)
            {
                npc.enabled = true;
                npc.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;
            }
            PlayerManager.instance.UnLockPlayer();
            UIController.instance.CloseJoystickObj(false);
        }
        
        Destroy(gameObject,0.2f);
    }
}
public enum Scenes
{
    None,
    Auth,
    Game,
    Menu,
    Init,
    TutorialLevel
}
