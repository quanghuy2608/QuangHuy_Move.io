using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        public bool loop = false;

        [HideInInspector] public AudioSource source;
    }

    [Header("Sound Effects")]
    [SerializeField] private Sound[] soundEffects;

    [Header("Background Music")]
    [SerializeField] private Sound[] musicTracks;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    private Dictionary<string, Sound> soundEffectDict = new Dictionary<string, Sound>();
    private Dictionary<string, Sound> musicDict = new Dictionary<string, Sound>();
    private string currentMusicName;

    #region Initialization
    private void Start()
    {
        // Create audio sources if not assigned
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFX_Source");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
        }

        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("Music_Source");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        InitializeSounds();
        LoadVolumeSettings();
    }

    private void InitializeSounds()
    {
        // Initialize sound effects
        foreach (Sound sound in soundEffects)
        {
            if (sound.clip != null)
            {
                soundEffectDict[sound.name] = sound;
            }
        }

        // Initialize music tracks
        foreach (Sound music in musicTracks)
        {
            if (music.clip != null)
            {
                musicDict[music.name] = music;
            }
        }
    }

    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);

        ApplyVolumeSettings();
    }
    #endregion

    #region Sound Effects
    public void PlaySFX(string soundName)
    {
        if (!soundEffectDict.ContainsKey(soundName))
        {
            Debug.LogWarning($"Sound '{soundName}' not found!");
            return;
        }

        Sound sound = soundEffectDict[soundName];
        sfxSource.pitch = sound.pitch;
        sfxSource.PlayOneShot(sound.clip, sound.volume * sfxVolume * masterVolume);
    }

    public void PlaySFXAtPosition(string soundName, Vector3 position)
    {
        if (!soundEffectDict.ContainsKey(soundName))
        {
            Debug.LogWarning($"Sound '{soundName}' not found!");
            return;
        }

        Sound sound = soundEffectDict[soundName];
        AudioSource.PlayClipAtPoint(sound.clip, position, sound.volume * sfxVolume * masterVolume);
    }

    public void PlayRandomPitchSFX(string soundName, float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        if (!soundEffectDict.ContainsKey(soundName))
        {
            Debug.LogWarning($"Sound '{soundName}' not found!");
            return;
        }

        Sound sound = soundEffectDict[soundName];
        sfxSource.pitch = Random.Range(minPitch, maxPitch);
        sfxSource.PlayOneShot(sound.clip, sound.volume * sfxVolume * masterVolume);
        sfxSource.pitch = 1f; // Reset pitch
    }
    #endregion

    #region Music
    public void PlayMusic(string musicName, bool fadeIn = false, float fadeDuration = 1f)
    {
        if (!musicDict.ContainsKey(musicName))
        {
            Debug.LogWarning($"Music '{musicName}' not found!");
            return;
        }

        // Don't restart if same music is playing
        if (currentMusicName == musicName && musicSource.isPlaying)
            return;

        Sound music = musicDict[musicName];
        currentMusicName = musicName;

        if (fadeIn)
        {
            StartCoroutine(FadeInMusic(music, fadeDuration));
        }
        else
        {
            musicSource.clip = music.clip;
            musicSource.volume = music.volume * musicVolume * masterVolume;
            musicSource.pitch = music.pitch;
            musicSource.loop = music.loop;
            musicSource.Play();
        }
    }

    public void StopMusic(bool fadeOut = false, float fadeDuration = 1f)
    {
        if (fadeOut)
        {
            StartCoroutine(FadeOutMusic(fadeDuration));
        }
        else
        {
            musicSource.Stop();
            currentMusicName = null;
        }
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    private IEnumerator FadeInMusic(Sound music, float duration)
    {
        musicSource.clip = music.clip;
        musicSource.volume = 0f;
        musicSource.pitch = music.pitch;
        musicSource.loop = music.loop;
        musicSource.Play();

        float targetVolume = music.volume * musicVolume * masterVolume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    private IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume;
        currentMusicName = null;
    }
    #endregion

    #region Volume Control
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        ApplyVolumeSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        ApplyVolumeSettings();
    }

    private void ApplyVolumeSettings()
    {
        if (musicSource != null && musicSource.isPlaying && !string.IsNullOrEmpty(currentMusicName))
        {
            if (musicDict.ContainsKey(currentMusicName))
            {
                Sound currentMusic = musicDict[currentMusicName];
                musicSource.volume = currentMusic.volume * musicVolume * masterVolume;
            }
        }
    }

    public float GetMasterVolume() => masterVolume;
    public float GetSFXVolume() => sfxVolume;
    public float GetMusicVolume() => musicVolume;
    #endregion

    #region Utility
    public bool IsMusicPlaying() => musicSource.isPlaying;
    public string GetCurrentMusicName() => currentMusicName;
    #endregion
}