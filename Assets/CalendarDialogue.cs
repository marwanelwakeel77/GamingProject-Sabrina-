using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// The instruction panel above the calendar. It only tells the player what to do:
// it never reveals dates or solves the puzzle, and closing it does not reset anything.
[RequireComponent(typeof(CanvasGroup))]
public class CalendarDialogue : MonoBehaviour
{
    public float fadeTime = 0.35f;
    public Button closeButton;   // the X in the panel's upper-right corner

    private CanvasGroup group;
    private Coroutine fadeRoutine;
    private bool visible;
    private bool dismissed;

    public bool IsDismissed { get { return dismissed; } }

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        if (closeButton != null) closeButton.onClick.AddListener(Dismiss);
        group.alpha = 0f;
    }

    // Shows the panel unless the player already closed it. It is never forced open again.
    public void ShowOnce()
    {
        if (dismissed || visible) return;

        visible = true;
        gameObject.SetActive(true);
        Fade(1f, null);
    }

    // Called by the X button. The calendar keeps its progress.
    public void Dismiss()
    {
        if (!visible) return;

        visible = false;
        dismissed = true;
        Fade(0f, () => gameObject.SetActive(false));
    }

    void Fade(float target, Action onDone)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(target, onDone));
    }

    IEnumerator FadeRoutine(float target, Action onDone)
    {
        float start = group.alpha;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, target, t / fadeTime);
            yield return null;
        }

        group.alpha = target;
        fadeRoutine = null;
        if (onDone != null) onDone();
    }
}
