using System.Collections;
using UnityEngine;

/// <summary>Full-screen black fade used for capture, cut-scenes and the level ending. Runs on unscaled time.</summary>
[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    static ScreenFader instance;
    public static ScreenFader Instance => instance != null ? instance : (instance = FindObjectOfType<ScreenFader>(true));

    [SerializeField] float defaultDuration = 0.45f;

    CanvasGroup group;
    Coroutine routine;

    public bool IsBlack => group != null && group.alpha > 0.99f;

    void Awake()
    {
        instance = this;
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public Coroutine FadeOut(float duration = -1f) => Fade(1f, duration);
    public Coroutine FadeIn(float duration = -1f) => Fade(0f, duration);

    public Coroutine Fade(float target, float duration)
    {
        if (group == null) group = GetComponent<CanvasGroup>();
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FadeRoutine(target, duration < 0f ? defaultDuration : duration));
        return routine;
    }

    IEnumerator FadeRoutine(float target, float duration)
    {
        group.blocksRaycasts = target > 0.5f;
        float start = group.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }
        group.alpha = target;
        routine = null;
    }
}
