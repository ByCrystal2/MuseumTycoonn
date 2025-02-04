using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [Header("Buttons")]
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button achievementButton;

    [Header("Audio Settings")]
    [SerializeField] private Slider generalVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider soundEffectSlider;
    [SerializeField] private Slider dialogVolumeSlider;

    [Header("Graphics Settings")]
    [SerializeField] private Dropdown graphicQualityDropdown;
    [SerializeField] private Dropdown resolutionDropdown;

    [Header("URP Assets")]
    [SerializeField] private UniversalRenderPipelineAsset highFidelityURPAsset;
    [SerializeField] private UniversalRenderPipelineAsset balancedURPAsset;
    [SerializeField] private UniversalRenderPipelineAsset performantURPAsset;

    private void Awake()
    {
        achievementButton.onClick.AddListener(ShowAchievementsUI);
        creditsButton.onClick.AddListener(CreditsHandler.instance.CreditsPanelSetActivation);
    }

    private void Start()
    {
        InitializeSettings();
    }

    private void OnEnable()
    {
        OnSettingsPanelOpened();
    }
    public void OnSettingsPanelOpened()
    {
        LoadCurrentAudioSettings();
        LoadCurrentGraphicSettings();
    }

    private void InitializeSettings()
    {
        // Slider ayarlarý
        generalVolumeSlider.maxValue = 100f;
        musicVolumeSlider.maxValue = 100f;
        soundEffectSlider.maxValue = 100f;
        dialogVolumeSlider.maxValue = 100f;

        // Dinleyicileri ekle
        generalVolumeSlider.onValueChanged.AddListener(_ => SetGeneralVolume());
        musicVolumeSlider.onValueChanged.AddListener(_ => SetMusicVolume());
        soundEffectSlider.onValueChanged.AddListener(_ => SetSoundEffectsVolume());
        dialogVolumeSlider.onValueChanged.AddListener(_ => SetDialogVolume());

        // Grafik ve çözünürlük dropdown'larýný hazýrla
        RefreshDropdowns();
    }

    

    private void LoadCurrentAudioSettings()
    {
        // Mevcut ses ayarlarýný AudioManager'dan çek
        generalVolumeSlider.value = AudioManager.instance.GetGeneralVolume() * 100f;
        musicVolumeSlider.value = AudioManager.instance.GetMusicVolume() * 100f;
        soundEffectSlider.value = AudioManager.instance.GetSoundEffectsVolume() * 100f;
        dialogVolumeSlider.value = AudioManager.instance.GetDialogsVolume() * 100f;
    }

    private void LoadCurrentGraphicSettings()
    {
        // Grafik kalitesi ayarýný ve çözünürlüðü güncelle
        graphicQualityDropdown.value = QualitySettings.GetQualityLevel();
        resolutionDropdown.value = GetResolutionDropdownIndex(Screen.currentResolution);
    }

    private int GetResolutionDropdownIndex(Resolution resolution)
    {
        if (resolution.width == 1280 && resolution.height == 720) return 0;
        if (resolution.width == 1600 && resolution.height == 900) return 1;
        if (resolution.width == 1920 && resolution.height == 1080) return 2;
        return 0; // Varsayýlan olarak 720p
    }

    private void RefreshDropdowns()
    {
        ConfigureDropdown(graphicQualityDropdown, new List<string> { "Low", "Medium", "High" }, OnGraphicQualityChanged);
        ConfigureDropdown(resolutionDropdown, new List<string> { "1280x720", "1600x900", "1920x1080" }, OnResolutionChanged);
    }

    private void ConfigureDropdown(Dropdown dropdown, List<string> options, UnityEngine.Events.UnityAction<int> callback)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.onValueChanged.AddListener(callback);
    }
    private void SetGeneralVolume()
    {
        float volume = generalVolumeSlider.value * 0.01f;
        AudioManager.instance.SetGeneralVolume(volume);
    }
    private void SetMusicVolume()
    {
        float volume = musicVolumeSlider.value * 0.01f;
        AudioManager.instance.SetMusicVolume(volume);
    }

    private void SetSoundEffectsVolume()
    {
        float volume = soundEffectSlider.value * 0.01f;
        AudioManager.instance.SetSoundEffectsVolume(volume);
    }

    private void SetDialogVolume()
    {
        float volume = dialogVolumeSlider.value * 0.01f;
        AudioManager.instance.SetDialogsVolume(volume);
    }

    private void OnGraphicQualityChanged(int value)
    {
        UniversalRenderPipelineAsset selectedURPAsset = value switch
        {
            0 => performantURPAsset,
            1 => balancedURPAsset,
            2 => highFidelityURPAsset,
            _ => performantURPAsset
        };

        GraphicsSettings.defaultRenderPipeline = selectedURPAsset;
        QualitySettings.SetQualityLevel(value, true);
        Debug.Log($"Graphic quality changed to: {graphicQualityDropdown.options[value].text}");
    }

    private void OnResolutionChanged(int value)
    {
        Vector2Int resolution = value switch
        {
            0 => new Vector2Int(1280, 720),
            1 => new Vector2Int(1600, 900),
            2 => new Vector2Int(1920, 1080),
            _ => new Vector2Int(Screen.width, Screen.height)
        };

        Screen.SetResolution(resolution.x, resolution.y, true);
        Debug.Log($"Resolution changed to: {resolution.x}x{resolution.y}");
    }

    private void ShowAchievementsUI()
    {
        GPGamesManager.instance.achievementController.ShowAchievements();
    }
}
