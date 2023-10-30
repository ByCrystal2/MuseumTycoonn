using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class SoundData
{
    public int ID;
    public AudioSource AudioSource;
    public SoundEffectType EffectType;
    public float VolumeValue = 1f;
    public SoundData(int iD, AudioSource _audioSource, SoundEffectType _effectType)
    {
        this.ID = iD;
        this.AudioSource = _audioSource;
        this.EffectType = _effectType;       
    }
    public SoundData(SoundData soundData)
    {
        this.ID = soundData.ID;
        this.AudioSource = soundData.AudioSource;
        this.EffectType = soundData.EffectType;
        this.VolumeValue = soundData.VolumeValue;
    }
}
