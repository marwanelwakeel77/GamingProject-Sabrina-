using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CipherInspectionController : MonoBehaviour
{
    public Image pageImage;
    public TMP_Text titleText;
    public TMP_Text counterText;

    private CipherPage currentPage;

    public void OpenInspection(Sprite page, CipherPage source)
    {
        currentPage = source;

        pageImage.sprite = page;

        if (titleText != null)
            titleText.text = source.pageTitle;

        if (counterText != null)
            counterText.text = "Pages found: " + (PlayerStats.cipherPagesCollected + 1) + " / 3      Esc to close";

        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseInspection();
        }
    }

    public void CloseInspection()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;

        if (currentPage != null)
        {
            currentPage.CompleteInspection();
            currentPage = null;
        }
    }
}
