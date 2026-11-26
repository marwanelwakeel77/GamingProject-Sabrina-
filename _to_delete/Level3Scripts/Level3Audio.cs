using System;
using UnityEngine;

public enum Level3Sfx
{
    GuardFootstep,
    GuardAlert,
    FlashlightSearch,
    PlayerCaught,
    PagePickup,
    BookInteract,
    WrongBook,
    CorrectBook,
    MechanismMove,
    PassageOpen,
    SecretDiscovery,
    HintOpen,
    MoonCollect
}

/// <summary>
/// One place for every Level 3 sound. Each event has a clip slot in the Inspector;
/// an empty slot is simply silent, so the level works before the sounds are added.
/// </summary>
public class Level3Audio : MonoBehaviour
{
    [Serializable]
    public class SfxEntry
    {
        public Level3Sfx id;
        public AudioClip clip;
        [Tooltip("Optional extra takes. One of them (or the main clip) is picked at random, never the same twice in a row.")]
        public AudioClip[] variations;
        [Range(0f, 1f)] public float volume = 1f;

        [NonSerialized] public int lastPick = -1;

        public AudioClip Pick()
        {
            int extra = variations != null ? variations.Length : 0;
            int count = 1 + extra;
            if (count == 1) return clip;
            int index = UnityEngine.Random.Range(0, count);
            if (index == lastPick) index = (index + 1) % count;
            lastPick = index;
            AudioClip picked = index == 0 ? clip : variations[index - 1];
            return picked != null ? picked : clip;
        }
    }

    static Level3Audio instance;
    public static Level3Audio Instance => instance != null ? instance : (instance = FindObjectOfType<Level3Audio>());

    [Header("Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Background music (kept quiet so effects stay clear)")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float musicVolume = 0.18f;
    public float musicFadeInTime = 3f;

    [Header("Sound effects: drop a clip into each slot")]
    public SfxEntry[] effects;

    [Header("Positional sounds (guard)")]
    [Tooltip("Guard sounds fade out with distance from the camera and are silent beyond this.")]
    public float hearingDistance = 9f;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;
        EnsureEntries();

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.volume = musicFadeInTime > 0f ? 0f : musicVolume;
            if (!musicSource.isPlaying) musicSource.Play();
        }
    }

    void Update()
    {
        // Gentle fade-in of the background music; runs on real time so it also works while paused.
        if (musicSource == null || !musicSource.isPlaying || musicSource.volume >= musicVolume) return;
        float step = musicFadeInTime > 0f ? musicVolume * Time.unscaledDeltaTime / musicFadeInTime : musicVolume;
        musicSource.volume = Mathf.Min(musicVolume, musicSource.volume + step);
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    void Reset() => EnsureEntries();
    void OnValidate() => EnsureEntries();

    void EnsureEntries()
    {
        var ids = (Level3Sfx[])Enum.GetValues(typeof(Level3Sfx));
        if (effects != null && effects.Length == ids.Length) return;

        var updated = new SfxEntry[ids.Length];
        for (int i = 0; i < ids.Length; i++)
        {
            SfxEntry existing = null;
            if (effects != null)
                foreach (var e in effects)
                    if (e != null && e.id == ids[i]) { existing = e; break; }
            updated[i] = existing ?? new SfxEntry { id = ids[i], volume = 1f };
        }
        effects = updated;
    }

    public static void Play(Level3Sfx id, float volumeScale = 1f)
    {
        if (Instance != null) Instance.PlayInternal(id, volumeScale);
    }

    /// <summary>Plays a sound whose loudness depends on how far the source is from the camera.</summary>
    public static void PlayAt(Level3Sfx id, Vector3 worldPosition, float volumeScale = 1f)
    {
        if (Instance == null) return;
        Camera cam = Camera.main;
        float scale = volumeScale;
        if (cam != null)
        {
            float distance = Vector2.Distance(cam.transform.position, worldPosition);
            scale *= Mathf.Clamp01(1f - distance / Mathf.Max(0.01f, Instance.hearingDistance));
        }
        if (scale > 0.01f) Instance.PlayInternal(id, scale);
    }

    void PlayInternal(Level3Sfx id, float volumeScale)
    {
        if (sfxSource == null || effects == null) return;
        foreach (var e in effects)
        {
            if (e == null || e.id != id) continue;
            AudioClip picked = e.Pick();
            if (picked != null) sfxSource.PlayOneShot(picked, e.volume * volumeScale);
            return;
        }
    }
}
