using System.Collections;
using System.Collections.Generic;
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
    private float moveInput;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Read input every frame. No key held = 0, so the character stops as soon as the arrow is released.
        moveInput = 0f;
        if (Input.GetKey(L)) moveInput = -1f;
        if (Input.GetKey(R)) moveInput = 1f;

        // Face the direction of movement; keep the last facing when standing still.
        if (moveInput != 0f && sr != null)
            sr.flipX = moveInput < 0f;

        if (Input.GetKeyDown(Spacebar) && grounded)
            Jump();

        anim.SetFloat("Speed", Mathf.Abs(moveInput));
        anim.SetFloat("Height", rb.velocity.y);
        anim.SetBool("Grounded", grounded);
    }

    void FixedUpdate()
    {
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        // Set horizontal speed every physics step, so releasing the key sets it back to 0.
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
    }
}