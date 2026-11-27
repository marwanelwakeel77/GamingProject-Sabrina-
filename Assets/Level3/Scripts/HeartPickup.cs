using UnityEngine;

public class HeartPickup : MonoBehaviour
{
    public int healAmount = 1;
    public AudioClip pickupSound;

    public float bobHeight = 0.08f;
    public float bobSpeed = 2.5f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.position = startPosition + new Vector3(0, Mathf.Sin(Time.time * bobSpeed) * bobHeight, 0);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();

            if (stats != null && stats.health < 3)
            {
                stats.Heal(healAmount);

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayMusicSFX(pickupSound);

                Destroy(gameObject);
            }
        }
    }
}
