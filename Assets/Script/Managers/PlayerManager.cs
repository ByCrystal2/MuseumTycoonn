using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] ThirdPersonController player;

    public static PlayerManager instance { get; private set; }  
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
        if (SceneManager.GetActiveScene().name == Scenes.TutorialLevel.ToString()) return;
        DontDestroyOnLoad(gameObject);
        UnLockPlayer();
    }
    
    public void LockPlayer()
    {
        player.MoveSpeed = 0;
        player.JumpHeight = 0;
        player.enabled = false;
    }
    public void UnLockPlayer()
    {
        player.MoveSpeed = 2;
        player.JumpHeight = 1.2f;
        player.enabled = true;
    }
    public ThirdPersonController GetPlayer()
    {
        return player;
    }
}
