using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject CurrentCheckpoint;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void RespawnPlayer()
    {
        FindObjectOfType<PlayerController>().transform.position=CurrentCheckpoint.transform.position;

        // Restart the calendar puzzle (if this level has one) so the dates hide again.
        CalendarPuzzle calendar = FindObjectOfType<CalendarPuzzle>();
        if (calendar != null)
        {
            calendar.OnPlayerRespawned();
        }
    }
}
