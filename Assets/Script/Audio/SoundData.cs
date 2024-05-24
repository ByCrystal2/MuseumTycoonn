using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundData
{
    public int ID;
    public AudioClip MyClip;
    public SoundEffectType EffectType;
    public float VolumeValue = 1f;
    public SoundData(int iD, AudioSource _audioSource, SoundEffectType _effectType)
    {
        this.ID = iD;
        this.MyClip = _audioSource.clip;
        this.EffectType = _effectType;       
    }
    public SoundData(SoundData soundData)
    {
        this.ID = soundData.ID;
        this.MyClip = soundData.MyClip;
        this.EffectType = soundData.EffectType;
        this.VolumeValue = soundData.VolumeValue;
    }
}
