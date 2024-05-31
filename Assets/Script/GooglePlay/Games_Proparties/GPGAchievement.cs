using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPGAchievement{
    
    public void ShowAchievementsUI()
    {
        Social.ShowAchievementsUI();
    }
    public void ShowAchievementInSentId(string _creditinalId)
    {
        Social.ReportProgress(_creditinalId, 0, (bool success) => {
            if (success)
            {
                Debug.Log($"Showing achievement with {_creditinalId} ids ");
            }
        });
    }
}
