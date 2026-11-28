using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//put this on an empty GameObject (Hint Manager), like the DialogueManager in lab 9
public class HintText : MonoBehaviour
{
    public TextMeshProUGUI textDisplay; //the text inside the frame
    public GameObject hintBox; //the frame ("task completed")
    public float typingSpeed = 0.03f;
    public float showTime = 2.5f; //how long the message stays on the screen

    private int hintNumber = 0; //the number of the newest message

    // Start is called before the first frame update
    void Start()
    {
        //the frame is hidden until there is a message
        textDisplay.text = "";
        hintBox.SetActive(false);
    }

    //other scripts call this to show a message, e.g. "All four dates are revealed!"
    public void ShowHint(string message)
    {
        hintNumber++;
        StartCoroutine(TypeHint(message, hintNumber));
    }

    //linked to the OnClick() of a close button on the frame (Level 5): hides the message at once
    public void CloseHint()
    {
        hintNumber++; //the message that is typing or waiting stops, it will not show or hide anything any more
        textDisplay.text = "";
        hintBox.SetActive(false);
    }

    //shows the frame, types the message letter by letter, waits, then hides the frame
    IEnumerator TypeHint(string message, int number)
    {
        hintBox.SetActive(true);
        textDisplay.text = "";

        foreach (char letter in message.ToCharArray())
        {
            //a newer message started, so this old one stops typing
            if (number != hintNumber)
            {
                break;
            }

            textDisplay.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(showTime);

        //only the newest message hides the frame
        if (number == hintNumber)
        {
            textDisplay.text = "";
            hintBox.SetActive(false);
        }
    }
}
