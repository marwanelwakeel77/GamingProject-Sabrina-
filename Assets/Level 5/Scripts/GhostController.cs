using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Sabrina's ghost, an enemy like in lab 6 (it uses maxSpeed, damage and sr from EnemyController).
//It follows alex (like the saw in lab 6) and hurts him once every 3 seconds while it touches him.
//The bullets of alex call EnemyTakeDamage() (lab 9)
public class GhostController : EnemyController
{
    public Animator anim; //bools: Moving, Attacking, Hurt, Dead (like lab 3)
    public Transform player;
    public Transform aimPoint; //the ShootingPoint of alex: the ghost floats at the height of his gun
    public GhostHealthBar healthBar;
    public int health = 10;
    public int maxHealth = 10;
    public float attackTime = 3f; //alex is hurt once every 3 seconds
    public float minX = -21.5f; //the ghost stays in its part of the room
    public float maxX = -2f;

    public AudioClip appearSound;
    public AudioClip whisperSound;
    public AudioClip hitSound;
    public AudioClip deathSound;
    public AudioClip hurtSound; //the damage sound of alex
    public ParticleSystem appearEffect;
    public ParticleSystem deathEffect;

    private bool active = false;
    private bool dead = false;
    private bool touchingPlayer = false;
    private float attackTimer = 0f;
    private float whisperTimer = 5f;

    //called by the Level 5 Manager after the power is back
    public void Appear()
    {
        gameObject.SetActive(true);
        StartCoroutine(AppearSlowly());
    }

    IEnumerator AppearSlowly()
    {
        Level5Manager.Instance.PlaySound(appearSound);
        appearEffect.Play();

        //the ghost fades in (like the Lerp in lab 4)
        float t = 0f;
        while (t < 1f)
        {
            t = t + Time.deltaTime / 1.2f;
            sr.color = new Color(1, 1, 1, Mathf.Lerp(0f, 1f, t));
            yield return null;
        }

        gameObject.tag = "Enemy"; //now the bullets can hit it (lab 9)
        healthBar.ShowBar();
        Level5Manager.Instance.ShowHint("Sabrina's ghost! Press F to shoot it.");
        active = true;
    }

    void FixedUpdate()
    {
        if (active == false || dead == true || Level5Manager.Instance.failing == true)
        {
            return;
        }

        //follow alex at the height of his gun, so the bullets can hit it (MoveTowards like the saw in lab 6)
        float targetX = Mathf.Clamp(player.position.x, minX, maxX); //Clamp like the camera in lab 4
        Vector3 target = new Vector3(targetX, aimPoint.position.y, 0);
        transform.position = Vector3.MoveTowards(transform.position, target, maxSpeed * Time.deltaTime);

        //the picture looks to the right, so it is flipped when alex is on the left
        if (player.position.x < transform.position.x)
        {
            sr.flipX = true;
        }
        else
        {
            sr.flipX = false;
        }

        anim.SetBool("Moving", touchingPlayer == false);

        //a damage timer, so alex is hurt once every 3 seconds and not every frame
        if (touchingPlayer == true)
        {
            attackTimer = attackTimer - Time.deltaTime;
            if (attackTimer <= 0)
            {
                attackTimer = attackTime;
                StartCoroutine(Attack());
            }
        }

        whisperTimer = whisperTimer - Time.deltaTime;
        if (whisperTimer <= 0)
        {
            Level5Manager.Instance.PlaySound(whisperSound);
            whisperTimer = Random.Range(5f, 9f); //like the random sounds in lab 7
        }
    }

    IEnumerator Attack()
    {
        anim.SetBool("Attacking", true);
        yield return new WaitForSeconds(0.35f); //the hand reaches out

        if (touchingPlayer == true && dead == false)
        {
            PlayerStats stats = FindObjectOfType<PlayerStats>();
            if (stats.isImmune == false)
            {
                Level5Manager.Instance.PlaySound(hurtSound);
            }
            stats.TakeDamage(damage); //the health system of alex (lab 6)
        }

        yield return new WaitForSeconds(0.45f);
        anim.SetBool("Attacking", false);
    }

    //called by the bullets (like lab 9)
    public void EnemyTakeDamage(int damage)
    {
        if (active == false || dead == true)
        {
            return;
        }

        health -= damage;
        if (health <= 0)
        {
            health = 0;
        }
        healthBar.SetHealth((float)health / maxHealth);
        Level5Manager.Instance.PlaySound(hitSound);

        //if the ghost is dead
        if (health == 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(Hurt());
        }
    }

    IEnumerator Hurt()
    {
        anim.SetBool("Hurt", true);
        sr.color = new Color(1f, 0.55f, 0.55f, 1f); //red for a moment
        yield return new WaitForSeconds(0.15f);
        anim.SetBool("Hurt", false);
        if (dead == false)
        {
            sr.color = Color.white;
        }
    }

    void Die()
    {
        dead = true;
        active = false;
        gameObject.tag = "Untagged";
        GetComponent<Collider2D>().enabled = false; //no more bullets and no more attacks
        anim.SetBool("Attacking", false);
        anim.SetBool("Hurt", false);
        anim.SetBool("Dead", true);
        Level5Manager.Instance.PlaySound(deathSound);
        deathEffect.Play();
        healthBar.HideBar();
        StartCoroutine(Disappear());
    }

    IEnumerator Disappear()
    {
        yield return new WaitForSeconds(1.4f); //the death animation

        float t = 0f;
        while (t < 1f)
        {
            t = t + Time.deltaTime / 0.8f;
            sr.color = new Color(1, 1, 1, Mathf.Lerp(1f, 0f, t));
            yield return null;
        }

        Level5Manager.Instance.OnGhostDefeated();
        gameObject.SetActive(false);
    }

    //the ghost does not flip when it touches alex, so the Trigger functions are written again (like the ball of level 2)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            touchingPlayer = true;
            if (attackTimer <= 0)
            {
                attackTimer = 0.8f; //a short moment before the first attack
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            touchingPlayer = false;
        }
    }
}
