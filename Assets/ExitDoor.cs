using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{
    public int nextLevel = 3; //the number of Level 3 (the library) in File > Build Settings
    public GameObject interactText; //"Press E To Interact" in the Canvas

    private bool playerNear = false;

    // Update is called once per frame
    void Update()
    {
        if (playerNear == true && Input.GetKeyDown(KeyCode.E))
        {
            //the door opens only if the player has the Library Permit
            if (PlayerStats.hasLibraryPermit == true)
            {
                SceneManager.LoadScene(nextLevel);
            }
            else
            {
                FindObjectOfType<HintText>().ShowHint("The door is locked. You need the Library Permit.");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNear = true;
            interactText.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNear = false;
            interactText.SetActive(false);
        }
    }
}
