using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// The close-up of the statue's shelf. Every book is a Button (Lab 8): clicking one shows its title
// written in Sabrina's cipher. The player decodes the titles with the three pages and pulls one book.
public class BookShelfInspectionUI : MonoBehaviour
{
    public BookMechanism bookMechanism;

    public Image[] books;            // the book buttons on the shelf
    public GameObject[] bookTitles;  // the cipher title of each book, in the same order
    public int correctBook = 2;      // THE FORBIDDEN RESEARCH
    public Sprite pulledBookSprite;  // the correct book after it is pulled out
    public RectTransform selectionGlow;

    public TMP_Text messageText;
    public GameObject chooseText;    // "Select a book to read its title"

    // Sabrina's pages, so the player can read them again while decoding
    public GameObject[] cipherPages;     // the pages in the level
    public GameObject[] pageThumbnails;  // a button for each page (same order)
    public Sprite[] pageSprites;         // the image of each page (same order)
    public GameObject pageViewer;
    public Image pageViewerImage;

    public AudioClip bookSound;
    public AudioClip wrongBookSound;
    public AudioClip correctBookSound;

    // trap: pulling a wrong book springs a hidden blade out of the shelf
    public RectTransform blade;        // the blade image (its pivot is on the left, so it grows out to the right)
    public Image redFlash;             // red flash over the close-up
    public RectTransform shelfWindow;  // the close-up window, it shakes when the blade snaps out
    public AudioClip bladeSound;
    public int bladeDamage = 1;

    private int selectedBook = -1;
    private bool pulling = false;
    private Sprite bookStartSprite; // the correct book before it was pulled

    public void OpenShelf()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;

        selectedBook = -1;
        pulling = false;
        HideBlade();
        ShowTitle(-1);
        selectionGlow.gameObject.SetActive(false);
        pageViewer.SetActive(false);

        // a page that is no longer in the level has been collected, so show its button
        for (int i = 0; i < pageThumbnails.Length; i++)
        {
            pageThumbnails[i].SetActive(!cipherPages[i].activeSelf);
        }

        if (PlayerStats.cipherPagesCollected < 3)
            messageText.text = "The titles are written in Sabrina's cipher, but your clues are incomplete. Find all three pages first.";
        else
            messageText.text = "Each spine bears a title in Sabrina's cipher. Choose the book her pages describe.";

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(bookSound);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pageViewer.activeSelf)
                HidePage();
            else
                CloseShelf();
        }

        if (pageViewer.activeSelf || pulling)
            return;

        // choose a book with the arrow keys
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (selectedBook < books.Length - 1)
                SelectBook(selectedBook + 1);
            else
                SelectBook(0); //wrap around to the first book
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (selectedBook > 0)
                SelectBook(selectedBook - 1);
            else
                SelectBook(books.Length - 1); //wrap around to the last book
        }
        // pull the chosen book with Enter
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            PullBook();
        }
    }

    // OnClick of every book button, with the book's number
    public void SelectBook(int index)
    {
        if (pulling) return;

        selectedBook = index;
        ShowTitle(index);

        selectionGlow.gameObject.SetActive(true);
        selectionGlow.position = books[index].transform.position;
    }

    void ShowTitle(int index)
    {
        for (int i = 0; i < bookTitles.Length; i++)
        {
            bookTitles[i].SetActive(i == index);
        }
        chooseText.SetActive(index < 0);
    }

    // OnClick of the "Pull the Book" button
    public void PullBook()
    {
        if (selectedBook < 0 || pulling) return;

        if (PlayerStats.cipherPagesCollected < 3)
        {
            messageText.text = "Your clues are incomplete. Find all three cipher pages before you disturb these books.";
            return;
        }

        if (selectedBook == correctBook)
        {
            StartCoroutine(PullCorrectBook());
        }
        else
        {
            // wrong book: it is a trap
            StartCoroutine(WrongBookTrap());
        }
    }

    // a wrong book springs a hidden blade (Coroutine, Lab 9). Realtime, because the game is paused here
    IEnumerator WrongBookTrap()
    {
        pulling = true;

        // the wrong book slides out a little...
        RectTransform book = books[selectedBook].rectTransform;
        Vector2 bookStart = book.anchoredPosition;
        book.anchoredPosition = bookStart + new Vector2(0, -18);
        messageText.text = "The book slides out... CLICK.";
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(wrongBookSound);
        yield return new WaitForSecondsRealtime(0.35f);

        // ...and a blade snaps out of the shelf next to it
        blade.gameObject.SetActive(true);
        blade.anchoredPosition = bookStart + new Vector2(book.rect.width / 2f, 0);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(bladeSound);
        messageText.text = "A hidden blade snaps out of the shelf! That was the wrong book.";

        Vector2 windowStart = shelfWindow.anchoredPosition;
        float timer = 0f;
        while (timer < 0.5f)
        {
            timer = timer + Time.unscaledDeltaTime;
            blade.localScale = new Vector3(Mathf.Clamp01(timer / 0.08f), 1, 1);                // fast
            redFlash.color = new Color(0.8f, 0f, 0f, Mathf.Lerp(0.45f, 0f, timer / 0.5f));      // red flash
            float shake = 1f - timer / 0.5f;
            shelfWindow.anchoredPosition = windowStart + new Vector2(Random.Range(-12f, 12f), Random.Range(-8f, 8f)) * shake;
            yield return null;
        }
        shelfWindow.anchoredPosition = windowStart;
        redFlash.color = new Color(0.8f, 0f, 0f, 0f);

        yield return new WaitForSecondsRealtime(0.9f);

        // the book slides back, the close-up closes and Alex is hurt and thrown back from the shelf
        book.anchoredPosition = bookStart;
        HideBlade();
        pulling = false;
        gameObject.SetActive(false);
        Time.timeScale = 1f;

        PlayerStats stats = FindObjectOfType<PlayerStats>();
        stats.TakeDamage(bladeDamage); // Lab 6
        stats.GetComponent<Rigidbody2D>().velocity = new Vector2(-4f, 5f);

        bookMechanism.ShowPrompt();
    }

    void HideBlade()
    {
        if (blade != null)
            blade.gameObject.SetActive(false);
        if (redFlash != null)
            redFlash.color = new Color(0.8f, 0f, 0f, 0f);
    }

    IEnumerator PullCorrectBook()
    {
        pulling = true;

        // swap the sprite to the pulled-out book (like the brick in Lab 5)
        bookStartSprite = books[correctBook].sprite;
        books[correctBook].sprite = pulledBookSprite;
        messageText.text = "The book resists... then something clicks deep inside the shelf.";

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(correctBookSound);

        // Realtime, because the game is paused while the close-up is open
        yield return new WaitForSecondsRealtime(1.5f);

        gameObject.SetActive(false);
        Time.timeScale = 1f;
        bookMechanism.ActivateMechanism();
    }

    // called by the BookMechanism when the guard catches Alex: the book goes back into the shelf
    public void ResetShelf()
    {
        StopAllCoroutines();
        pulling = false;
        HideBlade();
        if (bookStartSprite != null)
            books[correctBook].sprite = bookStartSprite;
    }

    // OnClick of the "Step Back" and X buttons
    public void CloseShelf()
    {
        if (pulling) return;

        gameObject.SetActive(false);
        Time.timeScale = 1f;
        bookMechanism.ShowPrompt();
    }

    // OnClick of a page button
    public void ShowPage(int index)
    {
        pageViewerImage.sprite = pageSprites[index];
        pageViewer.SetActive(true);
    }

    // OnClick of the page viewer
    public void HidePage()
    {
        pageViewer.SetActive(false);
    }
}
