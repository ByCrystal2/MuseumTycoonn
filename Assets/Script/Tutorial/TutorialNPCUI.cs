using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialNPCUI : MonoBehaviour
{
    Animator anim;
    public string[] talkAnimNames = { "Talk1", "Talk2", "Talk3", "Talk4", "Talk5", "Talk6", "Talk7", "Talk8" };
    private int randomIndex;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    public void StartAnim()
    {
        randomIndex = Random.Range(0, talkAnimNames.Length);
        anim.SetBool(talkAnimNames[randomIndex], true);
    }
    public void StopAnim()
    {
        anim.SetBool(talkAnimNames[randomIndex], false);
    }
}