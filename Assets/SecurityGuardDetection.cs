using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityGuardDetection : MonoBehaviour
{
    private SecurityGuardPatrol guardPatrol;

    private void Start()
    {
        guardPatrol = GetComponentInParent<SecurityGuardPatrol>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            guardPatrol.StartChasing(other.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            guardPatrol.StopChasing();
        }
    }
}
