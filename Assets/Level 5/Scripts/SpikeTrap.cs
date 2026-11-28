using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//a spike trap in the floor with two pictures: hidden and sharp.
//The trigger collider is on only when the spikes are sharp, so hidden spikes never hurt alex.
//Touching sharp spikes kills alex and Level 5 starts again
public class SpikeTrap : MonoBehaviour
{
    public SpriteRenderer sr;
    public Sprite hiddenSprite;
    public Sprite sharpSprite;
    public Collider2D spikeCollider; //trigger around the sharp spikes
    public float startDelay = 0f; //the second trap is not at the same time as the first one
    public float hiddenTime = 1.8f;
    public float sharpTime = 1.4f;
    public AudioClip spikeSound;

    // Start is called before the first frame update
    void Start()
    {
        //both traps start hidden
        sr.sprite = hiddenSprite;
        spikeCollider.enabled = false;
    }

    //called by the Level 5 Manager after the ghost is defeated
    public void StartTrap()
    {
        StartCoroutine(SpikesUpAndDown());
    }

    IEnumerator SpikesUpAndDown()
    {
        yield return new WaitForSeconds(startDelay);

        while (Level5Manager.Instance.levelComplete == false)
        {
            //hidden
            sr.sprite = hiddenSprite;
            spikeCollider.enabled = false; //the collider always matches the picture
            yield return new WaitForSeconds(hiddenTime);

            //sharp
            sr.sprite = sharpSprite;
            spikeCollider.enabled = true;
            Level5Manager.Instance.PlaySound(spikeSound);
            yield return new WaitForSeconds(sharpTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            Level5Manager.Instance.FailLevel("The spikes pierced Alex!");
        }
    }
}
