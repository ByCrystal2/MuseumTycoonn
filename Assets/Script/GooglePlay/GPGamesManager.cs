using GooglePlayGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPGamesManager : MonoBehaviour
{
    
    [SerializeField] public AchievementController achievementController;
    public static GPGamesManager instance { get; private set; }
    private void Awake()
    {
        if (instance !=null)
        {
            Destroy(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        achievementController.ResetValues();
    }

}