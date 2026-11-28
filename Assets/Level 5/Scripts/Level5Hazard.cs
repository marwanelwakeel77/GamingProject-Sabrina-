using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//the fire under the collapsing floor: falling in starts Level 5 again (like the lake in lab 5)
public class Level5Hazard : MonoBehaviour
{
    public string message = "Alex fell into the fire!";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            Level5Manager.Instance.FailLevel(message);
        }
    }
}
