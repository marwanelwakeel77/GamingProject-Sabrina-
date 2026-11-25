using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//put this on an empty GameObject with a triggered collider after the broken floor
//(like the SpawnPoint in lab 6, exercise 21)
public class BallStopPoint : MonoBehaviour
{
    public GameObject ball; //the ball that chases alex

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && ball != null)
        {
            Destroy(ball); //alex jumped over the broken floor, so the ball stops chasing him
        }
    }
}
