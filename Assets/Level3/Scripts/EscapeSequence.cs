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
    public HiddenPressurePlate[] armedTraps;
    public CursedTorch[] torches;
    public SecurityGuardPatrol guard;
    public float guardSpeedBoost = 1.5f;
    public float musicPitch = 1.2f;

    public AudioClip bellSound;
    public float bellInterval = 1.5f;
    public AudioClip sealSound;

    private bool escaping = false;
    private float normalGuardSpeed;

    void Start()
    {
        timerPanel.SetActive(false);
        alarmGlow.gameObject.SetActive(false);
    }

    public void StartEscape()
    {
        if (!escaping)
            StartCoroutine(EscapeRoutine());
    }

    IEnumerator EscapeRoutine()
    {
        escaping = true;
        SetAlarm(true);

        float timeLeft = escapeTime;
        float bellTimer = 0f;

        while (timeLeft > 0)
        {
            if (secretPassage.levelCompletePanel.activeSelf || !secretPassage.IsOpen())
            {
                SetAlarm(false);
                escaping = false;
                yield break;
            }

            timeLeft = timeLeft - Time.deltaTime;

            bellTimer = bellTimer - Time.deltaTime;
            if (bellTimer <= 0)
            {
                bellTimer = bellInterval;
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayMusicSFX(bellSound);
            }

            timerText.text = "ESCAPE!   " + Mathf.CeilToInt(timeLeft);

            float pulse = (Mathf.Sin(Time.time * 6f) + 1f) / 2f;
            alarmGlow.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.35f, 0.85f, pulse));

            yield return null;
        }

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
