using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//put this on the Level Manager. The button on the start screen calls StartLevel() (like the buttons in lab 8)
public class StartScreen : MonoBehaviour
{
    public GameObject startScreen; //Canvas > Start Screen
    public TextMeshProUGUI titleText; //"Alex... welcome to Level 2."
    public TextMeshProUGUI storyText; //the story under the title
    public GameObject exploreButton; //the "Click on ... To Explore" button
    public TextMeshProUGUI buttonText; //the text on the button
    public float typingSpeed = 0.02f;
    public GameObject ball; //the ball waits until the player starts the level
    public Animator entranceDoor; //alex comes from level 1 through this door, then it closes behind him

    //static so the start screen does not appear again when the level restarts after alex dies (like the statics in lab 7)
    public static bool levelStarted = false;

    private PlayerController player;
    private Rigidbody2D playerRB;
    private string title;
    private string story;
    private string button;
    private bool musicStopped = false;
    private SpriteRenderer playerSR;
    private Animator playerAnim;
    private float spawnX; //where alex stops after walking out of the door

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        playerRB = player.GetComponent<Rigidbody2D>();
        playerSR = player.GetComponent<SpriteRenderer>();
        playerAnim = player.GetComponent<Animator>();

        if (levelStarted == true)
        {
            startScreen.SetActive(false); //the player already started the level before, so go straight to the level
        }
        else
        {
            startScreen.SetActive(false); //it appears after alex walks in from the door

            //freezing the player in place while the start screen is shown (like lab 9)
            playerRB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
            player.enabled = false;
            ball.SetActive(false);

            //keep the sentences written in the Inspector, then empty the texts so they can be typed
            title = titleText.text;
            story = storyText.text;
            button = buttonText.text;
            titleText.text = "";
            storyText.text = "";
            buttonText.text = "";
            exploreButton.SetActive(false);

            //alex starts inside the door (hidden), then walks in to his normal start point
            spawnX = player.transform.position.x;
            player.transform.position = new Vector3(entranceDoor.transform.position.x + 1.2f, player.transform.position.y, 0);
            playerSR.enabled = false; //like the flicker in lab 6

            StartCoroutine(EnterFromDoor());
        }
    }

    // Update is called once per frame
    void Update()
    {
        //the first Update runs after the Start() of every object, so the Audio Manager already started the music.
        //stop it here, so the background music waits until the player clicks Explore
        if (levelStarted == false && musicStopped == false)
        {
            AudioManager.Instance.musicSource.enabled = false; //turned off (like sr.enabled in lab 6)
            musicStopped = true;
        }
    }

    //the door opens, alex walks in from level 1, the door closes, then the start screen appears
    IEnumerator EnterFromDoor()
    {
        yield return new WaitForSeconds(0.5f);
        entranceDoor.SetBool("isOpen", true); //like the animations in lab 3
        yield return new WaitForSeconds(0.4f);

        playerSR.enabled = true;
        playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;
        playerAnim.SetFloat("Speed", player.moveSpeed); //walking animation
        while (player.transform.position.x < spawnX)
        {
            playerRB.velocity = new Vector2(player.moveSpeed / 2, playerRB.velocity.y); //walking slowly with velocity like lab 2
            yield return new WaitForSeconds(0.02f);
        }
        playerRB.velocity = new Vector2(0, 0);
        playerAnim.SetFloat("Speed", 0);
        playerRB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;

        yield return new WaitForSeconds(0.3f);
        entranceDoor.SetBool("isOpen", false);
        yield return new WaitForSeconds(0.6f);

        startScreen.SetActive(true);
        StartCoroutine(TypeStartScreen());
    }

    //types the title, then the story, then the button, letter by letter (like the dialogue in lab 9)
    IEnumerator TypeStartScreen()
    {
        foreach (char letter in title.ToCharArray())
        {
            titleText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        foreach (char letter in story.ToCharArray())
        {
            storyText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        exploreButton.SetActive(true); //the button appears when the story is written

        foreach (char letter in button.ToCharArray())
        {
            buttonText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    //linked to the OnClick() of the button on the start screen
    public void StartLevel()
    {
        levelStarted = true;
        startScreen.SetActive(false);

        //unfreeze the player, and the ball starts following him
        playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;
        player.enabled = true;
        ball.SetActive(true);

        AudioManager.Instance.musicSource.enabled = true;
        AudioManager.Instance.PlayMusic(AudioManager.Instance.overworldMusic); //the background music starts now (like lab 7)
    }
}
