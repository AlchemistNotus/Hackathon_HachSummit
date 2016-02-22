using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    const bool AutoPause = true;

    public float DefaultMusicVolume = 1f;
    public float DefaultSoundVolume = 1f;

    const float MusicFadeTime = 2f;

    public AudioMixerGroup musicAudioMixerGroup;
    public AudioMixerGroup soundAudioMixerGroup;

    public GameObject sound3DPrefab;

    List<string> _tempSoundsNames = new List<string>();
    List<AudioSource> _sounds = new List<AudioSource>();
    AudioSource _currentMusicSource;

    string _currentMusicName;

    bool isFading;
    float fadeTimer;

    struct MusicFader
    {
        public AudioSource audio;
        public float timer;
        public float fadingTime;
        public float startVolume;
        public float targetVolume;
        public bool destroyOnComplete;
    }

    List<MusicFader> _musicFadings = new List<MusicFader>();

    float _volumeMusic;
    float _volumeSound;

    bool _mutedMusic;
    bool _mutedSound;

#region Public functions

    public static void PlayMusic(string name)
    {
        Instance.PlayMusicInternal(name);
    }

    public static void StopMusic()
    {
        Instance.StopMusicInternal();
    }

    public static void PlaySound(string name, bool pausable = true)
    {
        Instance.PlaySoundInternal(name, pausable);
    }

    public static void Play3DSound(string name, Vector3 position, int max, bool pausable = true)
    {
        Instance.Play3DSoundInternal(name, pausable, position, max);
    }

    public static void PlaySoundWithDelay(string name, float delay, bool pausable = true)
    {
        Instance.PlaySoundWithDelayInternal(name, delay, pausable);
    }

    public static void Pause()
    {
        if (AutoPause)
            return;

        // Supress Unreachable code warning
#pragma warning disable
        AudioListener.pause = true;
#pragma warning restore
    }

    public static void UnPause()
    {
        if (AutoPause)
            return;

        // Supress Unreachable code warning
#pragma warning disable
        AudioListener.pause = false;
#pragma warning restore
    }

    public static void StopAllPausableSounds()
    {
        Instance.StopAllPausableSoundsInternal();
    }

    // Volume [0 - 1]
    public static void SetMusicVolume(float volume)
    {
        Instance.SetMusicVolumeInternal(volume);
    }

    // Volume [0 - 1]
    public static float GetMusicVolume()
    {
        return Instance.GetMusicVolumeInternal();
    }

    public static void SetMusicMuted(bool mute)
    {
        Instance.SetMusicMutedInternal(mute);
    }

    public static bool GetMusicMuted()
    {
        return Instance.GetMusicMutedInternal();
    }

    // Volume [0 - 1]
    public static void SetSoundVolume(float volume)
    {
        Instance.SetSoundVolumeInternal(volume);
    }

    // Volume [0 - 1]
    public static float GetSoundVolume()
    {
        return Instance.GetSoundVolumeInternal();
    }

    public static void SetSoundMuted(bool mute)
    {
        Instance.SetSoundMutedInternal(mute);
    }

    public static bool GetSoundMuted()
    {
        return Instance.GetSoundMutedInternal();
    }

#endregion

#region Singleton
    private static SoundManager _instance;

    public static SoundManager Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                Debug.LogWarning("[Singleton] Instance '" + typeof(SoundManager) +
                    "' already destroyed on application quit." +
                    " Won't create again - returning null.");
                return null;
            }

            if (_instance != null)
            {
                return _instance;
            }

            // Do not modify _instance here. It will be assigned in awake
            GameObject prefabFromResources = Resources.Load<GameObject>("SoundManager/SoundManager");
            if (prefabFromResources != null)
            {
                prefabFromResources = Instantiate<GameObject>(prefabFromResources);
                prefabFromResources.name = "SoundManager (singleton)";
                return prefabFromResources.GetComponent<SoundManager>();
            }

            Debug.LogWarning("SoundManager prefab not found in Resources. It will be created with default settings.");
            return new GameObject("SoundManager (singleton)").AddComponent<SoundManager>();
        }
    }

    private static bool applicationIsQuitting = false;

    /// <summary>
    /// When Unity quits, it destroys objects in a random order.
    /// In principle, a Singleton is only destroyed when application quits.
    /// If any script calls Instance after it have been destroyed, 
    ///   it will create a buggy ghost object that will stay on the Editor scene
    ///   even after stopping playing the Application. Really bad!
    /// So, this was made to be sure we're not creating that buggy ghost object.
    /// </summary>
    public void OnDestroy()
    {
        applicationIsQuitting = true;
    }
