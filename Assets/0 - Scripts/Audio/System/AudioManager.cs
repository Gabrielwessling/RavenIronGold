using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Volumes (0..1)")]
    [Range(0, 1)] public float masterVolume = 1f;
    [Range(0, 1)] public float musicVolume = 1f;
    [Range(0, 1)] public float sfxVolume = 1f;

    [Header("SFX pitch variation")]
    public float sfxPitchVariance = 0.08f;

    private AudioDatabase currentDatabase;
    private Coroutine musicRoutine;

    const string PrefMaster = "audio_master";
    const string PrefMusic = "audio_music";
    const string PrefSfx = "audio_sfx";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.loop = false;
        musicSource.loop = false;

        masterVolume = PlayerPrefs.GetFloat(PrefMaster, 1f);
        musicVolume = PlayerPrefs.GetFloat(PrefMusic, 1f);
        sfxVolume = PlayerPrefs.GetFloat(PrefSfx, 1f);

        ApplyVolumes();

        SceneManager.sceneLoaded += OnSceneLoaded;

        PlayBGMForScene(SceneManager.GetActiveScene().name, 0f);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGMForScene(scene.name);
    }

    void ApplyVolumes()
    {
        if (musicSource != null) musicSource.volume = masterVolume * musicVolume;
        if (sfxSource != null) sfxSource.volume = masterVolume * sfxVolume;
    }

    public void SetMasterVolume(float v)
    {
        masterVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(PrefMaster, masterVolume);
        ApplyVolumes();
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(PrefMusic, musicVolume);
        ApplyVolumes();
    }

    public void SetSfxVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(PrefSfx, sfxVolume);
        ApplyVolumes();
    }

    void OnValidate()
    {
        masterVolume = Mathf.Clamp01(masterVolume);
        musicVolume = Mathf.Clamp01(musicVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);

        if (musicSource == null) musicSource = GetComponent<AudioSource>();
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();

        ApplyVolumes();

        if (Application.isPlaying)
        {
            PlayerPrefs.SetFloat(PrefMaster, masterVolume);
            PlayerPrefs.SetFloat(PrefMusic, musicVolume);
            PlayerPrefs.SetFloat(PrefSfx, sfxVolume);
        }
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;

        GameObject sfxObject = new GameObject("SFX Temp");
        sfxObject.transform.SetParent(transform);

        AudioSource tempSource = sfxObject.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = volumeScale * sfxSource.volume;
        tempSource.pitch = 1f + Random.Range(-sfxPitchVariance, sfxPitchVariance);
        tempSource.Play();

        Destroy(sfxObject, clip.length / Mathf.Max(Mathf.Abs(tempSource.pitch), 0.01f) + 0.05f);
    }

    public void PlayBGMForScene(string sceneName = null, float crossfadeTime = 1f)
    {
        if (string.IsNullOrEmpty(sceneName)) sceneName = SceneManager.GetActiveScene().name;

        string path = $"Audio/Databases/{sceneName}AudioDatabase";
        var db = Resources.Load<AudioDatabase>(path);
        if (db == null || db.entries == null || db.ValidCount == 0)
        {
            Debug.LogWarning($"AudioManager: No database found at Resources/{path}");
            return;
        }

        currentDatabase = db;

        var validEntries = currentDatabase.entries.FindAll(e => e != null && e.clip != null);
        var entry = validEntries[Random.Range(0, validEntries.Count)];

        if (musicRoutine != null) StopCoroutine(musicRoutine);
        musicRoutine = StartCoroutine(CrossfadeToClip(entry.clip, entry.volume, crossfadeTime));
    }

    public void StopBGM(float fadeOut = 0.5f)
    {
        if (musicRoutine != null) StopCoroutine(musicRoutine);
        if (fadeOut > 0)
            StartCoroutine(FadeOut(musicSource, fadeOut));
        else
            musicSource.Stop();
    }

    private IEnumerator CrossfadeToClip(AudioClip clip, float clipVolume, float time)
    {
        if (clip == null) yield break;

        if (musicSource.isPlaying && time > 0)
            yield return StartCoroutine(FadeOut(musicSource, time / 2f));

        musicSource.clip = clip;
        musicSource.Play();

        if (time > 0)
            yield return StartCoroutine(FadeIn(musicSource, clipVolume, time / 2f));
        else
            musicSource.volume = masterVolume * musicVolume * clipVolume;
    }

    private IEnumerator FadeOut(AudioSource src, float time)
    {
        float start = src.volume;
        float t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(start, 0f, t / time);
            yield return null;
        }
        src.volume = 0f;
        src.Stop();
        ApplyVolumes();
    }

    private IEnumerator FadeIn(AudioSource src, float clipVolume, float time)
    {
        float target = masterVolume * musicVolume * clipVolume;
        float t = 0f;
        src.volume = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(0f, target, t / time);
            yield return null;
        }
        src.volume = target;
    }
}
