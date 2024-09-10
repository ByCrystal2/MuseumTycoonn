using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSoundHandler : MonoBehaviour
{
    [SerializeField] AudioClip sFX;
    [SerializeField,Range(0,1)] float volume=1;
    [SerializeField,Range(0,1)] float pitch=1;
    Button m_Button;
    private void Awake()
    {
        m_Button = GetComponent<Button>();
    }
    private void Start()
    {
        m_Button.onClick.AddListener(SendSoundToAudioManager);
    }
    void SendSoundToAudioManager()
    {
        AudioManager.instance.PlaySound(sFX, transform.position,volume,pitch);
    }
    public void SetVolume(float _volume)
    {
        volume = _volume;
    }
}
