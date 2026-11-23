using UnityEngine;
using UnityEngine.Events;
using TMPro;

// Put this on the CalendarZone (BoxCollider2D, Is Trigger) that covers the calendar.
// Alex stands in the zone and presses E: the calendar fills the screen.
// Left/Right choose a hidden block, E breaks it, Escape closes the calendar.
public class CalendarPuzzle : MonoBehaviour
{
    [Header("Guide dialogue")]
    public GameObject guidePanel;
    public TextMeshProUGUI guideText;
    public string guideMessage = "Reveal the 4 hidden dates";

    [Header("Timer")]
    public float timeLimit = 30f;
    public TextMeshProUGUI timerText;

    [Header("Date blocks (drag all 4 here)")]
    public CalendarDateBlock[] dateBlocks;

    [Header("When solved")]
    public UnityEvent onSolved;

    [Header("Big calendar view")]
    public float viewPadding = 1.6f;
    public CalendarDialogue dialogue;   // instruction panel above the calendar

    private float timeLeft;
    private bool timerRunning;
    private bool solved;
    private bool playerInside;
    private bool viewing;
    private int revealedCount;
    private int selectedIndex = -1;

    private Camera cam;
    private Vector3 savedCamPosition;
    private float savedCamSize;
    private PlayerController player;
    private Collider2D zone;

    public bool PlayerInside { get { return playerInside; } }
    public bool IsSolved { get { return solved; } }
    public bool IsViewing { get { return viewing; } }

    public bool CanReveal
    {
        get { return !solved; }
    }

    void Awake()
    {
        zone = GetComponent<Collider2D>();
    }

    void Start()
    {
        cam = Camera.main;
        if (guideText != null) guideText.text = guideMessage;
        if (timerText != null) timerText.gameObject.SetActive(false);
        ResetPuzzle();
    }

    void Update()
    {
        if (viewing)
        {
            HandleViewInput();
        }
        else if (playerInside && !solved && Input.GetKeyDown(KeyCode.E))
        {
            OpenView();
        }

        if (timerRunning && !solved)
        {
            timeLeft -= Time.deltaTime;
            UpdateTimerText();

            if (timeLeft <= 0f)
            {
                timeLeft = 0f;
                UpdateTimerText();
                Fail();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            ShowDialogue();
            RefreshGuide();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            RefreshGuide();
        }
    }

    // ----- Big calendar view -----

    void OpenView()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null || zone == null) return;

        viewing = true;
        ShowDialogue();

        // Freeze Alex while the calendar is on screen.
        player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
            player.enabled = false;
        }

        // Move the camera onto the calendar and size it so the whole calendar fits the screen.
        savedCamPosition = cam.transform.position;
        savedCamSize = cam.orthographicSize;

        Bounds b = zone.bounds;
        float halfHeight = b.extents.y * viewPadding;
        float halfWidth = b.extents.x * viewPadding / cam.aspect;
        cam.transform.position = new Vector3(b.center.x, b.center.y, savedCamPosition.z);
        cam.orthographicSize = Mathf.Max(halfHeight, halfWidth);

        SelectFirstAvailable();
        RefreshGuide();
    }

    void ShowDialogue()
    {
        if (!solved && dialogue != null) dialogue.ShowOnce();
    }

    void CloseView()
    {
        viewing = false;
        selectedIndex = -1;

        if (dateBlocks != null)
        {
            foreach (CalendarDateBlock block in dateBlocks)
            {
                if (block != null) block.SetSelected(false);
            }
        }

        if (cam != null)
        {
            cam.transform.position = savedCamPosition;
            cam.orthographicSize = savedCamSize;
        }

        if (player != null) player.enabled = true;

        RefreshGuide();
    }

    void HandleViewInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseView();
            return;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow)) MoveSelection(1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveSelection(-1);

        if (Input.GetKeyDown(KeyCode.E) && selectedIndex >= 0)
        {
            BlockPressed(dateBlocks[selectedIndex]);
        }
    }

    void SelectFirstAvailable()
    {
        selectedIndex = -1;
        if (dateBlocks == null) return;

        for (int i = 0; i < dateBlocks.Length; i++)
        {
            if (dateBlocks[i] != null && !dateBlocks[i].IsRevealed)
            {
                Select(i);
                return;
            }
        }
    }

    void MoveSelection(int direction)
    {
        if (dateBlocks == null || dateBlocks.Length == 0) return;
        if (selectedIndex < 0)
        {
            SelectFirstAvailable();
            return;
        }

        int n = dateBlocks.Length;
        int i = selectedIndex;
        for (int step = 0; step < n; step++)
        {
            i = (i + direction + n) % n;
            if (dateBlocks[i] != null && !dateBlocks[i].IsRevealed)
            {
                Select(i);
                return;
            }
        }
    }

    void Select(int index)
    {
        selectedIndex = index;
        for (int i = 0; i < dateBlocks.Length; i++)
        {
            if (dateBlocks[i] != null) dateBlocks[i].SetSelected(i == index);
        }
    }

    // ----- Puzzle logic -----

    // Called when Alex presses E on the selected block.
    public void BlockPressed(CalendarDateBlock block)
    {
        if (solved || block == null || block.IsRevealed) return;

        // The 30-second timer starts on the first block Alex breaks.
        if (!timerRunning)
        {
            timerRunning = true;
            timeLeft = timeLimit;
            if (timerText != null) timerText.gameObject.SetActive(true);
        }

        block.Break();
        revealedCount++;
        UpdateTimerText();

        if (revealedCount >= dateBlocks.Length)
        {
            Solve();
            return;
        }

        if (viewing) SelectFirstAvailable();
        RefreshGuide();
    }

    // Restarts the puzzle: dates hidden again, timer cleared, view closed.
    public void ResetPuzzle()
    {
        if (viewing) CloseView();

        timerRunning = false;
        solved = false;
        revealedCount = 0;
        timeLeft = timeLimit;
        selectedIndex = -1;

        if (dateBlocks != null)
        {
            foreach (CalendarDateBlock block in dateBlocks)
            {
                if (block != null) block.ResetBlock();
            }
        }

        if (timerText != null) timerText.gameObject.SetActive(false);
        RefreshGuide();
    }

    // Called by LevelManager after every respawn (falling, timer, damage).
    // Hides the dates again unless the puzzle is already solved.
    public void OnPlayerRespawned()
    {
        if (!solved) ResetPuzzle();
    }

    void Solve()
    {
        solved = true;
        timerRunning = false;

        if (timerText != null) timerText.gameObject.SetActive(false);
        if (viewing) CloseView();
        RefreshGuide();

        onSolved.Invoke();
    }

    void Fail()
    {
        ResetPuzzle();
        HintText.Show("Time is up. The dates are hidden again.");

        LevelManager levelManager = FindObjectOfType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.RespawnPlayer();
        }
    }

    void RefreshGuide()
    {
        if (guidePanel == null) return;

        // While the calendar is open, the guide lists the controls.
        if (viewing)
        {
            if (guideText != null) guideText.text = "E: break the selected block     Left/Right: choose     Esc: close";
            guidePanel.SetActive(true);
            return;
        }

        // Near the calendar, before the timer starts, the guide asks for the 4 dates.
        bool show = playerInside && !timerRunning && !solved;
        if (show && guideText != null) guideText.text = guideMessage;
        guidePanel.SetActive(show);
    }

    void UpdateTimerText()
    {
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(timeLeft).ToString();
        }
    }
}
