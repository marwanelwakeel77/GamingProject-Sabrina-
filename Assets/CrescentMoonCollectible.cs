using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CrescentMoonCollectible : MonoBehaviour
{
    public AudioClip moonSound;
    public GameObject inventoryIcon; // the moon's icon in the inventory (press H)

    // Level 3: the moon can only be taken once moonlight falls on it (the ShutterChain opens the window).
    // Before that it is only a faint shape on the nightstand.
    public bool needsMoonlight = true;
    public float hiddenAlpha = 0.15f;
    public GameObject[] glowObjects;   // MoonGlow and Sparkles, switched on when it is revealed
    public GameObject prompt;          // the "Press E" box in the Canvas, used for a clue
    public TMP_Text promptText;
    public AudioClip revealSound;

    private bool revealed = false;
    private bool collected = false; // so it can't be collected twice
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (needsMoonlight)
        {
            SetAlpha(hiddenAlpha);
            for (int i = 0; i < glowObjects.Length; i++)
                glowObjects[i].SetActive(false);
        }
        else
        {
            revealed = true;
        }
    }

    // called by the ShutterChain when the moonlight reaches the moon
    public void Reveal()
    {
        if (!revealed)
            StartCoroutine(RevealRoutine());
    }

    // the moon fades in (Lerp, like the camera in Lab 4)
    IEnumerator RevealRoutine()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(revealSound);

        float timer = 0f;
        while (timer < 1.2f)
        {
            timer = timer + Time.deltaTime;
            SetAlpha(Mathf.Lerp(hiddenAlpha, 1f, timer / 1.2f));
            yield return null;
        }
        SetAlpha(1f);

        for (int i = 0; i < glowObjects.Length; i++)
            glowObjects[i].SetActive(true);

        revealed = true;
    }

    void SetAlpha(float alpha)
    {
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            if (!revealed)
            {
                // a clue: the moon is here, but out of reach for now
                if (prompt != null)
                {
                    promptText.text = "Only moonlight can reveal the crescent moon...";
                    prompt.SetActive(true);
                }
                return;
            }

            Collect();
        }
    }

    // if Alex is already standing at the nightstand when the moon appears
    private void OnTriggerStay2D(Collider2D other)
    {
        if (revealed && other.tag == "Player")
            Collect();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player" && prompt != null)
            prompt.SetActive(false);
    }

    void Collect()
    {
        if (collected) return;
        collected = true;

        PlayerStats.hasCrescentMoon = true; //static, so it is kept even when the guard catches Alex

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(moonSound);

        // the moon also goes into the inventory
        InventoryManager inventory = FindObjectOfType<InventoryManager>();
        if (inventory != null && inventoryIcon != null)
            inventory.AddItem(inventoryIcon);

        if (prompt != null)
            prompt.SetActive(false);

        Debug.Log("Crescent Moon collected!");

        Destroy(gameObject);
    }
}
