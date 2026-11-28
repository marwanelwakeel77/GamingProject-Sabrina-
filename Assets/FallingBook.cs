using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//put this on the Falling Book prefab (it works like the bullet in lab 9)
public class FallingBook : MonoBehaviour
{
    public int damage = 1; //alex loses 1 health
    public AudioClip hitSound;
    public float lifeTime = 3f; //the book disappears if it did not hit anything after 3 seconds

    private float timer = 0f;

    // Update is called once per frame
    void Update()
    {
        timer = timer + Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    //like the OnCollisionEnter2D in lab 5
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerStats stats = FindObjectOfType<PlayerStats>();

            //the sound plays only when alex really loses health (not while he is immune)
            if (stats.isImmune == false)
            {
                AudioManager.Instance.PlayRandomSFX(hitSound);
            }

            stats.TakeDamage(damage); //like the enemies in lab 6
        }

        Destroy(gameObject); //the book disappears when it hits alex or the floor
    }
}
