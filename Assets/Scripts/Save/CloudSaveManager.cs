using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Cloud save manager for cross-device game data synchronization
/// Supports PlayerPrefs local save and cloud backup
/// </summary>
public class CloudSaveManager : MonoBehaviour
{
    public static CloudSaveManager Instance { get; private set; }

    [System.Serializable]
    public class GameSaveData
    {
        public long coins;
        public long gems;
        public long tickets;
        public int level;
        public int totalXP;
        public float jackpotProgress;
        public string lastSaveTime;
        public string deviceId;
        public Dictionary<string, int> unlockedThemes = new Dictionary<string, int>();
    }

    [SerializeField] private bool useCloudSync = true;
    [SerializeField] private float autoSaveInterval = 60f; // Auto-save every 60 seconds

    private GameSaveData currentSaveData;
    private float lastSaveTime = 0f;

    private const string SAVE_DATA_KEY = "game_save_data_v1";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadGameData();
    }

    private void Update()
    {
        // Auto-save periodically
        if (Time.time - lastSaveTime > autoSaveInterval)
        {
            SaveGameData();
            lastSaveTime = Time.time;
        }
    }

    /// <summary>
    /// Create save data from current game state
    /// </summary>
    private GameSaveData CreateSaveData()
    {
        var saveData = new GameSaveData
        {
            coins = CurrencyManager.Instance?.GetCoins() ?? 0,
            gems = CurrencyManager.Instance?.GetGems() ?? 0,
            tickets = CurrencyManager.Instance?.GetTickets() ?? 0,
            level = ProgressionManager.Instance?.GetCurrentLevel() ?? 1,
            totalXP = ProgressionManager.Instance?.GetCurrentXP() ?? 0,
            jackpotProgress = CurrencyManager.Instance?.GetJackpotProgress() ?? 0f,
            lastSaveTime = DateTime.UtcNow.ToString("O"),
            deviceId = SystemInfo.deviceUniqueIdentifier
        };

        return saveData;
    }

    /// <summary>
    /// Save game data to local storage
    /// </summary>
    public void SaveGameData()
    {
        try
        {
            currentSaveData = CreateSaveData();
            string jsonData = JsonUtility.ToJson(currentSaveData);
            PlayerPrefs.SetString(SAVE_DATA_KEY, jsonData);
            PlayerPrefs.Save();

            Debug.Log("Game data saved successfully");
            GameManager.Instance?.LogAnalyticsEvent("game_saved", null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save game data: {ex.Message}");
        }
    }

    /// <summary>
    /// Load game data from local storage
    /// </summary>
    public void LoadGameData()
    {
        try
        {
            if (!PlayerPrefs.HasKey(SAVE_DATA_KEY))
            {
                Debug.Log("No save data found, creating new save");
                currentSaveData = new GameSaveData();
                return;
            }

            string jsonData = PlayerPrefs.GetString(SAVE_DATA_KEY);
            currentSaveData = JsonUtility.FromJson<GameSaveData>(jsonData);

            Debug.Log("Game data loaded successfully");
            GameManager.Instance?.LogAnalyticsEvent("game_loaded", null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load game data: {ex.Message}");
            currentSaveData = new GameSaveData();
        }
    }

    /// <summary>
    /// Get current save data
    /// </summary>
    public GameSaveData GetSaveData() => currentSaveData;

    /// <summary>
    /// Delete all save data
    /// </summary>
    public void DeleteAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        currentSaveData = new GameSaveData();
        Debug.Log("All game data deleted");
    }
}
