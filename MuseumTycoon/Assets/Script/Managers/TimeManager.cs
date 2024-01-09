using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    private string apiUrl = "https://worldtimeapi.org/api/timezone/Europe/Istanbul";
    public DateTime CurrentDateTime;
    public TimeData timeData;
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
    }
    void Start()
    {
        // Belirli aralýklarla API'ye istek göndererek saati güncelle
        //InvokeRepeating("UpdateCurrentTime", 0f, 60f); // Her 60 saniyede bir güncelle
        UpdateCurrentTime();
    }
    void UpdateCurrentTime()
    {
        StartCoroutine(GetTime());
    }
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
                // JSON verisini iþle
                timeData = JsonUtility.FromJson<TimeData>(responseData);

                // Saati alma
                CurrentDateTime = DateTime.Parse(timeData.datetime);
                Debug.Log("Þu an saat: " + CurrentDateTime.ToString("HH:mm:ss"));
            }
        }
    }    
}

[Serializable]
public class TimeData
{
    public string datetime;
    public byte WhatDay;
}