using UnityEngine;

public class GameplayButtonManager : MonoBehaviour
{
    // Debug flags: set in code, not part of normal gameplay flow.
    private const bool FORCE_RESET_BOOST_ON_START = true;
    private const bool FORCE_UNLIMITED_BOOST_ON_START = false;

    [Header("Settings UI")]
    [SerializeField] private UISettingsManager _settings;

    [Header("Win UI")]
    [SerializeField] private UIWinGame _winGame;

    [Header("Lose UI")]
    [SerializeField] private UILoseGame _loseGame;

    [Header("Boosts")]
    [SerializeField] private BoostRemove _boostRemoveThree;
    [SerializeField] private BoostSwap _boostSwapForMerge;

    [Header("Audio")]
    [SerializeField] private VolumeButton _volumeButton;

    private void Start()
    {
        // _boostRemoveThree.EnableUnlimitedUse();
        _boostRemoveThree.ResetBoostToDefault();
        _boostSwapForMerge.EnableUnlimitedUse();
        // _boostSwapForMerge.ResetBoostToDefault();

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

    public void SettingsMenu()
    {
        _settings?.LoadMenu();
    }

    public void SettingsPlayAgain()
    {
        _settings?.LoadPlayAgainGame();
    }

    public void WinMenu()
    {
        _winGame?.LoadMenu();
    }

    public void WinNextLevel()
    {
        _winGame?.NextLevel();
    }

    public void LoseMenu()
    {
        _loseGame?.LoadMenu();
    }

    public void LosePlayAgain()
    {
        _loseGame?.LoadPlayAgainGame();
    }

    public void BoostRemoveThree()
    {
        _boostRemoveThree?.OnUseBoost();
    }

    public void BoostSwapForMerge()
    {
        _boostSwapForMerge?.OnUseBoost();
    }

    public void ToggleVolume()
    {
        _volumeButton?.ToggleVolume();
    }
}
