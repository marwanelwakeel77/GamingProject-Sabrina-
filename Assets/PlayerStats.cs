using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int health=3;
    public int lives=3;
    private float flickerTime=0f;
    private float flickerDuration=0.1f;
    private SpriteRenderer sr;
    public bool isImmune=false;
    public float immunityTime=0f;
    public float immunityDuration=1.5f;

    public static bool hasTriangle = false; //Triangle Symbol from level 2, needed in level 5
    public static bool hasLibraryPermit = false; //needed to enter the library (level 3)

    // Start is called before the first frame update
    void Start()
    {
        sr=GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isImmune==true)
        {
            SpriteFlicker();
            immunityTime=immunityTime+Time.deltaTime;
            if(immunityTime>=immunityDuration)
            {
                isImmune=false;
                sr.enabled=true;
            }
        }
    }
    void SpriteFlicker()
    {
        if(flickerTime<flickerDuration)
        {
            flickerTime=flickerTime+Time.deltaTime;
        }
        else if(flickerTime>=flickerDuration)
        {
            flickerTime=0;
            sr.enabled=!(sr.enabled);
        }
    }
    public void TakeDamage(int damage)
    {
        if (isImmune == false)
        {
            health = health - damage;

            if (health < 0)
                health = 0;

            if (lives > 0 && health == 0)
            {
                FindObjectOfType<LevelManager>().RespawnPlayer();
                health = 3;
                lives--;
            }
            else if (lives == 0 && health == 0)
            {
                Debug.Log("Gameover");
                Destroy(this.gameObject);
            }

            Debug.Log("Player Health: " + health.ToString());
            Debug.Log("Player Lives: " + lives.ToString());

            isImmune = true;
            immunityTime = 0f;
        }
    }
}
