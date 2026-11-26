using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The guard's vision: a triggered collider in front of him, where his flashlight points
public class SecurityGuardDetection : MonoBehaviour
{
    private SecurityGuardPatrol guardPatrol;

    private void Start()
    {
        guardPatrol = GetComponentInParent<SecurityGuardPatrol>();
    }

    // Alex is inside the flashlight
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            guardPatrol.StartChasing(other.transform);
        }
    }

    // Alex got away from the flashlight
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            guardPatrol.StopChasing();
        }
    }
}
