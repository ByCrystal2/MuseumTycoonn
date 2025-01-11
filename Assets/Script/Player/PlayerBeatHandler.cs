using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBeatHandler : MonoBehaviour
{
    public Button HitButton;

    public GameObject BeatCollider;
    public ThirdPersonController Controller;

    public float SecondsBetweenHits = 2;
    public List<float> SecondsActivateHit;
    public List<float> HitLengths;

    private bool _Beating;
    private int _currentCombo;
    private float _timerBetweenCombos;

#if UNITY_EDITOR
    [Header("DEBUG")]
    public bool DebugActive;
#endif

    private void Start()
    {
        HitButton.onClick.AddListener(Beat);
    }

    private void Update()
    {
        if (_Beating && _timerBetweenCombos + 2.5f < Time.time)
        {
            _currentCombo = 0;
            EndBeat();
            EndHit();
        }
    }


    public void Beat()
    {
        if (_currentCombo == 4)
            return;

        if (_timerBetweenCombos < Time.time)
        {
            CancelInvoke();

            _currentCombo++;

            _Beating = true;
            Controller.Beat(_currentCombo);

            _timerBetweenCombos = Time.time + HitLengths[_currentCombo - 1];

            Invoke(nameof(Hit), SecondsActivateHit[_currentCombo - 1]);
            Invoke(nameof(EndBeat), SecondsBetweenHits);
#if UNITY_EDITOR
            if (DebugActive)
                Debug.Log("Beat Triggered! _timerBetweenCombos: " + _timerBetweenCombos + "/Time.time: " + Time.time + "/Combo: " + _currentCombo);
#endif
        }
        else
        {
            
        }
    }

    public void EndBeat()
    {
        _currentCombo = 0;

        _Beating = false;
        Controller.EndBeat();
#if UNITY_EDITOR
        if (DebugActive)
            Debug.Log("Beat Ends!");
#endif
    }

    public void Hit()
    {
        BeatCollider.SetActive(true);

        Invoke(nameof(EndHit), 0.1f);

#if UNITY_EDITOR
        if (DebugActive)
            Debug.Log("Hit success");
#endif
    }

    public void EndHit()
    {
#if UNITY_EDITOR
        if (DebugActive)
            Debug.Log("Hit ends");
#endif
        BeatCollider.SetActive(false);

        Controller.EndHit();
    }
}
