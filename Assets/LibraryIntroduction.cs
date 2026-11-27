using System.Collections;
using UnityEngine;
using TMPro;

public class LibraryIntroduction : MonoBehaviour
{
    public GameObject introPanel;
    public TMP_Text introText;
    public GameObject startButton;

    public float typingSpeed = 0.04f;

    private string fullText;

    void Start()
    {
        Time.timeScale = 0f;

        introPanel.SetActive(true);

        fullText = introText.text;
        introText.text = "";

        startButton.SetActive(false);

        StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        foreach (char letter in fullText)
        {
            introText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        startButton.SetActive(true);
    }

    public void CloseIntroduction()
    {
        StopAllCoroutines();

        introPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
