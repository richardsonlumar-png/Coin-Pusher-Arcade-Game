using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Audio Manager - Handles all sound effects and music
/// Provides audio caching and pooling for optimal mobile performance
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private float masterVolume = 0.8f;
    [SerializeField] private float sfxVolume = 0.8f;
    [SerializeField] private float musicVolume = 0.6f;
    [SerializeField] private bool soundEnabled = true;
    [SerializeField] private bool musicEnabled = true;

    private Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();
    private AudioSource musicSource;
    private List<AudioSource> sfxSources = new List<AudioSource>();
    private const int MAX_SFX_SOURCES = 8;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeAudioSources();
    }

    /// <summary>
    /// Initialize audio sources for SFX pooling
    /// </summary>
    private void InitializeAudioSources()
    {
        // Music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume * masterVolume;

        // SFX sources pool
        for (int i = 0; i < MAX_SFX_SOURCES; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            sfxSources.Add(source);
        }
    }

    /// <summary>
    /// Play SFX with optional parameters
    /// </summary>
    public void PlaySFX(string clipName, float pitch = 1f, float volumeScale = 1f)
    {
        if (!soundEnabled) return;

        AudioClip clip = GetAudioClip($"SFX/{clipName}");
        if (clip == null)
        {
            Debug.LogWarning($"SFX '{clipName}' not found");
            return;
        }

        AudioSource source = GetAvailableSFXSource();
        if (source != null)
        {
            source.pitch = pitch;
            source.volume = sfxVolume * volumeScale * masterVolume;
            source.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Play background music
    /// </summary>
    public void PlayMusic(string musicName, float fadeInDuration = 1f)
    {
        if (!musicEnabled) return;

        AudioClip clip = GetAudioClip($"Music/{musicName}");
        if (clip == null)
        {
            Debug.LogWarning($"Music '{musicName}' not found");
            return;
        }

        if (musicSource.isPlaying)
        {
            StartCoroutine(FadeMusicOut(fadeInDuration / 2f));
        }

        musicSource.clip = clip;
        musicSource.Play();
    }

    /// <summary>
    /// Stop music with fade out
    /// </summary>
    public void StopMusic(float fadeDuration = 1f)
    {
        StartCoroutine(FadeMusicOut(fadeDuration));
    }

    /// <summary>
    /// Get audio clip from cache or load from resources
    /// </summary>
    private AudioClip GetAudioClip(string path)
    {
        if (audioClipCache.TryGetValue(path, out AudioClip clip))
        {
            return clip;
        }

        clip = Resources.Load<AudioClip>(path);
        if (clip != null)
        {
            audioClipCache[path] = clip;
        }

        return clip;
    }

    /// <summary>
    /// Get available SFX audio source
    /// </summary>
    private AudioSource GetAvailableSFXSource()
    {
        foreach (AudioSource source in sfxSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // All sources busy, reuse the oldest
        return sfxSources[0];
    }

    /// <summary>
    /// Fade music out
    /// </summary>
    private System.Collections.IEnumerator FadeMusicOut(float duration)
    {
        float startVolume = musicSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = musicVolume * masterVolume;
    }

    /// <summary>
    /// Set master volume
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume * masterVolume;
    }

    /// <summary>
    /// Toggle sound on/off
    /// </summary>
    public void SetSoundEnabled(bool enabled)
    {
        soundEnabled = enabled;
    }

    /// <summary>
    /// Toggle music on/off
    /// </summary>
    public void SetMusicEnabled(bool enabled)
    {
        musicEnabled = enabled;
        if (!enabled) musicSource.Stop();
    }
}
