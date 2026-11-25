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

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void RespawnPlayer()
    {
        if (respawnSound != null)
        {
            AudioManager.Instance.PlayMusicSFX(respawnSound);
        }

        if (restartLevel == true)
        {
            //the items of this level are lost, and the calendar and the safe are not solved any more
            PlayerStats.hasTriangle = false;
            PlayerStats.hasLibraryPermit = false;
            SceneManager.LoadScene(levelName); //load the level again from the beginning
            return;
        }

        FindObjectOfType<PlayerController>().transform.position=CurrentCheckpoint.transform.position;
    }
}
