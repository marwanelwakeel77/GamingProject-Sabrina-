using System.Collections;
using UnityEngine;

// A hidden pressure plate in the library floor.
// Alex steps on it -> the plate glows red and shakes (warning) -> spikes rise -> they stay up for a while
// -> they go back down -> the plate is normal again and can be triggered again.
//
// Built only with things from the labs:
//   sprite swap between the 4 plate sprites (Lab 5, BrickBreak), the whole sequence in a Coroutine (Lab 9),
//   MoveTowards to move the spikes (Lab 5 moving platform / Lab 6), a particle burst for dust (Lab 4),
//   damage through PlayerStats.TakeDamage (Lab 6) and sounds through the AudioManager (Lab 7).
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
    public float frameTime = 0.12f;   // how long the Emerging and Active plate sprites show before the spikes shoot up

    [Header("Spikes that rise out of the plate")]
    public Transform spikes;
    public float riseSpeed = 3f;      // units per second
    public float retractSpeed = 1.5f;

    [Header("Timing (seconds)")]
    public float warningTime = 0.7f;  // fair warning before the spikes come up
    public float activeTime = 2f;     // how long the spikes stay up and dangerous
    public float cooldownTime = 1f;   // wait before the plate can be triggered again

    [Header("Warning shake")]
    public float shakeAmount = 0.012f;

    [Header("Damage")]
    public int damage = 1;
    public Vector2 hitboxSize = new Vector2(0.36f, 0.3f); // area of the raised spikes that hurts Alex
    public float knockUp = 6f;                              // small bounce when Alex is hit

    [Header("Effects (optional)")]
    public ParticleSystem dust;
    public AudioClip activationSound;   // click of the plate
    public AudioClip spikesUpSound;
    public AudioClip spikesDownSound;

    private bool trapRunning = false;     // true while the sequence plays, so it can't start twice
    private bool spikesDangerous = false; // true only while the spikes are fully up
    private bool hitThisTime = false;     // the spikes hurt Alex only once each time they rise
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
            // the spikes wait hidden below the floor in the scene; they rise by their own height
            spikesDownPosition = spikes.localPosition;
            float spikeHeight = spikes.GetComponent<SpriteRenderer>().bounds.size.y;
            spikesUpPosition = spikesDownPosition + new Vector3(0, spikeHeight + 0.05f, 0);
        }
        else
        {
            Debug.LogWarning("HiddenPressurePlate: no Spikes assigned, the plate will only change sprites.", this);
        }
    }

    void Update()
    {
        // while the spikes are up, check if Alex is touching them
        // (same idea as the ground check in the PlayerController: an overlap test every frame)
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

    // Alex steps on the plate
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
            TryActivate();
    }

    // Alex is still standing on the plate when it is ready again
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player")
            TryActivate();
    }

    void TryActivate()
    {
        if (trapRunning)
            return; // the trap is already running

        StartCoroutine(TrapSequence());
    }

    IEnumerator TrapSequence()
    {
        trapRunning = true;
        hitThisTime = false;
        Debug.Log("Pressure plate: stepped on!");

        // 1) WARNING: red glow and a small shake
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

        // 2) EMERGING: the spikes start to come out of the plate (Emerging, then Active sprite)
        Debug.Log("Pressure plate: spikes emerging");
        plateRenderer.sprite = emergingSprite;
        PlaySound(spikesUpSound);
        yield return new WaitForSeconds(frameTime);
        plateRenderer.sprite = activeSprite;
        yield return new WaitForSeconds(frameTime);

        // the spikes shoot up out of the plate, leaving the hole open
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

        // 3) ACTIVE: spikes fully up and dangerous
        Debug.Log("Pressure plate: spikes active");
        spikesDangerous = true;
        yield return new WaitForSeconds(activeTime);
        spikesDangerous = false;

        // 4) RETRACTING: the spikes go back down into the plate
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

        // the spikes sink back into the plate (Active, then Emerging sprite)
        plateRenderer.sprite = activeSprite;
        yield return new WaitForSeconds(frameTime);
        plateRenderer.sprite = emergingSprite;
        yield return new WaitForSeconds(frameTime);

        // 5) NORMAL again, then a short cooldown before it can be triggered again
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

        // PlayerStats already gives Alex a short immunity after a hit, so he isn't hit every frame
        if (stats.isImmune)
            return;

        // small bounce up off the spikes
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = new Vector2(rb.velocity.x, knockUp);

        hitThisTime = true;
        Debug.Log("Pressure plate: Alex hit the spikes!");
        stats.TakeDamage(damage); // existing health, lives and checkpoint respawn (Lab 6)
    }

    void PlaySound(AudioClip clip)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(clip);
    }

    // shows the spike hitbox in the Scene view when the plate is selected
    void OnDrawGizmosSelected()
    {
        if (spikes == null) return;
        Vector3 center = spikes.position + new Vector3(0, hitboxSize.y / 2, 0);
        if (!Application.isPlaying) // in the editor the spikes are still hidden, so draw where they will rise to
            center += new Vector3(0, spikes.GetComponent<SpriteRenderer>().bounds.size.y + 0.05f, 0);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, new Vector3(hitboxSize.x, hitboxSize.y, 0));
    }
}
