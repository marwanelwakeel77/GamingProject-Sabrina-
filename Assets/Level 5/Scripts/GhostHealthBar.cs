using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//the health bar of Sabrina's ghost (alex keeps his own Health Bar prefab of level 2)
public class GhostHealthBar : MonoBehaviour
{
    public GameObject bar; //the whole bar with the name
    public Image redBar; //goes down when the ghost is hit (fillAmount like the health bar of level 2)
    public Image lightBar; //follows slowly behind the red bar

    private float health = 1f;

    // Start is called before the first frame update
    void Start()
    {
        bar.SetActive(false);
        redBar.fillAmount = 1;
        lightBar.fillAmount = 1;
    }

    // Update is called once per frame
    void Update()
    {
        //the bars move smoothly to the new health (like the Lerp in lab 4)
        redBar.fillAmount = Mathf.Lerp(redBar.fillAmount, health, 10f * Time.deltaTime);
        lightBar.fillAmount = Mathf.Lerp(lightBar.fillAmount, health, 2f * Time.deltaTime);
    }

    public void SetHealth(float amount)
    {
        health = amount;
    }

    public void ShowBar()
    {
        bar.SetActive(true);
    }

    public void HideBar()
    {
        bar.SetActive(false);
    }
}
