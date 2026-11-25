using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//put this on each ? block on the big calendar (Calendar Panel)
public class CalendarDateBlock : MonoBehaviour
{
    public Color selectedColor = new Color(0.72f, 0.53f, 0.04f); //dark gold = the date the player is on

    private Image img;
    private Animator anim;
    public bool isBroken = false;

    //Awake runs as soon as the calendar panel is shown for the first time
    void Awake()
    {
        img = GetComponent<Image>();
        anim = GetComponent<Animator>();
    }

    //called by the calendar when the player moves between the dates
    public void Select(bool isSelected)
    {
        if (isSelected == true)
        {
            img.color = selectedColor;
        }
        else
        {
            img.color = Color.white;
        }
    }

    //called by the calendar when the player presses E on this date
    public void BreakBlock()
    {
        isBroken = true;
        Select(false);
        StartCoroutine(Explode());
    }

    IEnumerator Explode()
    {
        //plays the break animation (solid block -> broken block)
        anim.SetBool("isBroken", true);

        //wait until the animation is seen, then remove the block so the date under it is seen
        yield return new WaitForSeconds(0.4f);
        HideBlock();
    }

    public void HideBlock()
    {
        img.enabled = false;
    }

    //called when the time is up, so the date is hidden again
    //(the Animator goes back to the solid block by itself when the panel is shown again)
    public void ResetBlock()
    {
        isBroken = false;
        anim.SetBool("isBroken", false);
        img.color = Color.white;
        img.enabled = true;
    }
}
