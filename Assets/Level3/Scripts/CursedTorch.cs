using System.Collections;
using UnityEngine;

// A cursed wall torch that breathes fire down onto the walkway every few seconds.
// Warning first: the flame flickers and grows. Then a burst of fire (Particle System, Lab 4)
// hurts Alex if he is under the torch (TakeDamage, Lab 6). The loop is a Coroutine (Lab 9).
public class CursedTorch : MonoBehaviour
{
    public ParticleSystem fireBurst;

    public float startDelay = 0f;
    public float pauseTime = 2.2f;        // time between two bursts
    public float escapePauseTime = 0.9f;  // shorter pause during the escape at the end of the level
    public float warningTime = 0.7f;
    public float burstTime = 1f;

    // the area under the torch that the fire covers (seen as a red box in the Scene view)
    public Vector2 fireOffset = new Vector2(0f, -0.9f);
    public Vector2 fireSize = new Vector2(0.7f, 1.2f);
    public int damage = 1;
    public float knockUp = 4f;

    // sounds play on the torch's own AudioSource and are quieter further away (like the guard's steps)
    public AudioSource torchAudio;
    public AudioClip warnSound;
    public AudioClip breathSound;
    public float hearingDistance = 7f;

    public bool escapeMode = false;       // set by the EscapeSequence

    private bool burning = false;
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
            // pause between bursts
            if (escapeMode)
                yield return new WaitForSeconds(escapePauseTime);
            else
                yield return new WaitForSeconds(pauseTime);

            // warning: the flame flickers and grows
            PlaySound(warnSound);
            float timer = 0f;
            while (timer < warningTime)
            {
                timer = timer + Time.deltaTime;
                float grow = 1f + 0.15f * (timer / warningTime) + Random.Range(-0.04f, 0.04f);
                transform.localScale = startScale * grow;
                sr.color = Color.Lerp(Color.white, new Color(1f, 0.6f, 0.35f), Random.Range(0f, 1f));
                yield return null;
            }
            transform.localScale = startScale;
            sr.color = Color.white;

            // the torch breathes fire
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
        // while the fire burns, check if Alex is inside it (same idea as the spike trap)
        if (burning)
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll((Vector2)transform.position + fireOffset, fireSize, 0f);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].tag == "Player")
                {
                    PlayerStats stats = hits[i].GetComponent<PlayerStats>();
                    if (stats != null && stats.isImmune == false)
                    {
                        Rigidbody2D rb = hits[i].GetComponent<Rigidbody2D>();
                        rb.velocity = new Vector2(rb.velocity.x, knockUp);
                        stats.TakeDamage(damage);
                    }
                    break;
                }
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

    // shows the fire area in the Scene view when the torch is selected
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + (Vector3)fireOffset, new Vector3(fireSize.x, fireSize.y, 0));
    }
}
