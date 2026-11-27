using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BookMechanism : MonoBehaviour
{
    public GameObject interactionPrompt;
    public TMP_Text messageText;
    public TMP_Text promptText;

    public BookShelfInspectionUI shelfUI;
    public SecretPassage secretPassage;

    public SpriteRenderer sigilBook;
    public Sprite sigilBookPulled;

    public GameObject sigilSparkles;

    public Transform[] shakeObjects;
    public float shakeTime = 0.6f;
    public float shakeAmount = 0.03f;
    public AudioClip mechanismSound;

    private bool playerNearby = false;
    private bool mechanismActivated = false;
    private bool shaking = false;

    private Sprite sigilBookStart;
    private Vector3[] shakeStartPositions;

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        sigilBookStart = sigilBook.sprite;
        shakeStartPositions = new Vector3[shakeObjects.Length];
        for (int i = 0; i < shakeObjects.Length; i++)
        {
            shakeStartPositions[i] = shakeObjects[i].position;
        }
    }

    public void ResetMechanism()
    {
        StopAllCoroutines();
        shaking = false;

        for (int i = 0; i < shakeObjects.Length; i++)
        {
            shakeObjects[i].position = shakeStartPositions[i];
        }

        mechanismActivated = false;
        sigilBook.sprite = sigilBookStart;

        shelfUI.ResetShelf();
        secretPassage.ClosePassage();
    }

    void Update()
    {
        if (shaking)
        {
            float offset = Random.Range(-shakeAmount, shakeAmount);
            for (int i = 0; i < shakeObjects.Length; i++)
            {
                shakeObjects[i].position = shakeStartPositions[i] + new Vector3(offset, 0, 0);
            }
        }

        if (sigilSparkles != null)
            sigilSparkles.SetActive(PlayerStats.cipherPagesCollected >= 3 && !mechanismActivated);

        if (playerNearby &&
            !mechanismActivated &&
            Time.timeScale > 0 &&
            Input.GetKeyDown(KeyCode.E))
        {
            interactionPrompt.SetActive(false);
            shelfUI.OpenShelf();
        }
    }

    public bool IsActivated()
    {
        return mechanismActivated;
    }

    public void ShowPrompt()
    {
        if (playerNearby && !mechanismActivated)
        {
            promptText.text = "Press E to Inspect the Shelf";
            interactionPrompt.SetActive(true);
        }
    }

    public void ActivateMechanism()
    {
        if (mechanismActivated) return;

        mechanismActivated = true;

        Debug.Log("The secret mechanism is activated!");

        sigilBook.sprite = sigilBookPulled;

        StartCoroutine(ShakeShelf());
    }

    IEnumerator ShakeShelf()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(mechanismSound);

        shaking = true;
        yield return new WaitForSeconds(shakeTime);
        shaking = false;

        for (int i = 0; i < shakeObjects.Length; i++)
        {
            shakeObjects[i].position = shakeStartPositions[i];
        }

        secretPassage.OpenPassage();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNearby = true;
            ShowPrompt();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNearby = false;

            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }
}
