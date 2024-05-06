using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public DateTime lastDailyRewardTime;
    public DateTime lastWeeklyRewardTime;

    public TimeSpan dailyRewardInterval = TimeSpan.FromHours(24);
    public TimeSpan weeklyRewardInterval = TimeSpan.FromDays(7);
    public DateTime TimeRemaining;

    public DateTime currentTime;

    public bool Busy = true;
    private bool IsDailyRewardTimeEnd()
    {

        // Gunluk odul kontrolu
        //  13 + 24 - 7                   <=                                  24
        //Debug.Log("lastDailyRewardTime => " + lastDailyRewardTime.ToString());
        //Debug.Log("Kalan Zaman => " + ((lastDailyRewardTime + dailyRewardInterval) - currentTime).ToString());
        //Debug.Log("Kalan Zaman => " + ((lastDailyRewardTime + dailyRewardInterval) - currentTime) + " <= " + TimeSpan.Zero.ToString());
        if ((lastDailyRewardTime + dailyRewardInterval) - currentTime <= TimeSpan.Zero/* || TimeManager.instance.WhatDay == 0*/)
        {
            TimeManager.instance.FirstOpen = false;
            return true;
        }
        else
        {
            return false;
        }
        
    }
    public void CheckRewards(bool _overWrite = false)
    {
        currentTime = TimeManager.instance.CurrentDateTime;
        if (Busy) return;
        if (!_overWrite)
        {
            if (!IsDailyRewardTimeEnd()) return;
        }
        
        // G�nl�k �d�l verme i�lemi
        // ...
        lastDailyRewardTime = currentTime; // Son al�nan gunluk odul zamanini guncelle
        if (TimeManager.instance.WhatDay == 7)
        {
            // Haftal�k �d�l verme i�lemi
            // ...
            Debug.Log("Haftalik guncelleme");
            ItemManager.instance.SetCalculatedDailyRewardItems();                
            TimeManager.instance.WhatDay = 0;
            lastWeeklyRewardTime = currentTime; // Son alinan haftalik odul zamanini guncelle
        }
            

        List<ItemData> dailyRewards = ItemManager.instance.GetAllDailyRewardItemDatas();
        Debug.Log("Gunluk guncelleme");

        int index = TimeManager.instance.WhatDay;

        // E�er bulunduysa
        if (index != -1)
        {
            // Orijinal listedeki ��eyi al
            var originalItem = ItemManager.instance.CurrentDailyRewardItems[index];

            // Orijinal ��enin bir kopyas�n� olu�tur
            var updatedItem = originalItem;

            // Kopyan�n �zerinde de�i�iklik yap
            updatedItem.IsLocked = false;

            // Kopyay� orijinal listeye geri yerle�tir
            ItemManager.instance.CurrentDailyRewardItems[index] = updatedItem;
        }

        Debug.Log("[BEFORE] TimeManager.instance.timeData.WhatDay => " + TimeManager.instance.WhatDay);
        TimeManager.instance.WhatDay++;
        Debug.Log("[AFTER] TimeManager.instance.timeData.WhatDay => " + TimeManager.instance.WhatDay);

        UIController.instance.SetUpdateWeeklyRewards();
        GameManager.instance.Save();
        
    }
    WaitForEndOfFrame wait;
    public IEnumerator WaitForLastDailyRewardTime()
    {
        Busy = true;
        while (lastDailyRewardTime.Year < 2024)
        {
            yield return wait;
            Debug.Log("LastDailyRewardTime verisi dogrulanmay� bekliyor... => " + lastDailyRewardTime.Year);
        }
        Busy = false;
    }
    
}
