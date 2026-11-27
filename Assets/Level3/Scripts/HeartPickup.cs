using UnityEngine;

// A heart that gives Alex 1 health back (Lab 7, Exercise #19: item pickup, like collectHeart).
// It has a triggered collider: when Alex touches it he heals, a sound plays and the heart disappears.
public class HeartPickup : MonoBehaviour
{
    public int healAmount = 1;
    public AudioClip pickupSound;

    public float bobHeight = 0.08f;   // it floats gently up and down so the player notices it
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

    // Stay instead of Enter: if Alex is standing on it when he gets hurt, it still heals him
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();

            // the heart is only used when Alex is hurt
            if (stats != null && stats.health < 3)
            {
                stats.Heal(healAmount);

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayMusicSFX(pickupSound); // Lab 7

                Destroy(gameObject);
            }
        }
    }
}
