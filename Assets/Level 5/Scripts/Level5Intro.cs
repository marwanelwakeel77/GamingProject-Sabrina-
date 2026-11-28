using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//the start screen of Level 5. The sentences are written in the TextMeshPro objects in the Inspector,
//then they are typed letter by letter (like the dialogue in lab 9 and the start screen of level 2)
public class Level5Intro : MonoBehaviour
{
    public GameObject startScreen; //Level 5 Start Screen
    public TextMeshProUGUI titleText; //"THE CURSE OF SABRINA"
    public TextMeshProUGUI dialogueText; //the message in the big frame
    public TextMeshProUGUI lastLineText; //"The truth is waiting for you." in gold
    public TextMeshProUGUI buttonText; //"Press Any Key to Continue" in the small frame
    public float typingSpeed = 0.02f;
    public AudioClip continueSound;

    private string title;
    private string dialogue;
    private string lastLine;
    private string button;
    private bool typing = false;
    private bool skipTyping = false;
    private bool waitingForKey = false;

    // Start is called before the first frame update
    void Start()
    {
        //keep the sentences written in the Inspector, then empty the texts so they can be typed
        title = titleText.text;
        dialogue = dialogueText.text;
        lastLine = lastLineText.text;
        button = buttonText.text;
        titleText.text = "";
        dialogueText.text = "";
        lastLineText.text = "";
        buttonText.text = "";
        startScreen.SetActive(false);
    }

    //called by the Level 5 Manager when alex arrives
    public void ShowIntro()
    {
        StartCoroutine(TypeIntro());
    }

    IEnumerator TypeIntro()
    {
        startScreen.SetActive(true);
        yield return StartCoroutine(Level5Manager.Instance.FadeFromBlack());

        typing = true;
        foreach (char letter in title.ToCharArray()) //like TypeDialogue() in lab 9
        {
            if (skipTyping == true) break;
            titleText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        foreach (char letter in dialogue.ToCharArray())
        {
            if (skipTyping == true) break;
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        foreach (char letter in lastLine.ToCharArray())
        {
            if (skipTyping == true) break;
            lastLineText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        typing = false;

        //a key was pressed while typing: the whole message is shown at once
        titleText.text = title;
        dialogueText.text = dialogue;
        lastLineText.text = lastLine;

        yield return new WaitForSeconds(0.3f);
        buttonText.text = button; //the button appears when the message is written
        waitingForKey = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (typing == true && Input.anyKeyDown)
        {
            skipTyping = true;
        }
        else if (waitingForKey == true && Input.anyKeyDown)
        {
            waitingForKey = false;
            StartCoroutine(Continue());
        }
    }

    IEnumerator Continue()
    {
        Level5Manager.Instance.PlaySound(continueSound);
        yield return StartCoroutine(Level5Manager.Instance.FadeToBlack());
        startScreen.SetActive(false);
        Level5Manager.introSeen = true;
        yield return StartCoroutine(Level5Manager.Instance.FadeFromBlack());
        Level5Manager.Instance.UnfreezePlayer();
        Level5Manager.Instance.ShowHint("Explore the room. Find a way to restore the power.");
    }
}
