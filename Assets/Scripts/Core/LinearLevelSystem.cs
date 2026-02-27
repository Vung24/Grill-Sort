using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LinearLevelSystem : MonoBehaviour
{
    public static LinearLevelSystem Instance { get; private set; }

    public static LinearLevelSystem EnsureInstance()
    {
        if (Instance != null)
        {
            Instance.Initialize();
            return Instance;
        }

        LinearLevelSystem existing = FindObjectOfType<LinearLevelSystem>();
        if (existing != null)
        {
            Instance = existing;
            DontDestroyOnLoad(existing.gameObject);
            Instance.Initialize();
            return existing;
        }

        GameObject go = new GameObject(nameof(LinearLevelSystem));
        LinearLevelSystem created = go.AddComponent<LinearLevelSystem>();
        created.Initialize();
        return created;
    }

    [Header("Level Configuration")]
    [SerializeField] private int _maxLevels = 20;
    [SerializeField] private int _startingLevel = 1;

    [Header("Scene Names")]
    [SerializeField] private string _menuSceneName = "Menu";
    [SerializeField] private string _gameSceneName = "MainScene";

    private int _currentLevel;
    private int _highestLevel;
    private bool _hasLoadedProgress;

    public event System.Action<int> OnLevelStarted;
    public event System.Action<int, bool> OnLevelCompleted;
    public event System.Action OnAllLevelsCompleted;

    public int CurrentLevel => _currentLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == _gameSceneName)
        {
            StartCoroutine(DelayedStartLevel());
        }
    }

    private IEnumerator DelayedStartLevel()
    {
        yield return new WaitForEndOfFrame();
        StartCurrentLevel();
    }

    private void LoadProgress()
    {
        _currentLevel = PlayerPrefs.GetInt("CurrentLevel", _startingLevel);
        _highestLevel = PlayerPrefs.GetInt("HighestLevel", _startingLevel);
    }

    private void Initialize()
    {
        if (_hasLoadedProgress)
        {
            return;
        }

        // ResetProgress();
        LoadProgress();
        _hasLoadedProgress = true;
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt("CurrentLevel", _currentLevel);
        PlayerPrefs.SetInt("HighestLevel", _highestLevel);
        PlayerPrefs.Save();
        Debug.Log($"Saved - Current: {_currentLevel}, Highest: {_highestLevel}");
    }

    public void ResetProgress()
    {
        _currentLevel = _startingLevel;
        _highestLevel = _startingLevel;
        SaveProgress();
    }

    public void StartCurrentLevel()
    {
        Debug.Log($"Starting Level {_currentLevel}");

        if (CustomLevelGenerator.Instance == null)
        {
            Debug.LogWarning("Cannot start level because CustomLevelGenerator is missing.");
            return;
        }

        EnsureCurrentLevelIsValid();

        if (!CustomLevelGenerator.Instance.HasLevel(_currentLevel))
        {
            Debug.LogWarning($"Cannot start level {_currentLevel}: level file is missing.");
            return;
        }

        OnLevelStarted?.Invoke(_currentLevel);
        CustomLevelGenerator.Instance.GenerateLevel(_currentLevel);
    }

    public void CompleteCurrentLevel()
    {
        int completedLevel = _currentLevel;

        _currentLevel++;

        if (_currentLevel > _highestLevel)
        {
            _highestLevel = _currentLevel;
        }
        SaveProgress();

        OnLevelCompleted?.Invoke(completedLevel, _currentLevel > completedLevel);

        if (_currentLevel > _maxLevels)
        {
            OnAllLevelsCompleted?.Invoke();
        }

        Debug.Log($"Next: Level {_currentLevel}");
    }

    public void NextLevel()
    {
        LoadGameScene();
    }

    public void RestartLevel()
    {
        LoadGameScene();
    }

    public void StartNewGame()
    {
        _currentLevel = _startingLevel;
        SaveProgress();
        LoadGameScene();
    }

    public void ContinueGame()
    {
        LoadGameScene();
    }

    public void LoadGameScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(_gameSceneName);
    }

    public void LoadMenuScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(_menuSceneName);
    }

    public int GetDifficultyTier(int levelNumber)
    {
        if (levelNumber <= 5) return 1;
        if (levelNumber <= 10) return 2;
        if (levelNumber <= 20) return 3;
        return 4;
    }

    public int CurrentDifficulty => GetDifficultyTier(_currentLevel);

    public string GetDifficultyName(int tier)
    {
        return tier switch
        {
            1 => "Easy",
            2 => "Medium",
            3 => "Hard",
            _ => "Unknown"
        };
    }

    private void EnsureCurrentLevelIsValid()
    {
        if (CustomLevelGenerator.Instance == null)
        {
            return;
        }

        if (CustomLevelGenerator.Instance.HasLevel(_currentLevel))
        {
            return;
        }

        int totalLevels = CustomLevelGenerator.Instance.GetTotalLevels();
        int fallbackLevel = Mathf.Clamp(_currentLevel, _startingLevel, Mathf.Max(_startingLevel, totalLevels));

        if (!CustomLevelGenerator.Instance.HasLevel(fallbackLevel))
        {
            fallbackLevel = _startingLevel;
        }

        if (!CustomLevelGenerator.Instance.HasLevel(fallbackLevel))
        {
            Debug.LogWarning("No level data found in Resources/Levels.");
            return;
        }

        Debug.LogWarning($"Level {_currentLevel} not found. Fallback to level {fallbackLevel}.");
        _currentLevel = fallbackLevel;
        _highestLevel = Mathf.Max(_highestLevel, _currentLevel);
        SaveProgress();
    }
}
