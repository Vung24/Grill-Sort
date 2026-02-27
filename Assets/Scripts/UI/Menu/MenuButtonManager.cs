using UnityEngine;

public class MenuButtonManager : MonoBehaviour
{
    [Header("Menu UI")]
    [SerializeField] private Menu _menu;
    [SerializeField] private UIMenuSettingsManager _settings;
    [SerializeField] private VolumeButton _volumeButton;

    public void MenuPlay()
    {
        _menu?.OnPlayClicked();
    }

    public void MenuQuit()
    {
        _menu?.OnQuitClicked();
    }

    public void SettingsToggle()
    {
        _settings?.ToggleSettings();
    }

    public void SettingsOpen()
    {
        _settings?.OpenSettings();
    }

    public void SettingsClose()
    {
        _settings?.CloseSettings();
    }

    public void ToggleVolume()
    {
        _volumeButton?.ToggleVolume();
    }
}
