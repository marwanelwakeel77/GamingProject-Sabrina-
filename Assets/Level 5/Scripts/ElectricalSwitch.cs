using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//the switch on the wall. Alex presses E near it to bring the power back (only once)
public class ElectricalSwitch : MonoBehaviour
{
    public SpriteRenderer sr;
    public Sprite offSprite; //red button
    public Sprite onSprite; //green button
    public AudioClip switchSound;
    public ParticleSystem sparks; //like the particles in lab 4
    public RoomPower room; //the lights of the room

    private bool playerNear = false;
    private bool used = false;

    // Start is called before the first frame update
    void Start()
    {
        sr.sprite = offSprite;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerNear == true && used == false && Level5Manager.Instance.PlayerIsFree() == true)
        {
            Level5Manager.Instance.ShowPrompt("Press E to restore power.");

            if (Input.GetKeyDown(KeyCode.E))
            {
                used = true; //the switch works only once
                Level5Manager.Instance.HidePrompt();
                sr.sprite = onSprite;
                Level5Manager.Instance.PlaySound(switchSound);
                sparks.Play();
                StartCoroutine(room.PowerUp());
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNear = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNear = false;
            Level5Manager.Instance.HidePrompt();
        }
    }
}
