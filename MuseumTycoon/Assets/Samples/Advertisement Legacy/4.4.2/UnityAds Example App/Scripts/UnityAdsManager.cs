using System;
using System.Collections;
using UnityEngine.Advertisements;
using UnityEngine;

public class UnityAdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public AdverstingData adsData;
    //protected const string BANNER_PLACEMENT = "Banner_Andorid";
    //protected const string VIDEO_PLACEMENT = "Interstitial_Android";
    //protected const string REWARDED_VIDEO_PLACEMENT = "Rewarded_Android";
#if UNITY_ANDROID
    protected string GAME_ID = "5499008"; //replace with your gameID from dashboard. note: will be different for each platform.

    protected const string BANNER_PLACEMENT = "Banner_Andorid";
    protected const string VIDEO_PLACEMENT = "Interstitial_Android";
    protected const string REWARDED_VIDEO_PLACEMENT = "Rewarded_Android";

#elif UNITY_IPHONE
  public string GAME_ID = "5499009"; //replace with your gameID from dashboard. note: will be different for each platform.

    protected const string BANNER_PLACEMENT = "Banner_IOS_B";
    protected const string VIDEO_PLACEMENT = "Interstitial_iOS";
    protected const string REWARDED_VIDEO_PLACEMENT = "Rewarded_iOS";
#else
    public string GAME_ID = "5499008";
#endif
    [SerializeField] protected BannerPosition bannerPosition = BannerPosition.BOTTOM_CENTER;

    protected bool testMode = true;
    protected bool showBanner = false;

    //utility wrappers for debuglog
    public delegate void DebugEvent(string msg);
    public static event DebugEvent OnDebugLog;

    public static UnityAdsManager instance { get; set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
    }

    public void Initialize()
    {
        if (Advertisement.isSupported)
        {
            DebugLog(Application.platform + " supported by Advertisement");
        }
        Advertisement.Initialize(GAME_ID, testMode, this);
    }

    public void ToggleBanner() 
    {
        showBanner = !showBanner;

        if (showBanner)
        {
            Advertisement.Banner.SetPosition(bannerPosition);
            Advertisement.Banner.Show(BANNER_PLACEMENT);
        }
        else
        {
            Advertisement.Banner.Hide(false);
        }
    }

    public void LoadRewardedAd()
    {
        if (!adsData.RemovedAllAds)
        {
           Advertisement.Load(REWARDED_VIDEO_PLACEMENT, this);
        }
        else
        {
            Debug.Log("Odullu Video Yukleme Basarisiz. Reklamlandirma Durumu: " + adsData.RemovedAllAds);
        }
    }

    public void ShowRewardedAd()
    {
        
        if (!adsData.RemovedAllAds)
        {
            Advertisement.Show(REWARDED_VIDEO_PLACEMENT, this);
        }
        else
        {
            Debug.Log("Odullu Video Gosterme Basarisiz. Reklamlandirma Durumu: " + adsData.RemovedAllAds);
        }
    }

    public void LoadNonRewardedAd()
    {        
        if (!adsData.RemovedAllAds)
        {
            Advertisement.Load(VIDEO_PLACEMENT, this);
        }
        else
        {
            Debug.Log("Normal Video Yukleme Basarisiz. Reklamlandirma Durumu: " + adsData.RemovedAllAds);
        }
    }

    public void ShowNonRewardedAd()
    {
        
        if (!adsData.RemovedAllAds)
        {
            Advertisement.Show(VIDEO_PLACEMENT, this);
        }
        else
        {
            Debug.Log("Normal Video Gosterme Basarisiz. Reklamlandirma Durumu: " + adsData.RemovedAllAds);
        }
    }

    #region Interface Implementations
    public void OnInitializationComplete()
    {
        DebugLog("Init Success");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        DebugLog($"Init Failed: [{error}]: {message}");
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        DebugLog($"Load Success: {placementId}");
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        DebugLog($"Load Failed: [{error}:{placementId}] {message}");
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        DebugLog($"OnUnityAdsShowFailure: [{error}]: {message}");
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        DebugLog($"OnUnityAdsShowStart: {placementId}");
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        DebugLog($"OnUnityAdsShowClick: {placementId}");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        DebugLog($"OnUnityAdsShowComplete: [{showCompletionState}]: {placementId}");
    }
    #endregion

    public void OnGameIDFieldChanged(string newInput)
    {
        GAME_ID = newInput;
    }

    public void ToggleTestMode(bool isOn)
    {
        testMode = isOn;
    }

    //wrapper around debug.log to allow broadcasting log strings to the UI
    void DebugLog(string msg)
    {
        OnDebugLog?.Invoke(msg);
        Debug.Log(msg);
    }
}
[System.Serializable]
public class AdverstingData
{
    public bool RemovedAllAds; // Tüm reklamlar kaldırıldı.
}
