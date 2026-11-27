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

    void Start()
    {
        Time.timeScale = 1f;
        howToPlayPanel.SetActive(false);
        fadeScreen.gameObject.SetActive(true);
        fadeScreen.color = new Color(0f, 0f, 0f, 1f);
        fadeScreen.raycastTarget = true;
        StartCoroutine(FadeIn());
    }

    void Update()
    {
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
        float timer = 0f;
        while (timer < fadeTime)
        {
            timer = timer + Time.deltaTime;
            fadeScreen.color = new Color(0f, 0f, 0f, Mathf.Lerp(1f, 0f, timer / fadeTime));
            yield return null;
        }
        fadeScreen.color = new Color(0f, 0f, 0f, 0f);
        fadeScreen.raycastTarget = false;
    }

    IEnumerator StartGame()
    {
        fadeScreen.raycastTarget = true;
        float startAlpha = fadeScreen.color.a;
        float startVolume = musicSource.volume;

        float timer = 0f;
        while (timer < fadeTime)
        {
            timer = timer + Time.deltaTime;
            fadeScreen.color = new Color(0f, 0f, 0f, Mathf.Lerp(startAlpha, 1f, timer / fadeTime));
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeTime);
            yield return null;
        }

        SceneManager.LoadScene(firstLevel);
    }
}
