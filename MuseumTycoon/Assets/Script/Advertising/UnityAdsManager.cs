using System;
//using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityAdsManager : MonoBehaviour
{
#if UNITY_ANDROID
    private string _bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111"; // original id: => ca-app-pub-1301273545593514/3164054074
    private string _interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712"; // original id: => ca-app-pub-1301273545593514/7020426654
    private string _rewardAdUnitId = "ca-app-pub-3940256099942544/5224354917"; // original id: => ca-app-pub-1301273545593514/1783006872
#elif UNITY_IPHONE
  private string _bannerAdUnitId = "ca-app-pub-1301273545593514/8273798579";
  private string _interstitialAdUnitId= "ca-app-pub-1301273545593514/1321377412";
  private string _rewardAdUnitId= "ca-app-pub-1301273545593514/7695214079";
#else
  private string _bannerAdUnitId = "unused";
  private string _interstitialAdUnitId= "unused";
#endif

    public AdverstingData adsData;
    //BannerView _bannerView;
    //InterstitialAd _interstitialAd;
    //RewardedAd _rewardedAd;
    public static UnityAdsManager instance { get; private set; }
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
        //MobileAds.Initialize((InitializationStatus initStatus) =>
        //{
        //    // This callback is called once the MobileAds SDK is initialized.
            
        //});
    }

    /// <summary>
    /// Creates a 320x50 banner view at top of the screen.
    /// </summary>
    public void CreateBannerView()
    {
        //Debug.Log("Creating banner view");

        //// If we already have a banner, destroy the old one.
        //if (_bannerView != null)
        //{
        //    DestroyBannerAd();
        //}

        //// Create a 320x50 banner at top of the screen
        //_bannerView = new BannerView(_bannerAdUnitId, AdSize.Banner, AdPosition.Top);
    }
    public void LoadBannerAd()
    {
        //// create an instance of a banner view first.
        //if (_bannerView == null)
        //{
        //    CreateBannerView();
        //}

        //// create our request used to load the ad.
        //var adRequest = new AdRequest();

        //// send the request to load the ad.
        //Debug.Log("Loading banner ad.");
        //_bannerView.LoadAd(adRequest);
    }
    /// <summary>
    /// Loads the interstitial ad.
    /// </summary>
    public void LoadInterstitialAd()
    {


        //Debug.Log("Loading the interstitial ad.");

        //// create our request used to load the ad.
        //var adRequest = new AdRequest();

        //// send the request to load the ad.
        //InterstitialAd.Load(_interstitialAdUnitId, adRequest,
        //    (InterstitialAd ad, LoadAdError error) =>
        //    {
        //        // if error is not null, the load request failed.
        //        if (error != null || ad == null)
        //        {
        //            Debug.LogError("interstitial ad failed to load an ad " +
        //                           "with error : " + error);
        //            return;
        //        }

        //        Debug.Log("Interstitial ad loaded with response : "
        //                  + ad.GetResponseInfo());

        //        _interstitialAd = ad;
        //    });
    }


    public void ShowBannerAd()
    {
        //if (_bannerView != null)
        //{
        //    _bannerView.Show();
        //}
    }
    public void DestroyBannerAd()
    {
        //if (_bannerView != null)
        //{
        //    _bannerView.Destroy();
        //    _bannerView = null;
        //}
    }

    public void ShowNonRewardedAd()
    {
        //if (_interstitialAd != null && _interstitialAd.CanShowAd())
        //{
        //    Debug.Log("Showing interstitial ad.");
        //    _interstitialAd.Show();
        //}
        //else
        //{
        //    Debug.LogError("Interstitial ad is not ready yet.");
        //}
    }

    public void ShowRewardedAd()
    {
        //const string rewardMsg =
        //"Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

        //if (_rewardedAd != null && _rewardedAd.CanShowAd())
        //{
        //    _rewardedAd.Show((Reward reward) =>
        //    {
        //        // TODO: Reward the user.
        //        Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
        //    });
        //}
    }
    public void LoadRewardedAd()
    {// Clean up the old ad before loading a new one.
        //if (_rewardedAd != null)
        //{
        //    _rewardedAd.Destroy();
        //    _rewardedAd = null;
        //}

        //Debug.Log("Loading the rewarded ad.");

        //// create our request used to load the ad.
        //var adRequest = new AdRequest();

        //// send the request to load the ad.
        //RewardedAd.Load(_rewardAdUnitId, adRequest,
        //    (RewardedAd ad, LoadAdError error) =>
        //    {
        //        // if error is not null, the load request failed.
        //        if (error != null || ad == null)
        //        {
        //            Debug.LogError("Rewarded ad failed to load an ad " +
        //                           "with error : " + error);
        //            return;
        //        }

        //        Debug.Log("Rewarded ad loaded with response : "
        //                  + ad.GetResponseInfo());

        //        _rewardedAd = ad;
        //        RegisterRewardedEventHandlers(_rewardedAd);
        //    });
    }

    //private void RegisterRewardedEventHandlers(RewardedAd ad)
    //{
    //    // Raised when the ad is estimated to have earned money.
    //    ad.OnAdPaid += (AdValue adValue) =>
    //    {
    //        Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
    //            adValue.Value,
    //            adValue.CurrencyCode));
    //    };
    //    // Raised when an impression is recorded for an ad.
    //    ad.OnAdImpressionRecorded += () =>
    //    {
    //        Debug.Log("Rewarded ad recorded an impression.");
    //    };
    //    // Raised when a click is recorded for an ad.
    //    ad.OnAdClicked += () =>
    //    {
    //        Debug.Log("Rewarded ad was clicked.");
    //    };
    //    // Raised when an ad opened full screen content.
    //    ad.OnAdFullScreenContentOpened += () =>
    //    {
    //        Debug.Log("Rewarded ad full screen content opened.");
    //    };
    //    // Raised when the ad closed full screen content.
    //    ad.OnAdFullScreenContentClosed += () =>
    //    {
    //        Debug.Log("Rewarded ad full screen content closed.");
    //    };
    //    // Raised when the ad failed to open full screen content.
    //    ad.OnAdFullScreenContentFailed += (AdError error) =>
    //    {
    //        Debug.LogError("Rewarded ad failed to open full screen content " +
    //                       "with error : " + error);
    //    };
    //}
   
}
    public sealed class AdverstingData
{
    public bool RemovedAllAds = false;
}