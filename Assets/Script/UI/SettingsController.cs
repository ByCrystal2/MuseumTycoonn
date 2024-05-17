using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    public Slider musicVolumeSlider;
    public Slider soundEffectSlider;
    public Slider dialogVolumeSlider;
    public Dropdown GraphicQualityDropdown;
    public Dropdown ResolutionDropdown;

    public UniversalRenderPipelineAsset highFidelityURPAsset;
    public UniversalRenderPipelineAsset balancedURPAsset;
    public UniversalRenderPipelineAsset performantURPAsset;
    void Start()
    {
        musicVolumeSlider.maxValue = 100f;
        soundEffectSlider.maxValue = 100f; 
        dialogVolumeSlider.maxValue = 100f; 

        musicVolumeSlider.value = 50f;
        soundEffectSlider.value = 50f; 
        dialogVolumeSlider.value = 50f;

        RefreshDropdownText(); 
    }

    private void OnDestroy()
    {
        GraphicQualityDropdown.onValueChanged.RemoveAllListeners();
    }

    public void RefreshDropdownText()
    {
        GraphicQualityDropdown.onValueChanged.RemoveAllListeners();
        GraphicQualityDropdown.ClearOptions();
        List<string> graphicStrings = new List<string>();
        graphicStrings.Add("Low");
        graphicStrings.Add("Medium");
        graphicStrings.Add("High");
        GraphicQualityDropdown.AddOptions(graphicStrings);
        GraphicQualityDropdown.onValueChanged.AddListener((int x) => OnGraphicDropdownValueChanged(x));

        ResolutionDropdown.onValueChanged.RemoveAllListeners();
        ResolutionDropdown.ClearOptions();
        List<string> resolutioncStrings = new List<string>();
        resolutioncStrings.Add("1280x720");
        resolutioncStrings.Add("1600x900");
        resolutioncStrings.Add("1920x1080");
        ResolutionDropdown.AddOptions(resolutioncStrings);
        ResolutionDropdown.onValueChanged.AddListener((int x) => OnResolutionDropdownValueChanged(x));
    }

    public void SetMusicSlider()
    {
        float volume = musicVolumeSlider.value;
        AudioManager.instance.SetMusicVolume(volume * 0.01f);
    }

    public void SetSoundEffectsSlider()
    {
        float volume = soundEffectSlider.value;
        AudioManager.instance.SetSoundEffectsVolume(volume * 0.01f);
    }

    public void SetDialogsSlider()
    {
        float volume = dialogVolumeSlider.value;
        AudioManager.instance.SetDialogsVolume(volume * 0.01f);
    }

    public void OnGraphicDropdownValueChanged(int x)
    {
        Debug.Log("Graphic value changed: " + GraphicQualityDropdown.value);
        if (GraphicQualityDropdown.value == 0)
            GraphicsSettings.defaultRenderPipeline = performantURPAsset;
        else if (GraphicQualityDropdown.value == 1)
            GraphicsSettings.defaultRenderPipeline = balancedURPAsset;
        else if (GraphicQualityDropdown.value == 2)
            GraphicsSettings.defaultRenderPipeline = highFidelityURPAsset;

        QualitySettings.SetQualityLevel(GraphicQualityDropdown.value, true);
    }

    public void OnResolutionDropdownValueChanged(int x)
    {
        Debug.Log("Resolution value changed: " + ResolutionDropdown.value);
        Vector2Int resolution = Vector2Int.zero;
        if (ResolutionDropdown.value == 0)
            resolution = new Vector2Int(1280, 720);
        else if (ResolutionDropdown.value == 1)
            resolution = new Vector2Int(1600, 900);
        else if (ResolutionDropdown.value == 2)
            resolution = new Vector2Int(1920, 1080);

        Screen.SetResolution(resolution.x, resolution.y, true);
    }
}
