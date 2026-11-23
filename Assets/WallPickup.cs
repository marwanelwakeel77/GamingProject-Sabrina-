using UnityEngine;

// Put this on the wall triangle. Alex collects it with E, but only after the calendar is solved.
[RequireComponent(typeof(SpriteRenderer))]
public class WallPickup : MonoBehaviour
{
    public string itemName = "Triangle Symbol";
    public CalendarPuzzle requiredPuzzle;   // the calendar that must be solved first

    private SpriteRenderer sr;
    private bool playerNearby;
    private bool collected;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (collected || !playerNearby) return;

        if (Input.GetKeyDown(KeyCode.E) && requiredPuzzle != null && requiredPuzzle.IsSolved)
        {
            Collect();
        }
    }

    void Collect()
    {
        collected = true;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(itemName, sr.sprite);
        }

        gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNearby = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNearby = false;
    }
}
