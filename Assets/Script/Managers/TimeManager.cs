using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    private string apiUrl = "https://worldtimeapi.org/api/timezone/Europe/Istanbul";
    public DateTime CurrentDateTime; // Su an ki mevcut zamani tutar.
    public TimeData timeData;
    public byte WhatDay = 0;
    public bool FirstOpen = true;
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
    public void UpdateCurrentTime()
    {
        StartCoroutine(GetTime());
    }
    public bool lastDailyCheck;
    IEnumerator GetTime()
    {
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
                CurrentDateTime = DateTime.Parse(timeData.datetime);
                // CurrentDateTime => 08:00:00
                if (GameManager.instance._rewardManager != null)
                {
                    if (!lastDailyCheck)
                        GameManager.instance._rewardManager.WaitForLastDailyRewardTime();
                    GameManager.instance._rewardManager.CheckRewards();
                    //Debug.Log("�u an saat: " + CurrentDateTime.ToString("HH:mm:ss"));
                }
                
            }
        }
    }    
}

[Serializable]
public class TimeData
{
    public string datetime;    
}