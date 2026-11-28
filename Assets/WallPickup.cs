using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallPickup : MonoBehaviour
{
    public CalendarPuzzle requiredPuzzle; //the calendar next to the symbol
    public GameObject inventoryIcon; //the Triangle Symbol icon in the inventory
    public AudioClip pickupSound;
    public GameObject collectText; //"Press E To Collect" in the Canvas

    private bool playerNear = false;

    // Start is called before the first frame update
    void Start()
    {
        collectText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //the symbol can be collected any time, but not while the calendar is open,
        //and not while the player also stands at the calendar before solving it (there E opens the calendar)
        if (playerNear == true && requiredPuzzle.calendarOpen == false && (requiredPuzzle.isSolved == true || requiredPuzzle.playerNear == false))
        {
            collectText.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                Collect();
            }
        }
        else if (playerNear == true)
        {
            collectText.SetActive(false); //the player is at the calendar now, so E is for the calendar
        }
    }

    void Collect()
    {
        collectText.SetActive(false);
        AudioManager.Instance.PlayRandomSFX(pickupSound); //like the coin pickup in lab 7
        PlayerStats.hasTriangle = true; //saved for level 5
        FindObjectOfType<InventoryManager>().AddItem(inventoryIcon);
        FindObjectOfType<HintText>().ShowHint("You collected the Triangle Symbol.");
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNear = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNear = false;
            collectText.SetActive(false);
        }
    }
}
