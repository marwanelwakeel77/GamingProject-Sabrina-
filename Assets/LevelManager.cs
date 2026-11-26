using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject CurrentCheckpoint;

    // Level 3: the three cipher pages, so they can be put back when the guard catches Alex
    public GameObject[] cipherPages;

    // Start is called before the first frame update
    void Start()
    {
        CurrentCheckpoint=null;

        // Level 3 starts with no pages and no moon
        if (cipherPages.Length > 0)
        {
            PlayerStats.cipherPagesCollected = 0;
            PlayerStats.hasCrescentMoon = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void RespawnPlayer()
    {
        if (CurrentCheckpoint != null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            player.transform.position=CurrentCheckpoint.transform.position;
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero; //don't keep falling or running after respawning
        }
    }

    // Level 3 rule: when the guard catches Alex, he loses every cipher page he collected
    public void ResetCipherPages()
    {
        PlayerStats.cipherPagesCollected = 0;
        for (int i = 0; i < cipherPages.Length; i++)
        {
            cipherPages[i].SetActive(true); //the page appears again in its hiding place
        }
    }
}
