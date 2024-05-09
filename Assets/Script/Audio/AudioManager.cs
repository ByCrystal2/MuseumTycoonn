using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private List<AudioSource> MenuSources;
    [SerializeField] private List<AudioSource> GameSources;

    [SerializeField] private List<AudioSource> SoundEffectsSources;
    [SerializeField] private List<AudioSource> DialogsSources;

    List<SoundData> SoundEffects = new List<SoundData>();
    List<DialogData> Dialogs = new List<DialogData>();

    
    public static AudioManager instance { get; private set; }

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
       
    }

    public void PlayMusicOfMenu()
    {
        int randomIndex = Random.Range(0, MenuSources.Count);
        if (GameSources.Find(x => x.isPlaying == true) != null)
        {
            GameSources.Find(x => x.isPlaying == true).Stop();
        }
        MenuSources[randomIndex].Play();
    }
    public void PlayMusicOfGame()
    {
        int randomIndex = Random.Range(0, GameSources.Count);
        if (MenuSources.Find(x => x.isPlaying == true) != null)
        {
            MenuSources.Find(x=> x.isPlaying == true).Stop();
        }
        GameSources[randomIndex].Play();
    }

    public void SetMusicVolume(float volume) 
    {        
        Debug.Log("Gelen Music Volume Deðeri: " + volume);
        foreach (AudioSource audioSource in GameSources)
        {
            audioSource.volume = volume;
        }
        foreach (AudioSource audioSource in MenuSources)
        {
            audioSource.volume = volume;
        }
    }

    public void SetSoundEffectsVolume(float volume) 
    {
        Debug.Log("Gelen SoundEffects Volume Deðeri: " + volume);
        foreach (AudioSource audioSource in SoundEffects.Select(x => x.AudioSource).ToList())
        {
            audioSource.volume = volume;
        }
    }

    public void SetDialogsVolume(float volume) 
    {
        Debug.Log("Gelen Dialogs Volume Deðeri: " + volume);
        List<GameObject> gos = FindObjectsOfType<NPCBehaviour>().Select(x => x.gameObject).ToList();

        //for (int i = 0; i < gos.Count; i++)
        //{
        //    GetDialogAudios(gos[i].GetComponent<NPCBehaviour>().MySources)[i].volume = volume;
        //}
        
    }

    public void AllAudioSourcesOptions()
    {
        // Önceki listeden temizlenir.
        SoundEffects.Clear();
        Dialogs.Clear();
        GameManager.instance.allAudioSources.Clear();

        AddingSoundEffects();
        AddingDialogs();
        
        AudioSource[] audioSourcesInScene = FindObjectsOfType<AudioSource>();
        GameManager.instance.allAudioSources.AddRange(audioSourcesInScene);
    }
    public void AddingSoundEffects()
    {
        //Ses Effectleri eklenecek.
        SoundData sd1 = new SoundData(0, SoundEffectsSources[0],SoundEffectType.EarnGold);
        SoundData sd2 = new SoundData(1, SoundEffectsSources[1],SoundEffectType.Punch);
        SoundData sd3 = new SoundData(2, SoundEffectsSources[2],SoundEffectType.ComeToKing);
        SoundData sd4 = new SoundData(3, SoundEffectsSources[3],SoundEffectType.GoldBoxOpen);
        SoundData sd5 = new SoundData(4, SoundEffectsSources[4],SoundEffectType.Victory);
        SoundData sd6 = new SoundData(5, SoundEffectsSources[5],SoundEffectType.Writing);

        //Adding
        SoundEffects.Add(sd1);
        SoundEffects.Add(sd2);
        SoundEffects.Add(sd3);
        SoundEffects.Add(sd4);
        SoundEffects.Add(sd5);
        SoundEffects.Add(sd6);
    }
    public void AddingDialogs()
    {
        //Dialoglar eklenecek.

        //Male
        DialogData dd1 = new DialogData(0, DialogsSources[0], DialogType.NpcAngry,true,false);
        DialogData dd2 = new DialogData(1, DialogsSources[1], DialogType.NpcHappiness, true, false);
        DialogData dd3 = new DialogData(2, DialogsSources[2], DialogType.NpcHmm, true, false);
        DialogData dd4 = new DialogData(3, DialogsSources[3], DialogType.NpcHmm, true, false);
        DialogData dd5 = new DialogData(4, DialogsSources[4], DialogType.NpcSad, true, false);
        DialogData dd6 = new DialogData(5, DialogsSources[5], DialogType.NpcTalking, true, false);
        DialogData dd18 = new DialogData(16, DialogsSources[17], DialogType.NpcByeBye, true, false  );
        DialogData dd19 = new DialogData(17, DialogsSources[18], DialogType.NpcDisLike, true, false  );
        DialogData dd20 = new DialogData(18, DialogsSources[19], DialogType.NpcLike, true, false  );
        DialogData dd21 = new DialogData(19, DialogsSources[20], DialogType.NpcLike, true, false  );
        //Female
        DialogData dd7 = new DialogData(6, DialogsSources[6], DialogType.NpcLike, false, false);
        DialogData dd8 = new DialogData(7, DialogsSources[7], DialogType.NpcHmm, false, false);

        //Punchs
        DialogData dd9 = new DialogData(8, DialogsSources[8], DialogType.NpcPunch, false, true);
        DialogData dd10 = new DialogData(9, DialogsSources[9], DialogType.NpcPunch, false, true);
        DialogData dd11 = new DialogData(10, DialogsSources[10], DialogType.NpcPunch, false, true);
        DialogData dd12 = new DialogData(11, DialogsSources[11], DialogType.NpcPunch, false, true);
        DialogData dd13 = new DialogData(12, DialogsSources[12], DialogType.NpcPunch, false, true);
        DialogData dd14 = new DialogData(13, DialogsSources[13], DialogType.NpcPunch, false, true);
        DialogData dd15 = new DialogData(14, DialogsSources[14], DialogType.NpcPunch, false, true);
        DialogData dd16 = new DialogData(15, DialogsSources[15], DialogType.NpcPunch, false, true);
        DialogData dd17 = new DialogData(15, DialogsSources[16], DialogType.NpcPunch, false, true);

        //Adding
        Dialogs.Add(dd1);
        Dialogs.Add(dd2);
        Dialogs.Add(dd3);
        Dialogs.Add(dd4);
        Dialogs.Add(dd5);
        Dialogs.Add(dd6);
        Dialogs.Add(dd7);
        Dialogs.Add(dd8);
        Dialogs.Add(dd9);
        Dialogs.Add(dd10);
        Dialogs.Add(dd11);
        Dialogs.Add(dd12);
        Dialogs.Add(dd13);
        Dialogs.Add(dd14);
        Dialogs.Add(dd15);
        Dialogs.Add(dd16);
        Dialogs.Add(dd17);
        Dialogs.Add(dd18);
        Dialogs.Add(dd19);
        Dialogs.Add(dd20);
        Dialogs.Add(dd21);
    }

    public List<SoundData> GetSoundEffects(SoundEffectType _effectType)
    {
        return SoundEffects.Where(x =>  x.EffectType == _effectType).ToList();
    }
    public List<DialogData> GetDialogs(bool _isMale)
    {
        return Dialogs.Where(x => x.IsMale == _isMale || x.IsGlobal).ToList();
    }
    public List<DialogData> GetDialogs(bool _isMale, DialogType _dialogType, bool _isGlobal)
    {
        return Dialogs.Where(x=> x.IsMale == _isMale && x.DiaType == _dialogType && x.IsGlobal == _isGlobal).ToList();
    }

    public void GetDialogAudios(List<DialogData> _dialogDatas, DialogType _dialogType, AudioSource _audioSource)
    {
        _audioSource.Stop();
        List<AudioSource> audioSources = _dialogDatas.Where(x=> x.DiaType == _dialogType).Select(x => x.AudioSource).ToList();
        if(audioSources.Count > 0)
        {
            _audioSource = audioSources[Random.Range(0,audioSources.Count)];
            _audioSource.Play();
        }
    }

    public List<AudioSource> GetDialogAudios(List<DialogData> _dialogDatas)
    {
        return _dialogDatas.Select(x => x.AudioSource).ToList();
    }

}
public enum SoundEffectType
{   
    ComeToKing,
    EarnGold,
    Punch,
    Victory,
    GoldBoxOpen,
    Writing
    
    // Diðer ses efekti türleri buraya eklenebilir
}
public enum DialogType
{
    NpcLike,
    NpcDisLike,
    NpcHappiness,
    NpcByeBye,
    NpcSad,
    NpcAngry,
    NpcPunch,
    NpcHmm,
    NpcTalking

}

