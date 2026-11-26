using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrescentMoonCollectible : MonoBehaviour
{
    public AudioClip moonSound;
    public GameObject inventoryIcon; // the moon's icon in the inventory (press H)

   private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            PlayerStats.hasCrescentMoon = true; //static, so it is kept even when the guard catches Alex

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMusicSFX(moonSound);

            Debug.Log("Crescent Moon collected!");

            // the moon also goes into the inventory
            InventoryManager inventory = FindObjectOfType<InventoryManager>();
            if (inventory != null && inventoryIcon != null)
                inventory.AddItem(inventoryIcon);

            Destroy(gameObject);
        }
    }
}
