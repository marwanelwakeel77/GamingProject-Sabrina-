using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CalendarDialogue : MonoBehaviour
{
    public TextMeshProUGUI textDisplay; //the text inside the dialogue box
    public string sentence = "REVEAL THE FOUR DATES & SOLVE THE PUZZLE";
    public float typingSpeed = 0.02f;
    public GameObject closeButton; //the X button

    private bool wasClosed = false;
    private bool isShown = false;

    //called by the calendar when the player opens it with E
    public void ShowDialogue()
    {
        //do not open it again after the player closed it
        if (wasClosed == false && isShown == false)
        {
            isShown = true;
            gameObject.SetActive(true);
            StartCoroutine(TypeDialogue());
        }
    }

    //types the sentence letter by letter (like the dialogue in lab 9)
    IEnumerator TypeDialogue()
    {
        textDisplay.text = "";
        closeButton.SetActive(false);

        foreach (char letter in sentence.ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        closeButton.SetActive(true); //the X appears when the whole sentence is written
    }

    //linked to the OnClick() of the X button in the Inspector
    public void CloseDialogue()
    {
        wasClosed = true;
        HideDialogue();
    }

    //called by the calendar when it closes, so the dialogue appears again next time
    public void HideDialogue()
    {
        isShown = false;
        gameObject.SetActive(false);
    }
}
