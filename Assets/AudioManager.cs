using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //Why static? Bc whole game will have only 1 instance of AudioManager
    public static AudioManager Instance;

    public AudioSource musicSource; //source that plays background music
    public AudioSource sfxSource; //source that plays sound effects

    // background music of this level (level 2: overworld music, level 3: library music)
    public AudioClip overworldMusic;

    public AudioClip[] variousSFX; //array of sound effects clips to keep things varied

    void Awake()
    {
        //make sure the entire game only has one Audio Manager throughout
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            //a new level was loaded (for example level 2 -> level 3):
            //the Audio Manager we already have stays, but it plays this level's music
            if (overworldMusic != null && Instance.overworldMusic != overworldMusic)
            {
                Instance.overworldMusic = overworldMusic;
                Instance.musicSource.volume = musicSource.volume;
                Instance.PlayMusic(overworldMusic);
            }
            Destroy(gameObject);
        }
    }

    void Start()
    {
        musicSource.clip = overworldMusic;

        musicSource.Play();
    }

    //Public in case another object needs to call for a specific sound effect to begin playing
    public void PlayMusicSFX(AudioClip clip)
    {
        if (clip == null) return; //nothing assigned in the Inspector yet

        sfxSource.PlayOneShot(clip); //PlayOneShot so two sounds can play at the same time (steps + trap)
    }

    //Public in case another object needs to call for a specific soundtrack to begin playing
    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }

    //Function takes a bunch of sound clips as parameters
    public void PlayRandomSFX(params AudioClip[] clips)
    {
        //assign the incoming array of items to our local arraylist variable called 'variousSFX'
        variousSFX = clips;

        //randomly select a sound clip from the arraylist, then play that clip
        int index = Random.Range(0, variousSFX.Length);
        if (variousSFX[index] != null)
            sfxSource.PlayOneShot(variousSFX[index]);
    }
}
