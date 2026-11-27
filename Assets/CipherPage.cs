using UnityEngine;
using TMPro;

public class CipherPage : MonoBehaviour
{
    public GameObject inspectionPanel;
    public GameObject inspectPrompt;
    public TMP_Text promptText;
    public Sprite pageSprite;
    public AudioClip pageSound;

    public string pageTitle = "Cipher Page";

    public GameObject inventoryIcon;

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

        InventoryManager inventory = FindObjectOfType<InventoryManager>();
        if (inventory != null && inventoryIcon != null)
            inventory.AddItem(inventoryIcon);

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
