using System.Collections;
using UnityEngine;

public class CursedTorch : MonoBehaviour
{
    public ParticleSystem fireBurst;
    public TrapHitbox fireHitbox;

    public float startDelay = 0f;
    public float pauseTime = 2.2f;
    public float escapePauseTime = 0.9f;
    public float warningTime = 0.7f;
    public float burstTime = 1f;

    public int damage = 1;
    public float knockUp = 4f;

    public AudioSource torchAudio;
    public AudioClip warnSound;
    public AudioClip breathSound;
    public float hearingDistance = 7f;

    public bool escapeMode = false;

    private bool warning = false;
    private bool burning = false;
    private float warningTimer = 0f;
    private SpriteRenderer sr;
    private Vector3 startScale;
    private Transform alex;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
        alex = FindObjectOfType<PlayerController>().transform;
        StartCoroutine(BreatheFire());
    }

    IEnumerator BreatheFire()
    {
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            if (escapeMode)
                yield return new WaitForSeconds(escapePauseTime);
            else
                yield return new WaitForSeconds(pauseTime);

            PlaySound(warnSound);
            warningTimer = 0f;
            warning = true;
            yield return new WaitForSeconds(warningTime);
            warning = false;
            transform.localScale = startScale;
            sr.color = Color.white;

            PlaySound(breathSound);
            burning = true;
            fireBurst.Play();
            yield return new WaitForSeconds(burstTime);
            fireBurst.Stop();
            burning = false;
        }
    }

    void Update()
    {
        if (warning)
        {
            warningTimer = warningTimer + Time.deltaTime;
            float grow = 1f + 0.15f * (warningTimer / warningTime) + Random.Range(-0.04f, 0.04f);
            transform.localScale = startScale * grow;

            float glow = Random.Range(0f, 1f);
            sr.color = new Color(1f, Mathf.Lerp(1f, 0.6f, glow), Mathf.Lerp(1f, 0.35f, glow));
        }

        if (burning && fireHitbox.playerInside)
        {
            PlayerStats stats = fireHitbox.player.GetComponent<PlayerStats>();
            if (stats.isImmune == false)
            {
                Rigidbody2D rb = fireHitbox.player.GetComponent<Rigidbody2D>();
                rb.velocity = new Vector2(rb.velocity.x, knockUp);
                stats.TakeDamage(damage);
            }
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (torchAudio == null || clip == null) return;

        float distance = Vector2.Distance(transform.position, alex.position);
        float volume = 1f - distance / hearingDistance;
        if (volume > 0)
            torchAudio.PlayOneShot(clip, volume);
    }
}
