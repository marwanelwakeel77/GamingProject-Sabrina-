using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    public Image fadeScreen;
    public float fadeTime = 1f;
    public int mainMenuScene = 0;

    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioClip gameOverSound;
    public AudioClip clickSound;

    private bool leaving = false;
    private float fadeTarget = 0f;
    private float fadeSpeed;
    private float volumeSpeed;

    void Start()
    {
        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
            Destroy(AudioManager.Instance.gameObject);

        fadeScreen.gameObject.SetActive(true);
        fadeScreen.color = new Color(0f, 0f, 0f, 1f);
        fadeScreen.raycastTarget = true;
        fadeTarget = 0f;
        fadeSpeed = 1f / fadeTime;

        if (gameOverSound != null)
            sfxSource.PlayOneShot(gameOverSound);

        StartCoroutine(FadeIn());
    }

    void Update()
    {
        Color c = fadeScreen.color;
        c.a = Mathf.MoveTowards(c.a, fadeTarget, fadeSpeed * Time.deltaTime);
        fadeScreen.color = c;

        if (leaving)
        {
            musicSource.volume = Mathf.MoveTowards(musicSource.volume, 0f, volumeSpeed * Time.deltaTime);
        }
    }

    public void onClickRetry()
    {
        if (leaving)
            return;

        PlayClick();
        StopAllCoroutines();
        StartCoroutine(LeaveTo(PlayerStats.lastLevel));
    }

    public void onClickMainMenu()
    {
        if (leaving)
            return;

        PlayClick();
        StopAllCoroutines();
        StartCoroutine(LeaveTo(mainMenuScene));
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

    IEnumerator LeaveTo(int scene)
    {
        leaving = true;
        fadeScreen.raycastTarget = true;
        fadeSpeed = (1f - fadeScreen.color.a) / fadeTime;
        volumeSpeed = musicSource.volume / fadeTime;
        fadeTarget = 1f;

        yield return new WaitForSeconds(fadeTime);

        SceneManager.LoadScene(scene);
    }
}
