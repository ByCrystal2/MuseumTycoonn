using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class TimeManager : MonoBehaviour
{    
    public DateTime CurrentDateTime; // Su an ki mevcut zamani tutar.
    public TimeData timeData;
    public byte WhatDay = 0;
    public bool FirstOpen = true;
    private Coroutine timeProgressCoroutine;
    //Delegates
    public delegate void MinutePassedDelegate();
    public event MinutePassedDelegate OnMinutePassed;
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
    private IEnumerator TimeProgress()
    {
        int lastMinute = CurrentDateTime.Minute;
        while (true)
        {
            yield return new WaitForSeconds(1f); // 1 saniye bekle

            // Her dakika baþýnda delegate'i çaðýr
            if (CurrentDateTime.Minute != lastMinute)
            {
                OnMinutePassed?.Invoke();
                lastMinute = CurrentDateTime.Minute;
            }

            if (GameManager.instance._rewardManager != null)
            {
                if (!lastDailyCheck)
                    GameManager.instance._rewardManager.WaitForLastDailyRewardTime();
                GameManager.instance._rewardManager.CheckRewards();
                //Debug.Log("Þu an saat: " + CurrentDateTime.ToString("HH:mm:ss"));
            }

            CurrentDateTime = CurrentDateTime.AddSeconds(1); // Mevcut zamani 1 saniye arttir            
        }
    }
}

[Serializable]
public class TimeData
{
    public string dateTime;    
}