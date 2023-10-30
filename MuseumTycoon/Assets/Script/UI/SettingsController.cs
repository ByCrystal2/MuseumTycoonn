using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    public Slider musicVolumeSlider;
    public Slider soundEffectSlider;
    public Slider dialogVolumeSlider;

    
    void Start()
    {
        musicVolumeSlider.maxValue = 100f;
        soundEffectSlider.maxValue = 100f; 
        dialogVolumeSlider.maxValue = 100f; 

        musicVolumeSlider.value = 50f;
        soundEffectSlider.value = 50f; 
        dialogVolumeSlider.value = 50f;
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

}
