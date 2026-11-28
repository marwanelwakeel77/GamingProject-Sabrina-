using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//put this on the bookshelf. While alex stands in front of it, a book falls on him every 5 seconds
//(the trigger works like the spawn point in lab 6 exercise 21, and Instantiate is used like the bullet in lab 9)
public class FallingBooks : MonoBehaviour
{
    public GameObject bookPrefab; //Assets/prefab/Falling Book
    public Transform dropPoint; //the top of the bookshelf, where the books fall from
    public float dropTime = 5f; //a book falls every 5 seconds

    private float timer = 0f;
    private bool playerNear = false;
    private Transform player;

    // Update is called once per frame
    void Update()
    {
        if (playerNear == true && player != null)
        {
            timer = timer + Time.deltaTime;
            if (timer >= dropTime)
            {
                timer = 0f;
                DropBook();
            }
        }
    }

    void DropBook()
    {
        //the book falls from the top of the bookshelf, right above alex
        Vector3 position = new Vector3(player.position.x, dropPoint.position.y, 0);
        Instantiate(bookPrefab, position, dropPoint.rotation);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNear = true;
            player = other.transform;
            timer = dropTime - 1f; //the first book falls 1 second after alex comes near the bookshelf
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNear = false;
        }
    }
}
