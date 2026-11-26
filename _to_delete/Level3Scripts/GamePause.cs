using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One place that owns Time.timeScale. Every panel that pauses the game pushes itself here
/// and pops itself when it closes, so two panels can never leave the game frozen or unpause each other.
/// </summary>
public static class GamePause
{
    static readonly HashSet<object> owners = new HashSet<object>();
    static int lastChangeFrame = -1;

    /// <summary>Raised with true when the first owner pauses and false when the last owner resumes.</summary>
    public static event Action<bool> PauseChanged;

    public static bool IsPaused => owners.Count > 0;

    /// <summary>
    /// True while paused and on the frame a pause started or ended, so the key press that closed
    /// a panel can't also trigger a world interaction in the same frame.
    /// </summary>
    public static bool InputBlocked => owners.Count > 0 || Time.frameCount <= lastChangeFrame;

    public static void Push(object owner)
    {
        if (owner == null || !owners.Add(owner)) return;
        lastChangeFrame = Time.frameCount;
        Time.timeScale = 0f;
        if (owners.Count == 1) PauseChanged?.Invoke(true);
    }

    public static void Pop(object owner)
    {
        if (owner == null || !owners.Remove(owner)) return;
        lastChangeFrame = Time.frameCount;
        if (owners.Count == 0)
        {
            Time.timeScale = 1f;
            PauseChanged?.Invoke(false);
        }
    }

    /// <summary>Clears every pause owner (used before loading another scene).</summary>
    public static void ReleaseAll()
    {
        bool wasPaused = owners.Count > 0;
        owners.Clear();
        lastChangeFrame = Time.frameCount;
        Time.timeScale = 1f;
        if (wasPaused) PauseChanged?.Invoke(false);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        owners.Clear();
        lastChangeFrame = -1;
        PauseChanged = null;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    static void OnSceneUnloaded(Scene scene)
    {
        if (owners.Count > 0) ReleaseAll();
    }
}
