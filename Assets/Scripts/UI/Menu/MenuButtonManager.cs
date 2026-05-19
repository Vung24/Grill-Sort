using UnityEngine;

public class MenuButtonManager : MonoBehaviour
{
    [Header("Menu UI")]
    [SerializeField] private Menu _menu;
    [SerializeField] private UIMenuSettingsManager _settings;
    [SerializeField] private VolumeButton _volumeButton;

    public void MenuPlay()
    {
        if (_menu != null)
        {
            _menu.OnPlayClicked();
            return;
        }

        LinearLevelSystem.EnsureInstance().ContinueGame();
    }

    public void MenuQuit()
    {
        if (_menu != null)
        {
            _menu.OnQuitClicked();
            return;
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SettingsToggle()
    {
        if (_settings != null)
        {
            _settings.ToggleSettings();
        }
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
