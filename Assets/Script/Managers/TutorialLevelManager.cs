using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLevelManager : MonoBehaviour
{
    public bool IsWatchTutorial = false;
    [SerializeField] AnimationClip[] RandomAnimations;
    public static TutorialLevelManager instance { get; private set; }
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        if (GameManager.instance != null)
            GameManager.instance.LoadIsWatchTutorial();            
        
        if (IsWatchTutorial)
        {
            Debug.LogError("Tutorial zaten izlenmis!");
            return;
        }
    }
    public AnimationClip GetRandomAnim()
    {
        int index = Random.Range(0, RandomAnimations.Length);
        return RandomAnimations[index];
    }

    public void OnEndFlyCutscene()
    {
        if (AudioManager.instance != null)
        AudioManager.instance.TutorialSource.Stop();
        DialogueTrigger kingTrigger = GameObject.FindWithTag("TutorialNPC").GetComponent<DialogueTrigger>();
        kingTrigger.TriggerDialog(Steps.Step1);
        kingTrigger.GetComponent<TutorialNPCBehaviour>().ProcessAnim("Sit", false);
    }
}
