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

    // Level 3: the three cipher pages, so they can be put back when the guard catches Alex
    public GameObject[] cipherPages;

    // Awake runs before every Start, so the inventory already sees these values when it fills its slots
    void Awake()
    {
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
        InventoryManager inventory = FindObjectOfType<InventoryManager>();

        for (int i = 0; i < cipherPages.Length; i++)
        {
            cipherPages[i].SetActive(true); //the page appears again in its hiding place

            //and it is taken out of the inventory
            CipherPage page = cipherPages[i].GetComponent<CipherPage>();
            if (inventory != null && page != null && page.inventoryIcon != null)
                inventory.RemoveItem(page.inventoryIcon);
        }
    }
}
