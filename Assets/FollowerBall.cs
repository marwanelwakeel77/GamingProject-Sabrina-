using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//the cursed ball that follows alex (lab 6, exercise 22)
public class FollowerBall : EnemyController
{
    public Transform player;
    public AudioClip damageSound; //plays when the ball hurts alex

    private bool touchingPlayer = false;

    void FixedUpdate()
    {
        //stop if the player is gone (game over)
        if (player != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, maxSpeed * Time.deltaTime);
        }

        //keeps hurting alex while the ball is touching him, so he must keep running
        //(PlayerStats makes him immune for 1.5 seconds after each hit, so he loses 1 health every 1.5 seconds)
        if (touchingPlayer == true && player != null)
        {
            PlayerStats stats = FindObjectOfType<PlayerStats>();

            //the sound plays only when alex really loses health (not while he is immune)
            if (stats.isImmune == false)
            {
                AudioManager.Instance.PlayRandomSFX(damageSound);
            }

            stats.TakeDamage(damage);
        }
    }

    //Since the ball is not supposed to flip if it collides with player, the Trigger function will be overridden as follows:
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            touchingPlayer = true;
        }
        else if (other.tag == "Wall")
        {
            Flip();
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
