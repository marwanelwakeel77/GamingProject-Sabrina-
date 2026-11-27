using System.Collections;
using UnityEngine;

public class HiddenPressurePlate : MonoBehaviour
{
    [Header("Plate (the tile in the floor)")]
    public SpriteRenderer plateRenderer;
    public Sprite normalSprite;
    public Sprite warningSprite;
    public Sprite emergingSprite;
    public Sprite activeSprite;
    [Tooltip("The plate with an empty hole, shown while the big spikes are out of it")]
    public Sprite openSprite;
    public float frameTime = 0.12f;

    [Header("Spikes that rise out of the plate")]
    public Transform spikes;
    public float riseSpeed = 3f;
    public float retractSpeed = 1.5f;
    public float riseHeight = 0f;

    [Header("Timing (seconds)")]
    public float warningTime = 0.7f;
    public float activeTime = 2f;
    public float cooldownTime = 1f;

    [Header("Warning shake")]
    public float shakeAmount = 0.012f;

    [Header("Damage")]
    public int damage = 1;
    public Vector2 hitboxSize = new Vector2(0.36f, 0.3f);
    public float knockUp = 6f;

    [Header("Effects (optional)")]
    public ParticleSystem dust;
    public AudioClip activationSound;
    public AudioClip spikesUpSound;
    public AudioClip spikesDownSound;

    public bool autoRepeat = false;

    private bool trapRunning = false;
    private bool spikesDangerous = false;
    private bool hitThisTime = false;
    private Vector3 plateStartPosition;
    private Vector3 spikesUpPosition;
    private Vector3 spikesDownPosition;

    void Start()
    {
        if (plateRenderer == null)
        {
            Debug.LogError("HiddenPressurePlate: the Plate Renderer is not assigned in the Inspector.", this);
            enabled = false;
            return;
        }

        plateStartPosition = plateRenderer.transform.localPosition;
        plateRenderer.sprite = normalSprite;

        if (spikes != null)
        {
            spikesDownPosition = spikes.localPosition;
            spikesUpPosition = spikesDownPosition + new Vector3(0, GetRiseHeight(), 0);
        }
        else
        {
            Debug.LogWarning("HiddenPressurePlate: no Spikes assigned, the plate will only change sprites.", this);
        }
    }

    void Update()
    {
        if (autoRepeat)
            TryActivate();

        if (spikesDangerous && !hitThisTime && spikes != null)
        {
            Vector2 center = (Vector2)spikes.position + new Vector2(0, hitboxSize.y / 2);
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, hitboxSize, 0f);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].tag == "Player")
                {
                    HitPlayer(hits[i].gameObject);
                    break;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
            TryActivate();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player")
            TryActivate();
    }

    void TryActivate()
    {
        if (trapRunning)
            return;

        StartCoroutine(TrapSequence());
    }

    IEnumerator TrapSequence()
    {
        trapRunning = true;
        hitThisTime = false;
        Debug.Log("Pressure plate: stepped on!");

        plateRenderer.sprite = warningSprite;
        PlaySound(activationSound);

        float timer = 0f;
        while (timer < warningTime)
        {
            timer = timer + Time.deltaTime;
            float offset = Random.Range(-shakeAmount, shakeAmount);
            plateRenderer.transform.localPosition = plateStartPosition + new Vector3(offset, 0, 0);
            yield return null;
        }
        plateRenderer.transform.localPosition = plateStartPosition;

        Debug.Log("Pressure plate: spikes emerging");
        plateRenderer.sprite = emergingSprite;
        PlaySound(spikesUpSound);
        yield return new WaitForSeconds(frameTime);
        plateRenderer.sprite = activeSprite;
        yield return new WaitForSeconds(frameTime);

        if (dust != null) dust.Play();
        if (openSprite != null)
            plateRenderer.sprite = openSprite;

        if (spikes != null)
        {
            while (spikes.localPosition != spikesUpPosition)
            {
                spikes.localPosition = Vector3.MoveTowards(spikes.localPosition, spikesUpPosition, riseSpeed * Time.deltaTime);
                yield return null;
            }
        }

        Debug.Log("Pressure plate: spikes active");
        spikesDangerous = true;
        yield return new WaitForSeconds(activeTime);
        spikesDangerous = false;

        Debug.Log("Pressure plate: spikes retracting");
        PlaySound(spikesDownSound);
        if (dust != null) dust.Play();

        if (spikes != null)
        {
            while (spikes.localPosition != spikesDownPosition)
            {
                spikes.localPosition = Vector3.MoveTowards(spikes.localPosition, spikesDownPosition, retractSpeed * Time.deltaTime);
                yield return null;
            }
        }

        plateRenderer.sprite = activeSprite;
        yield return new WaitForSeconds(frameTime);
        plateRenderer.sprite = emergingSprite;
        yield return new WaitForSeconds(frameTime);

        plateRenderer.sprite = normalSprite;
        yield return new WaitForSeconds(cooldownTime);

        trapRunning = false;
        Debug.Log("Pressure plate: ready again");
    }

    void HitPlayer(GameObject player)
    {
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats == null)
        {
            Debug.LogWarning("HiddenPressurePlate: the Player has no PlayerStats component.", this);
            return;
        }

        if (stats.isImmune)
            return;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = new Vector2(rb.velocity.x, knockUp);

        hitThisTime = true;
        Debug.Log("Pressure plate: Alex hit the spikes!");
        stats.TakeDamage(damage);
    }

    void PlaySound(AudioClip clip)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(clip);
    }

    float GetRiseHeight()
    {
        if (riseHeight > 0)
            return riseHeight;
        return spikes.GetComponent<SpriteRenderer>().bounds.size.y + 0.05f;
    }

    void OnDrawGizmosSelected()
    {
        if (spikes == null) return;
        Vector3 center = spikes.position + new Vector3(0, hitboxSize.y / 2, 0);
        if (!Application.isPlaying)
            center += new Vector3(0, GetRiseHeight(), 0);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, new Vector3(hitboxSize.x, hitboxSize.y, 0));
    }
}
