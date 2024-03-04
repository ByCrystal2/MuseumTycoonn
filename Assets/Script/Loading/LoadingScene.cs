using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    public Slider loadingSlider; // E�er y�kleme �ubu�u kullanacaksan�z
    public TextMeshProUGUI loadingText;
    bool isLoading = false; // Y�klenme durumunu takip eden bayrak
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
        isLoading = true; // Y�klenme ba�lad���nda bayra�� true olarak ayarla

        Debug.Log("Current Scene => " + targetScene);

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(targetScene.ToString());

        while (!asyncOperation.isDone)
        {
            float progress = Mathf.Lerp(loadingSlider.value, asyncOperation.progress, Time.deltaTime * 5f);
            progress = Mathf.Clamp01(progress); // Asla 0 ve 1 aras�ndan ��kmas�n

            if (progress >= 0.99f) // %100'e �ok yakla�t���nda
            {
                progress = 1f; // Tamamlanm�� olarak kabul et
            }
            if (loadingSlider != null)
                loadingSlider.value = progress;

            if (loadingText != null)
                loadingText.text = (int)(progress * 100) + "%";

            yield return null;
        }

        isLoading = false; // Y�kleme tamamland���nda bayra�� false olarak ayarla
        Debug.Log("Target Sahnesi Yuklendi => " + targetScene.ToString());
    }

    private Scenes GetNextScene(Scene currentScene)
    {
        Scenes nextScene;

        if (Enum.TryParse(currentScene.name, out nextScene))
        {
            // �rnek bir durum: Auth sahnesinden sonra Game sahnesine ge�
            switch (nextScene)
            {
                case Scenes.Auth:
                    return Scenes.Init;
                case Scenes.Init:
                    return Scenes.Menu;
                case Scenes.Menu:
                    return Scenes.Game;
                // Di�er durumlar i�in gerekli ge�i�leri ekleyin
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
