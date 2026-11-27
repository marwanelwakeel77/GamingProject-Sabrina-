using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CrescentMoonCollectible : MonoBehaviour
{
    public AudioClip moonSound;
    public GameObject inventoryIcon;

    public bool needsMoonlight = true;
    public float hiddenAlpha = 0.15f;
    public GameObject[] glowObjects;
    public GameObject prompt;
    public TMP_Text promptText;
    public AudioClip revealSound;

    private bool revealed = false;
    private bool collected = false;
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

    public void Reveal()
    {
        if (!revealed)
            StartCoroutine(RevealRoutine());
    }

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

        PlayerStats.hasCrescentMoon = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(moonSound);

        InventoryManager inventory = FindObjectOfType<InventoryManager>();
        if (inventory != null && inventoryIcon != null)
            inventory.AddItem(inventoryIcon);

        if (prompt != null)
            prompt.SetActive(false);

        Debug.Log("Crescent Moon collected!");

        Destroy(gameObject);
    }
}
