using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityGuardDetection : MonoBehaviour
{
    public SecurityGuardPatrol guardPatrol;

    private bool playerInside = false;
    private Transform player;

    void Update()
    {
        if (playerInside)
        {
            guardPatrol.StartChasing(player);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerInside = true;
            player = other.transform;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerInside = false;
            guardPatrol.StopChasing();
        }
    }
}
