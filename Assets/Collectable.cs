using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
 public KeyCode pickupKey = KeyCode.E;
    public GameObject prompt;   // optional "Press E" hint

    private bool playerInRange;

    void Start()
    {
        if (prompt != null) prompt.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(pickupKey))
        {
            InventoryUI.Instance.AddKey();
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (prompt != null) prompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (prompt != null) prompt.SetActive(false);
        }
    }
}