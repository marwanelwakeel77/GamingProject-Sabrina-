using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// The shared "Press E to ..." plaque. Cipher pages, the statue shelf and the secret passage all use it.
/// Only the object that showed the prompt can hide it, so leaving one trigger never hides another's prompt.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class InteractionPrompt : MonoBehaviour
{
    public TMP_Text label;
    public float fadeDuration = 0.18f;
    public float bobAmount = 4f;
    public float bobSpeed = 2.2f;

    CanvasGroup group;
    RectTransform rect;
    Vector2 basePos;
    object owner;
    Coroutine fade;
    bool initialised;

    void Awake() => Init();

    void Init()
    {
        if (initialised) return;
        initialised = true;
        group = GetComponent<CanvasGroup>();
        rect = transform as RectTransform;
        if (rect != null) basePos = rect.anchoredPosition;
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    public bool IsShownBy(object who) => who != null && owner == who;

    public void Show(object who, string message)
    {
        Init();
        owner = who;
        if (label != null && !string.IsNullOrEmpty(message)) label.text = message;
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        StartFade(1f);
    }

    public void Hide(object who)
    {
        if (who == null || who != owner) return;
        owner = null;
        if (gameObject.activeInHierarchy) StartFade(0f);
        else if (group != null) group.alpha = 0f;
    }

    void Update()
    {
        if (rect != null) rect.anchoredPosition = basePos + Vector2.up * (Mathf.Sin(Time.unscaledTime * bobSpeed) * bobAmount);
    }

    void StartFade(float target)
    {
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(FadeTo(target));
    }

    IEnumerator FadeTo(float target)
    {
        float start = group.alpha;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        group.alpha = target;
        fade = null;
    }
}
