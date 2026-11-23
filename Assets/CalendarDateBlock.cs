using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(SpriteRenderer))]
public class CalendarDateBlock : MonoBehaviour
{
    public Sprite hiddenSprite;       // the question block
    public string dateNumber;         // the date hidden here, e.g. "5"
    public TextMeshPro numberLabel;   
    public CalendarPuzzle puzzle;     

    private SpriteRenderer sr;
    private Vector3 originPosition;
    private Coroutine breakRoutine;

    public bool IsRevealed { get; private set; }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originPosition = transform.localPosition;
    }

    void Start()
    {
        ResetBlock();
    }

    // Yellow tint = the block Alex will break next.
    public void SetSelected(bool selected)
    {
        if (sr == null || IsRevealed) return;
        sr.color = selected ? new Color(1f, 0.9f, 0.45f) : Color.white;
    }

    // Breaks the block: it shakes, the question block disappears and the date is shown.
    public void Break()
    {
        if (IsRevealed) return;

        IsRevealed = true;
        SetSelected(false);

        if (numberLabel != null)
        {
            numberLabel.text = dateNumber;
            numberLabel.gameObject.SetActive(true);
        }

        breakRoutine = StartCoroutine(BreakEffect());
    }

    IEnumerator BreakEffect()
    {
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            transform.localPosition = originPosition + (Vector3)Random.insideUnitCircle * 0.05f;
            yield return null;
        }

        transform.localPosition = originPosition;
        sr.enabled = false;
        breakRoutine = null;
    }

    public void ResetBlock()
    {
        if (breakRoutine != null)
        {
            StopCoroutine(breakRoutine);
            breakRoutine = null;
        }

        IsRevealed = false;
        transform.localPosition = originPosition;

        if (sr != null)
        {
            sr.enabled = true;
            sr.color = Color.white;
            if (hiddenSprite != null) sr.sprite = hiddenSprite;
        }

        if (numberLabel != null) numberLabel.gameObject.SetActive(false);
    }
}
