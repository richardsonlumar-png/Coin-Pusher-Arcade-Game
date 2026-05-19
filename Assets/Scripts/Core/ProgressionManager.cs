using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Progression Manager - Handles level, XP, achievements, and unlocks
/// </summary>
public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int xpPerLevel = 1000;
    [SerializeField] private float xpMultiplier = 1.1f; // XP requirements increase by this factor each level

    [SerializeField] private Dictionary<string, bool> unlockedMachines = new Dictionary<string, bool>();
    [SerializeField] private Dictionary<string, bool> unlockedCoinSkins = new Dictionary<string, bool>();
    [SerializeField] private Dictionary<string, int> achievements = new Dictionary<string, int>();

    private const string LEVEL_KEY = "player_level";
    private const string XP_KEY = "player_xp";
    private const string MACHINES_KEY = "unlocked_machines";

    public delegate void XPChangeHandler(int newXP);
    public delegate void LevelUpHandler(int newLevel);

    public static event XPChangeHandler OnXPChanged;
    public static event LevelUpHandler OnLevelUp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadProgressionData();
    }

    /// <summary>
    /// Add XP and check for level up
    /// </summary>
    public void AddXP(int amount)
    {
        currentXP += amount;
        OnXPChanged?.Invoke(currentXP);

        // Check for level up
        while (currentXP >= GetXPRequiredForLevel(currentLevel + 1))
        {
            LevelUp();
        }

        SaveProgressionData();
    }

    /// <summary>
    /// Level up the player
    /// </summary>
    private void LevelUp()
    {
        currentLevel++;
        OnLevelUp?.Invoke(currentLevel);

        GameManager.Instance?.LogAnalyticsEvent("player_leveled_up", new Dictionary<string, object> { { "level", currentLevel } });
        AudioManager.Instance?.PlaySFX("level_up");
    }

    /// <summary>
    /// Get XP required for a specific level
    /// </summary>
    public int GetXPRequiredForLevel(int level)
    {
        if (level <= 1) return 0;
        return Mathf.RoundToInt(xpPerLevel * Mathf.Pow(xpMultiplier, level - 2));
    }

    /// <summary>
    /// Get current level
    /// </summary>
    public int GetCurrentLevel() => currentLevel;

    /// <summary>
    /// Get current XP
    /// </summary>
    public int GetCurrentXP() => currentXP;

    /// <summary>
    /// Get XP progress for current level (0-1)
    /// </summary>
    public float GetLevelProgress()
    {
        int currentLevelXP = GetXPRequiredForLevel(currentLevel);
        int nextLevelXP = GetXPRequiredForLevel(currentLevel + 1);
        int xpInCurrentLevel = currentXP - currentLevelXP;
        int xpForLevel = nextLevelXP - currentLevelXP;
        return xpForLevel > 0 ? (float)xpInCurrentLevel / xpForLevel : 0f;
    }

    /// <summary>
    /// Unlock a machine theme
    /// </summary>
    public void UnlockMachine(string machineId)
    {
        if (!unlockedMachines.ContainsKey(machineId))
        {
            unlockedMachines[machineId] = true;
            SaveProgressionData();
            GameManager.Instance?.LogAnalyticsEvent("machine_unlocked", new Dictionary<string, object> { { "machine", machineId } });
        }
    }

    /// <summary>
    /// Check if machine is unlocked
    /// </summary>
    public bool IsMachineUnlocked(string machineId)
    {
        return unlockedMachines.ContainsKey(machineId) && unlockedMachines[machineId];
    }

    /// <summary>
    /// Unlock a coin skin
    /// </summary>
    public void UnlockCoinSkin(string skinId)
    {
        if (!unlockedCoinSkins.ContainsKey(skinId))
        {
            unlockedCoinSkins[skinId] = true;
            SaveProgressionData();
        }
    }

    /// <summary>
    /// Unlock an achievement
    /// </summary>
    public void UnlockAchievement(string achievementId)
    {
        if (!achievements.ContainsKey(achievementId))
        {
            achievements[achievementId] = 1;
            SaveProgressionData();
            GameManager.Instance?.LogAnalyticsEvent("achievement_unlocked", new Dictionary<string, object> { { "achievement", achievementId } });
        }
    }

    /// <summary>
    /// Save progression data
    /// </summary>
    private void SaveProgressionData()
    {
        PlayerPrefs.SetInt(LEVEL_KEY, currentLevel);
        PlayerPrefs.SetInt(XP_KEY, currentXP);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load progression data
    /// </summary>
    private void LoadProgressionData()
    {
        currentLevel = PlayerPrefs.GetInt(LEVEL_KEY, 1);
        currentXP = PlayerPrefs.GetInt(XP_KEY, 0);

        // Initialize default unlocks
        if (!unlockedMachines.ContainsKey("pirate_treasure"))
        {
            unlockedMachines["pirate_treasure"] = true; // First machine is always unlocked
        }
    }
}
