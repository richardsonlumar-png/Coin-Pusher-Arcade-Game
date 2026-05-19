using UnityEngine;
using GoogleMobileAds.Api;
using System;

/// <summary>
/// AdMob integration manager
/// Handles rewarded ads, interstitial ads, and banner ads
/// Optimized for mobile with proper frequency capping
/// </summary>
public class AdMobManager : MonoBehaviour
{
    public static AdMobManager Instance { get; private set; }

    [SerializeField] private string adMobAppId = "ca-app-pub-xxxxxxxxxxxxxxxx~yyyyyyyyyyyyyy";
    [SerializeField] private string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917"; // Test ID
    [SerializeField] private string interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712"; // Test ID
    [SerializeField] private string bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111"; // Test ID

    [SerializeField] private int interstitialFrequency = 3; // Show every N sessions
    [SerializeField] private bool testMode = true;

    private RewardedAd rewardedAd;
    private InterstitialAd interstitialAd;
    private BannerView bannerView;

    private int sessionCount = 0;
    private bool isInitialized = false;
    private bool adRewardPending = false;

    public delegate void AdRewardHandler();
    public static event AdRewardHandler OnAdRewarded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeAdMob();
    }

    /// <summary>
    /// Initialize Google Mobile Ads SDK
    /// </summary>
    private void InitializeAdMob()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (isInitialized) return;

        try
        {
            MobileAds.Initialize(initStatus => {
                isInitialized = true;
                Debug.Log("AdMob initialized successfully");

                // Load initial ads
                LoadRewardedAd();
                LoadInterstitialAd();
                LoadBannerAd();
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"AdMob initialization failed: {ex.Message}");
        }
#endif
    }

    /// <summary>
    /// Load rewarded video ad
    /// </summary>
    private void LoadRewardedAd()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (!isInitialized) return;

        AdRequest request = new AdRequest();

        RewardedAd.Load(rewardedAdUnitId, request,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load: " + error?.GetMessage());
                    return;
                }

                rewardedAd = ad;
                RegisterRewardedAdEvents(rewardedAd);
            });
#endif
    }

    /// <summary>
    /// Register rewarded ad events
    /// </summary>
    private void RegisterRewardedAdEvents(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad closed");
            LoadRewardedAd(); // Reload for next show
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError($"Rewarded ad failed: {error?.GetMessage()}");
            LoadRewardedAd();
        };
    }

    /// <summary>
    /// Show rewarded ad
    /// </summary>
    public void ShowRewardedAd()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (rewardedAd != null && rewardedAd.IsLoaded())
        {
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"Reward earned: {reward.Type} = {reward.Amount}");
                adRewardPending = true;
                OnAdRewarded?.Invoke();
                GiveAdReward();
            });
        }
        else
        {
            Debug.LogWarning("Rewarded ad not ready");
            LoadRewardedAd();
        }
#endif
    }

    /// <summary>
    /// Give player reward for watching ad
    /// </summary>
    private void GiveAdReward()
    {
        if (CurrencyManager.Instance != null)
        {
            long rewardAmount = 500; // Coins for watching ad
            CurrencyManager.Instance.AddCoins(rewardAmount);
            GameManager.Instance?.LogAnalyticsEvent("ad_reward_claimed", null);
        }
        adRewardPending = false;
    }

    /// <summary>
    /// Load interstitial ad
    /// </summary>
    private void LoadInterstitialAd()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (!isInitialized) return;

        AdRequest request = new AdRequest();

        InterstitialAd.Load(interstitialAdUnitId, request,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Interstitial ad failed to load: " + error?.GetMessage());
                    return;
                }

                interstitialAd = ad;
                RegisterInterstitialAdEvents(interstitialAd);
            });
#endif
    }

    /// <summary>
    /// Register interstitial ad events
    /// </summary>
    private void RegisterInterstitialAdEvents(InterstitialAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad closed");
            LoadInterstitialAd(); // Reload for next show
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError($"Interstitial ad failed: {error?.GetMessage()}");
            LoadInterstitialAd();
        };
    }

    /// <summary>
    /// Show interstitial ad (between game sessions)
    /// </summary>
    public void ShowInterstitialAd()
    {
#if UNITY_ANDROID || UNITY_IOS
        sessionCount++;

        if (sessionCount % interstitialFrequency == 0 && interstitialAd != null && interstitialAd.IsLoaded())
        {
            interstitialAd.Show();
            GameManager.Instance?.LogAnalyticsEvent("interstitial_ad_shown", null);
        }
        else
        {
            LoadInterstitialAd();
        }
#endif
    }

    /// <summary>
    /// Load banner ad
    /// </summary>
    private void LoadBannerAd()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (!isInitialized) return;

        bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
        AdRequest request = new AdRequest();
        bannerView.LoadAd(request);
#endif
    }

    /// <summary>
    /// Destroy and clean up ads
    /// </summary>
    private void OnDestroy()
    {
        rewardedAd?.Destroy();
        interstitialAd?.Destroy();
        bannerView?.Destroy();
    }

    /// <summary>
    /// Check if rewarded ad is ready
    /// </summary>
    public bool IsRewardedAdReady()
    {
#if UNITY_ANDROID || UNITY_IOS
        return rewardedAd != null && rewardedAd.IsLoaded();
#else
        return false;
#endif
    }
}
