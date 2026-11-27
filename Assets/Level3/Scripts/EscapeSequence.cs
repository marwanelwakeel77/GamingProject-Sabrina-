using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EscapeSequence : MonoBehaviour
{
    public SecretPassage secretPassage;
    public BookMechanism bookMechanism;
    public float escapeTime = 10f;

    public TMP_Text timerText;
    public GameObject timerPanel;
    public Image alarmGlow;
    public float glowSpeed = 0.95f;
    public HiddenPressurePlate[] armedTraps;
    public CursedTorch[] torches;
    public SecurityGuardPatrol guard;
    public float guardSpeedBoost = 1.5f;
    public float musicPitch = 1.2f;

    public AudioClip bellSound;
    public float bellInterval = 1.5f;
    public AudioClip sealSound;

    private bool escaping = false;
    private bool timerRunning = false;
    private bool glowRising = true;
    private float timeLeft;
    private float bellTimer;
    private float normalGuardSpeed;

    void Start()
    {
        timerPanel.SetActive(false);
        alarmGlow.gameObject.SetActive(false);
    }

    public void StartEscape()
    {
        if (escaping)
            return;

        escaping = true;
        timerRunning = true;
        timeLeft = escapeTime;
        bellTimer = 0f;
        SetAlarm(true);
    }

    void Update()
    {
        if (timerRunning == false)
            return;

        if (secretPassage.levelCompletePanel.activeSelf || !secretPassage.IsOpen())
        {
            SetAlarm(false);
            timerRunning = false;
            escaping = false;
            return;
        }

        timeLeft = timeLeft - Time.deltaTime;

        bellTimer = bellTimer - Time.deltaTime;
        if (bellTimer <= 0)
        {
            bellTimer = bellInterval;
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMusicSFX(bellSound);
        }

        Color c = alarmGlow.color;
        if (glowRising == true)
            c.a = Mathf.MoveTowards(c.a, 0.85f, glowSpeed * Time.deltaTime);
        else
            c.a = Mathf.MoveTowards(c.a, 0.35f, glowSpeed * Time.deltaTime);
        alarmGlow.color = c;

        if (c.a >= 0.85f)
            glowRising = false;
        else if (c.a <= 0.35f)
            glowRising = true;

        if (timeLeft > 0)
        {
            int secondsLeft = (int)timeLeft + 1;
            timerText.text = "ESCAPE!   " + secondsLeft;
        }
        else
        {
            timerRunning = false;
            StartCoroutine(SealPassage());
        }
    }

    IEnumerator SealPassage()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(sealSound);
        secretPassage.dust.Play();
        bookMechanism.ResetMechanism();
        SetAlarm(false);

        timerPanel.SetActive(true);
        timerText.text = "The passage sealed itself! Pull the book again.";
        yield return new WaitForSeconds(3f);
        timerPanel.SetActive(false);

        escaping = false;
    }

    void SetAlarm(bool on)
    {
        timerPanel.SetActive(on);
        alarmGlow.gameObject.SetActive(on);

        if (on)
        {
            alarmGlow.color = new Color(1f, 1f, 1f, 0.35f);
            glowRising = true;
        }

        for (int i = 0; i < armedTraps.Length; i++)
            armedTraps[i].autoRepeat = on;

        for (int i = 0; i < torches.Length; i++)
            torches[i].escapeMode = on;

        if (AudioManager.Instance != null)
        {
            if (on)
                AudioManager.Instance.musicSource.pitch = musicPitch;
            else
                AudioManager.Instance.musicSource.pitch = 1f;
        }

        if (guard != null)
        {
            if (on)
            {
                normalGuardSpeed = guard.maxSpeed;
                guard.maxSpeed = normalGuardSpeed * guardSpeedBoost;
                guard.flashlight.color = guard.alertLight;
            }
            else
            {
                guard.maxSpeed = normalGuardSpeed;
                guard.flashlight.color = guard.calmLight;
            }
        }
    }
}
