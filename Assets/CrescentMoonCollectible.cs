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
    private bool revealing = false;
    private bool collected = false;
    private bool playerNearby = false;
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

    void Update()
    {
        if (revealing)
        {
            SetAlpha(Mathf.MoveTowards(sr.color.a, 1f, (1f - hiddenAlpha) / 1.2f * Time.deltaTime));
        }

        if (playerNearby && revealed)
        {
            Collect();
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

        revealing = true;
        yield return new WaitForSeconds(1.2f);
        revealing = false;
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNearby = true;

            if (!revealed && prompt != null)
            {
                promptText.text = "Only moonlight can reveal the crescent moon...";
                prompt.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNearby = false;

            if (prompt != null)
                prompt.SetActive(false);
        }
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