#endregion

#region Settings

    void SetMusicVolumeInternal(float volume)
    {
        _volumeMusic = volume;
        SaveSettings();
        ApplyMusicVolume();
    }

    float GetMusicVolumeInternal()
    {
        return _volumeMusic;
    }

    void SetMusicMutedInternal(bool mute)
    {
        _mutedMusic = mute;
        SaveSettings();
        ApplyMusicMuted();
    }

    bool GetMusicMutedInternal()
    {
        return _mutedMusic;
    }

    void SetSoundVolumeInternal(float volume)
    {
        _volumeSound = volume;
        SaveSettings();
        ApplySoundVolume();
    }

    float GetSoundVolumeInternal()
    {
        return _volumeSound;
    }

    void SetSoundMutedInternal(bool mute)
    {
        _mutedSound = mute;
        SaveSettings();
        ApplySoundMuted();
    }

    bool GetSoundMutedInternal()
    {
        return _mutedSound;
    }

#endregion // Settings

#region Music

    void PlayMusicInternal(string musicName)
    {
        if (string.IsNullOrEmpty(musicName)) {
            Debug.Log("Music empty or null");
            return;
        }

        if (_currentMusicName == musicName) {
            Debug.Log("Music already playing: " + musicName);
            return;
        }

        StopMusicInternal();

        _currentMusicName = musicName;

        AudioClip musicClip = LoadClip("Music/" + musicName);

        GameObject music = new GameObject("Music: " + musicName);
        AudioSource musicSource = music.AddComponent<AudioSource>();

        music.transform.parent = transform;

        musicSource.outputAudioMixerGroup = musicAudioMixerGroup;
        
        musicSource.loop = true;
        musicSource.priority = 0;
        musicSource.playOnAwake = false;
        musicSource.mute = _mutedMusic;
        musicSource.ignoreListenerPause = true;
        musicSource.clip = musicClip;
        musicSource.Play();

        musicSource.volume = 0;
        StartFadeMusic(musicSource, MusicFadeTime, _volumeMusic * DefaultMusicVolume, false);
        //musicSource.DOFade(_volumeMusic * DefaultMusicVolume, MusicFadeTime).SetUpdate(true);

        _currentMusicSource = musicSource;
    }

    void StopMusicInternal()
    {
        _currentMusicName = "";
        if (_currentMusicSource != null)
        {
            StartFadeMusic(_currentMusicSource, MusicFadeTime, 0, true);
            _currentMusicSource = null;
        }
    }

#endregion // Music

