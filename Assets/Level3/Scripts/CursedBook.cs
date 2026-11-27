using UnityEngine;

// The cursed flying book on the upper floor.
// It is an enemy like the walking enemy in Lab 6 (Exercise #20): it inherits from the EnemyController
// super class and moves along the x-axis in FixedUpdate. The super class does the rest:
// when it touches Alex he takes damage, and when it touches Alex or a "Wall" it flips and flies back.
// Its Rigidbody2D has gravity 0 (like the bullet in Lab 9), so it flies instead of falling.
public class CursedBook : EnemyController
{
    public float bobHeight = 0.12f;   // how far it floats up and down while flying
    public float bobSpeed = 3f;

    public AudioSource flutterSource; // looping flutter sound on the book (Lab 7)
    public float hearingDistance = 6f;

    private Rigidbody2D rb;
    private Transform alex;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        alex = FindObjectOfType<PlayerController>().transform;
    }

    void FixedUpdate()
    {
        // fly left or right depending on which way the book is turned (like the walking enemy)
        float direction = 1f;
        if (sr.flipX == true)
            direction = -1f;

        // float gently up and down
        float bob = Mathf.Cos(Time.time * bobSpeed) * bobHeight * bobSpeed;

        rb.velocity = new Vector2(direction * maxSpeed, bob);
    }

    void Update()
    {
        // the flutter is quieter the further away Alex is (like the guard's footsteps)
        if (flutterSource != null)
        {
            float distance = Vector2.Distance(transform.position, alex.position);
            flutterSource.volume = Mathf.Clamp01(1f - distance / hearingDistance) * 0.5f;
        }
    }
}
