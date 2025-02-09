using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

public class TimeManager : MonoBehaviour
{    
    public DateTime CurrentDateTime; // Su an ki mevcut zamani tutar.
    public TimeData timeData;
    public byte WhatDay = 0;
    public bool FirstOpen = true;
    private Coroutine timeProgressCoroutine;
    //Delegates
    public delegate void MinutePassedDelegate();
    public event MinutePassedDelegate OnOneMinutePassed;
    public event MinutePassedDelegate OnTwoMinutePassed;
    public event MinutePassedDelegate OnThreeMinutePassed;
    public event MinutePassedDelegate OnFourMinutePassed;
    public event MinutePassedDelegate OnFiveMinutePassed;
    public event MinutePassedDelegate OnTenMinutePassed;
    //Delegates
    public static TimeManager instance { get; set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        lastDailyCheck = false;
    }
    void Start()
    {
        // Belirli aralýklarla API'ye istek göndererek saati güncelle
        //InvokeRepeating("UpdateCurrentTime", 0f, 60f); // Her 60 saniyede bir güncelle
        
    }
    public IEnumerator UpdateCurrentTime()
    {
        yield return StartCoroutine(GetTime());
    }
    public bool lastDailyCheck;
    IEnumerator GetTime()
    {
        string apiUrl = "https://timeapi.io/api/Time/current/zone?timeZone=Europe/Istanbul";
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Hata: " + request.error);
            }
            else
            {
                string responseData = request.downloadHandler.text;
                // JSON verisini iþle
                timeData = JsonUtility.FromJson<TimeData>(responseData);

                // Saati alma

                // CurrentDateTime => 07:59:59
                CurrentDateTime = DateTime.Parse(timeData.dateTime);
                // CurrentDateTime => 08:00:00                
            }
        }
    }
    public void StartProgressCoroutine()
    {
        if (timeProgressCoroutine == null)
        {
            timeProgressCoroutine = StartCoroutine(TimeProgress());
        }
    }

    public void StopProgressCoroutine()
    {
        if (timeProgressCoroutine != null)
        {
            StopCoroutine(timeProgressCoroutine);
            timeProgressCoroutine = null;
        }
    }
    //private IEnumerator TimeProgress()
    //{
    //    int lastOneMinute = CurrentDateTime.Minute;
    //    int lastFiveMinute = CurrentDateTime.Minute + 5;
    //    int lastTenMinute = CurrentDateTime.Minute + 10;
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(1f); // 1 saniye bekle

    //        // Her dakika baþýnda delegate'i çaðýr
    //        if (CurrentDateTime.Minute != lastOneMinute)
    //        {
    //            OnOneMinutePassed?.Invoke();
    //            lastOneMinute = CurrentDateTime.Minute;
    //        }
    //        if (CurrentDateTime.Minute != lastFiveMinute)
    //        {
    //            OnFiveMinutePassed?.Invoke();
    //            lastFiveMinute = CurrentDateTime.Minute;
    //        }
    //        if (CurrentDateTime.Minute != lastTenMinute)
    //        {
    //            OnTenMinutePassed?.Invoke();
    //            lastTenMinute = CurrentDateTime.Minute;
    //        }

    //        if (GameManager.instance._rewardManager != null)
    //        {
    //            if (!lastDailyCheck)
    //                GameManager.instance._rewardManager.WaitForLastDailyRewardTime();
    //            GameManager.instance._rewardManager.CheckRewards();
    //            //Debug.Log("Þu an saat: " + CurrentDateTime.ToString("HH:mm:ss"));
    //        }

    //        CurrentDateTime = CurrentDateTime.AddSeconds(1); // Mevcut zamani 1 saniye arttir            
    //    }
    //}
    private IEnumerator TimeProgress()
    {
        int lastMinute = CurrentDateTime.Minute;

        // Her dakika baþýna doðru olacak þekilde tetikleme noktalarý
        HashSet<int> twoMinuteMarks = new HashSet<int>();
        HashSet<int> threeMinuteMarks = new HashSet<int>();
        HashSet<int> fourMinuteMarks = new HashSet<int>();
        HashSet<int> fiveMinuteMarks = new HashSet<int>();
        HashSet<int> tenMinuteMarks = new HashSet<int>();

        // Dakikalarý iþaretleyerek baþlatýyoruz
        for (int i = 2; i < 60; i += 2) twoMinuteMarks.Add(i);
        for (int i = 3; i < 60; i += 3) threeMinuteMarks.Add(i);
        for (int i = 4; i < 60; i += 4) fourMinuteMarks.Add(i);
        for (int i = 5; i < 60; i += 5) fiveMinuteMarks.Add(i);
        for (int i = 10; i < 60; i += 10) tenMinuteMarks.Add(i);

        while (true)
        {
            yield return new WaitForSeconds(1f); // 1 saniye bekle
            CurrentDateTime = CurrentDateTime.AddSeconds(1); // Zamaný ilerlet
            if (CurrentDateTime.Minute != lastMinute)
            {
                Debug.Log("CurrentDateTime dakika: " + CurrentDateTime.Minute);
                lastMinute = CurrentDateTime.Minute;

                // **1 Dakika iþlemleri**
                Debug.Log("1 dakika iþlemleri çalýþýyor...");
                OnOneMinutePassed?.Invoke();

                // **Diðer iþlemler**
                if (twoMinuteMarks.Contains(CurrentDateTime.Minute))
                {
                    Debug.Log("2 dakika iþlemleri çalýþýyor...");
                    OnTwoMinutePassed?.Invoke();
                }
                if (threeMinuteMarks.Contains(CurrentDateTime.Minute))
                {
                    Debug.Log("3 dakika iþlemleri çalýþýyor...");
                    OnThreeMinutePassed?.Invoke();
                }
                if (fourMinuteMarks.Contains(CurrentDateTime.Minute))
                {
                    Debug.Log("4 dakika iþlemleri çalýþýyor...");
                    OnFourMinutePassed?.Invoke();
                }
                if (fiveMinuteMarks.Contains(CurrentDateTime.Minute))
                {
                    Debug.Log("5 dakika iþlemleri çalýþýyor...");
                    OnFiveMinutePassed?.Invoke();
                }
                if (tenMinuteMarks.Contains(CurrentDateTime.Minute))
                {
                    Debug.Log("10 dakika iþlemleri çalýþýyor...");
                    OnTenMinutePassed?.Invoke();
                }
            }

            // **Ödül sistemi kontrolü**
            if (GameManager.instance._rewardManager != null)
            {
                if (!lastDailyCheck)
                    GameManager.instance._rewardManager.WaitForLastDailyRewardTime();

                GameManager.instance._rewardManager.CheckRewards();
            }
        }
    }








}

[Serializable]
public class TimeData
{
    public string dateTime;    
}