using System.Collections;
using UnityEngine;
using TMPro;

public class HintSystem : MonoBehaviour
{
    public GameObject hintPanel;
    public TMP_Text hintText;
    public TMP_Text footerText;
    public GameObject closeButton;
    public float typingSpeed = 0.02f;

    public BookMechanism bookMechanism;
    public AudioClip hintSound;

    public GameObject[] cipherPages;
    public string[] pageHints;

    public string indirectHint = "The three pages may reveal more than just symbols. Look closely at what they have in common.";
    public string directHint = "Combine the symbols from all three cipher pages to discover the title of the book. Search for that title on the bookshelf beneath the statue.";
    public string moonHint = "The crescent moon still hides on the balcony, and only moonlight can reveal it. A chain at the far end of the balcony opens the old window shutters.";
    public string solvedHint = "The passage behind the shelf stands open. Whatever Sabrina found lies beyond it.";

    private int hintStage = 0;

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
            else if (Time.timeScale > 0)
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
        Time.timeScale = 0f;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(hintSound);

        StartCoroutine(TypeHint(hint));
    }

    string ChooseHint()
    {
        if (bookMechanism.IsActivated())
        {
            footerText.text = "F to close";
            if (PlayerStats.hasCrescentMoon == false)
                return solvedHint + "\n\n" + moonHint;
            return solvedHint;
        }

        if (PlayerStats.cipherPagesCollected < 3)
        {
            footerText.text = "Cipher pages found: " + PlayerStats.cipherPagesCollected + " / 3";
            for (int i = 0; i < cipherPages.Length; i++)
            {
                if (cipherPages[i].activeSelf)
                    return pageHints[i];
            }
        }

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

    IEnumerator TypeHint(string hint)
    {
        hintText.text = "";

        foreach (char letter in hint.ToCharArray())
        {
            hintText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        closeButton.SetActive(true);
    }

    public void CloseHint()
    {
        StopAllCoroutines();
        hintText.text = "";
        hintPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
