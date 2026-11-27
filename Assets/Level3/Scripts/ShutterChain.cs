using System.Collections;
using UnityEngine;
using TMPro;

public class ShutterChain : MonoBehaviour
{
    public SpriteRenderer chainRenderer;
    public Sprite pulledChainSprite;

    public SpriteRenderer shutters;
    public Sprite openShutters;
    public SpriteRenderer moonbeam;
    public float moonbeamAlpha = 1f;
    public CrescentMoonCollectible moon;

    public GameObject prompt;
    public TMP_Text promptText;

    public AudioClip chainSound;
    public AudioClip shuttersSound;

    public float retractHeight = 4f;
    public float retractSpeed = 3f;

    private bool playerNearby = false;
    private bool pulled = false;

    void Start()
    {
        moonbeam.color = new Color(1f, 1f, 1f, 0f);
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

        shutters.sprite = openShutters;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(shuttersSound);
        StartCoroutine(RetractChain());
        yield return new WaitForSeconds(0.4f);

        float timer = 0f;
        while (timer < 1.2f)
        {
            timer = timer + Time.deltaTime;
            moonbeam.color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, moonbeamAlpha, timer / 1.2f));
            yield return null;
        }

        moon.Reveal();
    }

    IEnumerator RetractChain()
    {
        Transform chain = chainRenderer.transform;
        Vector3 target = chain.position + new Vector3(0, retractHeight, 0);

        while (chain.position != target)
        {
            chain.position = Vector3.MoveTowards(chain.position, target, retractSpeed * Time.deltaTime);
            yield return null;
        }

        chainRenderer.gameObject.SetActive(false);
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
