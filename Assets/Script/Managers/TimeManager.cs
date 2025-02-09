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
        // Belirli aral�klarla API'ye istek g�ndererek saati g�ncelle
        //InvokeRepeating("UpdateCurrentTime", 0f, 60f); // Her 60 saniyede bir g�ncelle
        
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
                // JSON verisini i�le
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

    //        // Her dakika ba��nda delegate'i �a��r
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
    //            //Debug.Log("�u an saat: " + CurrentDateTime.ToString("HH:mm:ss"));
    //        }

    //        CurrentDateTime = CurrentDateTime.AddSeconds(1); // Mevcut zamani 1 saniye arttir            
    //    }
    //}
    private IEnumerator TimeProgress()
    {
        int lastMinute = CurrentDateTime.Minute;

        // Her dakika ba��na do�ru olacak �ekilde tetikleme noktalar�
        HashSet<int> twoMinuteMarks = new HashSet<int>();
        HashSet<int> threeMinuteMarks = new HashSet<int>();
        HashSet<int> fourMinuteMarks = new HashSet<int>();
        HashSet<int> fiveMinuteMarks = new HashSet<int>();
        HashSet<int> tenMinuteMarks = new HashSet<int>();

        // Dakikalar� i�aretleyerek ba�lat�yoruz
        for (int i = 2; i < 60; i += 2) twoMinuteMarks.Add(i);
        for (int i = 3; i < 60; i += 3) threeMinuteMarks.Add(i);
        for (int i = 4; i < 60; i += 4) fourMinuteMarks.Add(i);
        for (int i = 5; i < 60; i += 5) fiveMinuteMarks.Add(i);
        for (int i = 10; i < 60; i += 10) tenMinuteMarks.Add(i);

        while (true)
        {
            yield return new WaitForSeconds(1f); // 1 saniye bekle
            CurrentDateTime = CurrentDateTime.AddSeconds(1); // Zaman� ilerlet
            if (CurrentDateTime.Minute != lastMinute)
            {
                Debug.Log("CurrentDateTime dakika: " + CurrentDateTime.Minute);
                lastMinute = CurrentDateTime.Minute;

                // **1 Dakika i�lemleri**
                Debug.Log("1 dakika i�lemleri �al���yor...");
                OnOneMinutePassed?.Invoke();

                // **Di�er i�lemler**
                if (twoMinuteMarks.Contains(CurrentDateTime.Minute))
                {
                    Debug.Log("2 dakika i�lemleri �al���yor...");
                    OnTwoMinutePassed?.Invoke();
                }
                if (threeMinuteMarks.Contains(CurrentDateTime.Minute))
                {
                    Debug.Log("3 dakika i�lemleri �al���yor...");
                    OnThreeMinutePassed?.Invoke();
                }
                if (fourMinuteMarks.Contains(CurrentDateTime.Minute))
                {
                    Debug.Log("4 dakika i�lemleri �al���yor...");
                    OnFourMinutePassed?.Invoke();
                }
                if (fiveMinuteMarks.Contains(CurrentDateTime.Minute))
                {
                    Debug.Log("5 dakika i�lemleri �al���yor...");
                    OnFiveMinutePassed?.Invoke();
                }
                if (tenMinuteMarks.Contains(CurrentDateTime.Minute))
                {
                    Debug.Log("10 dakika i�lemleri �al���yor...");
                    OnTenMinutePassed?.Invoke();
                }
            }

            // **�d�l sistemi kontrol�**
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