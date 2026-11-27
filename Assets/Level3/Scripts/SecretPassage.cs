using System.Collections;
using UnityEngine;
using TMPro;

public class SecretPassage : MonoBehaviour
{
    public Animator animator;
    public ParticleSystem dust;
    public GameObject revealLight;
    public BoxCollider2D entryTrigger;

    public GameObject interactionPrompt;
    public TMP_Text promptText;

    public CameraFollow cameraFollow;
    public Transform cameraFocus;

    public GameObject levelCompletePanel;

    public AudioClip openSound;
    public AudioClip discoverySound;

    public EscapeSequence escape;

    private bool isOpen = false;
    private bool playerNearby = false;

    void Start()
    {
        entryTrigger.enabled = false;
        revealLight.SetActive(false);
        levelCompletePanel.SetActive(false);
    }

    public void OpenPassage()
    {
        StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        Rigidbody2D playerRB = FindObjectOfType<PlayerController>().GetComponent<Rigidbody2D>();
        playerRB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;

        Transform alex = cameraFollow.Target;
        cameraFollow.Target = cameraFocus;
        yield return new WaitForSeconds(1f);

        animator.SetBool("IsOpen", true);
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

        cameraFollow.Target = alex;
        playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (escape != null)
            escape.StartEscape();
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    public void ClosePassage()
    {
        StopAllCoroutines();

        isOpen = false;
        playerNearby = false;
        entryTrigger.enabled = false;
        revealLight.SetActive(false);

        animator.SetBool("IsOpen", false);

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
