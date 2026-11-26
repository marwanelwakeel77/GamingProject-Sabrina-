using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallPickup : MonoBehaviour
{
    public CalendarPuzzle requiredPuzzle; //the calendar must be solved first
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
        //the symbol can be collected only after the calendar is solved and closed
        if (playerNear == true && requiredPuzzle.isSolved == true && requiredPuzzle.calendarOpen == false)
        {
            collectText.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                Collect();
            }
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
