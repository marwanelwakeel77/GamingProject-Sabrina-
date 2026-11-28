using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

//put this on the Level 5 Manager. It knows the order of Level 5 and starts the level again when alex fails:
//start screen -> switch -> ghost -> spikes -> device -> Sabrina's message -> door -> collapsing floor -> end screen
public class Level5Manager : MonoBehaviour
{
    public static Level5Manager Instance; //only one in the level (like the Audio Manager in lab 7)

    //static so the start screen does not appear again when the level restarts after alex dies (like the statics in lab 7)
    public static bool introSeen = false;

    public string levelName = "level 5"; //the scene that is loaded again when alex fails

    public PlayerController player;
    public PlayerShooting playerShooting;
    public Transform startPoint; //the Level 5 checkpoint, where alex comes in from Level 4

    public InventoryManager inventory; //the inventory of level 2
    public GameObject triangleIcon; //the Triangle Symbol icon in the inventory
    public GameObject moonIcon; //the Crescent Moon Symbol icon
    public GameObject sunIcon; //the Sun Symbol icon
    public bool giveAllSymbolsForTesting = true; //levels 3 and 4 are not made yet, so alex gets the moon and the sun here

    public Level5Intro intro;
    public GhostController ghost;
    public SpikeTrap[] spikeTraps;
    public GameObject ghostBarrier; //invisible wall, removed when the ghost is defeated
    public Level5EndScreen endScreen;

    public HintText hint; //the message frame (the script of level 2)
    public GameObject promptBox; //"Press E to ..." frame
    public TextMeshProUGUI promptText;
    public Image fader; //black image over the whole screen for the fades
    public TextMeshProUGUI faderText; //why alex died

    public AudioClip levelMusic;
    public AudioClip deathSound;
    public AudioClip checkpointSound;

    //the progress of the level
    public bool powerOn = false;
    public bool ghostDefeated = false;
    public bool deviceActivated = false;
    public bool messageDone = false;
    public bool floorCrossed = false;
    public bool levelComplete = false;
    public bool failing = false;

    //the symbols that are in the inventory now (0 = Sun, 1 = Moon, 2 = Triangle)
    private bool[] symbolInInventory = new bool[3];

    private Rigidbody2D playerRB;
    private Animator playerAnim;
    private SpriteRenderer playerSR;

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        playerRB = player.GetComponent<Rigidbody2D>();
        playerAnim = player.GetComponent<Animator>();
        playerSR = player.GetComponent<SpriteRenderer>();

        player.transform.position = startPoint.position; //alex starts at the Level 5 checkpoint
        promptBox.SetActive(false);
        ghostBarrier.SetActive(true);
        fader.gameObject.SetActive(true);
        fader.color = new Color(0, 0, 0, 1); //the level starts black, then fades in
        faderText.text = "";

