using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMenuManager : MonoBehaviour
{
    [SerializeField] private AudioSource menuSource;
    [SerializeField] private AudioSource gameSource;

    public static AudioMenuManager instance { get; private set; }

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

    public void PlayMusicOfMenu()
    {
        if (gameSource.isPlaying)
        {
            gameSource.Stop();            
        }
        menuSource.Play();
    }
    public void PlayMusicOfGame()
    {
        if (menuSource.isPlaying)
        {
            menuSource.Stop();
        }
        gameSource.Play();
    }
}
