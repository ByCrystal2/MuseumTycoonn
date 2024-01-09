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

        // G�nl�k �d�l kontrol�
        if (currentTime >= lastDailyRewardTime + dailyRewardInterval)
        {
            // G�nl�k �d�l verme i�lemi
            // ...
            List<DailyRewardItemOptions> dailyRewards = FindObjectsOfType<DailyRewardItemOptions>().ToList();
            Debug.Log("Gunluk guncelleme");
            TimeManager.instance.timeData.WhatDay++;
            lastDailyRewardTime = currentTime; // Son al�nan g�nl�k �d�l zaman�n� g�ncelle
        }

        // Haftal�k �d�l kontrol�
        if (currentTime >= lastWeeklyRewardTime + weeklyRewardInterval)
        {
            // Haftal�k �d�l verme i�lemi
            // ...
            Debug.Log("Haftalik guncelleme");
            UIController.instance.SetNewWeeklyRewards();
            TimeManager.instance.timeData.WhatDay = 0;
            lastWeeklyRewardTime = currentTime; // Son al�nan haftal�k �d�l zaman�n� g�ncelle
        }
    }
}
