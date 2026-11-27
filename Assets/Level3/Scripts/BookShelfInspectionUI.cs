using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BookShelfInspectionUI : MonoBehaviour
{
    public BookMechanism bookMechanism;

    public Image[] books;
    public GameObject[] bookTitles;
    public int correctBook = 2;
    public Sprite pulledBookSprite;
    public RectTransform selectionGlow;

    public TMP_Text messageText;
    public GameObject chooseText;

    public GameObject[] cipherPages;
    public GameObject[] pageThumbnails;
    public Sprite[] pageSprites;
    public GameObject pageViewer;
    public Image pageViewerImage;

    public AudioClip bookSound;
    public AudioClip wrongBookSound;
    public AudioClip correctBookSound;

    public RectTransform blade;
    public Image redFlash;
    public RectTransform shelfWindow;
    public AudioClip bladeSound;
    public int bladeDamage = 1;

    private int selectedBook = -1;
    private bool pulling = false;
    private Sprite bookStartSprite;

    private bool bladeAnimating = false;
    private float bladeTimer = 0f;
    private Vector2 windowStart;

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
        if (bladeAnimating)
        {
            bladeTimer = bladeTimer + Time.unscaledDeltaTime;
            blade.localScale = new Vector3(Mathf.Clamp(bladeTimer / 0.08f, 0f, 1f), 1, 1);
            redFlash.color = new Color(0.8f, 0f, 0f, Mathf.Lerp(0.45f, 0f, bladeTimer / 0.5f));
            float shake = 1f - bladeTimer / 0.5f;
            shelfWindow.anchoredPosition = windowStart + new Vector2(Random.Range(-12f, 12f), Random.Range(-8f, 8f)) * shake;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pageViewer.activeSelf)
                HidePage();
            else
                CloseShelf();
        }

        if (pageViewer.activeSelf || pulling)
            return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (selectedBook < books.Length - 1)
                SelectBook(selectedBook + 1);
            else
                SelectBook(0);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (selectedBook > 0)
                SelectBook(selectedBook - 1);
            else
                SelectBook(books.Length - 1);
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            PullBook();
        }
    }

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
            StartCoroutine(WrongBookTrap());
        }
    }

    IEnumerator WrongBookTrap()
    {
        pulling = true;

        RectTransform book = books[selectedBook].rectTransform;
        Vector2 bookStart = book.anchoredPosition;
        book.anchoredPosition = bookStart + new Vector2(0, -18);
        messageText.text = "The book slides out... CLICK.";
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(wrongBookSound);
        yield return new WaitForSecondsRealtime(0.35f);

        blade.localScale = new Vector3(0, 1, 1);
        blade.gameObject.SetActive(true);
        blade.anchoredPosition = bookStart + new Vector2(book.rect.width / 2f, 0);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(bladeSound);
        messageText.text = "A hidden blade snaps out of the shelf! That was the wrong book.";

        windowStart = shelfWindow.anchoredPosition;
        bladeTimer = 0f;
        bladeAnimating = true;
        yield return new WaitForSecondsRealtime(0.5f);
        bladeAnimating = false;
        shelfWindow.anchoredPosition = windowStart;
        redFlash.color = new Color(0.8f, 0f, 0f, 0f);

        yield return new WaitForSecondsRealtime(0.9f);

        book.anchoredPosition = bookStart;
        HideBlade();
        pulling = false;
        gameObject.SetActive(false);
        Time.timeScale = 1f;

        PlayerStats stats = FindObjectOfType<PlayerStats>();
        stats.TakeDamage(bladeDamage);
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

        bookStartSprite = books[correctBook].sprite;
        books[correctBook].sprite = pulledBookSprite;
        messageText.text = "The book resists... then something clicks deep inside the shelf.";

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(correctBookSound);

        yield return new WaitForSecondsRealtime(1.5f);

        gameObject.SetActive(false);
        Time.timeScale = 1f;
        bookMechanism.ActivateMechanism();
    }

    public void ResetShelf()
    {
        StopAllCoroutines();
        pulling = false;
        bladeAnimating = false;
        HideBlade();
        if (bookStartSprite != null)
            books[correctBook].sprite = bookStartSprite;
    }

    public void CloseShelf()
    {
        if (pulling) return;

        gameObject.SetActive(false);
        Time.timeScale = 1f;
        bookMechanism.ShowPrompt();
    }

    public void ShowPage(int index)
    {
        pageViewerImage.sprite = pageSprites[index];
        pageViewer.SetActive(true);
    }

    public void HidePage()
    {
        pageViewer.SetActive(false);
    }
}