#region Sound

    void PlaySoundInternal(string soundName, bool pausable)
    {
        if (string.IsNullOrEmpty(soundName)) {
            Debug.Log("Sound null or empty");
            return;
        }

        int sameCountGuard = 0;
        foreach (AudioSource audioSource in _sounds)
        {
            if (audioSource.clip.name == soundName)
                sameCountGuard++;
        }

        if (sameCountGuard > 8)
        {
            //Debug.Log("Too much duplicates for sound: " + soundName);
            return;
        }

        if (_sounds.Count > 16) {
            Debug.Log("Too much sounds");
            return;
        }

        //PlaySoundInternalNow(soundName, pausable);
        StartCoroutine(PlaySoundInternalSoon(soundName, pausable));
    }

    void Play3DSoundInternal(string soundName, bool pausable, Vector3 position, int max)
    {
        if (string.IsNullOrEmpty(soundName))
        {
            Debug.Log("Sound null or empty");
            return;
        }

        int sameCountGuard = 0;
        foreach (AudioSource audioSource in _sounds)
        {
            if (audioSource.clip.name == soundName)
                sameCountGuard++;
        }

        foreach (string tempSoundName in _tempSoundsNames)
        {
            if (tempSoundName == soundName)
                sameCountGuard++;
        }

        if (sameCountGuard >= max)
        {
            //Debug.Log("Too much duplicates for sound: " + soundName);
            return;
        }

        if (_sounds.Count > 16)
        {
            Debug.Log("Too much sounds");
            return;
        }

        //PlaySoundInternalNow(soundName, pausable);
        StartCoroutine(Play3DSoundInternalSoon(soundName, pausable, position));
    }
    IEnumerator PlaySoundInternalSoon(string soundName, bool pausable)
    {
        ResourceRequest request = LoadClipAsync("Sounds/" + soundName);
        while (!request.isDone)
        {
            yield return null;
        }

        AudioClip soundClip = (AudioClip)request.asset;
        if (null == soundClip)
        {
            Debug.Log("Sound not loaded: " + soundName);
        }

        GameObject sound = new GameObject("Sound: " + soundName);
        AudioSource soundSource = sound.AddComponent<AudioSource>();
        sound.transform.parent = transform;

        soundSource.outputAudioMixerGroup = soundAudioMixerGroup;
        soundSource.priority = 128;
        soundSource.playOnAwake = false;
        soundSource.mute = _mutedSound;
        soundSource.volume = _volumeSound * DefaultSoundVolume;
        soundSource.clip = soundClip;
        soundSource.Play();
        soundSource.ignoreListenerPause = !pausable;

        _sounds.Add(soundSource);
    }

    IEnumerator Play3DSoundInternalSoon(string soundName, bool pausable, Vector3 position)
    {
        _tempSoundsNames.Add(soundName);
        ResourceRequest request = LoadClipAsync("Sounds/" + soundName);
        while (!request.isDone)
        {
            yield return null;
        }
        _tempSoundsNames.Remove(soundName);

        AudioClip soundClip = (AudioClip)request.asset;
        if (null == soundClip)
        {
            Debug.Log("Sound not loaded: " + soundName);
        }

        GameObject sound = Instantiate(sound3DPrefab);
        AudioSource soundSource = sound.GetComponent<AudioSource>();
        sound.name = "3D - " + soundName;
        sound.transform.parent = transform;
        sound.transform.position = position;

        soundSource.outputAudioMixerGroup = soundAudioMixerGroup;
        soundSource.priority = 128;
        soundSource.playOnAwake = false;
        soundSource.mute = _mutedSound;
        soundSource.volume = _volumeSound * DefaultSoundVolume;
        soundSource.clip = soundClip;
        soundSource.pitch = Random.Range(0.9f, 1.1f);
        soundSource.Play();
        soundSource.ignoreListenerPause = !pausable;

        _sounds.Add(soundSource);
    }

    void PlaySoundWithDelayInternal(string soundName, float delay, bool pausable)
    {
        StartCoroutine(PlaySoundWithDelayCoroutine(soundName, delay, pausable));
    }

    void StopAllPausableSoundsInternal()
    {
        foreach (AudioSource sound in _sounds)
        {
            if (!sound.ignoreListenerPause)
                sound.Stop();
        }
    }

    #endregion // Sound

