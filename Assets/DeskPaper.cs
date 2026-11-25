using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeskPaper : MonoBehaviour
{
    public GameObject paperScreen; //Sabrina's paper over the transparent background (Canvas > Paper Screen)
    public GameObject interactText; //"Press E To Interact" in the Canvas
    public GameObject ball; //the ball that follows the player
    public AudioClip paperSound; //plays when the paper opens

    private bool playerNear = false;
    private bool paperOpen = false;
    private Rigidbody2D playerRB;

    // Start is called before the first frame update
    void Start()
    {
        playerRB = FindObjectOfType<PlayerController>().GetComponent<Rigidbody2D>();
        paperScreen.SetActive(false); //the paper is hidden until the player presses E at the desk
    }

    // Update is called once per frame
    void Update()
    {
        if (playerNear == true && paperOpen == false && Input.GetKeyDown(KeyCode.E))
        {
            OpenPaper();
        }

        if (paperOpen == true && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePaper();
        }
    }

    void OpenPaper()
    {
        paperOpen = true;
        paperScreen.SetActive(true);
        AudioManager.Instance.PlayMusicSFX(paperSound); //like the sounds in lab 7
        interactText.SetActive(false);
        playerRB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY; //freezing the player in place while reading

        //the ball waits while the player reads the paper
        if (ball != null)
        {
            ball.SetActive(false);
        }
    }

    //linked to the OnClick() of the close button on the paper
    public void ClosePaper()
    {
        paperOpen = false;
        paperScreen.SetActive(false);
        playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (ball != null)
        {
            ball.SetActive(true);
        }

        if (playerNear == true)
        {
            interactText.SetActive(true);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNear = true;
            interactText.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNear = false;
            interactText.SetActive(false);
        }
    }
}
