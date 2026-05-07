using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class SettingsPopup : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private ButtonEffectLogic Setting;
    [SerializeField] private ButtonEffectLogic Menu, playAgain, Close;
    [Header("Popup")]
    [SerializeField] private GameObject _settingPopup;
    [SerializeField] private RestartPopup _restartPopup;
    [SerializeField] private TextMeshProUGUI _levelText;

    [Header("Audio Settings")]
    [SerializeField] private ButtonEffectLogic _btnSound;
    [SerializeField] private Image _soundIcon;
    [SerializeField] private Sprite _soundOnSprite;
    [SerializeField] private Sprite _soundOffSprite;

    private bool isOpen = false;

    void Start()
    {
        BindButtons();
        UpdateSoundIcon();
        if (_settingPopup != null)
        {
            _settingPopup.transform.localScale = Vector3.zero;
            _settingPopup.SetActive(false);
        }
        _restartPopup?.ClosePopup();
        UpdateLevel();
    }

    private void OnDestroy()
    {
        UnbindButtons();
    }

    public void ToggleSettings()
    {
        if (!isOpen)
            OpenSettings();
        else
            CloseSettings();
    }

    public void OpenSettings()
    {
        if (_settingPopup == null)
        {
            return;
        }
        isOpen = true;
        GameManager.Instance?.SetTimerPaused(true);
        _settingPopup.SetActive(true);
        _settingPopup.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
    }

    public void CloseSettings()
    {
        if (_settingPopup == null)
        {
            return;
        }
        isOpen = false;
        GameManager.Instance?.SetTimerPaused(false);
        _settingPopup.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack)
            .OnComplete(() => _settingPopup.SetActive(false));
        _restartPopup?.ClosePopup();
    }

    public void LoadMenu()
    {
        GameManager.Instance?.ConsumeEnergyIfAbandon();
        LinearLevelSystem.EnsureInstance().LoadMenuScene();
    }

    public void LoadRestartPopup()
    {
        if (_restartPopup == null)
        {
            return;
        }
        _restartPopup.OpenPopup();
    }

    public void OnRestartConfirmed()
    {
        isOpen = false;
        GameManager.Instance?.SetTimerPaused(false);
        if (_settingPopup != null)
        {
            _settingPopup.SetActive(false);
        }
    }
    public void UpdateLevel()
    {
        if (_levelText == null)
        {
            return;
        }

        int currentLevel = LinearLevelSystem.EnsureInstance().CurrentLevel;
        _levelText.text = $"<size=50>Level {currentLevel}</size>";
    }

    public void ToggleSound()
    {
        AudioManager.Instance?.ToggleSound();
        UpdateSoundIcon();
    }

    private void UpdateSoundIcon()
    {
        if (_soundIcon == null) return;
        
        bool isOn = AudioManager.Instance == null || AudioManager.Instance.IsSoundOn();
        _soundIcon.sprite = isOn ? _soundOnSprite : _soundOffSprite;
    }

    private void BindButtons()
    {
        if (Setting != null)
        {
            Setting.onClick.RemoveListener(OpenSettings);
            Setting.onClick.AddListener(OpenSettings);
        }

        if (Close != null)
        {
            Close.onClick.RemoveListener(CloseSettings);
            Close.onClick.AddListener(CloseSettings);
        }

        if (Menu != null)
        {
            Menu.onClick.RemoveListener(LoadMenu);
            Menu.onClick.AddListener(LoadMenu);
        }

        if (playAgain != null)
        {
            playAgain.onClick.RemoveListener(LoadRestartPopup);
            playAgain.onClick.AddListener(LoadRestartPopup);
        }

        if (_btnSound != null)
        {
            _btnSound.onClick.RemoveListener(ToggleSound);
            _btnSound.onClick.AddListener(ToggleSound);
        }
    }

    private void UnbindButtons()
    {
        if (Setting != null) Setting.onClick.RemoveListener(OpenSettings);
        if (Close != null) Close.onClick.RemoveListener(CloseSettings);
        if (Menu != null) Menu.onClick.RemoveListener(LoadMenu);
        if (playAgain != null) playAgain.onClick.RemoveListener(LoadRestartPopup);
        if (_btnSound != null) _btnSound.onClick.RemoveListener(ToggleSound);
    }
    private void OnDisable()
    {
        isOpen = false;
        GameManager.Instance?.SetTimerPaused(false);
    }
}
