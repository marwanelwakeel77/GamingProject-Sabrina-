using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Put this on safe_0 (with a BoxCollider2D, Is Trigger).
// E opens the keypad. Alex types a number and presses OK, four times: 5, 15, 25, 29.
// The right code gives the Library Permit to the inventory.
public class SafeKeypad : MonoBehaviour
{
    public const string PermitName = "Library Permit";

    public GameObject keypadPanel;      // "Safe Panel" under the Canvas
    public TextMeshProUGUI entryText;   // what is typed for the current number
    public TextMeshProUGUI slotsText;   // the numbers locked in so far
    public CalendarPuzzle calendar;     // the calendar that gives the code
    public Sprite permitSprite;         // icon for the inventory
    public int[] code = { 5, 15, 25, 29 };

    private bool playerInside;
    private bool open;
    private bool unlocked;
    private string entry = "";
    private readonly List<int> entered = new List<int>();
    private PlayerController player;

    void Start()
    {
        if (keypadPanel != null) keypadPanel.SetActive(false);
        Refresh();
    }

    void Update()
    {
        if (unlocked) return;

        if (open)
        {
            HandleKeyboard();
            return;
        }

        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            Open();
        }
    }

    void HandleKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseKeypad();
            return;
        }

        for (int d = 0; d <= 9; d++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha0 + d)) ||
                Input.GetKeyDown((KeyCode)((int)KeyCode.Keypad0 + d)))
            {
                PressDigit(d);
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace)) Backspace();
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Enter();
    }

    void Open()
    {
        open = true;
        entry = "";
        entered.Clear();

        // Freeze Alex while the keypad is open.
        player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
            player.enabled = false;
        }

        if (keypadPanel != null) keypadPanel.SetActive(true);
        Refresh();
    }

    public void CloseKeypad()
    {
        if (!open) return;

        open = false;
        if (keypadPanel != null) keypadPanel.SetActive(false);
        if (player != null) player.enabled = true;
    }

    // Called by the keypad buttons (0-9).
    public void PressDigit(int digit)
    {
        if (!open || unlocked) return;
        if (entry.Length >= 2) return;   // dates on the calendar go up to 31

        entry += digit.ToString();
        Refresh();
    }

    public void Backspace()
    {
        if (!open || unlocked || entry.Length == 0) return;

        entry = entry.Substring(0, entry.Length - 1);
        Refresh();
    }

    // Locks in the typed number (OK button).
    public void Enter()
    {
        if (!open || unlocked || entry.Length == 0) return;

        entered.Add(int.Parse(entry));
        entry = "";

        if (entered.Count < code.Length)
        {
            Refresh();
            return;
        }

        CheckCode();
    }

    void CheckCode()
    {
        bool correct = true;
        for (int i = 0; i < code.Length; i++)
        {
            if (entered[i] != code[i])
            {
                correct = false;
                break;
            }
        }

        if (correct)
        {
            Unlock();
            return;
        }

        entered.Clear();

        if (calendar == null || !calendar.IsSolved)
        {
            HintText.Show("Wrong code. Solve the calendar puzzle first.");
        }
        else
        {
            HintText.Show("Wrong code. Try again.");
        }

        Refresh();
    }

    void Unlock()
    {
        unlocked = true;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(PermitName, permitSprite);
        }

        HintText.Show("You took the Library Permit.");
        CloseKeypad();
    }

    void Refresh()
    {
        if (entryText != null) entryText.text = "Number: " + entry + "_";

        if (slotsText != null)
        {
            string s = "";
            for (int i = 0; i < code.Length; i++)
            {
                s += (i < entered.Count ? entered[i].ToString() : "?");
                if (i < code.Length - 1) s += "   ";
            }
            slotsText.text = s;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInside = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            CloseKeypad();
        }
    }
}
