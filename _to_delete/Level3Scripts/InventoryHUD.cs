using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Small on-screen inventory for Level 3: cipher pages found and the Crescent Moon Symbol,
/// plus short toast messages. Listens to PlayerStats events instead of polling.
/// </summary>
public class InventoryHUD : MonoBehaviour
{
    static InventoryHUD instance;
    public static InventoryHUD Instance => instance != null ? instance : (instance = FindObjectOfType<InventoryHUD>());

    [Header("Cipher pages")]
    public TMP_Text pageCountText;
    public RectTransform pageIcon;

    [Header("Crescent Moon Symbol")]
    public Image moonIcon;
    public Color moonMissingColor = new Color(0.32f, 0.36f, 0.5f, 0.35f);
    public Color moonCollectedColor = Color.white;

    [Header("Toast")]
    public CanvasGroup toastGroup;
    public TMP_Text toastText;
    public float toastDuration = 3.2f;

    private Coroutine toastRoutine;
    private int lastPageCount = -1;

    void Awake()
    {
        instance = this;
        if (toastGroup != null) toastGroup.alpha = 0f;
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    void OnEnable()
    {
        PlayerStats.CipherPagesChanged += OnPagesChanged;
        PlayerStats.CrescentMoonChanged += OnMoonChanged;
    }

    void OnDisable()
    {
        PlayerStats.CipherPagesChanged -= OnPagesChanged;
        PlayerStats.CrescentMoonChanged -= OnMoonChanged;
    }

    void Start()
    {
        RefreshPages(false);
        RefreshMoon(false);
    }

    void OnPagesChanged() => RefreshPages(true);

    void OnMoonChanged()
    {
        RefreshMoon(true);
        if (PlayerStats.hasCrescentMoon) Toast("Crescent Moon Symbol added to your inventory.");
    }

    void RefreshPages(bool announce)
    {
        int count = PlayerStats.cipherPagesCollected;
        if (pageCountText != null) pageCountText.text = count + " / " + PlayerStats.TotalCipherPages;

        if (announce && lastPageCount >= 0 && count > lastPageCount)
        {
            if (pageIcon != null) StartCoroutine(Pop(pageIcon));
            if (count >= PlayerStats.TotalCipherPages) Toast("All three pages found. Sabrina's message can now be read.");
            else Toast("Cipher page added  (" + count + " / " + PlayerStats.TotalCipherPages + ")");
        }
        lastPageCount = count;
    }

    void RefreshMoon(bool animate)
    {
        if (moonIcon == null) return;
        moonIcon.color = PlayerStats.hasCrescentMoon ? moonCollectedColor : moonMissingColor;
        if (animate && PlayerStats.hasCrescentMoon) StartCoroutine(Pop(moonIcon.rectTransform));
    }

    public static void Toast(string message)
    {
        if (Instance != null) Instance.ShowToast(message);
    }

    public void ShowToast(string message)
    {
        if (toastGroup == null || toastText == null || !isActiveAndEnabled) return;
        toastText.text = message;
        if (toastRoutine != null) StopCoroutine(toastRoutine);
        toastRoutine = StartCoroutine(ToastRoutine());
    }

    IEnumerator ToastRoutine()
    {
        yield return FadeGroup(toastGroup, 1f, 0.25f);
        yield return new WaitForSecondsRealtime(toastDuration);
        yield return FadeGroup(toastGroup, 0f, 0.45f);
        toastRoutine = null;
    }

    static IEnumerator FadeGroup(CanvasGroup group, float target, float duration)
    {
        float start = group.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        group.alpha = target;
    }

    static IEnumerator Pop(RectTransform target)
    {
        const float duration = 0.4f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = t / duration;
            target.localScale = Vector3.one * (1f + 0.35f * Mathf.Sin(k * Mathf.PI));
            yield return null;
        }
        target.localScale = Vector3.one;
    }
}
