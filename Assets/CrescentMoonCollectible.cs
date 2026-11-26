using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrescentMoonCollectible : MonoBehaviour
{
    public AudioClip moonSound;

   private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            PlayerStats.hasCrescentMoon = true; //static, so it is kept even when the guard catches Alex

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMusicSFX(moonSound);

            Debug.Log("Crescent Moon collected!");

            Destroy(gameObject);
        }
    }
}
