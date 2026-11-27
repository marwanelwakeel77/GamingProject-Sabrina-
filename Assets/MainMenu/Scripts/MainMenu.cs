using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject howToPlayPanel;
    public Image fadeScreen;
    public float fadeTime = 1.2f;
    public int firstLevel = 1;

    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioClip clickSound;

    private bool starting = false;
    private float fadeTarget = 0f;
    private float fadeSpeed;
    private float volumeSpeed;

    void Start()
    {
        Time.timeScale = 1f;
        howToPlayPanel.SetActive(false);
        fadeScreen.gameObject.SetActive(true);
        fadeScreen.color = new Color(0f, 0f, 0f, 1f);
        fadeScreen.raycastTarget = true;
        fadeTarget = 0f;
        fadeSpeed = 1f / fadeTime;
        StartCoroutine(FadeIn());
    }

    void Update()
    {
        Color c = fadeScreen.color;
        c.a = Mathf.MoveTowards(c.a, fadeTarget, fadeSpeed * Time.deltaTime);
        fadeScreen.color = c;

        if (starting)
        {
            musicSource.volume = Mathf.MoveTowards(musicSource.volume, 0f, volumeSpeed * Time.deltaTime);
        }

        if (howToPlayPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            onClickBack();
        }
    }

    public void onClickNewGame()
    {
        if (starting)
            return;

        starting = true;
        PlayClick();

        PlayerStats.cipherPagesCollected = 0;
        PlayerStats.hasCrescentMoon = false;
        PlayerStats.hasTriangle = false;
        PlayerStats.hasLibraryPermit = false;

        StopAllCoroutines();
        StartCoroutine(StartGame());
    }

    public void onClickHowToPlay()
    {
        if (starting)
            return;

        PlayClick();
        howToPlayPanel.SetActive(true);
    }

    public void onClickBack()
    {
        PlayClick();
        howToPlayPanel.SetActive(false);
    }

    public void onClickExit()
    {
        PlayClick();
        Application.Quit();
    }

    void PlayClick()
    {
        if (clickSound != null)
            sfxSource.PlayOneShot(clickSound);
    }

    IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(fadeTime);
        fadeScreen.color = new Color(0f, 0f, 0f, 0f);
        fadeScreen.raycastTarget = false;
    }

    IEnumerator StartGame()
    {
        fadeScreen.raycastTarget = true;
        fadeSpeed = (1f - fadeScreen.color.a) / fadeTime;
        volumeSpeed = musicSource.volume / fadeTime;
        fadeTarget = 1f;

        yield return new WaitForSeconds(fadeTime);

        SceneManager.LoadScene(firstLevel);
    }
}
