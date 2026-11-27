using System.Collections;
using UnityEngine;

public class FallingBooksTrap : MonoBehaviour
{
    public GameObject bookPrefab;
    public Transform[] spawnPoints;
    public Sprite[] bookSprites;
    public GameObject bookStack;
    public ParticleSystem landingDust;

    public float warningTime = 0.5f;
    public float wobbleAmount = 0.015f;
    public float timeBetweenBooks = 0.15f;
    public float resetTime = 4f;

    public AudioClip wobbleSound;

    private bool trapRunning = false;
    private bool wobbling = false;
    private Vector3 stackStartPosition;

    void Start()
    {
        stackStartPosition = bookStack.transform.position;
    }

    void Update()
    {
        if (wobbling)
        {
            bookStack.transform.position = stackStartPosition + new Vector3(Random.Range(-wobbleAmount, wobbleAmount), 0, 0);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && trapRunning == false)
        {
            StartCoroutine(DropBooks());
        }
    }

    IEnumerator DropBooks()
    {
        trapRunning = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(wobbleSound);

        wobbling = true;
        yield return new WaitForSeconds(warningTime);
        wobbling = false;
        bookStack.transform.position = stackStartPosition;
        bookStack.SetActive(false);

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject book = Instantiate(bookPrefab, spawnPoints[i].position, spawnPoints[i].rotation);

            if (i < bookSprites.Length)
                book.GetComponent<SpriteRenderer>().sprite = bookSprites[i];

            book.GetComponent<FallingBook>().landingDust = landingDust;

            yield return new WaitForSeconds(timeBetweenBooks);
        }

        yield return new WaitForSeconds(resetTime);
        bookStack.SetActive(true);
        trapRunning = false;
    }
}
