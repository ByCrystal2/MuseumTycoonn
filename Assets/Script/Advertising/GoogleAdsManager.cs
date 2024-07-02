using System;
using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GoogleAdsManager : MonoBehaviour
{
    public static GoogleAdsManager instance { get; private set; }
    public AdverstingData adsData;

    private RewardedAd _rewardedAd;
    public RewardedAd RewardedAd => _rewardedAd;
    private InterstitialAd _interstitialAd;
    public InterstitialAd InterstitialAd => _interstitialAd;
    private BannerView _bannerView;
    public BannerView BannerView => _bannerView;

#if UNITY_ANDROID
    //const string _rewardedAdUnitId = "ca-app-pub-1301273545593514/1783006872";
    readonly List<(string id, int focusedLevel)> _rewardedAdUnitIds = new List<(string id, int focusedLevel)>()
    {
        // => 5 - 10 Gem
        ("ca-app-pub-1301273545593514/1783006872",5),
        ("ca-app-pub-1301273545593514/4700490193",5),
        ("ca-app-pub-1301273545593514/9009015263",5),
        ("ca-app-pub-1301273545593514/5342568150",5),
        ("ca-app-pub-1301273545593514/3383539098",5),
        ("ca-app-pub-1301273545593514/1617610400",5),

        // => 11 - 16 Gem
        ("ca-app-pub-1301273545593514/6486793706",10),
        ("ca-app-pub-1301273545593514/7893629740",10),
        ("ca-app-pub-1301273545593514/3902413847",10),
        ("ca-app-pub-1301273545593514/1878885738",10),
        ("ca-app-pub-1301273545593514/5809472019",10),
        ("ca-app-pub-1301273545593514/4496390347",10),

        // => 500 - 1000 Gold
        ("ca-app-pub-1301273545593514/4093985536",5),
        ("ca-app-pub-1301273545593514/1882755169",5),
        ("ca-app-pub-1301273545593514/4052202059",5),
        ("ca-app-pub-1301273545593514/2739120388",5),
        ("ca-app-pub-1301273545593514/2443606918",5),
        ("ca-app-pub-1301273545593514/7122553682",5),

        // => 1100 - 1400 Gold
        ("ca-app-pub-1301273545593514/1870227003",10),
        ("ca-app-pub-1301273545593514/3004265148",10),
        ("ca-app-pub-1301273545593514/1211751457",10),
        ("ca-app-pub-1301273545593514/3867836392",10),
    };
    //const string _rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917"; //test id
    //const string _interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712"; // test id
    //const string _bannerViewAdId = "ca-app-pub-3940256099942544/6300978111"; // test id

    const string _interstitialAdUnitId = "ca-app-pub-1301273545593514/7020426654";
    const string _bannerViewAdId = "ca-app-pub-1301273545593514/3164054074";
#elif PLATFORM_ANDROID
    const string _rewardedAdUnitId = "";
    const string _interstitialAdUnitId = "";
    const string _bannerViewAdId = "";
#else
    const string _rewardedAdUnitId = "unexcepted";
    const string _interstitialAdUnitId = "unexcepted";
    const string _bannerViewAdId = "unexcepted";
#endif

    private bool IsInterstitialAdShow = true;
    private bool IsRewardAdShow = true;
    private bool IsBannerAdShow = true;

    private float InterstitialAdWaitingTime = 3; // second
    private float RewardAdWaitingTime = 180; // default: 180 second
    private float BannerAdWaitingTime = 10; // second

    RewardAdData currentRewardAdData;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        MobileAds.Initialize(initStatus => {
            //LoadBannerAd();
            //LoadRewardedAd();
            //LoadInterstitialAd();
        });
    }
    private void Update()
    {
        if(TutorialLevelManager.instance != null && !TutorialLevelManager.instance.IsWatchTutorial) { return; }
        if (!IsInterstitialAdShow)
        {
            InterstitialAdWaitingTime -= Time.deltaTime;
            if (InterstitialAdWaitingTime <= 0)
            {
                IsInterstitialAdShow = true;
                InterstitialAdWaitingTime = 60;
                LoadInterstitialAd();
            }
        }

        if (!IsRewardAdShow)
        {
            RewardAdWaitingTime -= Time.deltaTime;
            if (RewardAdWaitingTime <= 0)
            {
                IsRewardAdShow = true;
                RewardAdWaitingTime = 180;
                LoadRewardedAd();
                StartCoroutine(DelayForRewardAdsShowing());
            }
        }

        if (!IsBannerAdShow)
        {
            BannerAdWaitingTime -= Time.deltaTime;
            if (BannerAdWaitingTime <= 0)
            {
                IsBannerAdShow = true;
                BannerAdWaitingTime = 120;
                _bannerView = null;
                LoadBannerAd();
            }
        }
    }
    public void StartRewardAdBool(bool _start)
    {
        IsRewardAdShow = !_start;
    }
    public void StartInterstitialAdBool(bool _start)
    {
        IsInterstitialAdShow = !_start;
    }
    public void StartBannerAdBool(bool _start)
    {
        IsBannerAdShow = !_start;
    }
    IEnumerator DelayForRewardAdsShowing()
    {
        while (_rewardedAd == null)
            yield return new WaitForEndOfFrame();

        while (!_rewardedAd.CanShowAd())
            yield return new WaitForEndOfFrame();
        
        
        UIController.instance.CloseRewardAdPanel(false);
        Debug.Log("currentRewardAdData => " + currentRewardAdData.Amount);
        UIController.instance.RewardAdController.SetRewardAdUIS(currentRewardAdData);
    }
    

    public void ShowInterstitialAd()
    {
        if (adsData.RemovedAllAds) return;

        if (!IsInterstitialAdShow) return;
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            _interstitialAd.Show();
            IsInterstitialAdShow = false;
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet.");
        }
    }

    public void LoadInterstitialAd()
    {
        if (adsData.RemovedAllAds) return;

        // Clean up the old ad before loading a new one.
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        InterstitialAd.Load(_interstitialAdUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                _interstitialAd = ad;
                RegisterEventHandlers(_interstitialAd);
            });
    }

    public void LoadBannerAd()
    {
        if (adsData.RemovedAllAds) return;

        // Eðer _bannerView daha önce oluþturulmamýþsa, oluþtur.
        if (_bannerView == null)
        {
            CreateBannerView();
        }
        else
        {
            // Eðer _bannerView varsa, sadece mevcut reklamý yok et ve yeniden yükle.
            _bannerView.Destroy();
            _bannerView = null;
            CreateBannerView();
        }

        // Reklamý yüklemek için gerekli isteði oluþtur.
        var adRequest = new AdRequest();

        // Reklamý yüklemek için isteði gönder.
        Debug.Log("Loading banner ad.");
        _bannerView.LoadAd(adRequest);
        IsBannerAdShow = false;
    }

    private void CreateBannerView()
    {
        // BannerView'inizi burada oluþturun, örneðin:
        _bannerView = new BannerView(_bannerViewAdId, AdSize.Banner, AdPosition.Bottom);
    }


    public void LoadRewardedAd()
    {
        // Clean up the old ad before loading a new one.
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        Debug.Log("Loading the rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        List<(string _id, int _focusedLevel)> currentAdIds = new List<(string _id, int _focusedLevel)>();

        Debug.Log("_rewardedAdUnitIds.Count => " + _rewardedAdUnitIds.Count);
        foreach (var item in _rewardedAdUnitIds)
        {
            if (GameManager.instance.rewardManager.IsSendingFocusedLevelSuitable(item.focusedLevel))
            {
                currentAdIds.Add(item);
                Debug.Log("item.focusedLevel => " + item.focusedLevel);
            }
        }

        Debug.Log("currentAdIds => " + currentAdIds.Count);
        string currentRewardedAdUnityId = currentAdIds[UnityEngine.Random.Range(0, currentAdIds.Count)]._id;
        RewardedAd.Load(currentRewardedAdUnityId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                Reward r = ad.GetRewardItem();
                Debug.Log("reward.Type => " + r.Type + "reward.Amount => " + r.Amount);
                RewardAdData earnReward = new RewardAdData();
                if (int.TryParse(r.Type, out int result))
                {
                    earnReward = GameManager.instance.rewardManager.GetRewardAdsWithID(result);
                }
                else
                {

                    Debug.Log(r.Type + " isminde ki odul int deger vermedi.");
                    earnReward = GameManager.instance.rewardManager.GetRewardAdsWithID(1);
                    Debug.Log("earnReward.Amount => " + earnReward.Amount.ToString());
                }                    
                
                
                if (earnReward != null)
                {
                    currentRewardAdData = earnReward;
                }
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " +
                                   "with error : " + error);

                    //ADButton.interactable = false;
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : "
                          + ad.GetResponseInfo());

                _rewardedAd = ad;
                RegisterEventHandlers(_rewardedAd);
            });
    }

    public void ShowRewardedAd()
    {
        if (!IsRewardAdShow) return;
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                GiveRewards();
            });
        }
    }

    private void GiveRewards()
    {
        if (currentRewardAdData.Type == ItemType.Gem)
        {
            MuseumManager.instance.AddGem(currentRewardAdData.Amount);
        }
        else if (currentRewardAdData.Type == ItemType.Gold)
        {
            MuseumManager.instance.AddGold(currentRewardAdData.Amount);
        }

        GameManager.instance.Save();
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            UIController.instance.RewardAdController.adStarting = false;
            UIController.instance.CloseRewardAdPanel(true);
            IsRewardAdShow = false;
            Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));

            RewardAdData earnReward = GameManager.instance.rewardManager.GetRewardAdsWithID(int.Parse(adValue.CurrencyCode));

            if (earnReward.Type == ItemType.Gem)
            {
                MuseumManager.instance.AddGem(earnReward.Amount);
            }
            else if (earnReward.Type == ItemType.Gold)
            {
                MuseumManager.instance.AddGold(earnReward.Amount);
            }

            //Arttirilinabilinir. Mesela bir tablo verilebilir dusuk ihtimalle.
            
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
            UIController.instance.RewardAdController.adStarting = false;
            UIController.instance.CloseRewardAdPanel(true);
            IsRewardAdShow = false;
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
            UIController.instance.RewardAdController.adStarting = false;
            UIController.instance.CloseRewardAdPanel(true);
            IsRewardAdShow = false;
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
            UIController.instance.RewardAdController.adStarting = false;
            UIController.instance.CloseRewardAdPanel(true);
            IsRewardAdShow = false;
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            LoadRewardedAd();
            UIController.instance.RewardAdController.adStarting = false;
            UIController.instance.CloseRewardAdPanel(true);
            IsRewardAdShow = false;
            Debug.Log("Rewarded ad full screen content closed.");
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);            
            LoadRewardedAd();
            UIController.instance.RewardAdController.adStarting = false;
            UIController.instance.CloseRewardAdPanel(true);
            IsRewardAdShow = false;
        };
    }

    private void ListenToBannerAdEvents()
    {
        // Raised when an ad is loaded into the banner view.
        _bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                + _bannerView.GetResponseInfo());
        };
        // Raised when an ad fails to load into the banner view.
        _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                + error);
        };
        // Raised when the ad is estimated to have earned money.
        _bannerView.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        _bannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        _bannerView.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        _bannerView.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        _bannerView.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
        };
    }

    private void RegisterEventHandlers(InterstitialAd interstitialAd)
    {
        // Raised when the ad is estimated to have earned money.
        interstitialAd.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        interstitialAd.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        interstitialAd.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        interstitialAd.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            LoadInterstitialAd();
            Debug.Log("Interstitial ad full screen content closed.");
        };
        // Raised when the ad failed to open full screen content.
        interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            LoadInterstitialAd();
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
        };
    }
    public void RemoveAds()
    {
        adsData.RemovedAllAds = true;
        BannerAdWaitingTime = 120;
        InterstitialAdWaitingTime = 60;
        IsInterstitialAdShow = true;
        IsBannerAdShow = true;
        if (_bannerView != null)
        {
            _bannerView.Destroy();
            _bannerView = null;
        }
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }
    }
}
[System.Serializable]
public sealed class AdverstingData
{
    public bool RemovedAllAds = false;
    public AdverstingData()
    {
        RemovedAllAds = false;
    }
}