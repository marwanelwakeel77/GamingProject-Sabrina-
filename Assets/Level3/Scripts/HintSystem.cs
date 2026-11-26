using System.Collections;
using UnityEngine;
using TMPro;

// Hints on the F key, written like the Dialogue Manager in Lab 9:
// a panel, text typed letter by letter with a Coroutine, and a button that appears when the text is done.
public class HintSystem : MonoBehaviour
{
    public GameObject hintPanel;
    public TMP_Text hintText;
    public TMP_Text footerText;
    public GameObject closeButton;
    public float typingSpeed = 0.02f;

    public BookMechanism bookMechanism;
    public AudioClip hintSound;

    // while pages are missing, the hint points to the first missing page
    public GameObject[] cipherPages;
    public string[] pageHints;

    public string indirectHint = "The three pages may reveal more than just symbols. Look closely at what they have in common.";
    public string directHint = "Combine the symbols from all three cipher pages to discover the title of the book. Search for that title on the bookshelf beneath the statue.";
    public string solvedHint = "The passage behind the shelf stands open. Whatever Sabrina found lies beyond it.";

    private int hintStage = 0; //remembers which hint the player already got

    void Start()
    {
        hintPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (hintPanel.activeSelf)
                CloseHint();
            else if (Time.timeScale > 0) //no hint while another panel is open
                OpenHint();
        }
        else if (hintPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseHint();
        }
    }

    void OpenHint()
    {
        string hint = ChooseHint();

        hintPanel.SetActive(true);
        closeButton.SetActive(false);
        Time.timeScale = 0f; //pause the game while reading

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(hintSound);

        StartCoroutine(TypeHint(hint));
    }

    string ChooseHint()
    {
        if (bookMechanism.IsActivated())
        {
            footerText.text = "F to close";
            return solvedHint;
        }

        // pages still missing: point to the first one that is still in the level
        if (PlayerStats.cipherPagesCollected < 3)
        {
            footerText.text = "Cipher pages found: " + PlayerStats.cipherPagesCollected + " / 3";
            for (int i = 0; i < cipherPages.Length; i++)
            {
                if (cipherPages[i].activeSelf)
                    return pageHints[i];
            }
        }

        // all pages found: first the indirect hint, then the direct one
        if (hintStage == 0)
        {
            hintStage = 1;
            footerText.text = "Hint 1 of 2        Press F again later for a clearer hint";
            return indirectHint;
        }
        else
        {
            hintStage = 2;
            footerText.text = "Hint 2 of 2";
            return directHint;
        }
    }

    //types the hint letter by letter, like TypeDialogue() in Lab 9
    IEnumerator TypeHint(string hint)
    {
        hintText.text = "";

        foreach (char letter in hint.ToCharArray())
        {
            hintText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed); //Realtime, because the game is paused
        }

        closeButton.SetActive(true);
    }

    // OnClick of the Close button
    public void CloseHint()
    {
        StopAllCoroutines();
        hintText.text = "";
        hintPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
