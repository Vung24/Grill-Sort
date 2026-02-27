using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] public AudioSource backgroundAudio;
    [SerializeField] public AudioSource effectAudio;
    [SerializeField] public AudioClip backGroundClip;
    [SerializeField] public AudioClip completeMission;
    [SerializeField] public AudioClip missionStart;

    private bool soundOn = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    void Start()
    {
        PlayBackGroundMusic();
        PlayMissionStart();
    }

    public void PlayBackGroundMusic()
    {
        backgroundAudio.clip = backGroundClip;
        backgroundAudio.Play();
    }

    public void PlayCompleteMission()
    {
        if (soundOn)
            effectAudio.PlayOneShot(completeMission);
    }

    public void PlayMissionStart()
    {
        if (soundOn)
            effectAudio.PlayOneShot(missionStart);
    }
    public void ToggleSound()
    {
        soundOn = !soundOn;

        backgroundAudio.mute = !soundOn;
        effectAudio.mute = !soundOn;
    }

    public bool IsSoundOn() => soundOn;
}
