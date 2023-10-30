using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class DialogData
{
    public int ID;
    public AudioSource AudioSource;
    public DialogType DiaType;
    public float VolumeValue = 1f;
    public bool IsMale;
    public bool IsGlobal;
    public DialogData(int iD, AudioSource _audioSource, DialogType _dialogType, bool isMale, bool _isGlobal)
    {

        _audioSource.volume = VolumeValue;
        this.ID = iD;
        this.AudioSource = _audioSource;
        this.DiaType = _dialogType;
        this.IsMale = isMale;
        this.IsGlobal = _isGlobal;
    }
    public DialogData(DialogData _dialogData)
    {        
        this.ID = _dialogData.ID;
        this.AudioSource = _dialogData.AudioSource;
        this.DiaType = _dialogData.DiaType;
        this.VolumeValue = _dialogData.VolumeValue;
        this.IsMale = _dialogData.IsMale;
        this.IsGlobal = _dialogData.IsGlobal;
    }
}
