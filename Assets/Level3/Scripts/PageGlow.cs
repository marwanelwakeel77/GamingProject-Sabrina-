using UnityEngine;

public class PageGlow : MonoBehaviour
{
    public float minAlpha = 0.25f;
    public float maxAlpha = 0.7f;
    public float speed = 2f;
    public float growAmount = 0.15f;

    private SpriteRenderer sr;
    private Vector3 startScale;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
    }

    void Update()
    {
        float amount = (Mathf.Sin(Time.time * speed) + 1f) / 2f;

        Color c = sr.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, amount);
        sr.color = c;

        transform.localScale = startScale * (1f + growAmount * amount);
    }
}
