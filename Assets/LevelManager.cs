using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public GameObject CurrentCheckpoint;
    public AudioClip respawnSound; //only set in level 2, which has the Audio Manager
    public bool restartLevel = false; //checked in level 2: when alex dies the whole level starts again
    public string levelName = "level 0"; //the scene that is loaded again when restartLevel is checked
    public bool clearLevel2Items = true; //level 2 loses its items when it restarts. Level 5 keeps the symbols

    public static bool justRespawned = false; //static so it is still true after the level is loaded again (like the statics in lab 7)

    // Start is called before the first frame update
    void Start()
    {
        //alex just died, so the background music waits for the respawn sound to finish
        if (justRespawned == true)
        {
            justRespawned = false;
            StartCoroutine(MusicAfterRespawn());
        }
    }

    //the respawn sound is about 4 seconds, then the background music starts again
    IEnumerator MusicAfterRespawn()
    {
        yield return new WaitForSeconds(4.2f);
        AudioManager.Instance.musicSource.enabled = true;
        AudioManager.Instance.PlayMusic(AudioManager.Instance.overworldMusic); //like lab 7
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void RespawnPlayer()
    {
        //Level 5 has its own restart (death screen, then the whole level from the beginning)
        if (Level5Manager.Instance != null)
        {
            Level5Manager.Instance.FailLevel("Alex has lost all his health...");
            return;
        }

        if (respawnSound != null)
        {
            AudioManager.Instance.musicSource.enabled = false; //the background music is turned off so the respawn sound is heard (like sr.enabled in lab 6)
            AudioManager.Instance.PlayMusicSFX(respawnSound);
        }

        if (restartLevel == true)
        {
            //the items of this level are lost, and the calendar and the safe are not solved any more
            if (clearLevel2Items == true)
            {
                PlayerStats.hasTriangle = false;
                PlayerStats.hasLibraryPermit = false;
            }
            justRespawned = true; //the new level starts the music after the respawn sound
            SceneManager.LoadScene(levelName); //load the level again from the beginning
            return;
        }

        FindObjectOfType<PlayerController>().transform.position=CurrentCheckpoint.transform.position;
    }
}
