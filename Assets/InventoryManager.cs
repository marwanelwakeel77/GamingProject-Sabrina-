
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject InventoryPanel;

    void Start()
    {
        InventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            InventoryPanel.SetActive(!InventoryPanel.activeSelf);
        }
    }

public void CloseInventory()
{
    InventoryPanel.SetActive(false);
}

}
