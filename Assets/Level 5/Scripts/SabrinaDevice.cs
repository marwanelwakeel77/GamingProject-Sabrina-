using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Sabrina's device (works like the calendar of level 2).
//The first E opens the device screen. Left/Right arrows choose a symbol, E puts it in the glowing position.
//20 seconds for the whole puzzle: closing the screen does not stop or reset the timer.
//Symbols: 0 = Sun, 1 = Moon, 2 = Triangle (the same order as the positions on the device)
public class SabrinaDevice : MonoBehaviour
{
    public GameObject devicePanel; //the full screen of the device
    public Image[] slotGlows; //the glow of the 3 positions
    public Image[] slotSymbols; //the symbol in each position, hidden until it is put in
    public GameObject[] choices; //the symbols from the inventory at the bottom of the screen
    public GameObject[] choiceHighlights; //gold glow behind the chosen symbol
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI timerText;
    public Image timerBar;
    public GameObject hudTimer; //small timer on the game screen when the device screen is closed
    public TextMeshProUGUI hudTimerText;
    public float timeLimit = 20f;
    public AudioSource timerSound; //ticking sound, Loop checked (like the audio sources in lab 7)
    public AudioClip insertSound;
    public AudioClip wrongSound;
    public AudioClip timeUpSound;

    //the device in the room
    public SpriteRenderer[] symbolLights; //sun, moon and triangle light up one by one
    public SpriteRenderer coreGlow; //the big red circle
    public ParticleSystem magicEffect; //like the particles in lab 4
    public SpriteRenderer flash;
    public AudioClip activationSound;
    public SabrinaFinalMessage finalMessage;

    private string[] symbolNames = { "Sun Symbol", "Crescent Moon Symbol", "Triangle Symbol" };
    private Color[] glowColors = new Color[3];
    private bool[] filled = new bool[3];
    private bool playerNear = false;
    private bool panelOpen = false;
    private bool timerOn = false;
    private bool timerStarted = false;
    private bool solved = false;
    private float timeLeft;
    private int selected = 0;

