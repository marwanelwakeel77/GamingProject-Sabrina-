using UnityEngine;

public class PageGlow : MonoBehaviour
{
    public float minAlpha = 0.25f;
    public float maxAlpha = 0.7f;
    public float glowSpeed = 0.7f;
    public float growAmount = 0.15f;

    private SpriteRenderer sr;
    private Vector3 startScale;
    private float amount = 0.5f;
    private bool growing = true;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
    }

    void Update()
    {
        if (growing == true)
            amount = Mathf.MoveTowards(amount, 1f, glowSpeed * Time.deltaTime);
        else
            amount = Mathf.MoveTowards(amount, 0f, glowSpeed * Time.deltaTime);

        if (amount >= 1f)
            growing = false;
        else if (amount <= 0f)
            growing = true;

        Color c = sr.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, amount);
        sr.color = c;

        transform.localScale = startScale * (1f + growAmount * amount);
    }
}
