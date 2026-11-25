using System.Collections;
using UnityEngine;

public class SteamHazard : MonoBehaviour
{
    [Header("Steam Settings")]
    public ParticleSystem steamParticles;

    public float activeTime = 3f;
    public float inactiveTime = 2f;

    private void Start()
    {
        StartCoroutine(SteamCycle());
    }

    private IEnumerator SteamCycle()
    {
        while (true)
        {
            steamParticles.Play();
            yield return new WaitForSeconds(activeTime);

            steamParticles.Stop();
            yield return new WaitForSeconds(inactiveTime);
        }
    }
}
