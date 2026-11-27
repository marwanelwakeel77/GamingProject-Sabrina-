using System.Collections;
using UnityEngine;
using TMPro;

// The hidden door behind the bookshelf
public class SecretPassage : MonoBehaviour
{
    public Animator animator;          // Open trigger (Lab 3 animation states)
    public ParticleSystem dust;        // dust burst when it opens (Lab 4 particle system)
    public GameObject revealLight;
    public BoxCollider2D entryTrigger;

    public GameObject interactionPrompt;
    public TMP_Text promptText;

    public CameraFollow cameraFollow;
    public Transform cameraFocus;

    public GameObject levelCompletePanel;

    public AudioClip openSound;
    public AudioClip discoverySound;

    public EscapeSequence escape; // starts when the passage is open

    private bool isOpen = false;
    private bool playerNearby = false;

    void Start()
    {
        entryTrigger.enabled = false; //can't be entered until it opens
        revealLight.SetActive(false);
        levelCompletePanel.SetActive(false);
    }

    // called once by the BookMechanism
    public void OpenPassage()
    {
        StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        // freeze Alex during the short cut-scene (like during dialogue in Lab 9)
        Rigidbody2D playerRB = FindObjectOfType<PlayerController>().GetComponent<Rigidbody2D>();
        playerRB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;

        // the camera looks at the passage
        Transform alex = cameraFollow.Target;
        cameraFollow.Target = cameraFocus;
        yield return new WaitForSeconds(1f);

        animator.SetTrigger("Open");
        dust.Play();
        revealLight.SetActive(true);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(openSound);

        yield return new WaitForSeconds(1f);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(discoverySound);

        isOpen = true;
        entryTrigger.enabled = true;

        yield return new WaitForSeconds(1.5f);

        // back to Alex
        cameraFollow.Target = alex;
        playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;

        // the library notices: the escape begins
        if (escape != null)
            escape.StartEscape();
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    // called when the guard catches Alex: the passage closes and the puzzle must be solved again
    public void ClosePassage()
    {
        StopAllCoroutines(); // in case it was still opening

        isOpen = false;
        playerNearby = false;
        entryTrigger.enabled = false;
        revealLight.SetActive(false);

        // back to the Closed state (Lab 3: a trigger parameter starts the transition)
        animator.ResetTrigger("Open");
        animator.SetTrigger("Close");

        // the camera follows Alex again and he can move (in case it happened during the cut-scene)
        PlayerController alex = FindObjectOfType<PlayerController>();
        cameraFollow.Target = alex.transform;
        alex.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        if (isOpen && playerNearby && Time.timeScale > 0 && Input.GetKeyDown(KeyCode.E))
        {
            interactionPrompt.SetActive(false);
            levelCompletePanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNearby = true;
            promptText.text = "Press E to Enter the Passage";
            interactionPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNearby = false;
            interactionPrompt.SetActive(false);
        }
    }
}
