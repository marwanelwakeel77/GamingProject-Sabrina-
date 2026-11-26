using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject InventoryPanel;
    public Transform[] slots; //the 4 slots of the inventory
    public GameObject[] itemIcons; //the icons of the items, hidden until the player collects them

    //items Alex keeps for level 5. They are collected in earlier levels and saved in
    //static variables in PlayerStats (lab 7), so every level shows them again at the start
    public GameObject triangleIcon; //Triangle Symbol (level 2)
    public GameObject moonIcon;   //Crescent Moon (level 3)

    private int itemCount = 0; //how many items the player collected
    private bool inventoryOpen = false;

    void Start()
    {
        InventoryPanel.SetActive(false);

        //the inventory is empty at the start
        for (int i = 0; i < itemIcons.Length; i++)
        {
            itemIcons[i].SetActive(false);
        }

        //put back the items Alex already collected in the earlier levels
        if (PlayerStats.hasTriangle == true && triangleIcon != null)
            AddItem(triangleIcon);
        if (PlayerStats.hasCrescentMoon == true && moonIcon != null)
            AddItem(moonIcon);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            inventoryOpen = !inventoryOpen; //if true turn to false, if false turn to true (like Flip in lab 6)
            InventoryPanel.SetActive(inventoryOpen);
        }
    }

    //linked to the OnClick() of the close button of the inventory
    public void CloseInventory()
    {
        inventoryOpen = false;
        InventoryPanel.SetActive(false);
    }

    //puts the icon of the collected item in the next empty slot
    public void AddItem(GameObject icon)
    {
        if (itemCount < slots.Length)
        {
            icon.transform.SetParent(slots[itemCount], false);
            icon.SetActive(true);
            itemCount++;
        }
    }

    //takes an item out of the inventory (level 3: the guard takes the cipher pages back)
    public void RemoveItem(GameObject icon)
    {
        if (icon.activeSelf == false)
            return; //this item is not in the inventory

        icon.SetActive(false);

        //move the items that are left back next to each other, starting from the first slot
        itemCount = 0;
        for (int i = 0; i < itemIcons.Length; i++)
        {
            if (itemIcons[i].activeSelf == true)
            {
                itemIcons[i].transform.SetParent(slots[itemCount], false);
                itemCount++;
            }
        }
    }
}
