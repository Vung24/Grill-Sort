using UnityEngine;

public class RewardController : MonoBehaviour
{
    private const int WinRewardCoins = 50;

    private GameManager _gameManager;

    public void Initialize(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public void HandleLevelWin()
    {
        if (_gameManager == null || _gameManager.IsLevelComplete)
        {
            return;
        }

        _gameManager.SetLevelComplete(true);
        _gameManager.SetLevelWon(true);
        _gameManager.SetTimerVisibleForController(false);
        AudioManager.Instance?.PlayCompleteMission();

        if (UIManager.Instance != null)
        {
            _gameManager.SetCurrentLevelState(EnumManager.LevelState.RewardPanel);
            UIManager.Instance.ShowRewardPopup();
        }
        else
        {
            _gameManager.SetCurrentLevelState(EnumManager.LevelState.Win);
            CoinManager.Instance?.AddCoins(WinRewardCoins);
            UIManager.Instance?.ShowWinPopup();
        }

        LinearLevelSystem.Instance?.CompleteCurrentLevel();
    }

    public bool TryClaimWinReward()
    {
        return ClaimWinRewardInternal(false);
    }

    public bool TryClaimWinRewardX2WithAd()
    {
        ShowRewardedAdPanelPlaceholder();
        return ClaimWinRewardInternal(true);
    }

    private bool ClaimWinRewardInternal(bool doubled)
    {
        if (_gameManager == null || _gameManager.CurrentLevelState != EnumManager.LevelState.RewardPanel)
        {
            return false;
        }

        int rewardCoins = WinRewardCoins;
        if (doubled)
        {
            rewardCoins *= 2;
        }

        CoinManager.Instance?.AddCoins(rewardCoins);
        UIManager.Instance?.HideRewardPopup();

        _gameManager.SetCurrentLevelState(EnumManager.LevelState.Win);
        UIManager.Instance?.ShowWinPopup();

        return true;
    }

    private void ShowRewardedAdPanelPlaceholder()
    {
        // TODO: Open your rewarded-ad panel here and only grant x2 reward after ad success callback.
    }
}
