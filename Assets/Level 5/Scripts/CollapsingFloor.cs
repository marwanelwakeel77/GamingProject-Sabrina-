using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//the floor before the exit door. When alex first steps on it, it starts breaking: 8 pictures in 7 seconds.
//Alex can't jump here, he must walk. If the floor is gone before he reaches the other side, he falls into the fire
public class CollapsingFloor : MonoBehaviour
{
    public SpriteRenderer[] parts; //the pieces of the floor, all use the same 8 pictures
    public Sprite[] stages; //8 pictures from whole to broken
    public Collider2D floorCollider; //the ground alex walks on
    public float collapseTime = 7f;
    public float startX; //where the floor begins: the timer starts here
    public Transform safePoint; //the other side: alex is safe after this point
    public PlayerController player;
    public GameObject countdownBox;
    public TextMeshProUGUI countdownText;
    public AudioClip crackSound;
    public ParticleSystem dust; //like the particles in lab 4

    private bool started = false;
    private bool finished = false;
    private bool playerOnFloor = false;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].sprite = stages[0];
        }
        countdownBox.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerOnFloor == true && finished == false)
        {
            player.canJump = false; //no jumping over the floor, alex must walk

            //the timer starts the first time alex touches the floor and never starts again
            if (started == false && player.transform.position.x >= startX)
            {
                started = true;
                StartCoroutine(Collapse());
            }
        }

        //alex reached the other side in time
        if (started == true && finished == false && player.transform.position.x >= safePoint.position.x)
        {
            finished = true; //the floor stops breaking
            countdownBox.SetActive(false);
            player.canJump = true;
            Level5Manager.Instance.OnFloorCrossed();
            Level5Manager.Instance.ShowHint("You made it! Go through the door.");
        }
    }

    IEnumerator Collapse()
    {
        Level5Manager.Instance.PlaySound(crackSound);
        countdownBox.SetActive(true);
        dust.Play();

        float t = 0f;
        int stage = 0;
        while (t < collapseTime && finished == false)
        {
            t = t + Time.deltaTime;
            countdownText.text = "" + (int)(collapseTime - t + 1);

            //the 8 pictures are spread over the 7 seconds (the last one, with no floor, is at 7 seconds)
            int newStage = (int)(t / collapseTime * (stages.Length - 1));
            if (newStage != stage && newStage < stages.Length - 1)
            {
                stage = newStage;
                for (int i = 0; i < parts.Length; i++)
                {
                    parts[i].sprite = stages[stage];
                }
                Level5Manager.Instance.PlaySound(crackSound);
            }
            yield return null;
        }

        if (finished == true)
        {
            yield break; //alex is safe, the floor stays like this
        }

        //the floor is gone: alex falls into the fire (the fire under it starts the level again)
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].sprite = stages[stages.Length - 1];
        }
        floorCollider.enabled = false;
        countdownText.text = "0";
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerOnFloor = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerOnFloor = false;
            if (started == false || finished == true)
            {
                player.canJump = true; //back on the normal floor
            }
        }
    }
}
