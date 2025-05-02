using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    //public DateTime lastDailyRewardTime;
    public DateTime lastWeeklyRewardTime;

    public TimeSpan dailyRewardInterval = TimeSpan.FromHours(24);
    public TimeSpan weeklyRewardInterval = TimeSpan.FromDays(7);
    public DateTime TimeRemaining;

    public DateTime currentTime;

    public bool Busy = true;

    public List<RewardAdData> rewardAdDatas = new List<RewardAdData>();

    private void Awake()
    {
        AddRewardAdDatas();
    }
    public void AddRewardAdDatas()
    {
        //5'likler
        //Gem
        RewardAdData rad1 = new RewardAdData(1, 5, ItemType.Gem, 5);
        RewardAdData rad2 = new RewardAdData(2, 6, ItemType.Gem, 5);
        RewardAdData rad3 = new RewardAdData(3, 7, ItemType.Gem, 5);
        RewardAdData rad4 = new RewardAdData(4, 8, ItemType.Gem, 5);
        RewardAdData rad5 = new RewardAdData(5, 9, ItemType.Gem, 5);
        RewardAdData rad6 = new RewardAdData(6, 10, ItemType.Gem, 5);
        //Gem

        //Gold
        RewardAdData rad7 = new RewardAdData(7, 500, ItemType.Gold, 5);
        RewardAdData rad8 = new RewardAdData(8, 600, ItemType.Gold, 5);
        RewardAdData rad9 = new RewardAdData(9, 700, ItemType.Gold, 5);
        RewardAdData rad10 = new RewardAdData(10, 800, ItemType.Gold, 5);
        RewardAdData rad11 = new RewardAdData(11, 900, ItemType.Gold, 5);
        RewardAdData rad12 = new RewardAdData(12, 1000, ItemType.Gold, 5);
        //Gold
        //5'likler

        //10'luklar
        //Gem
        RewardAdData rad13 = new RewardAdData(13, 11, ItemType.Gem, 10);
        RewardAdData rad14 = new RewardAdData(14, 12, ItemType.Gem, 10);
        RewardAdData rad15 = new RewardAdData(15, 13, ItemType.Gem, 10);
        RewardAdData rad16 = new RewardAdData(16, 14, ItemType.Gem, 10);
        RewardAdData rad17 = new RewardAdData(17, 15, ItemType.Gem, 10);
        RewardAdData rad18 = new RewardAdData(18, 16, ItemType.Gem, 10);
        //Gem

        //Gold
        RewardAdData rad19 = new RewardAdData(19, 1100, ItemType.Gold, 10);
        RewardAdData rad20 = new RewardAdData(20, 1200, ItemType.Gold, 10);
        RewardAdData rad21 = new RewardAdData(21, 1300, ItemType.Gold, 10);
        RewardAdData rad22 = new RewardAdData(22, 1400, ItemType.Gold, 10);
        RewardAdData rad23 = new RewardAdData(23, 1500, ItemType.Gold, 10);
        RewardAdData rad24 = new RewardAdData(24, 1600, ItemType.Gold, 10);
        //Gold
        //10'luklar

        //20'likler
        //Gem
        RewardAdData rad25 = new RewardAdData(25, 20, ItemType.Gem, 20);
        RewardAdData rad26 = new RewardAdData(26, 21, ItemType.Gem, 20);
        //RewardAdData rad27 = new RewardAdData(27, 22, ItemType.Gem, 20); Google addmoba eklendikten sonra aktif edilecek.
        //RewardAdData rad28 = new RewardAdData(28, 23, ItemType.Gem, 20);
        //RewardAdData rad29 = new RewardAdData(29, 24, ItemType.Gem, 20);
        //RewardAdData rad30 = new RewardAdData(30, 25, ItemType.Gem, 20);
        ////Gem

        //Gold
        RewardAdData rad31 = new RewardAdData(31, 3000, ItemType.Gold, 20);
        RewardAdData rad32 = new RewardAdData(32, 3100, ItemType.Gold, 20);
        RewardAdData rad33 = new RewardAdData(33, 3200, ItemType.Gold, 20);
        RewardAdData rad34 = new RewardAdData(34, 3300, ItemType.Gold, 20);
        RewardAdData rad35 = new RewardAdData(35, 3400, ItemType.Gold, 20);
        RewardAdData rad36 = new RewardAdData(36, 3500, ItemType.Gold, 20);
        //Gold
        //20'likler

        rewardAdDatas.Add(rad1);
        rewardAdDatas.Add(rad2);
        rewardAdDatas.Add(rad3);
        rewardAdDatas.Add(rad4);
        rewardAdDatas.Add(rad5);
        rewardAdDatas.Add(rad6);
        rewardAdDatas.Add(rad7);
        rewardAdDatas.Add(rad8);
        rewardAdDatas.Add(rad9);
        rewardAdDatas.Add(rad10);
        rewardAdDatas.Add(rad11);
        rewardAdDatas.Add(rad12);
        rewardAdDatas.Add(rad13);
        rewardAdDatas.Add(rad14);
        rewardAdDatas.Add(rad15);
        rewardAdDatas.Add(rad16);
        rewardAdDatas.Add(rad17);
        rewardAdDatas.Add(rad18);
        rewardAdDatas.Add(rad19);
        rewardAdDatas.Add(rad20);
        rewardAdDatas.Add(rad21);
        rewardAdDatas.Add(rad22);
        rewardAdDatas.Add(rad23);
        rewardAdDatas.Add(rad24);
        rewardAdDatas.Add(rad25);
        rewardAdDatas.Add(rad26);
        //rewardAdDatas.Add(rad27); Google addmoba eklendikten sonra aktif edilecek.
        //rewardAdDatas.Add(rad28);
        //rewardAdDatas.Add(rad29);
        //rewardAdDatas.Add(rad30);
        rewardAdDatas.Add(rad31);
        rewardAdDatas.Add(rad32);
        rewardAdDatas.Add(rad33);
        rewardAdDatas.Add(rad34);
        rewardAdDatas.Add(rad35);
        rewardAdDatas.Add(rad36);
    }
    public RewardAdData GetRewardAdWithMuseumLevel()
    {
        int museumLevel = MuseumManager.instance.GetCurrentCultureLevel();

        List<RewardAdData> desiredRewards = (museumLevel) switch
        {
            var x when x <= 5 && x < 10 => GetRewardAdsWithFocusedLevel(5),
            var x when x >= 10 && x < 20 => GetRewardAdsWithFocusedLevel(10),
            var x when x >= 20 && x < 30 => GetRewardAdsWithFocusedLevel(20),
            var x when x >= 30 && x < 40 => GetRewardAdsWithFocusedLevel(20),
            var x when x >= 40 && x < 50 => GetRewardAdsWithFocusedLevel(20),
            var x when x >= 50 && x < 60 => GetRewardAdsWithFocusedLevel(20),
            var x when x >= 60 => GetRewardAdsWithFocusedLevel(20),
        };
        RewardAdData adData = desiredRewards[UnityEngine.Random.Range(0, desiredRewards.Count)];
        return adData;
    }
    public bool IsSendingFocusedLevelSuitable(int _focusedLevel)
    {
        int museumLevel = MuseumManager.instance.GetCurrentCultureLevel();
        
         if (museumLevel <= 5 && museumLevel < 10)
        {
            if (_focusedLevel == 5)
                return true;
            else
                return false;
        }
        else if (museumLevel <= 10 && museumLevel < 15)
        {
            if (_focusedLevel == 10)
                return true;
            else
                return false;
        }
        else if (museumLevel <= 15 && museumLevel < 20)
        {
            if (_focusedLevel == 15)
                return true;
            else
                return false;
        }
        else
            return false;
    }
    public List<RewardAdData> GetRewardAdsWithFocusedLevel(int _level)
    {
        return rewardAdDatas.Where(x => x.FocusedLevel == _level).ToList();
    }
    public RewardAdData GetRewardAdsWithID(int _id)
    {
        return rewardAdDatas.Where(x=> x.ID == _id).SingleOrDefault();
    }

    private bool IsDailyRewardTimeEnd()
    {

        // Gunluk odul kontrolu
        //  13 + 24 - 7                   <=                                  24
        //Debug.Log("lastDailyRewardTime => " + lastDailyRewardTime.ToString());
        //Debug.Log("Kalan Zaman => " + ((lastDailyRewardTime + dailyRewardInterval) - currentTime).ToString());
        //Debug.Log("Kalan Zaman => " + ((lastDailyRewardTime + dailyRewardInterval) - currentTime) + " <= " + TimeSpan.Zero.ToString());
        if ((MuseumManager.instance.lastDailyRewardTime + dailyRewardInterval) - currentTime <= TimeSpan.Zero/* || TimeManager.instance.WhatDay == 0*/)
        {
            //TimeManager.instance.FirstOpen = false;
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
        if (MuseumManager.instance.lastDailyRewardTime.Year == 1)
            MuseumManager.instance.lastDailyRewardTime = currentTime; // Son al�nan gunluk odul zamanini guncelle
        if (Busy) return;
        if (!_overWrite)
        {
            if (!IsDailyRewardTimeEnd()) return;
        }

        // Günlük ödül verme işlemi
        // ...
        MuseumManager.instance.lastDailyRewardTime = currentTime; // Son alınan gunluk odul zamanini guncelle
        if (TimeManager.instance.WhatDay == 7)
        {
            // Haftalık ödül verme işlemi
            // ...
            Debug.Log("Haftalik guncelleme");
            ItemManager.instance.SetCalculatedDailyRewardItems();                
            TimeManager.instance.WhatDay = 0;
            lastWeeklyRewardTime = currentTime; // Son alinan haftalik odul zamanini guncelle
        }
            

        List<ItemData> dailyRewards = ItemManager.instance.GetAllDailyRewardItemDatas();
        Debug.Log("Gunluk guncelleme");

        int index = TimeManager.instance.WhatDay;

        // Eğer bulunduysa
        if (index != -1)
        {
            // Orijinal listedeki öğeyi al
            var originalItem = ItemManager.instance.CurrentDailyRewardItems[index];

            // Orijinal öğenin bir kopyasını oluştur
            var updatedItem = originalItem;

            // Kopyanın üzerinde değişiklik yap
            updatedItem.IsLocked = false;

            // Kopyayı orijinal listeye geri yerleştir
            ItemManager.instance.CurrentDailyRewardItems[index] = updatedItem;
        }

        Debug.Log("[BEFORE] TimeManager.instance.timeData.WhatDay => " + TimeManager.instance.WhatDay);
        TimeManager.instance.WhatDay++;
        Debug.Log("[AFTER] TimeManager.instance.timeData.WhatDay => " + TimeManager.instance.WhatDay);

        UIController.instance.SetUpdateWeeklyRewards();
        //GameManager.instance.Save();

    }
    WaitForEndOfFrame wait;
    Coroutine currentReward; 
    public IEnumerator WaitForLastDailyRewardTimeCoroutine()
    {
        Busy = true;
        while (MuseumManager.instance.lastDailyRewardTime.Year < 2024)
        {
            yield return wait;
            Debug.Log("LastDailyRewardTime verisi dogrulanmayı bekliyor... => " + MuseumManager.instance.lastDailyRewardTime.Year);
        }
        Busy = false;
        currentReward = null;
        Debug.Log("WaitForLastDailyRewardTimeCoroutine coroutine i sonlandi.");
    }
    
    public void WaitForLastDailyRewardTime()
    {
        if (currentReward == null)
        {
            currentReward = StartCoroutine(WaitForLastDailyRewardTimeCoroutine());
            TimeManager.instance.lastDailyCheck = true;
        }
        else
        {
            Debug.Log("Coroutine hala calisiyor yenisini baslatma islemi iptal edildi.");
        }
    }
}
