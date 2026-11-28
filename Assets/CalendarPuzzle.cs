using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CalendarPuzzle : MonoBehaviour
{
    public GameObject calendarPanel; //the big calendar on the screen (Canvas > Calendar Panel)
    public CalendarDateBlock[] dateBlocks; //the 4 ? blocks on the big calendar: 5, 15, 25, 29
    public GameObject[] wallBlocks; //the 4 ? blocks on the calendar on the wall
    public CalendarDialogue dialogue; //the hint dialogue
    public TextMeshProUGUI timerText; //shows the seconds left
    public GameObject timerImage; //the alarm clock image behind the seconds (Canvas > timer)
    public float timeLimit = 30f; //the player has 30 seconds to reveal the dates
    public AudioClip timerSound; //ticking sound while the 30 seconds run
    public AudioClip breakSound; //plays one time when a date block breaks
    public AudioClip solvedSound; //plays with the "All four dates are revealed!" message
    public GameObject interactText; //"Press E To Interact" in the Canvas

    public bool isSolved = false;
    public bool playerNear = false; //WallPickup checks it
    public bool calendarOpen = false; //WallPickup checks it
    private bool timeIsUp = false; //the level is about to start again, so the calendar can not be opened
    private bool timerOn = false;
    private float timeLeft;
    private int selected = 0; //the date the player is on now
    private int brokenCount = 0; //how many dates are revealed

    private PlayerController player;
    private Rigidbody2D playerRB;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        playerRB = player.GetComponent<Rigidbody2D>();
        calendarPanel.SetActive(false); //the big calendar is hidden until the player presses E
        timerText.gameObject.SetActive(false);
        timerImage.SetActive(false);
        dialogue.gameObject.SetActive(false); //the dialogue is hidden until the player opens the calendar
        interactText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //first E opens the calendar, after that E breaks the date the player is on
        if (playerNear == true && isSolved == false && timeIsUp == false && Input.GetKeyDown(KeyCode.E))
        {
            if (calendarOpen == false)
            {
                OpenCalendar();
            }
            else
            {
                BreakSelectedBlock();
            }
        }

        //the arrows move between the hidden dates
        if (calendarOpen == true && isSolved == false)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveSelection(1);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveSelection(-1);
            }
        }

        if (calendarOpen == true && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseCalendar();
        }

        //the timer counts down after the first block is broken
        if (timerOn == true)
        {
            timeLeft = timeLeft - Time.deltaTime;
            timerText.text = "" + (int)timeLeft;

            if (timeLeft <= 0)
            {
                TimeUp();
            }
        }
    }

    void OpenCalendar()
    {
        calendarOpen = true;
        calendarPanel.SetActive(true); //the big calendar over the blurred classroom
        interactText.SetActive(false);
        dialogue.ShowDialogue(); //the hint dialogue appears on the big calendar

        //freeze the player in place while the calendar is open
        playerRB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        player.enabled = false; //so the arrows move between the dates, not the player

        //start on the first date that is not revealed yet
        selected = -1;
        MoveSelection(1);
    }

    void CloseCalendar()
    {
        calendarOpen = false;

        //remove the dark gold, and keep the broken blocks hidden
        for (int i = 0; i < dateBlocks.Length; i++)
        {
            dateBlocks[i].Select(false);

            if (dateBlocks[i].isBroken == true)
            {
                dateBlocks[i].HideBlock();
            }
        }

        calendarPanel.SetActive(false); //back to the normal scene
        dialogue.HideDialogue(); //the hint dialogue closes with the calendar

        //the player is still near the calendar and did not solve it yet
        if (playerNear == true && isSolved == false && timeIsUp == false)
        {
            interactText.SetActive(true);
        }

        //unfreeze the player, but keep his rotation frozen as it was before
        playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;
        player.enabled = true;
    }

    //moves to the next (1) or the previous (-1) date that is not revealed yet
    void MoveSelection(int direction)
    {
        for (int i = 0; i < dateBlocks.Length; i++)
        {
            selected = selected + direction;

            if (selected >= dateBlocks.Length)
            {
                selected = 0;
            }
            if (selected < 0)
            {
                selected = dateBlocks.Length - 1;
            }

            if (dateBlocks[selected].isBroken == false)
            {
                break;
            }
        }

        //only the selected date is dark gold
        for (int i = 0; i < dateBlocks.Length; i++)
        {
            if (dateBlocks[i].isBroken == false)
            {
                dateBlocks[i].Select(i == selected);
            }
        }
    }

    void BreakSelectedBlock()
    {
        if (dateBlocks[selected].isBroken == true)
        {
            return;
        }

        //the 30 seconds start with the first block
        if (timerOn == false)
        {
            timerOn = true;
            timeLeft = timeLimit;
            timerText.gameObject.SetActive(true);
            timerImage.SetActive(true);
            AudioManager.Instance.PlayMusic(timerSound); //the background music changes to the timer
        }

        dateBlocks[selected].BreakBlock();
        AudioManager.Instance.PlayMusicSFX(breakSound); //like the heart sound in lab 7
        brokenCount++;

        //all 4 dates are revealed
        if (brokenCount == dateBlocks.Length)
        {
            isSolved = true;
            timerOn = false;
            timerText.gameObject.SetActive(false);
            timerImage.SetActive(false);
            AudioManager.Instance.PlayMusic(AudioManager.Instance.overworldMusic); //back to the normal music
            dialogue.CloseDialogue(); //the "Solve The Puzzle" hint is not needed any more

            //the dates are also seen on the calendar on the wall now
            for (int i = 0; i < wallBlocks.Length; i++)
            {
                wallBlocks[i].SetActive(false);
            }

            StartCoroutine(CloseAfterSolved());
        }
        else
        {
            MoveSelection(1); //go to the next hidden date
        }
    }

    //wait a little so the player can read the dates, then close the calendar
    IEnumerator CloseAfterSolved()
    {
        yield return new WaitForSeconds(2f);

        if (calendarOpen == true)
        {
            CloseCalendar();
        }

        //a short pause between the calendar and the message, so they do not appear together
        yield return new WaitForSeconds(0.5f);
        FindObjectOfType<HintText>().ShowHint("All four dates are revealed!");
        AudioManager.Instance.PlayRandomSFX(solvedSound); //PlayOneShot, so the break sound of the last block is not cut
    }

    void TimeUp()
    {
        timerOn = false;
        timeIsUp = true;
        timerText.gameObject.SetActive(false);
        timerImage.SetActive(false);
        AudioManager.Instance.PlayMusic(AudioManager.Instance.overworldMusic); //back to the normal music

        if (calendarOpen == true)
        {
            CloseCalendar();
        }

        //hide all the dates again
        for (int i = 0; i < dateBlocks.Length; i++)
        {
            dateBlocks[i].ResetBlock();
        }
        brokenCount = 0;

        interactText.SetActive(false);
        StartCoroutine(RespawnAfterTimeUp());
    }

    //wait a little so the player can read the hint, then the level manager respawns him
    IEnumerator RespawnAfterTimeUp()
    {
        //a short pause after the calendar closes, then the message
        yield return new WaitForSeconds(0.5f);
        FindObjectOfType<HintText>().ShowHint("Time is up! Try again.");

        //time to read the message, then the level starts again
        yield return new WaitForSeconds(3f);
        FindObjectOfType<LevelManager>().RespawnPlayer();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNear = true;

            if (isSolved == false && timeIsUp == false)
            {
                interactText.SetActive(true);
            }
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
