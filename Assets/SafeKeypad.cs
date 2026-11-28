using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SafeKeypad : MonoBehaviour
{
    public GameObject keypadPanel; //the safe panel with the number buttons
    public TMP_InputField[] numberSlots; //the 7 boxes where the numbers are written, from left to right
    public TextMeshProUGUI wrongText; //red text: "Wrong passcode entered, try again"
    public GameObject permitIcon; //the Library Permit icon in the inventory
    public string password = "5152529"; //the 4 hidden dates: 5, 15, 25, 29
    public GameObject interactText; //"Press E To Interact" in the Canvas
    public GameObject collectText; //"Press E To Collect" in the Canvas
    public CalendarPuzzle requiredPuzzle; //the calendar must be solved first, because the passcode is the 4 dates
    public GameObject solvePuzzleHint; //"Solve The Puzzle To Collect The Passcode." in the Canvas

    public AudioClip keypadSound; //when a button is pressed
    public AudioClip wrongSound; //wrong passcode
    public AudioClip correctSound; //the safe opens
    public AudioClip pickupSound; //taking the Library Permit

    private int count = 0; //how many numbers are written
    private bool playerNear = false;
    private bool keypadOpen = false;
    private bool safeOpen = false;
    private bool permitTaken = false;
    private Animator anim;
    private Rigidbody2D playerRB;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        playerRB = FindObjectOfType<PlayerController>().GetComponent<Rigidbody2D>();
        keypadPanel.SetActive(false);
        interactText.SetActive(false);
        collectText.SetActive(false);
        solvePuzzleHint.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerNear == true && keypadOpen == false && Input.GetKeyDown(KeyCode.E))
        {
            if (safeOpen == false)
            {
                //the player does not know the passcode before solving the calendar
                if (requiredPuzzle.isSolved == false)
                {
                    interactText.SetActive(false);
                    solvePuzzleHint.SetActive(true);
                }
                else
                {
                    OpenKeypad();
                }
            }
            else if (permitTaken == false)
            {
                TakePermit();
            }
        }

        if (keypadOpen == true && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseKeypad();
        }
    }

    void OpenKeypad()
    {
        keypadOpen = true;
        ClearCode();
        wrongText.gameObject.SetActive(false);
        keypadPanel.SetActive(true);
        interactText.SetActive(false);
        playerRB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY; //freezing the player in place while typing
    }

    //linked to the X at the top right: closes the keypad
    public void CloseKeypad()
    {
        keypadOpen = false;
        keypadPanel.SetActive(false);
        playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;
        ShowPressE();
    }

    //shows "Press E To Interact" when the safe is closed, and "Press E To Collect" when the permit is inside the open safe
    void ShowPressE()
    {
        if (playerNear == true)
        {
            if (safeOpen == false)
            {
                interactText.SetActive(true);
            }
            else if (permitTaken == false)
            {
                collectText.SetActive(true);
            }
        }
    }

    //linked to the number buttons (0-9): writes the number in the next empty box
    public void PressNumber(int number)
    {
        if (count < numberSlots.Length)
        {
            AudioManager.Instance.PlayMusicSFX(keypadSound);
            numberSlots[count].text = "" + number;
            count++;
            wrongText.gameObject.SetActive(false);
        }
    }

    //linked to the X at the bottom: removes the last number
    public void RemoveLastNumber()
    {
        if (count > 0)
        {
            AudioManager.Instance.PlayMusicSFX(keypadSound);
            count--;
            numberSlots[count].text = "";
        }
    }

    //empties all the boxes
    void ClearCode()
    {
        count = 0;
        for (int i = 0; i < numberSlots.Length; i++)
        {
            numberSlots[i].text = "";
        }
    }

    //linked to the correct sign: checks the passcode
    public void CheckCode()
    {
        //put the written numbers together, from left to right
        string code = "";
        for (int i = 0; i < count; i++)
        {
            code = code + numberSlots[i].text;
        }

        if (code == password)
        {
            safeOpen = true;
            AudioManager.Instance.PlayMusicSFX(correctSound);
            anim.SetBool("IsOpen", true); //plays the open animation of the safe
            CloseKeypad();
            FindObjectOfType<HintText>().ShowHint("Safe opened");
        }
        else
        {
            ClearCode();
            AudioManager.Instance.PlayMusicSFX(wrongSound);
            wrongText.gameObject.SetActive(true); //the red text appears
        }
    }

    void TakePermit()
    {
        permitTaken = true;
        collectText.SetActive(false);
        AudioManager.Instance.PlayRandomSFX(pickupSound); //like the coin pickup in lab 7
        PlayerStats.hasLibraryPermit = true;
        FindObjectOfType<InventoryManager>().AddItem(permitIcon);
        FindObjectOfType<HintText>().ShowHint("You took the Library Permit.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNear = true;
            ShowPressE();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNear = false;
            interactText.SetActive(false);
            collectText.SetActive(false);
            solvePuzzleHint.SetActive(false);
        }
    }
}
