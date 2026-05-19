using UnityEngine;
using Firebase.Analytics;
using System.Collections.Generic;

/// <summary>
/// Main Game Manager - Handles core game flow, state management, and initialization
/// Singleton pattern for global game state access
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [System.Serializable]
    public enum GameState
    {
        Loading,
        MainMenu,
        InGame,
        Paused,
        GameOver,
        Settings
    }

    [SerializeField] private GameState currentState = GameState.Loading;
    [SerializeField] private float gameSessionStartTime;
    [SerializeField] private long sessionCoinsEarned = 0;
    [SerializeField] private long sessionGemsEarned = 0;

    // Events
    public delegate void GameStateChangeHandler(GameState newState);
    public static event GameStateChangeHandler OnGameStateChanged;

    private Dictionary<string, object> sessionData = new Dictionary<string, object>();

    private void Awake()
    {
        // Singleton implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize Firebase
        InitializeFirebase();

        // Set mobile-specific settings
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void Start()
    {
        gameSessionStartTime = Time.time;
        SetGameState(GameState.MainMenu);
        LogAnalyticsEvent("game_started", null);
    }

    private void InitializeFirebase()
    {
#if UNITY_ANDROID || UNITY_IOS
        try
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Firebase initialized successfully");
                }
                else
                {
                    Debug.LogError("Firebase initialization failed");
                }
            });
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Firebase initialization exception: {ex.Message}");
        }
#endif
    }

    /// <summary>
    /// Change the current game state and trigger state change event
    /// </summary>
    public void SetGameState(GameState newState)
    {
        if (currentState == newState) return;

        GameState previousState = currentState;
        currentState = newState;

        Debug.Log($"Game State Changed: {previousState} -> {newState}");
        OnGameStateChanged?.Invoke(newState);

        // Handle state-specific logic
        switch (newState)
        {
            case GameState.InGame:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
        }
    }

    /// <summary>
    /// Get current game state
    /// </summary>
    public GameState GetGameState() => currentState;

    /// <summary>
    /// Log coins earned in current session
    /// </summary>
    public void AddSessionCoins(long amount)
    {
        sessionCoinsEarned += amount;
    }

    /// <summary>
    /// Log gems earned in current session
    /// </summary>
    public void AddSessionGems(long amount)
    {
        sessionGemsEarned += amount;
    }

    /// <summary>
    /// Get session duration in seconds
    /// </summary>
    public float GetSessionDuration() => Time.time - gameSessionStartTime;

    /// <summary>
    /// Log analytics event to Firebase
    /// </summary>
    public void LogAnalyticsEvent(string eventName, Dictionary<string, object> parameters)
    {
#if UNITY_ANDROID || UNITY_IOS
        try
        {
            if (parameters != null)
            {
                var paramList = new List<Parameter>();
                foreach (var param in parameters)
                {
                    paramList.Add(new Parameter(param.Key, param.Value.ToString()));
                }
                FirebaseAnalytics.LogEvent(eventName, paramList.ToArray());
            }
            else
            {
                FirebaseAnalytics.LogEvent(eventName);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Failed to log Firebase event {eventName}: {ex.Message}");
        }
#endif
    }

    /// <summary>
    /// Set user property for Firebase Analytics
    /// </summary>
    public void SetUserProperty(string name, string value)
    {
#if UNITY_ANDROID || UNITY_IOS
        try
        {
            FirebaseAnalytics.SetUserProperty(name, value);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Failed to set user property {name}: {ex.Message}");
        }
#endif
    }

    /// <summary>
    /// End game session and log final metrics
    /// </summary>
    public void EndGameSession()
    {
        var sessionParams = new Dictionary<string, object>
        {
            { "session_duration_seconds", Mathf.RoundToInt(GetSessionDuration()) },
            { "coins_earned", sessionCoinsEarned },
            { "gems_earned", sessionGemsEarned }
        };
        LogAnalyticsEvent("session_ended", sessionParams);
    }

    /// <summary>
    /// Safely quit the game
    /// </summary>
    public void QuitGame()
    {
        EndGameSession();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
