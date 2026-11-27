using UnityEngine;

public class HeartPickup : MonoBehaviour
{
    public int healAmount = 1;
    public AudioClip pickupSound;

    public float bobHeight = 0.08f;
    public float moveSpeed = 0.13f;

    private Vector3 topPosition;
    private Vector3 bottomPosition;
    private bool movingUp = true;
    private bool playerNearby = false;
    private PlayerStats stats;

    void Start()
    {
        topPosition = transform.position + new Vector3(0, bobHeight, 0);
        bottomPosition = transform.position - new Vector3(0, bobHeight, 0);
    }

    void Update()
    {
        Vector3 targetPosition;
        if (movingUp == true)
            targetPosition = topPosition;
        else
            targetPosition = bottomPosition;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.y - targetPosition.y) < 0.001f)
            movingUp = !movingUp;

        if (playerNearby && stats.health < 3)
        {
            stats.Heal(healAmount);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMusicSFX(pickupSound);

            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNearby = true;
            stats = other.GetComponent<PlayerStats>();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerNearby = false;
        }
    }
}
