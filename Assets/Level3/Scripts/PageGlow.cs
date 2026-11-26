using UnityEngine;

// A soft golden glow that slowly pulses behind a cipher page, so the player notices it.
// Put it on the glow sprite (a child of the page): when the page is collected the glow disappears with it.
public class PageGlow : MonoBehaviour
{
    public float minAlpha = 0.25f;  // faintest
    public float maxAlpha = 0.7f;   // brightest
    public float speed = 2f;        // how fast it pulses
    public float growAmount = 0.15f; // how much bigger it gets at its brightest

    private SpriteRenderer sr;
    private Vector3 startScale;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
    }

    void Update()
    {
        // goes smoothly from 0 to 1 and back again (like Lerp in Lab 4)
        float amount = (Mathf.Sin(Time.time * speed) + 1f) / 2f;

        Color c = sr.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, amount);
        sr.color = c;

        transform.localScale = startScale * (1f + growAmount * amount);
    }
}