#region Internal

    void Awake()
    {
        // Only one instance of SoundManager at a time!
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }

    void Update()
    {
        var soundsToDelete = _sounds.FindAll(sound => !sound.isPlaying);

        foreach (AudioSource sound in soundsToDelete)
        {
            _sounds.Remove(sound);
            Destroy(sound.gameObject);
        }

        if (AutoPause)
        {
            bool curPause = Time.timeScale < 0.1f;
            if (curPause != AudioListener.pause)
            {
                AudioListener.pause = curPause;
            }
        }

        for (int i = 0; i < _musicFadings.Count ; i++)
        {
            MusicFader music = _musicFadings[i];
            if (music.audio == null)
            {
                _musicFadings.RemoveAt(i);
                i--;
            }
            else
            {
                music.timer += Time.unscaledDeltaTime;
                _musicFadings[i] = music;
                if (music.timer >= music.fadingTime)
                {
                    music.audio.volume = music.targetVolume;
                    if (music.destroyOnComplete)
                    {
                        Destroy(music.audio.gameObject);
                    }
                    _musicFadings.RemoveAt(i);
                    i--;
                }
                else
                {
                    float k = Mathf.Clamp01(music.timer / music.fadingTime);
                    music.audio.volume = Mathf.Lerp(music.startVolume, music.targetVolume, k);
                }
            }
        }
    }

    void StopFadingForMusic(AudioSource music)
    {
        for (int i = 0; i < _musicFadings.Count; i++)
        {
            MusicFader fader = _musicFadings[i];
            if (fader.audio == music)
            {
                if (fader.destroyOnComplete)
                {
                    Destroy(fader.audio.gameObject);
                }
                _musicFadings.RemoveAt(i);
                return;
            }
        }
    }
    void StartFadeMusic(AudioSource music, float duration, float targetVolume, bool destroyOnComplete)
    {
        MusicFader fader;
        fader.audio = music;
        fader.fadingTime = duration;
        fader.timer = 0;
        fader.startVolume = music.volume;
        fader.targetVolume = targetVolume;
        fader.destroyOnComplete = destroyOnComplete;
        _musicFadings.Add(fader);
    }

    private IEnumerator PlaySoundWithDelayCoroutine(string name, float delay, bool pausable)
    {
        float timer = delay;
        while (timer > 0)
        {
            timer -= pausable ? Time.deltaTime : Time.unscaledDeltaTime;
            yield return null;
        }

        PlaySound(name, pausable);
    }

    AudioClip LoadClip(string name)
    {
        string path = "SoundManager/" + name;
        AudioClip clip = Resources.Load<AudioClip>(path);
        return clip;
    }

    ResourceRequest LoadClipAsync(string name)
    {
        string path = "SoundManager/" + name;
        return Resources.LoadAsync<AudioClip>(path);
    }

    void SaveSettings()
    {
        PlayerPrefs.SetFloat("SM_MusicVolume", _volumeMusic);
        PlayerPrefs.SetFloat("SM_SoundVolume", _volumeSound);

        PlayerPrefs.SetInt("SM_MusicMute", _mutedMusic ? 1 : 0);
        PlayerPrefs.SetInt("SM_SoundMute", _mutedSound ? 1 : 0);
    }

    void LoadSettings()
    {
        _volumeMusic = PlayerPrefs.GetFloat("SM_MusicVolume", 1);
        _volumeSound = PlayerPrefs.GetFloat("SM_SoundVolume", 1);

        _mutedMusic = PlayerPrefs.GetInt("SM_MusicMute", 0) == 1;
        _mutedSound = PlayerPrefs.GetInt("SM_SoundMute", 0) == 1;

        ApplySoundVolume();
        ApplyMusicVolume();

        ApplySoundMuted();
        ApplyMusicMuted();
    }

    void ApplySoundVolume()
    {
        foreach (AudioSource sound in _sounds)
        {
            sound.volume = _volumeSound * DefaultSoundVolume;
        }
    }

    void ApplyMusicVolume()
    {
        if (_currentMusicSource != null)
        {
            StopFadingForMusic(_currentMusicSource);
            _currentMusicSource.volume = _volumeMusic * DefaultMusicVolume;
        }
    }

    void ApplySoundMuted()
    {
        foreach (AudioSource sound in _sounds)
        {
            sound.mute = _mutedSound;
        }
    }

    void ApplyMusicMuted()
    {
        if (_currentMusicSource != null)
        {
            _currentMusicSource.mute = _mutedMusic;
        }
    }

#endregion // Internal
}
