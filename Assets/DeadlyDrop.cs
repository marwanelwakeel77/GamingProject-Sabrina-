using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadlyDrop : MonoBehaviour
{
    public AudioClip deathSound; //only set in level 2, which has the Audio Manager

    private bool isDying = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //when the player falls, the level manager respawns him (like the lake in lab 5)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && isDying == false)
        {
            //level 2: the health bar empties, the death sound plays, then alex respawns
            if (deathSound != null)
            {
                isDying = true;
                FindObjectOfType<PlayerStats>().healthBar.fillAmount = 0; //the health bar is empty (like lab 8)
                AudioManager.Instance.musicSource.enabled = false; //the background music is turned off while the death sound plays (like sr.enabled in lab 6)
                AudioManager.Instance.PlayMusicSFX(deathSound); //like the sounds in lab 7
                StartCoroutine(RespawnAfterDeath());
            }
            else
            {
                FindObjectOfType<LevelManager>().RespawnPlayer();
            }
        }
    }

    //wait until the death sound is heard, then respawn (the respawn sound plays in the level manager)
    IEnumerator RespawnAfterDeath()
    {
        yield return new WaitForSeconds(2.5f);
        isDying = false;
        FindObjectOfType<LevelManager>().RespawnPlayer();
    }
}
