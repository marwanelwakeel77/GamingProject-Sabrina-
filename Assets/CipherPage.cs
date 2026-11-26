
using UnityEngine;
using TMPro;

public class CipherPage : MonoBehaviour
{
    public GameObject inspectionPanel;
    public GameObject inspectPrompt;
    public TMP_Text promptText;
    public Sprite pageSprite;
    public AudioClip pageSound;

    // which of Sabrina's pages this is, shown at the top of the inspection panel (e.g. "Cipher Page I")
    public string pageTitle = "Cipher Page";

    private bool playerNearby = false;
    private bool isReading = false;

    private CipherInspectionController inspectionController;

    void Start()
    {
        inspectionPanel.SetActive(false);
        inspectPrompt.SetActive(false);

        inspectionController =
            inspectionPanel.GetComponent<CipherInspectionController>();
    }

    void Update()
    {
        // Time.timeScale is 0 while another panel is open, so E is ignored then
        if (playerNearby && !isReading && Time.timeScale > 0 &&
            Input.GetKeyDown(KeyCode.E))
        {
            OpenPage();
        }
    }

    void OpenPage()
    {
        if (inspectionController == null)
        {
            Debug.LogError("CipherInspectionController is missing!");
            return;
        }

        isReading = true;
        inspectPrompt.SetActive(false);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(pageSound);

        inspectionController.OpenInspection(pageSprite, this);
    }

    public void CompleteInspection()
    {
        if (!isReading) return;

        isReading = false;
        playerNearby = false;

        PlayerStats.cipherPagesCollected++;

        Debug.Log("Cipher Pages: " +
            PlayerStats.cipherPagesCollected + "/3");

        // hide the page instead of destroying it,
        // so the LevelManager can put it back if the guard catches Alex
        gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNearby = true;

            if (!isReading)
            {
                promptText.text = "Press E to Inspect";
                inspectPrompt.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNearby = false;
            inspectPrompt.SetActive(false);
        }
    }
}
