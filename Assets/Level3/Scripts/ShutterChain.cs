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
    private bool retracting = false;
    private bool beamFading = false;
    private Vector3 chainTarget;

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

        if (retracting)
        {
            Transform chain = chainRenderer.transform;
            chain.position = Vector3.MoveTowards(chain.position, chainTarget, retractSpeed * Time.deltaTime);

            if (chain.position == chainTarget)
            {
                retracting = false;
                chainRenderer.gameObject.SetActive(false);
            }
        }

        if (beamFading)
        {
            float alpha = Mathf.MoveTowards(moonbeam.color.a, moonbeamAlpha, moonbeamAlpha / 1.2f * Time.deltaTime);
            moonbeam.color = new Color(1f, 1f, 1f, alpha);
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

        chainTarget = chainRenderer.transform.position + new Vector3(0, retractHeight, 0);
        retracting = true;
        yield return new WaitForSeconds(0.4f);

        beamFading = true;
        yield return new WaitForSeconds(1.2f);
        beamFading = false;
        moonbeam.color = new Color(1f, 1f, 1f, moonbeamAlpha);

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
