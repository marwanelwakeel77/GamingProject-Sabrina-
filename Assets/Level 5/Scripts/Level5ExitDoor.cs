using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//the big exit door. It opens once, after Sabrina's final message (closed -> opening -> open with golden light)
public class Level5ExitDoor : MonoBehaviour
{
    public Animator doorAnim; //bool isOpen (like the doors of level 2 and the animations in lab 3)
    public GameObject doorBlock; //a wall in front of the closed door
    public GameObject floorBlock; //a wall before the collapsing floor
    public SpriteRenderer doorLight; //golden light in the doorway
    public ParticleSystem lightEffect; //like the particles in lab 4
    public AudioClip openSound;
    public Transform doorway; //alex walks to this point when he leaves

    private bool open = false;
    private bool leaving = false;

    // Start is called before the first frame update
    void Start()
    {
        doorAnim.SetBool("isOpen", false);
        doorBlock.SetActive(true);
        floorBlock.SetActive(true);
        doorLight.color = new Color(doorLight.color.r, doorLight.color.g, doorLight.color.b, 0);
    }

    public void OpenDoor()
    {
        //only once, and never before Sabrina's message
        if (open == true || Level5Manager.Instance.messageDone == false)
        {
            return;
        }
        open = true;
        StartCoroutine(Opening());
    }

    IEnumerator Opening()
    {
        doorAnim.SetBool("isOpen", true);
        Level5Manager.Instance.PlaySound(openSound);
        lightEffect.Play();

        //the golden light comes in the doorway (like the Lerp in lab 4)
        Color c = doorLight.color;
        float t = 0f;
        while (t < 1f)
        {
            t = t + Time.deltaTime / 1.2f;
            doorLight.color = new Color(c.r, c.g, c.b, Mathf.Lerp(0f, 1f, t));
            yield return null;
        }

        doorBlock.SetActive(false);
        floorBlock.SetActive(false);
        Level5Manager.Instance.ShowHint("The door is open! Cross the floor quickly... it will not hold for long.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && open == true && leaving == false && Level5Manager.Instance.floorCrossed == true)
        {
            leaving = true;
            Level5Manager.Instance.CompleteLevel(doorway);
        }
    }
}
