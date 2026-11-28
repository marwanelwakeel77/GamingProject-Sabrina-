using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//put this on alex, together with an Audio Source that has the walking sound (Loop checked, Play On Awake unchecked)
public class PlayerFootsteps : MonoBehaviour
{
    private AudioSource footsteps;
    private Rigidbody2D rb;
    private bool soundOn = false;

    // Start is called before the first frame update
    void Start()
    {
        footsteps = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //alex is walking when he moves left or right and is not jumping or falling
        bool walking = Mathf.Abs(rb.velocity.x) > 0.1f && Mathf.Abs(rb.velocity.y) < 0.1f;

        if (walking == true && soundOn == false)
        {
            footsteps.enabled = true;
            footsteps.Play(); //like the audio sources in lab 7
            soundOn = true;
        }

        if (walking == false && soundOn == true)
        {
            footsteps.enabled = false; //turning the audio source off stops the sound (like sr.enabled in lab 6)
            soundOn = false;
        }
    }
}
