using UnityEngine;
using UnityEngine.UI;

public class VolumeButton : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private Sprite _soundOnSprite;
    [SerializeField] private Sprite _soundOffSprite;

    private void Awake()
    {
        if (_icon == null)
        {
            _icon = GetComponentInChildren<Image>(true);
        }
    }

    private void Start()
    {
        UpdateIcon();
    }

    private void OnEnable()
    {
        UpdateIcon();
    }

    public void ToggleVolume()
    {
        AudioManager.Instance?.ToggleSound();
        UpdateIcon();
    }

    public void UpdateIcon()
    {
        if (_icon == null)
        {
            return;
        }

        bool isOn = AudioManager.Instance == null || AudioManager.Instance.IsSoundOn();
        _icon.sprite = isOn ? _soundOnSprite : _soundOffSprite;
    }
}
