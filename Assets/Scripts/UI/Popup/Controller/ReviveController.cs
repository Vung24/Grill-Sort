using UnityEngine;

public class ReviveController : MonoBehaviour
{
    private GameManager _gameManager;

    public void Initialize(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public void HandleLevelLose(EnumManager.LoseReason reason)
    {
        if (_gameManager == null || _gameManager.IsLevelComplete)
        {
            return;
        }

        _gameManager.SetLevelComplete(true);
        _gameManager.SetLevelWon(false);
        _gameManager.SetCurrentLoseReason(reason);
        _gameManager.SetTimerVisibleForController(false);

        if (reason == EnumManager.LoseReason.OutOfSlot)
        {
            _gameManager.SetCurrentLevelState(EnumManager.LevelState.RevivePanel);
            _gameManager.SetCurrentLoseReason(EnumManager.LoseReason.RevivePanel);
            UIManager.Instance?.ShowRevivePopup();
            return;
        }

        _gameManager.SetCurrentLevelState(EnumManager.LevelState.Lose);
        UIManager.Instance?.ShowLosePopup();
        _gameManager.ConsumeEnergyForController();
        Debug.Log("Level Failed!");
    }

    public void CloseRevivePanelAndShowLose()
    {
        if (_gameManager == null || _gameManager.CurrentLoseReason != EnumManager.LoseReason.RevivePanel)
        {
            return;
        }

        UIManager.Instance?.HideRevivePopup();

        _gameManager.SetCurrentLevelState(EnumManager.LevelState.Lose);
        _gameManager.SetCurrentLoseReason(EnumManager.LoseReason.OutOfSlot);
        UIManager.Instance?.ShowLosePopup();
        _gameManager.ConsumeEnergyForController();
        Debug.Log("Level Failed!");
    }

    public bool TryReviveWithSwapByCoin(int coinCost)
    {
        if (_gameManager == null || _gameManager.CurrentLevelState != EnumManager.LevelState.RevivePanel)
        {
            return false;
        }

        if (CoinManager.Instance == null || !CoinManager.Instance.SpendCoins(coinCost))
        {
            return false;
        }

        bool revived = TryReviveWithSwapInternal();
        if (!revived)
        {
            CoinManager.Instance.AddCoins(coinCost);
        }

        return revived;
    }

    public bool TryReviveWithSwapFree()
    {
        if (_gameManager == null || _gameManager.CurrentLevelState != EnumManager.LevelState.RevivePanel)
        {
            return false;
        }

        return TryReviveWithSwapInternal();
    }

    private bool TryReviveWithSwapInternal()
    {
        UIManager.Instance?.HideRevivePopup();

        _gameManager.SetLevelComplete(false);
        _gameManager.SetLevelWon(false);
        _gameManager.SetCurrentLevelState(EnumManager.LevelState.Playing);
        _gameManager.SetCurrentLoseReason(EnumManager.LoseReason.None);
        _gameManager.SetTimerVisibleForController(true);

        bool usedSwap = BoostManager.Instance != null && BoostManager.Instance.UseSwapForMerge();
        if (usedSwap)
        {
            return true;
        }

        _gameManager.SetLevelComplete(true);
        _gameManager.SetLevelWon(false);
        _gameManager.SetCurrentLevelState(EnumManager.LevelState.RevivePanel);
        _gameManager.SetCurrentLoseReason(EnumManager.LoseReason.RevivePanel);
        _gameManager.SetTimerVisibleForController(false);
        UIManager.Instance?.ShowRevivePopup();

        return false;
    }
}
