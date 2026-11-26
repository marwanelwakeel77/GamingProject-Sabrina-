using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public int health=3;
    public int lives=3;
    public Image healthBar; //level 2 red bar (levels without one leave it empty)
    private float flickerTime=0f;
    private float flickerDuration=0.1f;
    private SpriteRenderer sr;
    public bool isImmune=false;
    public float immunityTime=0f;
    public float immunityDuration=1.5f;

    public static bool hasTriangle = false; //Triangle Symbol from level 2, needed in level 5
    public static bool hasLibraryPermit = false; //needed to enter the library (level 3)

    // Level 3 collectibles. STATIC so their values stay with us (Lab 7)
    public static int cipherPagesCollected = 0;
    public static bool hasCrescentMoon = false;

    // Level 3 HUD (Lab 8): drag the PageCount text and the MoonIcon image here
    public TMP_Text pageCounter;
    public Image moonIcon;

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

        // keep the HUD up to date (only in levels that have one)
        if (pageCounter != null)
        {
            pageCounter.text = cipherPagesCollected + " / 3";
        }
        if (moonIcon != null)
        {
            if (hasCrescentMoon == true)
                moonIcon.color = Color.white; //collected: full color
            else
                moonIcon.color = new Color(0.3f, 0.35f, 0.5f, 0.35f); //not collected yet: dark and faded
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
            if (healthBar != null)
                healthBar.fillAmount = this.health/3f; //reflect health points on the red bar. When player loses health points, the red bar shrinks

            if (health < 0)
                health = 0;

            if (lives > 0 && health == 0)
            {
                FindObjectOfType<LevelManager>().RespawnPlayer();
                health = 3;
                if (healthBar != null)
                    healthBar.fillAmount = this.health/3f; //replenish the health bar so it's full again
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
