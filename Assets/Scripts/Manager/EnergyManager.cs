using System;
using UnityEngine;

public class EnergyManager : MonoBehaviour
{
    public static EnergyManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int _maxHearts = 5;
    [SerializeField] private int _regenMinutes = 30;

    private const string HeartsKey = "Energy_Current";
    private const string NextRegenKey = "Energy_NextRegenTicks";

    private int _currentHearts;
    private long _nextRegenTicks;

    public event Action OnEnergyChanged;

    public static EnergyManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        EnergyManager existing = FindObjectOfType<EnergyManager>();
        if (existing != null)
        {
            Instance = existing;
            return existing;
        }

        GameObject go = new GameObject("EnergyManager");
        return go.AddComponent<EnergyManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Keep host GameObject if it contains other components (e.g. menu managers).
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
        RecalculateEnergy();
    }

    private void Update()
    {
        if (_currentHearts >= _maxHearts)
        {
            return;
        }

        if (_nextRegenTicks <= 0)
        {
            _nextRegenTicks = DateTime.UtcNow.AddMinutes(_regenMinutes).Ticks;
            Save();
            OnEnergyChanged?.Invoke();
            return;
        }

        DateTime now = DateTime.UtcNow;
        if (now.Ticks >= _nextRegenTicks)
        {
            RecalculateEnergy();
        }
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
        OnEnergyChanged?.Invoke();
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

    private void RecalculateEnergy()
    {
        if (_currentHearts >= _maxHearts)
        {
            _nextRegenTicks = 0;
            Save();
            OnEnergyChanged?.Invoke();
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
        OnEnergyChanged?.Invoke();
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
