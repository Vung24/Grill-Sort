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
    [SerializeField] public AudioClip swapClip;
    [SerializeField] public AudioClip pickUpFoodClip;
    [SerializeField] public AudioClip[] comboClips;

    private bool soundOn = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadCobmboClip();
    }


    void Start()
    {
        PlayBackGroundMusic();
        PlayMissionStart();
    }
    public void LoadCobmboClip()
    {
        comboClips = Resources.LoadAll<AudioClip>("AudioCombo");
        Debug.Log("Combo clips loaded: " + comboClips.Length);
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

    public void PlaySwap()
    {
        if (soundOn)
            effectAudio.PlayOneShot(swapClip);
    }

    public void PlayMissionStart()
    {
        if (soundOn)
            effectAudio.PlayOneShot(missionStart);
    }
    public void PlayPickUpFood()
    {
        if (soundOn)
            effectAudio.PlayOneShot(pickUpFoodClip);
    }

    public void PlayComboAudio(int comboCount)
    {
        if (!soundOn || comboClips == null || comboClips.Length == 0) return;

        // Vd combo 1 thì lấy clip ở index 0
        int index = comboCount - 1;
        
        // Max âm thanh combo giới hạn là 8 (index từ 0 đến 7)
        int maxIndex = Mathf.Min(7, comboClips.Length - 1);

        if (index > maxIndex)
        {
            index = maxIndex;
        }

        if (index >= 0)
        {
            effectAudio.PlayOneShot(comboClips[index]);
        }
    }

    public void ToggleSound()
    {
        soundOn = !soundOn;

        backgroundAudio.mute = !soundOn;
        effectAudio.mute = !soundOn;
    }

    public bool IsSoundOn() => soundOn;
}
