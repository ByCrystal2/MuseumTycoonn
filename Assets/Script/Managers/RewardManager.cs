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
    public DateTime TimeRemaining;
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
    private void Update()
    {
        
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
            UIController.instance.SetNewWeeklyRewards();
            Debug.Log("Gunluk guncelleme");
            TimeManager.instance.timeData.WhatDay++;
            lastDailyRewardTime = currentTime; // Son alýnan günlük ödül zamanýný güncelle
            TimeRemaining = currentTime;
        }

        // Haftalýk ödül kontrolü
        if (currentTime >= lastWeeklyRewardTime + weeklyRewardInterval)
        {
            // Haftalýk ödül verme iþlemi
            // ...
            Debug.Log("Haftalik guncelleme");
            ItemManager.instance.SetCalculatedDailyRewardItems();
            UIController.instance.SetNewWeeklyRewards();
            TimeManager.instance.timeData.WhatDay = 0;
            lastWeeklyRewardTime = currentTime; // Son alýnan haftalýk ödül zamanýný güncelle
        }
    }
    public bool CheckTheInRewardControl()
    {
        DateTime currentTime = TimeManager.instance.CurrentDateTime;
        if (currentTime >= lastDailyRewardTime + dailyRewardInterval || currentTime >= lastWeeklyRewardTime + weeklyRewardInterval)
            return true;
        else
            return false;
    }
    public DateTime GetTimeRemaining()
    {
        DateTime currentTime = TimeManager.instance.CurrentDateTime;
        TimeSpan timeRemaining = lastDailyRewardTime + dailyRewardInterval - currentTime;
        TimeRemaining = DayControl(currentTime, timeRemaining);  // DayControl'u burada kullanýn
        return TimeRemaining;
    }
    public DateTime DayControl( DateTime currentTime, TimeSpan TimeRemaining)
    {
        int Year = currentTime.Year;
        int Month = currentTime.Month;
        int Day = currentTime.Day;
        int Hour = TimeRemaining.Hours;
        int Minute = TimeRemaining.Minutes;
        int Second = TimeRemaining.Seconds;
        Debug.Log("Before=> " + Year + " " + Month + " " + Day + " " + Hour + " " + Minute + " " + Second);
        Second--;
        if (Second <= 0)
        {
            Minute--;
            Second = 60;
            if (Minute <= 0)
            {
                Hour--;
                Minute = 60;
                if (Hour < 0)
                {
                    Hour = 24;
                    Minute = 60;
                    Second = 60;
                }
            }
        }
        Debug.Log("After=> " + Year + " " + Month + " " + Day + " " + Hour + " " + Minute + " " + Second);
        return new DateTime(Year, Month, Day, Hour, Minute, Second);
    }
}
