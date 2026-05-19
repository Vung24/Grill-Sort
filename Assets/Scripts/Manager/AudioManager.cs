using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private string gameSceneName = "MainScene";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Keep one instance per scene; replace stale instance when changing scenes.
            if (Instance.gameObject.scene == gameObject.scene)
            {
                Destroy(gameObject);
                return;
            }
        }

        Instance = this;
        LoadCobmboClip();
    }


    void Start()
    {
        if (SceneManager.GetActiveScene().name == gameSceneName)
        {
            PlayBackGroundMusic();
            PlayMissionStart();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    public void LoadCobmboClip()
    {
        comboClips = Resources.LoadAll<AudioClip>("AudioCombo");
        Debug.Log("Combo clips loaded: " + comboClips.Length);
    }
    public void PlayBackGroundMusic()
    {
        if (backgroundAudio == null || backGroundClip == null) return;
        if (backgroundAudio.clip == backGroundClip && backgroundAudio.isPlaying) return;
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

        if (backgroundAudio != null)
        {
            backgroundAudio.mute = !soundOn;
        }

        if (effectAudio != null)
        {
            effectAudio.mute = !soundOn;
        }
    }

    public bool IsSoundOn() => soundOn;
}
