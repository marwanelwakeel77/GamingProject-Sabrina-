using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

//the End Screen of the game. It appears only after alex goes through the door.
//First the ending story, then THE END with two buttons: MAIN MENU and RETRY LEVEL (like the buttons in lab 9)
public class Level5EndScreen : MonoBehaviour
{
    public GameObject endScreen;
    public GameObject storyPanel; //dark panel with "THE CURSE IS BROKEN" and the story
    public TextMeshProUGUI storyTitle;
    public TextMeshProUGUI storyText;
    public GameObject buttons; //MAIN MENU and RETRY LEVEL
    public TextMeshProUGUI messageText; //small message under the buttons
    public float typingSpeed = 0.03f;
    public string mainMenuScene = ""; //the name of the Main Menu scene in File > Build Settings (not made yet)
    public AudioClip endingMusic;
    public AudioClip clickSound;

    private string title;
    private string story;
    private bool shown = false;
    private bool leaving = false;

    // Start is called before the first frame update
    void Start()
    {
        title = storyTitle.text;
        story = storyText.text;
        storyTitle.text = "";
        storyText.text = "";
        messageText.text = "";
        endScreen.SetActive(false);
    }

    //called by the Level 5 Manager when alex has gone through the door
    public void ShowEndScreen()
    {
        if (shown == true)
        {
            return; //only once
        }
        shown = true;
        StartCoroutine(TypeStory());
    }

    IEnumerator TypeStory()
    {
        endScreen.SetActive(true);
        storyPanel.SetActive(true);
        buttons.SetActive(false);
        AudioManager.Instance.musicSource.enabled = true;
        AudioManager.Instance.PlayMusic(endingMusic); //like lab 7
        yield return StartCoroutine(Level5Manager.Instance.FadeFromBlack());

        foreach (char letter in title.ToCharArray()) //like TypeDialogue() in lab 9
        {
            storyTitle.text += letter;
            yield return new WaitForSeconds(typingSpeed * 2);
        }
        yield return new WaitForSeconds(0.4f);
        foreach (char letter in story.ToCharArray())
        {
            storyText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        yield return new WaitForSeconds(3f); //time to read

        //the story goes away and THE END appears with the buttons
        yield return StartCoroutine(Level5Manager.Instance.FadeToBlack());
        storyPanel.SetActive(false);
        buttons.SetActive(true);
        yield return StartCoroutine(Level5Manager.Instance.FadeFromBlack());
    }

    //linked to the OnClick() of the MAIN MENU button
    public void MainMenu()
    {
        if (leaving == true)
        {
            return;
        }
        Level5Manager.Instance.PlaySound(clickSound);

        if (mainMenuScene == "")
        {
            messageText.text = "The Main Menu scene is not in the game yet.";
            return;
        }

        leaving = true;
        Level5Manager.introSeen = false; //next time Level 5 is played, the start screen appears again
        StartCoroutine(LeaveTo(mainMenuScene));
    }

    //linked to the OnClick() of the RETRY LEVEL button
    public void RetryLevel()
    {
        if (leaving == true)
        {
            return;
        }
        leaving = true;
        Level5Manager.Instance.PlaySound(clickSound);
        StartCoroutine(LeaveTo(Level5Manager.Instance.levelName));
    }

    IEnumerator LeaveTo(string scene)
    {
        yield return StartCoroutine(Level5Manager.Instance.FadeToBlack());
        if (scene == Level5Manager.Instance.levelName)
        {
            Level5Manager.Instance.RetryLevel(); //Level 5 again from the beginning, with the start screen
        }
        else
        {
            SceneManager.LoadScene(scene);
        }
    }
}
