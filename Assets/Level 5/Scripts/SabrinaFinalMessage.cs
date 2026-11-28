using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//Sabrina's final message. It appears once, only after the device is activated.
//The text is written in the TextMeshPro objects in the Inspector and typed letter by letter (lab 9)
public class SabrinaFinalMessage : MonoBehaviour
{
    public GameObject messageScreen;
    public TextMeshProUGUI titleText; //"SABRINA'S FINAL MESSAGE"
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI signatureText; //"— Sabrina"
    public GameObject continueButton; //OnClick() calls Continue()
    public Level5ExitDoor exitDoor;
    public float typingSpeed = 0.03f;
    public AudioClip messageMusic; //soft and mysterious while the message is on the screen
    public AudioClip levelMusic;
    public AudioClip continueSound;

    private string title;
    private string message;
    private string signature;
    private bool shown = false;
    private bool typing = false;
    private bool skipTyping = false;
    private bool closing = false;

    // Start is called before the first frame update
    void Start()
    {
        title = titleText.text;
        message = messageText.text;
        signature = signatureText.text;
        titleText.text = "";
        messageText.text = "";
        signatureText.text = "";
        continueButton.SetActive(false);
        messageScreen.SetActive(false);
    }

    public void ShowMessage()
    {
        //exactly once, and never before the device is activated
        if (shown == true || Level5Manager.Instance.deviceActivated == false)
        {
            return;
        }
        shown = true;
        StartCoroutine(TypeMessage());
    }

    IEnumerator TypeMessage()
    {
        Level5Manager.Instance.FreezePlayer();
        Level5Manager.Instance.HidePrompt();
        yield return StartCoroutine(Level5Manager.Instance.FadeToBlack());
        messageScreen.SetActive(true);
        AudioManager.Instance.PlayMusic(messageMusic); //like lab 7
        yield return StartCoroutine(Level5Manager.Instance.FadeFromBlack());

        typing = true;
        foreach (char letter in title.ToCharArray()) //like TypeDialogue() in lab 9
        {
            if (skipTyping == true) break;
            titleText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        yield return new WaitForSeconds(0.5f);
        foreach (char letter in message.ToCharArray())
        {
            if (skipTyping == true) break;
            messageText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        typing = false;

        //a key was pressed while typing: the whole message is shown at once
        titleText.text = title;
        messageText.text = message;

        yield return new WaitForSeconds(1f);
        signatureText.text = signature;
        yield return new WaitForSeconds(1.5f); //time to read the last lines
        continueButton.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (typing == true && Input.anyKeyDown)
        {
            skipTyping = true;
        }
    }

    //linked to the OnClick() of the Continue button (like the buttons in lab 9)
    public void Continue()
    {
        if (closing == true)
        {
            return;
        }
        closing = true;
        StartCoroutine(CloseMessage());
    }

    IEnumerator CloseMessage()
    {
        Level5Manager.Instance.PlaySound(continueSound);
        continueButton.SetActive(false);
        yield return StartCoroutine(Level5Manager.Instance.FadeToBlack());
        messageScreen.SetActive(false);
        AudioManager.Instance.PlayMusic(levelMusic);
        yield return StartCoroutine(Level5Manager.Instance.FadeFromBlack());

        Level5Manager.Instance.OnFinalMessageDone();
        Level5Manager.Instance.UnfreezePlayer();
        exitDoor.OpenDoor(); //the door opens only after the message
    }
}
