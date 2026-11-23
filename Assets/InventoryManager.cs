using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Inventory logic for the existing inventory UI (panel, slots and designed icons are unchanged).
// The inventory starts empty. An item appears only after Alex collects it, in the first free slot.
public class InventoryManager : MonoBehaviour
{
    [Serializable]
    public class ItemIcon
    {
        public string itemName;   // must match the name used by the pickup, e.g. "Triangle Symbol"
        public GameObject icon;   // the designed icon (with its label) that already exists in the Canvas
    }

    public static InventoryManager Instance;

    public GameObject InventoryPanel;
    public Transform itemsContainer;     // the "Items Container" under the panel (holds the slots)
    public GameObject itemSlotPrefab;    // prefab/Item Slot, used only if there is no free slot left
    public ItemIcon[] itemIcons;         // the designed icons, hidden until their item is collected

    private readonly HashSet<string> items = new HashSet<string>();
    private readonly List<Transform> slots = new List<Transform>();
    private readonly HashSet<Transform> usedSlots = new HashSet<Transform>();

    void Awake()
    {
        Instance = this;

        // Every designed icon starts hidden, so nothing shows before it is collected.
        if (itemIcons != null)
        {
            foreach (ItemIcon entry in itemIcons)
            {
                if (entry != null && entry.icon != null) entry.icon.SetActive(false);
            }
        }

        // Remember the slots from left to right.
        if (itemsContainer != null)
        {
            for (int i = 0; i < itemsContainer.childCount; i++)
            {
                slots.Add(itemsContainer.GetChild(i));
            }
        }
    }

    void Start()
    {
        if (InventoryPanel != null) InventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) && InventoryPanel != null)
        {
            InventoryPanel.SetActive(!InventoryPanel.activeSelf);
        }
    }

    public void CloseInventory()
    {
        if (InventoryPanel != null) InventoryPanel.SetActive(false);
    }

    public bool HasItem(string itemName)
    {
        return items.Contains(itemName);
    }

    // Adds a collected item once. Opening or closing the panel never changes this.
    public void AddItem(string itemName, Sprite icon)
    {
        if (string.IsNullOrEmpty(itemName) || items.Contains(itemName)) return;
        items.Add(itemName);

        Transform slot = FirstFreeSlot() ?? CreateSlot();
        if (slot == null) return;

        ItemIcon designed = FindIcon(itemName);
        if (designed != null && designed.icon != null)
        {
            // Move the designed icon into the first free slot and show it.
            designed.icon.transform.SetParent(slot, false);
            designed.icon.SetActive(true);
            usedSlots.Add(slot);
            return;
        }

        // No designed icon: show the sprite in a slot (fallback).
        Image img = slot.GetComponent<Image>();
        if (img != null && icon != null) img.sprite = icon;
        slot.name = itemName;
        usedSlots.Add(slot);
    }

    Transform FirstFreeSlot()
    {
        foreach (Transform slot in slots)
        {
            if (slot != null && !usedSlots.Contains(slot)) return slot;
        }
        return null;
    }

    Transform CreateSlot()
    {
        if (itemsContainer == null || itemSlotPrefab == null) return null;

        GameObject newSlot = Instantiate(itemSlotPrefab, itemsContainer, false);
        slots.Add(newSlot.transform);
        return newSlot.transform;
    }

    ItemIcon FindIcon(string itemName)
    {
        if (itemIcons == null) return null;

        foreach (ItemIcon entry in itemIcons)
        {
            if (entry != null && entry.itemName == itemName) return entry;
        }
        return null;
    }
}
