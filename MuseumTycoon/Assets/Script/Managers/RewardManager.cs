using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    private DateTime lastDailyRewardTime;
    private DateTime lastWeeklyRewardTime;

    private TimeSpan dailyRewardInterval = TimeSpan.FromHours(24);
    private TimeSpan weeklyRewardInterval = TimeSpan.FromDays(7);

    public static RewardManager instance { get; set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void CheckRewards()
    {
        DateTime currentTime = TimeManager.instance.CurrentDateTime;

        // Günlük ödül kontrolü
        if (currentTime >= lastDailyRewardTime + dailyRewardInterval)
        {
            // Günlük ödül verme iþlemi
            // ...
            List<DailyRewardItemOptions> dailyRewards = FindObjectsOfType<DailyRewardItemOptions>().ToList();
            Debug.Log("Gunluk guncelleme");
            TimeManager.instance.timeData.WhatDay++;
            lastDailyRewardTime = currentTime; // Son alýnan günlük ödül zamanýný güncelle
        }

        // Haftalýk ödül kontrolü
        if (currentTime >= lastWeeklyRewardTime + weeklyRewardInterval)
        {
            // Haftalýk ödül verme iþlemi
            // ...
            Debug.Log("Haftalik guncelleme");
            UIController.instance.SetNewWeeklyRewards();
            TimeManager.instance.timeData.WhatDay = 0;
            lastWeeklyRewardTime = currentTime; // Son alýnan haftalýk ödül zamanýný güncelle
        }
    }
}
