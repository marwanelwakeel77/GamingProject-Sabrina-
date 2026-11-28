using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//the room starts dark. When the switch is used, the lights flicker and come back
public class RoomPower : MonoBehaviour
{
    public SpriteRenderer darkness; //a black sprite over the room
    public float darkAlpha = 0.78f;
    public float litAlpha = 0.08f;
    public SpriteRenderer[] glowingThings; //device, candles, door: dim until the power is back
    public Color dimColor = new Color(0.45f, 0.4f, 0.45f, 1f);
    public SpriteRenderer flash; //a white sprite for the flash of light
    public ParticleSystem[] lightEffects; //candle embers that start with the power (lab 4)

    // Start is called before the first frame update
    void Start()
    {
        darkness.color = new Color(0, 0, 0, darkAlpha);
        flash.color = new Color(1, 1, 1, 0);
        for (int i = 0; i < glowingThings.Length; i++)
        {
            glowingThings[i].color = dimColor;
        }
    }

    public IEnumerator PowerUp()
    {
        //the lights flicker a few times (like the flicker in lab 6)
        for (int i = 0; i < 5; i++)
        {
            if (i % 2 == 0)
            {
                darkness.color = new Color(0, 0, 0, 0.3f);
            }
            else
            {
                darkness.color = new Color(0, 0, 0, 0.7f);
            }
            yield return new WaitForSeconds(0.1f);
        }

        //a flash, then everything lights up slowly (like the Lerp in lab 4)
        float t = 0f;
        while (t < 1f)
        {
            t = t + Time.deltaTime / 1.4f;
            darkness.color = new Color(0, 0, 0, Mathf.Lerp(0.3f, litAlpha, t));
            flash.color = new Color(1f, 0.85f, 0.6f, Mathf.Lerp(0.5f, 0f, t));
            for (int i = 0; i < glowingThings.Length; i++)
            {
                glowingThings[i].color = Color.Lerp(dimColor, Color.white, t);
            }
            yield return null;
        }

        for (int i = 0; i < lightEffects.Length; i++)
        {
            lightEffects[i].Play();
        }

        yield return new WaitForSeconds(0.4f);
        Level5Manager.Instance.OnPowerRestored();
    }
}
