using System.Collections;
using UnityEngine;

// The Falling Books trap on the upper floor.
// It works like the SpawnGhost trigger point in Lab 6 (Exercise #21): when Alex walks through
// this trigger collider, books are created with Instantiate() at the spawn points on the bookshelf.
public class FallingBooksTrap : MonoBehaviour
{
    public GameObject bookPrefab;       // the FallingBook prefab
    public Transform[] spawnPoints;     // where each book starts (in the stack on top of the bookshelf)
    public Sprite[] bookSprites;        // a different book picture for each spawn point
    public GameObject bookStack;        // the books lying on the shelf: they wobble, then fall
    public ParticleSystem landingDust;  // dust puff where a book lands

    public float warningTime = 0.5f;      // how long the books wobble before they fall
    public float wobbleAmount = 0.015f;
    public float timeBetweenBooks = 0.15f;
    public float resetTime = 4f;          // after this time the books are back on the shelf

    public AudioClip wobbleSound;

    private bool trapRunning = false;     // so the trap can't start twice at the same time

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && trapRunning == false)
        {
            StartCoroutine(DropBooks());
        }
    }

    // Coroutine (Lab 9): warning, then the books fall one after the other, then the trap resets
    IEnumerator DropBooks()
    {
        trapRunning = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusicSFX(wobbleSound);

        // warning: the stack wobbles on the shelf (like the bookshelf shake)
        Vector3 startPosition = bookStack.transform.position;
        float timer = 0f;
        while (timer < warningTime)
        {
            timer = timer + Time.deltaTime;
            bookStack.transform.position = startPosition + new Vector3(Random.Range(-wobbleAmount, wobbleAmount), 0, 0);
            yield return null; // wait for the next frame
        }
        bookStack.transform.position = startPosition;
        bookStack.SetActive(false);

        // the books fall (Instantiate, like the ghost in Lab 6)
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject book = Instantiate(bookPrefab, spawnPoints[i].position, Quaternion.identity);

            if (i < bookSprites.Length)
                book.GetComponent<SpriteRenderer>().sprite = bookSprites[i];

            book.GetComponent<FallingBook>().landingDust = landingDust;

            yield return new WaitForSeconds(timeBetweenBooks);
        }

        // after a while the books are back on the shelf and the trap is ready again
        yield return new WaitForSeconds(resetTime);
        bookStack.SetActive(true);
        trapRunning = false;
    }
}
