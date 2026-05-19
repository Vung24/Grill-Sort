using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Energy : MonoBehaviour
{
    public static Energy Instance { get; private set; }

    [Header("Logic Settings")]
    [SerializeField] private int _maxHearts = 5;
    [SerializeField] private int _regenMinutes = 30;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _countEnergyText;
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private Image _panelTime;
    [SerializeField] private Image _energyImage;

    private const string HeartsKey = "Energy_Current";
    private const string NextRegenKey = "Energy_NextRegenTicks";

    private int _currentHearts;
    private long _nextRegenTicks;

    public event Action OnEnergyChanged;

    public static Energy EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        Energy existing = FindObjectOfType<Energy>();
        if (existing != null)
        {
            Instance = existing;
            return existing;
        }

        GameObject go = new GameObject("Energy");
        return go.AddComponent<Energy>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Load();
        RecalculateEnergy();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Update()
    {
        TickRegen();
        UpdateTimeText();
    }

    public bool CanPlay()
    {
        return _currentHearts > 0;
    }

    public void OnLose()
    {
        if (_currentHearts <= 0)
        {
            return;
        }

        _currentHearts = Mathf.Max(0, _currentHearts - 1);
        if (_currentHearts < _maxHearts && _nextRegenTicks <= 0)
        {
            _nextRegenTicks = DateTime.UtcNow.AddMinutes(_regenMinutes).Ticks;
        }

        Save();
        NotifyChanged();
    }

    public void ResetHearts()
    {
        _currentHearts = _maxHearts;
        _nextRegenTicks = 0;
        Save();
        NotifyChanged();
    }

    public int CurrentHearts => _currentHearts;
    public int MaxHearts => _maxHearts;
    public bool IsFull => _currentHearts >= _maxHearts;

    public TimeSpan TimeToNext
    {
        get
        {
            if (_currentHearts >= _maxHearts || _nextRegenTicks <= 0)
            {
                return TimeSpan.Zero;
            }

            long remainingTicks = _nextRegenTicks - DateTime.UtcNow.Ticks;
            if (remainingTicks < 0) remainingTicks = 0;
            return TimeSpan.FromTicks(remainingTicks);
        }
    }

    private void TickRegen()
    {
        if (_currentHearts >= _maxHearts)
        {
            return;
        }

        if (_nextRegenTicks <= 0)
        {
            _nextRegenTicks = DateTime.UtcNow.AddMinutes(_regenMinutes).Ticks;
            Save();
            NotifyChanged();
            return;
        }

        if (DateTime.UtcNow.Ticks >= _nextRegenTicks)
        {
            RecalculateEnergy();
        }
    }

    private void RecalculateEnergy()
    {
        if (_currentHearts >= _maxHearts)
        {
            _nextRegenTicks = 0;
            Save();
            NotifyChanged();
            return;
        }

        DateTime now = DateTime.UtcNow;
        if (_nextRegenTicks <= 0)
        {
            _nextRegenTicks = now.AddMinutes(_regenMinutes).Ticks;
        }

        while (_currentHearts < _maxHearts && now.Ticks >= _nextRegenTicks)
        {
            _currentHearts++;
            _nextRegenTicks = new DateTime(_nextRegenTicks, DateTimeKind.Utc)
                .AddMinutes(_regenMinutes)
                .Ticks;
        }

        if (_currentHearts >= _maxHearts)
        {
            _nextRegenTicks = 0;
        }

        Save();
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        OnEnergyChanged?.Invoke();
        Refresh();
    }

    private void Refresh()
    {
        if (_countEnergyText != null)
        {
            _countEnergyText.text = _currentHearts.ToString();
        }

        UpdateTimeText();
    }

    private void UpdateTimeText()
    {
        if (_timeText == null)
        {
            return;
        }

        if (!_timeText.gameObject.activeInHierarchy)
        {
            return;
        }

        if (IsFull)
        {
            _timeText.text = "FULL";
            return;
        }

        TimeSpan remaining = TimeToNext;
        int minutes = Mathf.FloorToInt((float)remaining.TotalMinutes);
        int seconds = Mathf.FloorToInt((float)remaining.TotalSeconds % 60f);
        _timeText.text = $"{minutes:00}:{seconds:00}";
    }

    public void ResetHeartAmount()
    {
        ResetHearts();
    }

    private void Load()
    {
        _currentHearts = PlayerPrefs.GetInt(HeartsKey, _maxHearts);
        string ticksStr = PlayerPrefs.GetString(NextRegenKey, "0");
        if (!long.TryParse(ticksStr, out _nextRegenTicks))
        {
            _nextRegenTicks = 0;
        }
    }

    private void Save()
    {
        PlayerPrefs.SetInt(HeartsKey, _currentHearts);
        PlayerPrefs.SetString(NextRegenKey, _nextRegenTicks.ToString());
        PlayerPrefs.Save();
    }
}
