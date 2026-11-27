using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// The bookshelf with the statue on top. Pressing E opens the shelf close-up,
// where the player has to pull the book that Sabrina's three pages describe.
public class BookMechanism : MonoBehaviour
{
    public Animator passageAnimator;

    public GameObject interactionPrompt;
    public TMP_Text messageText;
    public TMP_Text promptText;

    public BookShelfInspectionUI shelfUI;
    public SecretPassage secretPassage;

    // the red book with the sigil in the gap of the shelf
    public SpriteRenderer sigilBook;
    public Sprite sigilBookPulled;

    // sparkles (Lab 4 particle system) that appear on the sigil book once all three pages are found
    public GameObject sigilSparkles;

    // the bookshelf, the statue and the book shake when the hidden mechanism moves
    public Transform[] shakeObjects;
    public float shakeTime = 0.6f;
    public float shakeAmount = 0.03f;
    public AudioClip mechanismSound;

    private bool playerNearby = false;
    private bool mechanismActivated = false;

    private Sprite sigilBookStart;        // the sigil book before it is pulled
    private Vector3[] shakeStartPositions; // where the shaking objects belong

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

    // called when the guard catches Alex: everything goes back to how it was at the start
    public void ResetMechanism()
    {
        StopAllCoroutines(); // stop the shaking if it was still going

        for (int i = 0; i < shakeObjects.Length; i++)
        {
            shakeObjects[i].position = shakeStartPositions[i];
        }

        mechanismActivated = false;
        sigilBook.sprite = sigilBookStart; // the sigil book slides back into the shelf

        shelfUI.ResetShelf();
        if (secretPassage != null)
            secretPassage.ClosePassage();
    }

    void Update()
    {
        // visual hint: the sigil book shimmers once the player holds all three pages
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

    // called by the shelf close-up when the player still near the shelf steps back
    public void ShowPrompt()
    {
        if (playerNearby && !mechanismActivated)
        {
            promptText.text = "Press E to Inspect the Shelf";
            interactionPrompt.SetActive(true);
        }
    }

    // called by the shelf close-up when the correct book is pulled. Works only once.
    public void ActivateMechanism()
    {
        if (mechanismActivated) return;

        mechanismActivated = true;

        Debug.Log("The secret mechanism is activated!");

        // the real book slides out of the shelf too (sprite swap, like the brick in Lab 5)
        sigilBook.sprite = sigilBookPulled;

        StartCoroutine(ShakeShelf());
    }

    // shakes the shelf for a moment (Coroutine, Lab 9), then opens the secret passage
    IEnumerator ShakeShelf()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(mechanismSound);

        // remember where everything was
        Vector3[] startPositions = new Vector3[shakeObjects.Length];
        for (int i = 0; i < shakeObjects.Length; i++)
        {
            startPositions[i] = shakeObjects[i].position;
        }

        float timer = 0f;
        while (timer < shakeTime)
        {
            timer = timer + Time.deltaTime;

            // move left and right by a small random amount every frame
            float offset = Random.Range(-shakeAmount, shakeAmount);
            for (int i = 0; i < shakeObjects.Length; i++)
            {
                shakeObjects[i].position = startPositions[i] + new Vector3(offset, 0, 0);
            }
            yield return null; //wait for the next frame
        }

        // put everything back exactly where it was
        for (int i = 0; i < shakeObjects.Length; i++)
        {
            shakeObjects[i].position = startPositions[i];
        }

        if (secretPassage != null)
            secretPassage.OpenPassage();
        else if (passageAnimator != null)
            passageAnimator.SetTrigger("Open");
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
