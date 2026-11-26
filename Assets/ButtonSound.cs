using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//put this on a close button, then add PlaySound() to the OnClick() of the button (like the buttons in lab 8)
public class ButtonSound : MonoBehaviour
{
    public AudioClip clickSound; //the X sound

    //linked to the OnClick() of the button
    public void PlaySound()
    {
        AudioManager.Instance.PlayMusicSFX(clickSound); //like the sounds in lab 7
    }
}
