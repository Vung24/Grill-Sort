using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _timerText;
    private float _currentTime;
    private float _levelTimeLimit;
    private bool _timerStarted;
    private bool _isTimerPaused;

    public event System.Action OnTimeUp;

    public float CurrentTime => _currentTime;
    public float LevelTimeLimit => _levelTimeLimit;
    public bool IsTimerPaused => _isTimerPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        TickTimer();
    }

    public void SetupLevelTimer(float levelSeconds)
    {
        _levelTimeLimit = Mathf.Max(0f, levelSeconds);
        _currentTime = _levelTimeLimit;
        _timerStarted = false;
        _isTimerPaused = false;
        SetTimerVisible(true);
        RefreshTimer();
    }

    public void StartTimer()
    {
        _timerStarted = true;
    }

    public void SetTimerPaused(bool isPaused)
    {
        _isTimerPaused = isPaused;
    }

    public void SetTimerVisible(bool isVisible)
    {
        if (_timerText == null) return;
        _timerText.gameObject.SetActive(isVisible);
    }

    public bool CanAddTime()
    {
        return _levelTimeLimit > 0f;
    }

    public void AddTime(float addSeconds)
    {
        _currentTime += Mathf.Max(0f, addSeconds);
        RefreshTimer();
    }

    private void TickTimer()
    {
        if (_levelTimeLimit <= 0f) return;
        if (!_timerStarted || _isTimerPaused) return;

        _currentTime -= Time.deltaTime;

        if (_currentTime <= 0f)
        {
            _currentTime = 0f;
            _timerStarted = false;
            OnTimeUp?.Invoke();
        }

        RefreshTimer();
    }

    private void RefreshTimer()
    {
        if (_timerText == null || _levelTimeLimit <= 0f) return;

        int minutes = Mathf.FloorToInt(_currentTime / 60f);
        int seconds = Mathf.FloorToInt(_currentTime % 60f);

        _timerText.text = $"{minutes:00}:{seconds:00}";
        if (_currentTime <= 30f)
        {
            _timerText.color = Color.red;
        }
        else if (_currentTime <= 60f)
        {
            _timerText.color = Color.yellow;
        }
        else
        {
            _timerText.color = Color.white;
        }
    }
}
