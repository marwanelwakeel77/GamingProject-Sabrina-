using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//the bullet of alex (lab 9). Rigidbody2D with gravity 0 and a trigger collider
public class PlayerBullet : MonoBehaviour
{
    public float speed = 16;
    public PlayerShooting player; //the shooting script on alex
    public int damage = 1;
    public GameObject impactEffect; //small particles when the bullet hits (lab 4)
    public AudioClip impactSound;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float timer = 0;
    private float lifeTime = 2;

    // Start is called before the first frame update
    void Start()
    {
        //the sprite renderer and rigidbody of the bullet, and the script on the player that shoots it
        player = FindObjectOfType<PlayerShooting>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        //the bullet and alex look the same way
        sr.flipX = player.sr.flipX;
    }

    // Update is called once per frame
    void Update()
    {
        if (sr.flipX)
        {
            rb.velocity = new Vector2(-speed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);
        }

        //the bullet is destroyed after a few seconds so it doesn't travel forever
        if ((timer += Time.deltaTime) >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    //damage to the enemy
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Enemy")
        {
            other.GetComponent<GhostController>().EnemyTakeDamage(damage);
            Hit();
        }

        if (other.tag == "Wall")
        {
            Hit();
        }
    }

    void Hit()
    {
        GameObject effect = Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(effect, 1f);
        Level5Manager.Instance.PlaySound(impactSound);
        Destroy(gameObject);
    }
}
