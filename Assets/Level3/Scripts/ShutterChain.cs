using System.Collections;
using UnityEngine;
using TMPro;

// The chain at the far end of the upper balcony. Pulling it (E) opens the old shutters of the
// stained-glass window: moonlight falls on the nightstand and reveals the crescent moon.
// Sprite swaps like the brick in Lab 5, a fade with Lerp (Lab 4) and a Coroutine (Lab 9).
public class ShutterChain : MonoBehaviour
{
    public SpriteRenderer chainRenderer;
    public Sprite pulledChainSprite;

    public SpriteRenderer shutters;
    public Sprite openShutters;
    public SpriteRenderer moonbeam;
    public float moonbeamAlpha = 1f;
    public CrescentMoonCollectible moon;

    public GameObject prompt;      // the "Press E" box in the Canvas
    public TMP_Text promptText;

    public AudioClip chainSound;
    public AudioClip shuttersSound;

    private bool playerNearby = false;
    private bool pulled = false;

    void Start()
    {
        moonbeam.color = new Color(1f, 1f, 1f, 0f); // no moonlight while the shutters are closed
    }

    void Update()
    {
        if (playerNearby && !pulled && Time.timeScale > 0 && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(PullChain());
        }
    }

    IEnumerator PullChain()
    {
        pulled = true;
        prompt.SetActive(false);

        chainRenderer.sprite = pulledChainSprite;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(chainSound);
        yield return new WaitForSeconds(0.6f);

        // far away, the shutters swing open
        shutters.sprite = openShutters;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(shuttersSound);
        yield return new WaitForSeconds(0.4f);

        // the moonbeam slowly appears
        float timer = 0f;
        while (timer < 1.2f)
        {
            timer = timer + Time.deltaTime;
            moonbeam.color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, moonbeamAlpha, timer / 1.2f));
            yield return null;
        }

        moon.Reveal();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNearby = true;
            if (!pulled)
            {
                promptText.text = "Press E to Pull the Chain";
                prompt.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNearby = false;
            prompt.SetActive(false);
        }
    }
}
