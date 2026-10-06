using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance; 

    public AudioSource musicSource; 
    public AudioSource sfxSource;   
    public AudioClip backgroundMusic;
    public AudioClip clickSound;
    public AudioClip achievementSound;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); 
    }

    void Start()
    {
        PlayBackgroundMusic(); 
    }

    public void PlayBackgroundMusic()
    {
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayClickSound()
    {
        sfxSource.PlayOneShot(clickSound);
    }

    public void PlayAchievementSound()
    {
        sfxSource.PlayOneShot(achievementSound);
    }
}