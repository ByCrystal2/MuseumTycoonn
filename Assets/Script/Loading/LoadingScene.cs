using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    public Slider loadingSlider; // Eðer yükleme çubuðu kullanacaksanýz
    public Text loadingText;
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
    static bool[] loadingSteps = new bool[] { false, false, false, false, false, false, false, false, false, false };
    static int loadingCompletedIndex = 0;
    static bool isProgress = false;

    public static void ComplateLoadingStep()
    {
        if (loadingCompletedIndex < loadingSteps.Length)
        {
            loadingSteps[loadingCompletedIndex] = true;
            loadingCompletedIndex++;
            isProgress = true;
            Debug.Log("Complate loading step. Next Step => " + loadingCompletedIndex);
        }
    }
    public IEnumerator LoadFirebaseData()
    {
        Debug.Log("LoadFirebaseData is starting...");        
        UIController.instance.CloseJoystickObj(true);

        List<NPCBehaviour> npcs = FindObjectsOfType<NPCBehaviour>().ToList();
        foreach (var npc in npcs)
        {
            npc.enabled = false;
        }

        float progressHelper = 0;
        float totalSteps = loadingSteps.Length;
        float stepProgress = 1f / totalSteps;

        while (!NpcManager.instance.databaseProcessComplated)
        {
            if (isProgress)
            {
                Debug.Log("Progress..." + progressHelper);
                isProgress = false;
                progressHelper += stepProgress * 100;
                float progress = Mathf.Clamp01(progressHelper / 100f);
                loadingSlider.value = progress;
                loadingText.text = (int)(progress * 100) + "%";

                yield return new WaitForEndOfFrame();
            }
            else
            {
                yield return null;
            }
        }
        OnDataLoaded();
    }

    private void OnDataLoaded()
    {
        // Yükleme ekranýný gizle
        Debug.Log("LoadFirebaseData is ending...");
        if (GameManager.instance.IsWatchTutorial)
        {
            //List<NPCBehaviour> nps = FindObjectsOfType<NPCBehaviour>().ToList();
            //foreach (var npc in nps)
            //{
            //    npc.enabled = true;
            //    //npc.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;
            //}
            NpcManager.instance.MuseumDoorsProcess(true);
            SpawnHandler.instance.StartSpawnProcess();
        }
        try
        {
            if (GameManager.instance != null)
            {
                if (!GameManager.instance.IsWatchTutorial)
                {
                    DialogueTrigger firstDialog = GameObject.FindWithTag("TutorialNPC").GetComponent<DialogueTrigger>();
                    if (firstDialog != null)
                        firstDialog.TriggerDialog(Steps.Step2);
                }
                else
                {
                    DialogueManager.instance.SetActivationDialoguePanel(false);
                    GameObject.FindWithTag("TutorialNPC").SetActive(false); // Destroyda edilebilirdi fakat lazim olabilir ilerde.
                    Destroy(UIController.instance.tutorialUISPanel.gameObject);
                }
            }
        }
        catch (System.Exception _ex)
        {
            Debug.Log("Tutorial Npc process form awake loading process method caught an error!. => " + _ex.Message);
        }

        RightUIPanelController.instance.EditMode();
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
