using System.Collections;
using UnityEngine;
using TMPro;

// One line of text at the top of the screen, for short messages such as "The door is locked."
// Put this on a TextMeshProUGUI object in the Canvas.
// The message types out one character at a time, stays, then fades out.
public class HintText : MonoBehaviour
{
    public static HintText Instance;

    public float charactersPerSecond = 40f;
    public float showSeconds = 2.5f;
    public float fadeOutTime = 0.4f;
    public Color textColor = new Color(1f, 0.85f, 0.55f, 1f);

    private TextMeshProUGUI label;
    private Coroutine hideRoutine;

    void Awake()
    {
        Instance = this;
        label = GetComponent<TextMeshProUGUI>();
        if (label != null)
        {
            label.text = "";
            label.color = textColor;
            label.maxVisibleCharacters = 0;
        }
    }

    public static void Show(string message)
    {
        if (Instance == null)
        {
            Debug.Log(message);
            return;
        }
        Instance.ShowMessage(message);
    }

    void ShowMessage(string message)
    {
        if (label == null) return;

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        label.text = message;
        label.color = textColor;
        label.ForceMeshUpdate();
        hideRoutine = StartCoroutine(TypeThenHide());
    }

    IEnumerator TypeThenHide()
    {
        // Typewriter: reveal one more character each step.
        int total = label.textInfo.characterCount;
        float t = 0f;
        label.maxVisibleCharacters = 0;
        while (label.maxVisibleCharacters < total)
        {
            t += Time.deltaTime * charactersPerSecond;
            label.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(t));
            yield return null;
        }

        yield return new WaitForSeconds(showSeconds);

        // Fade out.
        float elapsed = 0f;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutTime);
            label.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
            yield return null;
        }

        label.text = "";
        label.color = textColor;
        label.maxVisibleCharacters = 0;
        hideRoutine = null;
    }
}
