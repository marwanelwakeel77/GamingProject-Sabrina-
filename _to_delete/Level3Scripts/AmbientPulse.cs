using UnityEngine;

/// <summary>
/// Slow glow used for Level 3's visual hints (cipher pages, the statue, the sigil book).
/// It can stay hidden until the player holds all three cipher pages.
/// </summary>
public class AmbientPulse : MonoBehaviour
{
    public SpriteRenderer target;
    public float minAlpha = 0.1f;
    public float maxAlpha = 0.35f;
    public float speed = 1.2f;
    public float scaleAmount = 0.05f;
    [Tooltip("Stay invisible until all three cipher pages are collected.")]
    public bool onlyWhenAllPagesCollected = false;

    Vector3 baseScale;
    float phase;
    float visibility;

    void Awake()
    {
        if (target == null) target = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        phase = Random.Range(0f, Mathf.PI * 2f);
        visibility = onlyWhenAllPagesCollected ? 0f : 1f;
    }

    void Update()
    {
        if (target == null) return;
        float wanted = (!onlyWhenAllPagesCollected || PlayerStats.HasAllCipherPages) ? 1f : 0f;
        visibility = Mathf.MoveTowards(visibility, wanted, Time.deltaTime * 0.5f);

        float t = (Mathf.Sin(Time.time * speed + phase) + 1f) * 0.5f;
        Color c = target.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, t) * visibility;
        target.color = c;
        transform.localScale = baseScale * (1f + scaleAmount * t);
    }
}
