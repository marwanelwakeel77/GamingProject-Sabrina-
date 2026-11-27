using UnityEngine;

public class AlexFootsteps : MonoBehaviour
{
    public AudioClip[] footsteps;
    public float stepTime = 0.3f;

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
            stepTimer = 0f;
        }
    }
}
