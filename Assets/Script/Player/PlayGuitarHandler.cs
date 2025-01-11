using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class PlayGuitarHandler : MonoBehaviour
{
    public ThirdPersonController Controller;

    public List<Button> DanceButtons;
    public Button PlayGuitarButton;

    public Button StopDanceButton;

    public List<ParticleSystem> GuitarEffect;
    public GameObject GuitarObject;

    private int _currentPlay;
    private void Start()
    {
        StopDanceButton.gameObject.SetActive(false);

        int length = DanceButtons.Count;
        for (int i = 0; i < length; i++)
        {
            int x = (i + 1);
            DanceButtons[i].onClick.AddListener(() => { OnSelectedADance(x); });
        }

        PlayGuitarButton.onClick.AddListener(() => { OnSelectedADance(101); });
        StopDanceButton.onClick.AddListener(StopPlay);
    }

    public void OnSelectedADance(int _playID)
    {
        if (!Controller.IsReady(3))
            return;

        if (_currentPlay == _playID)
        {
            StopPlay();
        }
        else
        {
            StartPlay(_playID);
        }
    }

    public void StopPlay()
    {
        _currentPlay = 0;
        Controller.OnEndPlay();

        GuitarObject.gameObject.SetActive(false);
        StopDanceButton.gameObject.SetActive(false);
        foreach (var item in GuitarEffect)
            item.Stop();
    }

    public void StartPlay(int _playID)
    {
        _currentPlay = _playID;
        Controller.OnStartPlay(_playID);

        StopDanceButton.gameObject.SetActive(true);

        if (_currentPlay == 101)
        {
            GuitarObject.gameObject.SetActive(true);
            foreach (var item in GuitarEffect)
                item.Play();
        }
        else
        {
            GuitarObject.gameObject.SetActive(false);
            foreach (var item in GuitarEffect)
                item.Stop();
        }
    }
}
