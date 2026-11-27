using UnityEngine;

public class FallingBook : EnemyController
{
    public float minPush = 0.4f;
    public float maxPush = 1.2f;
    public float lifeTime = 3f;
    public AudioClip landSound;
    public ParticleSystem landingDust;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.velocity = new Vector2(Random.Range(minPush, maxPush), 0f);
        rb.angularVelocity = Random.Range(-250f, 250f);

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            FindObjectOfType<PlayerStats>().TakeDamage(damage);
            Land();
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Land();
        }
    }

    void Land()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(landSound);

        if (landingDust != null)
        {
            landingDust.transform.position = transform.position;
            landingDust.Play();
        }

        Destroy(gameObject);
    }
}
