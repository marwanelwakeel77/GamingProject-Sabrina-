using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playercontroller : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpHeight = 8f;

    public KeyCode Spacebar = KeyCode.Space;
    public KeyCode L = KeyCode.A;
    public KeyCode R = KeyCode.D;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask WhatIsGround;

    private bool grounded;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if (rb == null)
            Debug.LogError("Add Rigidbody2D to the player!", this);

        if (groundCheck == null)
            Debug.LogError("Assign GroundCheck in the Inspector!", this);
    }

    void Update()
    {
        if (Input.GetKeyDown(Spacebar) && grounded)
        {
            Jump();
        }

        if (Input.GetKey(L) && spriteRenderer != null)
            spriteRenderer.flipX = true;

        if (Input.GetKey(R) && spriteRenderer != null)
            spriteRenderer.flipX = false;
             anim.SetFloat("Speed",Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x));
             anim.SetFloat("Height", GetComponent<Rigidbody2D>().velocity.y);
             anim.SetBool("Grounded", grounded);

    }


    void FixedUpdate()
    {
        if (rb == null)
            return;

        float horizontal = 0f;

        if (Input.GetKey(L))
            horizontal = -1f;
        else if (Input.GetKey(R))
            horizontal = 1f;

        rb.velocity = new Vector2(
            horizontal * moveSpeed,
            rb.velocity.y
        );

        if (groundCheck != null)
        {
            grounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                WhatIsGround
            ) != null;
        }
        else
        {
            grounded = false;
        }
    }

    void Jump()
    {
        if (rb != null)
            rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
    }
}
