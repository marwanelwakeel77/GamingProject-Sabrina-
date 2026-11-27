using System.Collections;
using UnityEngine;

public class HiddenPressurePlate : MonoBehaviour
{
    public SpriteRenderer plateRenderer;
    public Sprite normalSprite;
    public Sprite warningSprite;
    public Sprite emergingSprite;
    public Sprite activeSprite;
    public Sprite openSprite;
    public float frameTime = 0.12f;

    public Transform spikes;
    public TrapHitbox spikesHitbox;
    public float riseHeight = 0.5f;
    public float riseSpeed = 3f;
    public float retractSpeed = 1.5f;

    public float warningTime = 0.7f;
    public float activeTime = 2f;
    public float cooldownTime = 1f;
    public float shakeAmount = 0.012f;

    public int damage = 1;
    public float knockUp = 6f;

    public ParticleSystem dust;
    public AudioClip activationSound;
    public AudioClip spikesUpSound;
    public AudioClip spikesDownSound;

    public bool autoRepeat = false;

    private bool playerOnPlate = false;
    private bool trapRunning = false;
    private bool shaking = false;
    private bool spikesDangerous = false;
    private bool hitThisTime = false;
    private float spikesSpeed = 0f;
    private Vector3 plateStartPosition;
    private Vector3 spikesDownPosition;
    private Vector3 spikesUpPosition;
    private Vector3 spikesTarget;

    void Start()
    {
        plateStartPosition = plateRenderer.transform.localPosition;
        plateRenderer.sprite = normalSprite;

        spikesDownPosition = spikes.localPosition;
        spikesUpPosition = spikesDownPosition + new Vector3(0, riseHeight, 0);
        spikesTarget = spikesDownPosition;
    }

    void Update()
    {
        if (autoRepeat || playerOnPlate)
            TryActivate();

        if (shaking)
        {
            float offset = Random.Range(-shakeAmount, shakeAmount);
            plateRenderer.transform.localPosition = plateStartPosition + new Vector3(offset, 0, 0);
        }

        spikes.localPosition = Vector3.MoveTowards(spikes.localPosition, spikesTarget, spikesSpeed * Time.deltaTime);

        if (spikesDangerous && !hitThisTime && spikesHitbox.playerInside)
            HitPlayer(spikesHitbox.player);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
            playerOnPlate = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
            playerOnPlate = false;
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

        shaking = true;
        yield return new WaitForSeconds(warningTime);
        shaking = false;
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

        spikesSpeed = riseSpeed;
        spikesTarget = spikesUpPosition;
        yield return new WaitForSeconds(riseHeight / riseSpeed);

        Debug.Log("Pressure plate: spikes active");
        spikesDangerous = true;
        yield return new WaitForSeconds(activeTime);
        spikesDangerous = false;

        Debug.Log("Pressure plate: spikes retracting");
        PlaySound(spikesDownSound);
        if (dust != null) dust.Play();

        spikesSpeed = retractSpeed;
        spikesTarget = spikesDownPosition;
        yield return new WaitForSeconds(riseHeight / retractSpeed);

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
        if (stats.isImmune)
            return;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
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
}
