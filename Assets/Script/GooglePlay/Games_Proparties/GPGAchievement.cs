using GooglePlayGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPGAchievement{
    
    public void ShowAchievementsUI()
    {
        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            PlayGamesPlatform.Instance.ShowAchievementsUI();
        }
        else
        {
            Debug.LogWarning("User is not authenticated.");
        }
    }
    public void ShowAchievementInSentId(string _credentialId)
    {
        PlayGamesPlatform.Instance.ReportProgress(_credentialId, 100.0f, (bool success) => {
            if (success)
            {
                Debug.Log($"Achievement with ID {_credentialId} reported successfully.");
            }
            else
            {
                Debug.Log($"Failed to report achievement with ID {_credentialId}.");
            }
        });
    }
}
