using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform _gridGrill;
    public BoostManager _boostManager;
    [SerializeField] private Combo _comboSystem;

    [Header("Hint System")]
    [SerializeField] private GameHintSystem _hintSystem;
    [SerializeField] private ReviveController _reviveController;
    [SerializeField] private RewardController _rewardController;
    [SerializeField] private GameplayConditionController _conditionController;

    private List<GrillStation> _grillStations;
    private int _totalItemsToMerge;
    private int _itemsMerged;
    private bool _levelComplete;
    private bool _levelWon;
    private bool _hasMovedFood;
    private bool _energyConsumedThisAttempt;
    private EnumManager.LevelState _currentLevelState;
    private EnumManager.LoseReason _currentLoseReason;

    public event System.Action<float, int, int> OnMergeProgressChanged;

    private void Awake()
    {
        Instance = this;
        _grillStations = Utillities.GetListInChild<GrillStation>(_gridGrill);
        
        if (_comboSystem == null)
        {
            _comboSystem = FindObjectOfType<Combo>();
        }

        if (_hintSystem == null)
        {
            _hintSystem = GetComponent<GameHintSystem>();
        }
        if (_hintSystem == null)
        {
            _hintSystem = gameObject.AddComponent<GameHintSystem>();
        }
        _hintSystem.Initialize(_grillStations);
        EnergyManager.EnsureInstance();
        BoostManager boostManager = BoostManager.EnsureInstance();
        if (boostManager != null)
        {
            boostManager.InitializeGameplay(this, _grillStations, _hintSystem);
        }
        _reviveController.Initialize(this);
        _rewardController.Initialize(this);
        _conditionController.Initialize(this, _grillStations);
        CoinManager.EnsureInstance();
    }

    private void Start()
    {
        _levelComplete = false;
        _currentLevelState = EnumManager.LevelState.None;
        _currentLoseReason = EnumManager.LoseReason.None;

        UIManager.Instance?.ResetGameplayPopups();
        SubscribeTimeManager();
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeUp -= HandleTimeUp;
        }
    }

    private void Update()
    {
        if (_currentLevelState == EnumManager.LevelState.Playing)
        {
            _hintSystem?.TickHint();
            _conditionController?.TickLoseConditions();
        }
    }

    public void OnLevelGenerated(int totalItems)
    {
        _totalItemsToMerge = totalItems;
        _itemsMerged = 0;
        _levelComplete = false;
        _levelWon = false;
        _currentLevelState = EnumManager.LevelState.Playing;
        _currentLoseReason = EnumManager.LoseReason.None;
        _hintSystem?.ResetHintTimer();
        _hasMovedFood = false;
        _energyConsumedThisAttempt = false;
        SetTimerVisible(true);
        UIManager.Instance?.ResetGameplayPopups();
        LevelDataFromJSON levelData = GeneratorLevel.Instance?.GetCurrentLevelData();

        float levelSeconds = levelData != null ? levelData.levelSeconds : 0f;
        SubscribeTimeManager();
        TimeManager.Instance?.SetupLevelTimer(levelSeconds);
        Debug.Log($"Level time limit: {levelSeconds} seconds");
        NotifyMerge();
    }

    public void OnItemsMerged(int count)
    {
        if (_levelComplete)
        {
            return;
        }

        _itemsMerged += count;
        _hintSystem?.ResetHintTimer();
        
        if (_comboSystem != null)
        {
            _comboSystem.AddCombo();
        }

        NotifyMerge();
        _conditionController?.CheckWinCondition();
    }
    #region STATE CHANGE UI
    private void OnLevelWin()
    {
        if (_rewardController != null)
        {
            _rewardController.HandleLevelWin();
        }
    }

    public bool TryClaimWinReward()
    {
        return _rewardController != null && _rewardController.TryClaimWinReward();
    }

    public bool TryClaimWinRewardX2WithAd()
    {
        return _rewardController != null && _rewardController.TryClaimWinRewardX2WithAd();
    }

    private void OnLevelLose(EnumManager.LoseReason reason)
    {
        if (_reviveController != null)
        {
            _reviveController.HandleLevelLose(reason);
        }
    }

    public void CloseRevivePanelAndShowLose()
    {
        _reviveController?.CloseRevivePanelAndShowLose();
    }

    public bool TryReviveWithSwapByCoin(int coinCost)
    {
        return _reviveController != null && _reviveController.TryReviveWithSwapByCoin(coinCost);
    }

    public bool TryReviveWithSwapFree()
    {
        return _reviveController != null && _reviveController.TryReviveWithSwapFree();
    }
    #endregion

    public void ShowHint()
    {
        _hintSystem?.ShowHintNow();
    }

    public void OnNextLevelButtonClicked()
    {
        LinearLevelSystem.Instance?.NextLevel();
    }

    public float MergeProgress => _totalItemsToMerge > 0 ? Mathf.Clamp01((float)_itemsMerged / _totalItemsToMerge) : 0f;
    public int ItemsMerged => _itemsMerged;
    public int TotalItemsToMerge => _totalItemsToMerge;
    public bool HasMovedFood => _hasMovedFood;
    public bool IsLevelComplete => _levelComplete;
    public bool IsLevelWon => _levelWon;
    public EnumManager.LevelState CurrentLevelState => _currentLevelState;
    public EnumManager.LoseReason CurrentLoseReason => _currentLoseReason;
    public bool IsTimerPaused => TimeManager.Instance != null && TimeManager.Instance.IsTimerPaused;

    public void MarkFoodMoved()
    {
        _hasMovedFood = true;
        TimeManager.Instance?.StartTimer();
    }

    public void SetTimerPaused(bool isPaused)
    {
        TimeManager.Instance?.SetTimerPaused(isPaused);
    }

    public void ConsumeEnergyIfAbandon()
    {
        if (_levelComplete)
        {
            return;
        }

        if (!_hasMovedFood)
        {
            return;
        }

        ConsumeEnergy();
    }

    private void ConsumeEnergy()
    {
        if (_energyConsumedThisAttempt)
        {
            return;
        }

        EnergyManager.Instance?.OnLose();
        _energyConsumedThisAttempt = true;
    }

    private void SetTimerVisible(bool isVisible)
    {
        TimeManager.Instance?.SetTimerVisible(isVisible);
    }

    private void HandleTimeUp()
    {
        _conditionController?.HandleTimeUp();
    }

    private void SubscribeTimeManager()
    {
        if (TimeManager.Instance == null)
        {
            return;
        }

        TimeManager.Instance.OnTimeUp -= HandleTimeUp;
        TimeManager.Instance.OnTimeUp += HandleTimeUp;
    }

    private void NotifyMerge()
    {
        OnMergeProgressChanged?.Invoke(MergeProgress, _itemsMerged, _totalItemsToMerge);
    }

    internal void SetLevelComplete(bool value)
    {
        _levelComplete = value;
    }

    internal void SetLevelWon(bool value)
    {
        _levelWon = value;
    }

    internal void SetCurrentLevelState(EnumManager.LevelState state)
    {
        _currentLevelState = state;
    }

    internal void SetCurrentLoseReason(EnumManager.LoseReason reason)
    {
        _currentLoseReason = reason;
    }

    internal void SetTimerVisibleForController(bool isVisible)
    {
        SetTimerVisible(isVisible);
    }

    internal void ConsumeEnergyForController()
    {
        ConsumeEnergy();
    }

    internal void TriggerLevelWin()
    {
        OnLevelWin();
    }

    internal void TriggerLevelLose(EnumManager.LoseReason reason)
    {
        OnLevelLose(reason);
    }
}
