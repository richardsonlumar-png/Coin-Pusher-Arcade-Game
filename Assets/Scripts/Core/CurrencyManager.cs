using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all in-game currencies: Coins, Gems, Tickets, and Jackpot
/// Handles earning, spending, and persistence of currency values
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private long coins = 1000;
    [SerializeField] private long gems = 100;
    [SerializeField] private long tickets = 0;
    [SerializeField] private float jackpotProgress = 0f;
    [SerializeField] private float maxJackpotProgress = 100f;
    [SerializeField] private long jackpotReward = 10000;

    private const string COINS_KEY = "coins";
    private const string GEMS_KEY = "gems";
    private const string TICKETS_KEY = "tickets";
    private const string JACKPOT_KEY = "jackpot_progress";

    // Events for UI updates
    public delegate void CurrencyChangeHandler(long newAmount);
    public delegate void JackpotChangeHandler(float newProgress);

    public static event CurrencyChangeHandler OnCoinsChanged;
    public static event CurrencyChangeHandler OnGemsChanged;
    public static event CurrencyChangeHandler OnTicketsChanged;
    public static event JackpotChangeHandler OnJackpotProgressChanged;
    public delegate void JackpotWinHandler();
    public static event JackpotWinHandler OnJackpotWin;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadCurrency();
    }

    /// <summary>
    /// Add coins to player's total
    /// </summary>
    public void AddCoins(long amount, bool saveImmediately = true)
    {
        coins += amount;
        if (coins < 0) coins = 0;

        OnCoinsChanged?.Invoke(coins);
        GameManager.Instance?.AddSessionCoins(amount);

        // Log analytics
        var param = new Dictionary<string, object> { { "amount", amount }, { "new_total", coins } };
        GameManager.Instance?.LogAnalyticsEvent("coins_earned", param);

        if (saveImmediately) SaveCurrency();
    }

    /// <summary>
    /// Spend coins from player's total
    /// </summary>
    public bool SpendCoins(long amount, bool saveImmediately = true)
    {
        if (coins >= amount)
        {
            coins -= amount;
            OnCoinsChanged?.Invoke(coins);

            var param = new Dictionary<string, object> { { "amount", amount }, { "remaining", coins } };
            GameManager.Instance?.LogAnalyticsEvent("coins_spent", param);

            if (saveImmediately) SaveCurrency();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get current coin balance
    /// </summary>
    public long GetCoins() => coins;

    /// <summary>
    /// Add gems to player's total
    /// </summary>
    public void AddGems(long amount, bool saveImmediately = true)
    {
        gems += amount;
        if (gems < 0) gems = 0;

        OnGemsChanged?.Invoke(gems);
        GameManager.Instance?.AddSessionGems(amount);

        var param = new Dictionary<string, object> { { "amount", amount }, { "new_total", gems } };
        GameManager.Instance?.LogAnalyticsEvent("gems_earned", param);

        if (saveImmediately) SaveCurrency();
    }

    /// <summary>
    /// Spend gems from player's total
    /// </summary>
    public bool SpendGems(long amount, bool saveImmediately = true)
    {
        if (gems >= amount)
        {
            gems -= amount;
            OnGemsChanged?.Invoke(gems);

            var param = new Dictionary<string, object> { { "amount", amount }, { "remaining", gems } };
            GameManager.Instance?.LogAnalyticsEvent("gems_spent", param);

            if (saveImmediately) SaveCurrency();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get current gem balance
    /// </summary>
    public long GetGems() => gems;

    /// <summary>
    /// Add tickets earned from coin pusher
    /// </summary>
    public void AddTickets(long amount, bool saveImmediately = true)
    {
        tickets += amount;
        if (tickets < 0) tickets = 0;

        OnTicketsChanged?.Invoke(tickets);

        var param = new Dictionary<string, object> { { "amount", amount }, { "new_total", tickets } };
        GameManager.Instance?.LogAnalyticsEvent("tickets_earned", param);

        if (saveImmediately) SaveCurrency();
    }

    /// <summary>
    /// Spend tickets in shop
    /// </summary>
    public bool SpendTickets(long amount, bool saveImmediately = true)
    {
        if (tickets >= amount)
        {
            tickets -= amount;
            OnTicketsChanged?.Invoke(tickets);

            var param = new Dictionary<string, object> { { "amount", amount }, { "remaining", tickets } };
            GameManager.Instance?.LogAnalyticsEvent("tickets_spent", param);

            if (saveImmediately) SaveCurrency();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get current ticket balance
    /// </summary>
    public long GetTickets() => tickets;

    /// <summary>
    /// Add progress to jackpot meter
    /// </summary>
    public void AddJackpotProgress(float amount)
    {
        jackpotProgress += amount;

        // Check for jackpot win
        if (jackpotProgress >= maxJackpotProgress)
        {
            WinJackpot();
        }
        else
        {
            OnJackpotProgressChanged?.Invoke(jackpotProgress);
            SaveCurrency();
        }
    }

    /// <summary>
    /// Win the jackpot and reset progress
    /// </summary>
    private void WinJackpot()
    {
        AddCoins(jackpotReward);
        jackpotProgress = 0f;

        OnJackpotWin?.Invoke();
        OnJackpotProgressChanged?.Invoke(jackpotProgress);

        GameManager.Instance?.LogAnalyticsEvent("jackpot_won", new Dictionary<string, object> { { "reward", jackpotReward } });
        SaveCurrency();
    }

    /// <summary>
    /// Get current jackpot progress (0-100)
    /// </summary>
    public float GetJackpotProgress() => jackpotProgress;

    /// <summary>
    /// Get jackpot progress as percentage
    /// </summary>
    public float GetJackpotProgressPercent() => (jackpotProgress / maxJackpotProgress) * 100f;

    /// <summary>
    /// Save currency to PlayerPrefs
    /// </summary>
    private void SaveCurrency()
    {
        PlayerPrefs.SetString(COINS_KEY, coins.ToString());
        PlayerPrefs.SetString(GEMS_KEY, gems.ToString());
        PlayerPrefs.SetString(TICKETS_KEY, tickets.ToString());
        PlayerPrefs.SetFloat(JACKPOT_KEY, jackpotProgress);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load currency from PlayerPrefs
    /// </summary>
    private void LoadCurrency()
    {
        if (PlayerPrefs.HasKey(COINS_KEY))
        {
            coins = long.Parse(PlayerPrefs.GetString(COINS_KEY, "1000"));
        }
        if (PlayerPrefs.HasKey(GEMS_KEY))
        {
            gems = long.Parse(PlayerPrefs.GetString(GEMS_KEY, "100"));
        }
        if (PlayerPrefs.HasKey(TICKETS_KEY))
        {
            tickets = long.Parse(PlayerPrefs.GetString(TICKETS_KEY, "0"));
        }
        if (PlayerPrefs.HasKey(JACKPOT_KEY))
        {
            jackpotProgress = PlayerPrefs.GetFloat(JACKPOT_KEY, 0f);
        }
    }

    /// <summary>
    /// Completely reset currency for testing
    /// </summary>
    public void ResetCurrency()
    {
        coins = 1000;
        gems = 100;
        tickets = 0;
        jackpotProgress = 0f;
        SaveCurrency();

        OnCoinsChanged?.Invoke(coins);
        OnGemsChanged?.Invoke(gems);
        OnTicketsChanged?.Invoke(tickets);
        OnJackpotProgressChanged?.Invoke(jackpotProgress);
    }
}
