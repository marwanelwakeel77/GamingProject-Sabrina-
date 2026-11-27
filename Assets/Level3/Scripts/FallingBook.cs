using UnityEngine;

// One book that falls off the bookshelf (Falling Books trap).
// Like the ghost in Lab 6 (Exercise #21) it inherits from the EnemyController super class,
// and like the bullet in Lab 9 (Exercise #27) it has a trigger collider and a Rigidbody2D
// and disappears when it hits something. Here gravity is ON (Lab 4/5: Dynamic body), so it falls.
public class FallingBook : EnemyController
{
    public float minPush = 0.4f;       // small push off the shelf (to the right)
    public float maxPush = 1.2f;
    public float lifeTime = 3f;        // disappears by itself after a few seconds (like the bullet)
    public AudioClip landSound;
    public ParticleSystem landingDust; // given by the trap when the book is created

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // every book falls a bit differently: a random push and a random spin
        rb.velocity = new Vector2(Random.Range(minPush, maxPush), 0f);
        rb.angularVelocity = Random.Range(-250f, 250f);

        Destroy(gameObject, lifeTime);
    }

    // overrides the super class's trigger function (like GhostEnemy in Lab 6):
    // a book doesn't turn around at walls, it hurts Alex or lands on the floor
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            FindObjectOfType<PlayerStats>().TakeDamage(damage); // Lab 6
            Land();
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Land(); // it hit the floor
        }
    }

    void Land()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(landSound); // Lab 7

        if (landingDust != null)
        {
            landingDust.transform.position = transform.position;
            landingDust.Play(); // Lab 4 particle system
        }

        Destroy(gameObject);
    }
}
