using UnityEngine;

// Audio Manager (Lab 7, Exercise #18)
public class AudioManager : MonoBehaviour
{
    //Why static? Bc whole game will have only 1 instance of AudioManager
    public static AudioManager Instance;

    public AudioSource musicSource; //source that plays background music
    public AudioSource sfxSource; //source that plays sound effects

    public AudioClip libraryMusic; // audio clip of background music for level 3

    public AudioClip[] variousSFX; //array of sound effects clips to keep things varied

    void Awake()
    {
        //make sure the entire game only has one Audio Manager throughout
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        musicSource.clip = libraryMusic;

        musicSource.Play();
    }

    //Public in case another object needs to call for a specific sound effect to begin playing
    public void PlayMusicSFX(AudioClip clip)
    {
        if (clip == null) return; //nothing assigned in the Inspector yet

        sfxSource.PlayOneShot(clip);
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