        StartCoroutine(StartLevel());
    }

    IEnumerator StartLevel()
    {
        //wait one frame: the Inventory Manager hides every icon in its Start(), so the symbols are added after it
        yield return null;
        FreezePlayer();

        if (giveAllSymbolsForTesting == true)
        {
            PlayerStats.hasTriangle = true;
            PlayerStats.hasMoon = true;
            PlayerStats.hasSun = true;
        }

        //the symbols collected in levels 2, 3 and 4 are put in the inventory (also after a restart, so they are never lost)
        if (PlayerStats.hasSun == true)
        {
            inventory.AddItem(sunIcon);
            symbolInInventory[0] = true;
        }
        if (PlayerStats.hasMoon == true)
        {
            inventory.AddItem(moonIcon);
            symbolInInventory[1] = true;
        }
        if (PlayerStats.hasTriangle == true)
        {
            inventory.AddItem(triangleIcon);
            symbolInInventory[2] = true;
        }

        AudioManager.Instance.musicSource.enabled = true;
        AudioManager.Instance.PlayMusic(levelMusic); //like lab 7

        if (introSeen == false)
        {
            intro.ShowIntro(); //the start screen unfreezes alex when the player continues
        }
        else
        {
            //alex died before, so he goes straight back to the checkpoint
            yield return StartCoroutine(FadeFromBlack());
            PlaySound(checkpointSound);
            UnfreezePlayer();
            ShowHint("Back at the checkpoint. Find a way to restore the power.");
        }
    }

    //called by the electrical switch when the lights are back
    public void OnPowerRestored()
    {
        powerOn = true;
        ghost.Appear(); //the ghost appears only after the power is back
    }

    //called by the ghost when its health reaches zero
    public void OnGhostDefeated()
    {
        ghostDefeated = true;
        ghostBarrier.SetActive(false); //alex can go on to the spikes now
        ShowHint("The spirit has weakened... The path ahead is clear.");

        for (int i = 0; i < spikeTraps.Length; i++)
        {
            spikeTraps[i].StartTrap(); //the spikes start coming out of the floor
        }
    }

    public void OnDeviceActivated()
    {
        deviceActivated = true;
    }

    public void OnFinalMessageDone()
    {
        messageDone = true;
    }

    public void OnFloorCrossed()
    {
        floorCrossed = true;
    }

    //alex walked into the open door: the exit, then the End Screen
    public void CompleteLevel(Transform doorway)
    {
        if (levelComplete == true || failing == true)
        {
            return;
        }
        levelComplete = true;
        StartCoroutine(ExitThroughDoor(doorway));
    }

    IEnumerator ExitThroughDoor(Transform doorway)
    {
        HidePrompt();
        FreezePlayer();

        //alex walks into the light of the doorway (like the entrance of level 2)
        playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;
        playerSR.flipX = false;
        playerAnim.SetFloat("Speed", player.moveSpeed); //walking animation
        float walkTime = 0f;
        while (player.transform.position.x < doorway.position.x && walkTime < 2f)
        {
            playerRB.velocity = new Vector2(player.moveSpeed / 2, playerRB.velocity.y); //walking slowly with velocity like lab 2
            walkTime = walkTime + Time.deltaTime;
            yield return null;
        }
        playerRB.velocity = new Vector2(0, 0);
        playerAnim.SetFloat("Speed", 0);
        playerRB.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(0.3f);
        playerSR.enabled = false; //alex is gone through the door (like sr.enabled in lab 6)

        yield return StartCoroutine(FadeToBlack());
        endScreen.ShowEndScreen();
    }

    //every failure starts Level 5 again from the beginning
    public void FailLevel(string reason)
    {
        if (failing == true || levelComplete == true)
        {
            return; //only one restart at a time
        }
        failing = true;
        StartCoroutine(Fail(reason));
    }

    IEnumerator Fail(string reason)
    {
        FreezePlayer();
        HidePrompt();
        PlaySound(deathSound);
        playerSR.color = new Color(1f, 0.35f, 0.35f, 1f); //alex turns red

        faderText.text = reason + "\n\nBack to the Level 5 checkpoint...";
        yield return StartCoroutine(FadeToBlack());
        yield return new WaitForSeconds(1.8f);

        //loading the scene again resets everything in Level 5.
        //the symbols come back to the inventory because PlayerStats keeps them in statics
        SceneManager.LoadScene(levelName);
    }

    //the RETRY LEVEL button of the End Screen: Level 5 again, with the start screen
    public void RetryLevel()
    {
        introSeen = false;
        SceneManager.LoadScene(levelName);
    }

    //freezing the player in place (like the dialogue in lab 9)
    public void FreezePlayer()
    {
        player.enabled = false;
        playerShooting.enabled = false;
        inventory.CloseInventory();
        inventory.enabled = false; //H does not open the inventory behind a screen
        playerRB.velocity = new Vector2(0, playerRB.velocity.y);
        playerRB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        playerAnim.SetFloat("Speed", 0);
    }

    public void UnfreezePlayer()
    {
        if (failing == true || levelComplete == true)
        {
            return;
        }
        player.enabled = true;
        playerShooting.enabled = true;
        inventory.enabled = true;
        playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public bool PlayerIsFree()
    {
        return player.enabled == true && failing == false && levelComplete == false;
    }

    //0 = Sun, 1 = Moon, 2 = Triangle
    public bool HasSymbol(int symbol)
    {
        return symbolInInventory[symbol];
    }

    //a symbol put in the device leaves the inventory (it comes back if the level starts again)
    public void UseSymbol(int symbol)
    {
        symbolInInventory[symbol] = false;
        if (symbol == 0) inventory.RemoveItem(sunIcon);
        if (symbol == 1) inventory.RemoveItem(moonIcon);
        if (symbol == 2) inventory.RemoveItem(triangleIcon);
    }

    public void ShowHint(string message)
    {
        hint.ShowHint(message);
    }

    public void ShowPrompt(string message)
    {
        promptText.text = message;
        promptBox.SetActive(true);
    }

    public void HidePrompt()
    {
        promptBox.SetActive(false);
    }

    //the sounds of Level 5 go through the Audio Manager (like the coin pickup in lab 7)
    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioManager.Instance.PlayRandomSFX(clip);
        }
    }

    //the black image becomes visible (like the Lerp in lab 4)
    public IEnumerator FadeToBlack()
    {
        fader.gameObject.SetActive(true);
        float t = 0f;
        while (t < 1f)
        {
            t = t + Time.deltaTime / 0.8f;
            fader.color = new Color(0, 0, 0, Mathf.Lerp(0f, 1f, t));
            yield return null;
        }
    }

    //the black image disappears
    public IEnumerator FadeFromBlack()
    {
        faderText.text = "";
        float t = 0f;
        while (t < 1f)
        {
            t = t + Time.deltaTime / 0.8f;
            fader.color = new Color(0, 0, 0, Mathf.Lerp(1f, 0f, t));
            yield return null;
        }
        fader.gameObject.SetActive(false);
    }
}
