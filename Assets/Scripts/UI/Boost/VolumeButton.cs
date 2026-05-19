using UnityEngine;
using UnityEngine.UI;

public class VolumeButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _icon;
    [SerializeField] private Sprite _soundOnSprite;
    [SerializeField] private Sprite _soundOffSprite;

    private void Awake()
    {
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }

        if (_icon == null)
        {
            _icon = GetComponentInChildren<Image>(true);
        }

        if (_button != null)
        {
            _button.onClick.RemoveListener(ToggleVolume);
            _button.onClick.AddListener(ToggleVolume);
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

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(ToggleVolume);
        }
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
