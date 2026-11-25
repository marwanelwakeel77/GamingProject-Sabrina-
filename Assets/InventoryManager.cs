using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject InventoryPanel;
    public Transform[] slots; //the 4 slots of the inventory
    public GameObject[] itemIcons; //the icons of the items, hidden until the player collects them

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
}
