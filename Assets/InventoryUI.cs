using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    public GameObject keyIcon;
    public bool HasKey { get; private set; }

    void Awake()
    {
        Instance = this;
        keyIcon.SetActive(false);
    }

    public void AddKey()
    {
        HasKey = true;
        keyIcon.SetActive(true);
    }

    public void UseKey()
    {
        HasKey = false;
        keyIcon.SetActive(false);
    }
}