    // Start is called before the first frame update
    void Start()
    {
        devicePanel.SetActive(false);
        hudTimer.SetActive(false);
        timeLeft = timeLimit;

        for (int i = 0; i < 3; i++)
        {
            glowColors[i] = slotGlows[i].color;
            slotSymbols[i].enabled = false;
            symbolLights[i].color = new Color(symbolLights[i].color.r, symbolLights[i].color.g, symbolLights[i].color.b, 0);
        }
        coreGlow.color = new Color(coreGlow.color.r, coreGlow.color.g, coreGlow.color.b, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerNear == true && panelOpen == false && solved == false && Level5Manager.Instance.PlayerIsFree() == true)
        {
            Level5Manager.Instance.ShowPrompt("Press E to interact with Sabrina's device.");
        }

        //first E opens the device, after that E puts the chosen symbol in
        if (playerNear == true && solved == false && Level5Manager.Instance.failing == false && Input.GetKeyDown(KeyCode.E))
        {
            if (panelOpen == false)
            {
                if (Level5Manager.Instance.PlayerIsFree() == true)
                {
                    OpenDevice();
                }
            }
            else
            {
                InsertSymbol();
            }
        }

        if (panelOpen == true && solved == false)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveSelection(1);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveSelection(-1);
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseDevice();
            }
        }

        //the timer keeps counting while the screen is open or closed (like the calendar of level 2)
        if (timerOn == true)
        {
            timeLeft = timeLeft - Time.deltaTime;
            timerText.text = "" + (int)(timeLeft + 1);
            hudTimerText.text = "DEVICE " + (int)(timeLeft + 1) + "s";
            timerBar.fillAmount = timeLeft / timeLimit;

            if (timeLeft <= 0)
            {
                TimeUp();
            }
        }
    }

    //true when all three symbols are in the inventory or already in the device
    bool AllSymbolsFound()
    {
        for (int i = 0; i < 3; i++)
        {
            if (filled[i] == false && Level5Manager.Instance.HasSymbol(i) == false)
            {
                return false;
            }
        }
        return true;
    }

    void OpenDevice()
    {
        if (AllSymbolsFound() == false)
        {
            Level5Manager.Instance.ShowHint("Missing symbol. I need to find all three symbols first.");
            return;
        }

        panelOpen = true;
        devicePanel.SetActive(true);
        hudTimer.SetActive(false);
        Level5Manager.Instance.HidePrompt();
        Level5Manager.Instance.FreezePlayer(); //the arrows choose the symbols, they do not move alex
        feedbackText.text = "Choose a symbol with the arrows and press E to place it in the glowing position.";
        feedbackText.color = Color.white;
        ShowChoices();

        //the timer starts the first time only, so opening the device again does not reset it
        if (timerStarted == false)
        {
            timerStarted = true;
            timerOn = true;
            timeLeft = timeLimit;
            timerSound.Play();
        }
    }

    void CloseDevice()
    {
        panelOpen = false;
        devicePanel.SetActive(false);
        Level5Manager.Instance.UnfreezePlayer();
        if (timerOn == true)
        {
            hudTimer.SetActive(true);
        }
    }

    //the first position that is still empty: 0 = Sun, 1 = Moon, 2 = Triangle
    int NextSlot()
    {
        for (int i = 0; i < 3; i++)
        {
            if (filled[i] == false)
            {
                return i;
            }
        }
        return -1;
    }

    //only the symbols that are still in the inventory can be chosen
    void ShowChoices()
    {
        for (int i = 0; i < 3; i++)
        {
            choices[i].SetActive(Level5Manager.Instance.HasSymbol(i));
        }
        if (Level5Manager.Instance.HasSymbol(selected) == false)
        {
            MoveSelection(1);
        }
        ShowHighlight();
    }

    void MoveSelection(int step)
    {
        //go to the next symbol that is in the inventory
        for (int tries = 0; tries < 3; tries++)
        {
            selected = selected + step;
            if (selected > 2) selected = 0;
            if (selected < 0) selected = 2;
            if (Level5Manager.Instance.HasSymbol(selected) == true)
            {
                break;
            }
        }
        ShowHighlight();
    }

    void ShowHighlight()
    {
        for (int i = 0; i < 3; i++)
        {
            choiceHighlights[i].SetActive(i == selected);
        }
    }

    void InsertSymbol()
    {
        int slot = NextSlot();
        if (slot < 0 || Level5Manager.Instance.HasSymbol(selected) == false)
        {
            return;
        }

        if (selected != slot)
        {
            //wrong symbol: the device refuses it. The player can try again while there is time
            Level5Manager.Instance.PlaySound(wrongSound);
            feedbackText.text = "The device rejects the " + symbolNames[selected] + ". It does not fit this position.";
            feedbackText.color = new Color(1f, 0.35f, 0.3f);
            StartCoroutine(RedFlash(slot));
            return;
        }

        //right symbol: it goes in the device and leaves the inventory
        filled[slot] = true;
        Level5Manager.Instance.UseSymbol(selected);
        Level5Manager.Instance.PlaySound(insertSound);
        slotSymbols[slot].enabled = true;
        slotGlows[slot].color = glowColors[slot];
        StartCoroutine(GrowSymbol(slotSymbols[slot].transform));
        feedbackText.text = "The " + symbolNames[selected] + " fits! The device hums...";
        feedbackText.color = new Color(1f, 0.82f, 0.48f);
        ShowChoices();

        if (NextSlot() == -1)
        {
            StartCoroutine(Activate());
        }
    }

    //the symbol gets smaller into its place
    IEnumerator GrowSymbol(Transform symbol)
    {
        float t = 0f;
        while (t < 1f)
        {
            t = t + Time.deltaTime / 0.3f;
            float size = Mathf.Lerp(1.6f, 1f, t);
            symbol.localScale = new Vector3(size, size, 1);
            yield return null;
        }
    }

    IEnumerator RedFlash(int slot)
    {
        slotGlows[slot].color = Color.red;
        yield return new WaitForSeconds(0.4f);
        slotGlows[slot].color = glowColors[slot];
    }

    //the three symbols are in the right places in time
    IEnumerator Activate()
    {
        solved = true;
        timerOn = false; //the timer stops
        timerSound.Stop();
        hudTimer.SetActive(false);
        feedbackText.text = "All three symbols are in place!";
        yield return new WaitForSeconds(1.3f);

        panelOpen = false;
        devicePanel.SetActive(false); //alex stays frozen until Sabrina's message is finished
        Level5Manager.Instance.OnDeviceActivated();
        Level5Manager.Instance.PlaySound(activationSound);

        //the sun, the moon and the triangle light up one after the other (like the Lerp in lab 4)
        for (int i = 0; i < 3; i++)
        {
            Color c = symbolLights[i].color;
            float t = 0f;
            while (t < 1f)
            {
                t = t + Time.deltaTime / 0.5f;
                symbolLights[i].color = new Color(c.r, c.g, c.b, Mathf.Lerp(0f, 1f, t));
                yield return null;
            }
        }

        //the red core wakes up, with magic particles and a flash of light
        magicEffect.Play();
        Color core = coreGlow.color;
        float f = 0f;
        while (f < 1f)
        {
            f = f + Time.deltaTime / 1.2f;
            coreGlow.color = new Color(core.r, core.g, core.b, Mathf.Lerp(0f, 1f, f));
            flash.color = new Color(1f, 0.55f, 0.45f, Mathf.Lerp(0.6f, 0f, f));
            yield return null;
        }

        Level5Manager.Instance.ShowHint("Sabrina's device is activated!");
        yield return new WaitForSeconds(3f);
        finalMessage.ShowMessage(); //Sabrina's final message appears only here
    }

    //20 seconds are over: the level starts again and the symbols go back to the inventory
    void TimeUp()
    {
        timerOn = false;
        timerSound.Stop();
        panelOpen = false;
        devicePanel.SetActive(false);
        hudTimer.SetActive(false);
        Level5Manager.Instance.PlaySound(timeUpSound);
        Level5Manager.Instance.FailLevel("Time expired! The device has rejected the symbols.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNear = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNear = false;
            Level5Manager.Instance.HidePrompt();
        }
    }
}
