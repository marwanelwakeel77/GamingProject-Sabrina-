using UnityEngine;

// Alex's footsteps. Put it on the player next to the PlayerController.
// Every few moments while Alex walks on the ground, a random step sound plays through
// the AudioManager's PlayRandomSFX (Lab 7), so it doesn't sound the same every time.
public class PlayerFootsteps : MonoBehaviour
{
    public AudioClip[] footsteps;
    public float stepTime = 0.3f; // seconds between two steps

    private PlayerController controller;
    private Rigidbody2D rb;
    private float stepTimer = 0f;

    void Start()
    {
        controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // same ground check as the PlayerController uses for jumping
        bool grounded = Physics2D.OverlapCircle(controller.groundCheck.position, controller.groundCheckRadius, controller.whatIsGround);
        bool walking = Mathf.Abs(rb.velocity.x) > 0.1f;

        if (grounded && walking && Time.timeScale > 0)
        {
            stepTimer = stepTimer - Time.deltaTime;
            if (stepTimer <= 0)
            {
                stepTimer = stepTime;

                if (AudioManager.Instance != null && footsteps.Length > 0)
                    AudioManager.Instance.PlayRandomSFX(footsteps);
            }
        }
        else
        {
            stepTimer = 0f; // the first step plays as soon as Alex starts walking again
        }
    }
}
