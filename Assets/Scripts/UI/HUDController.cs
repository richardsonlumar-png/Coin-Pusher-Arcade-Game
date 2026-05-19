using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Main HUD Controller - Displays real-time game information
/// Shows coins, gems, XP, jackpot, and session metrics
/// </summary>
public class HUDController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI gemsText;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private Image jackpotProgressBar;
    [SerializeField] private TextMeshProUGUI jackpotPercentText;
    [SerializeField] private TextMeshProUGUI sessionCoinsText;
    [SerializeField] private TextMeshProUGUI sessionGemsText;

    [SerializeField] private CanvasGroup jackpotNotificationGroup;
    [SerializeField] private TextMeshProUGUI jackpotRewardText;

    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Color positiveChangeColor = Color.green;
    [SerializeField] private Color negativeChangeColor = Color.red;

    private long lastDisplayedCoins = 0;
    private long lastDisplayedGems = 0;
    private float lastDisplayedJackpot = 0f;

    private void OnEnable()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.OnCoinsChanged += UpdateCoinsDisplay;
            CurrencyManager.OnGemsChanged += UpdateGemsDisplay;
            CurrencyManager.OnJackpotProgressChanged += UpdateJackpotDisplay;
            CurrencyManager.OnJackpotWin += ShowJackpotNotification;
        }

        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.OnXPChanged += UpdateXPDisplay;
        }

        UpdateAllDisplays();
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.OnCoinsChanged -= UpdateCoinsDisplay;
            CurrencyManager.OnGemsChanged -= UpdateGemsDisplay;
            CurrencyManager.OnJackpotProgressChanged -= UpdateJackpotDisplay;
            CurrencyManager.OnJackpotWin -= ShowJackpotNotification;
        }

        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.OnXPChanged -= UpdateXPDisplay;
        }
    }

    /// <summary>
    /// Update all HUD displays immediately
    /// </summary>
    private void UpdateAllDisplays()
    {
        if (CurrencyManager.Instance != null)
        {
            UpdateCoinsDisplay(CurrencyManager.Instance.GetCoins());
            UpdateGemsDisplay(CurrencyManager.Instance.GetGems());
            UpdateJackpotDisplay(CurrencyManager.Instance.GetJackpotProgress());
        }

        if (ProgressionManager.Instance != null)
        {
            UpdateXPDisplay(ProgressionManager.Instance.GetCurrentXP());
        }
    }

    /// <summary>
    /// Update coins display with animation
    /// </summary>
    private void UpdateCoinsDisplay(long newCoins)
    {
        if (coinsText != null)
        {
            StartCoroutine(AnimateNumberChange(coinsText, lastDisplayedCoins, newCoins));
            lastDisplayedCoins = newCoins;
        }

        if (sessionCoinsText != null)
        {
            sessionCoinsText.text = $"Session: +{newCoins}";
        }
    }

    /// <summary>
    /// Update gems display with animation
    /// </summary>
    private void UpdateGemsDisplay(long newGems)
    {
        if (gemsText != null)
        {
            StartCoroutine(AnimateNumberChange(gemsText, lastDisplayedGems, newGems));
            lastDisplayedGems = newGems;
        }

        if (sessionGemsText != null)
        {
            sessionGemsText.text = $"Session: +{newGems}";
        }
    }

    /// <summary>
    /// Update XP display
    /// </summary>
    private void UpdateXPDisplay(int newXP)
    {
        if (xpText != null)
        {
            xpText.text = $"XP: {newXP}";
        }
    }

    /// <summary>
    /// Update jackpot progress bar
    /// </summary>
    private void UpdateJackpotDisplay(float progress)
    {
        if (jackpotProgressBar != null)
        {
            float progressPercent = (progress / 100f);
            StartCoroutine(AnimateProgressBar(jackpotProgressBar, lastDisplayedJackpot, progressPercent));
            lastDisplayedJackpot = progressPercent;
        }

        if (jackpotPercentText != null)
        {
            jackpotPercentText.text = $"{Mathf.RoundToInt(progress)}%";
        }
    }

    /// <summary>
    /// Show jackpot win notification
    /// </summary>
    private void ShowJackpotNotification()
    {
        StartCoroutine(JackpotNotificationCoroutine());
    }

    /// <summary>
    /// Animate number change with lerp
    /// </summary>
    private IEnumerator AnimateNumberChange(TextMeshProUGUI text, long fromValue, long toValue)
    {
        float elapsedTime = 0f;
        Color originalColor = text.color;
        Color changeColor = toValue > fromValue ? positiveChangeColor : negativeChangeColor;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            long displayValue = Mathf.RoundToInt(Mathf.Lerp(fromValue, toValue, t));
            text.text = FormatLargeNumber(displayValue);
            text.color = Color.Lerp(changeColor, originalColor, t);
            yield return null;
        }

        text.text = FormatLargeNumber(toValue);
        text.color = originalColor;
    }

    /// <summary>
    /// Animate progress bar fill
    /// </summary>
    private IEnumerator AnimateProgressBar(Image progressBar, float fromValue, float toValue)
    {
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            float progress = Mathf.Lerp(fromValue, toValue, t);
            progressBar.fillAmount = progress;
            yield return null;
        }

        progressBar.fillAmount = toValue;
    }

    /// <summary>
    /// Show jackpot win notification with animation
    /// </summary>
    private IEnumerator JackpotNotificationCoroutine()
    {
        if (jackpotNotificationGroup == null) yield break;

        jackpotNotificationGroup.alpha = 0f;
        jackpotRewardText.text = "JACKPOT WON!";

        // Fade in
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            jackpotNotificationGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.3f);
            yield return null;
        }

        // Hold
        yield return new WaitForSeconds(2f);

        // Fade out
        t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            jackpotNotificationGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.3f);
            yield return null;
        }
    }

    /// <summary>
    /// Format large numbers with K, M, B suffixes
    /// </summary>
    private string FormatLargeNumber(long number)
    {
        if (number >= 1000000000)
            return (number / 1000000000f).ToString("F1") + "B";
        if (number >= 1000000)
            return (number / 1000000f).ToString("F1") + "M";
        if (number >= 1000)
            return (number / 1000f).ToString("F1") + "K";
        return number.ToString();
    }
}
