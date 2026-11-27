using UnityEngine;
using UnityEngine.UI;

public class MenuGlow : MonoBehaviour
{
    public float minAlpha = 0.3f;
    public float maxAlpha = 0.7f;
    public float changeTime = 0.1f;
    public float smoothing = 8f;

    private Image glow;
    private float targetAlpha;
    private float timer = 0f;

    void Start()
    {
        glow = GetComponent<Image>();
        targetAlpha = maxAlpha;
    }

    void Update()
    {
        timer = timer + Time.deltaTime;
        if (timer >= changeTime)
        {
            timer = 0f;
            targetAlpha = Random.Range(minAlpha, maxAlpha);
        }

        Color c = glow.color;
        c.a = Mathf.Lerp(c.a, targetAlpha, smoothing * Time.deltaTime);
        glow.color = c;
    }
}
