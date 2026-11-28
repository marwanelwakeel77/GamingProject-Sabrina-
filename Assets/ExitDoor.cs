using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{
    public int nextLevel = 3; //the number of Level 3 (the library) in File > Build Settings
    public GameObject interactText; //"Press E To Interact" in the Canvas
    public GameObject openSafeHint; //"Door Is Locked Collect The Library Permit" in the Canvas
    public GameObject collectSymbolHint; //"Door Is Locked Collect The Triangle Symbol" in the Canvas
    public Animator doorAnim; //the exit door panel, it opens before going to the next level

    private bool playerNear = false;

    // Start is called before the first frame update
    void Start()
    {
        openSafeHint.SetActive(false);
        collectSymbolHint.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerNear == true && Input.GetKeyDown(KeyCode.E))
        {
            //the door opens only if the player has the Library Permit and the Triangle Symbol
            if (PlayerStats.hasLibraryPermit == true && PlayerStats.hasTriangle == true)
            {
                playerNear = false; //so E can't be pressed again while the door is opening
                interactText.SetActive(false);
                doorAnim.SetBool("isOpen", true); //like the animations in lab 3
                StartCoroutine(OpenAndLeave());
            }
            else if (PlayerStats.hasLibraryPermit == false)
            {
                interactText.SetActive(false);
                openSafeHint.SetActive(true);
            }
            else
            {
                interactText.SetActive(false);
                collectSymbolHint.SetActive(true);
            }
        }
    }

    IEnumerator OpenAndLeave()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(nextLevel);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNear = true;
            interactText.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNear = false;
            interactText.SetActive(false);
            openSafeHint.SetActive(false);
            collectSymbolHint.SetActive(false);
        }
    }
}
