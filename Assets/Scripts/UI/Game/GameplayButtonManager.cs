using UnityEngine;

public class GameplayButtonManager : MonoBehaviour
{
    private const bool FORCE_RESET_BOOST_ON_START = true;
    private const bool FORCE_UNLIMITED_BOOST_ON_START = false;

    [Header("Settings UI")]
    [SerializeField] private UISettingsManager _settings;

    [Header("Win UI")]
    [SerializeField] private UIWinGame _winGame;

    [Header("Lose UI")]
    [SerializeField] private UILoseGame _loseGame;

    [Header("Boosts")]
    [SerializeField] private BoostRemove _boostRemove;
    [SerializeField] private BoostSwap _boostSwap;
    [SerializeField] private BoostAddTime _boostAddTime;

    [Header("Audio")]
    [SerializeField] private VolumeButton _volumeButton;

    private void Start()
    {
        _boostRemove.ResetBoostToDefault();
        _boostSwap.ResetBoostToDefault();
        _boostAddTime.ResetBoostToDefault();
        // // _boostRemove.EnableUnlimitedUse();
        // // _boostSwap.EnableUnlimitedUse();
        // _boostAddTime?.EnableUnlimitedUse();
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
        _boostRemove?.OnUseBoost();
    }

    public void BoostSwapForMerge()
    {
        _boostSwap?.OnUseBoost();
    }

    public void BoostAddTime30()
    {
        _boostAddTime?.OnUseBoost();
    }

    public void BoostAddTime30Unlimited()
    {
        _boostAddTime?.EnableUnlimitedUse();
    }

    public void BoostAddTime30Reset()
    {
        _boostAddTime?.ResetBoostToDefault();
    }

    public void ToggleVolume()
    {
        _volumeButton?.ToggleVolume();
    }
}
