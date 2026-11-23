using UnityEngine;
using UnityEngine.SceneManagement;

// Put this on the exit door (BoxCollider2D, Is Trigger).
// The door stays locked until the Library Permit is in the inventory, then Alex leaves for the next level.
public class ExitDoor : MonoBehaviour
{
    public string requiredItem = "Library Permit";
    public string nextScene = "level 3";

    private bool playerInside;

    void Update()
    {
        if (!playerInside || !Input.GetKeyDown(KeyCode.E)) return;

        if (InventoryManager.Instance == null || !InventoryManager.Instance.HasItem(requiredItem))
        {
            HintText.Show("The door is locked. Find the Library Permit.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(nextScene))
        {
            HintText.Show("Level 3 is not added to the build yet.");
            Debug.LogWarning("ExitDoor: scene '" + nextScene + "' is not in Build Settings.");
            return;
        }

        SceneManager.LoadScene(nextScene);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInside = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInside = false;
    }
}
