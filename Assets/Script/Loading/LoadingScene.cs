using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    public Slider loadingSlider; // Eðer yükleme çubuðu kullanacaksanýz
    public TextMeshProUGUI loadingText;
    bool isLoading = false; // Yüklenme durumunu takip eden bayrak
    private void OnEnable()
    {
        LoadNextScene();
    }
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
                    return Scenes.Game;
                // Diðer durumlar için gerekli geçiþleri ekleyin
                // ...
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
}
public enum Scenes
{
    None,
    Auth,
    Game,
    Menu,
    Init
}
