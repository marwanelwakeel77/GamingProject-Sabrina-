using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public float jumpHeight;

    public KeyCode Spacebar;
    public KeyCode L;
    public KeyCode R;
    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask whatIsGround;

    private bool grounded;
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Input.GetKeyDown(Spacebar) && grounded)
        {
            Jump();
        }

        if (Input.GetKey(L))
        {
            rb.velocity = new Vector2(-moveSpeed, rb.velocity.y);
            if (sr != null) sr.flipX = true;
        }
        else if (Input.GetKey(R))
        {
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
            if (sr != null) sr.flipX = false;
        }
        else
        {
            // No key held: stop horizontal movement so Speed returns to 0
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }

        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        anim.SetFloat("Height", rb.velocity.y);
        anim.SetBool("Grounded", grounded);
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
    }

    void FixedUpdate()
    {
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
    }
}