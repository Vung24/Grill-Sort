using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LinearLevelSystem : MonoBehaviour
{
    public static LinearLevelSystem Instance { get; private set; }

    [Header("Level Configuration")]
    [SerializeField] private int _maxLevels = 50;
    [SerializeField] private int _startingLevel = 1;

    [Header("Scene Names")]
    [SerializeField] private string _menuSceneName = "Menu";
    [SerializeField] private string _gameSceneName = "MainScene";

    [Header("Auto-Load Settings")]
    [SerializeField] private bool _autoLoadNextLevel = true;
    [SerializeField] private float _nextLevelDelay = 2f;

    private int _currentLevel;
    private int _highestLevel;

    public event System.Action<int> OnLevelStarted;
    public event System.Action<int, bool> OnLevelCompleted;
    public event System.Action OnAllLevelsCompleted;

    public int CurrentLevel => _currentLevel;
    public int HighestLevel => _highestLevel;
    public bool IsLastLevel => _currentLevel >= _maxLevels;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadProgress();
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == _gameSceneName)
        {
            StartCurrentLevel();
        }
    }

    private void LoadProgress()
    {
        _currentLevel = PlayerPrefs.GetInt("CurrentLevel", _startingLevel);
        _highestLevel = PlayerPrefs.GetInt("HighestLevel", _startingLevel);
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt("CurrentLevel", _currentLevel);
        PlayerPrefs.SetInt("HighestLevel", _highestLevel);
        PlayerPrefs.Save();
        Debug.Log($"Progress saved: Level {_currentLevel}");
    }

    public void ResetProgress()
    {
        _currentLevel = _startingLevel;
        _highestLevel = _startingLevel;
        SaveProgress();
        Debug.Log("Progress reset to Level 1");
    }

    public void StartCurrentLevel()
    {
        Debug.Log($"Starting Level {_currentLevel}");
        if (CustomLevelGenerator.Instance != null)
        {
            if (!CustomLevelGenerator.Instance.HasLevel(_currentLevel))
            {
                Debug.LogError($"💡 Create: Resources/Levels/level_{_currentLevel}.json");
                return;
            }

            OnLevelStarted?.Invoke(_currentLevel);
            CustomLevelGenerator.Instance.GenerateLevel(_currentLevel);
        }
        else if (CustomLevelGenerator.Instance != null)
        {
            OnLevelStarted?.Invoke(_currentLevel);
            CustomLevelGenerator.Instance.GenerateLevel(_currentLevel);
        }
        else
        {
            Debug.LogError("No level gene");
        }
    }

    public void CompleteCurrentLevel()
    {
        bool isNewRecord = _currentLevel >= _highestLevel;
        if (isNewRecord)
        {
            _highestLevel = _currentLevel + 1;
            Debug.Log($"New record{_highestLevel}");
        }

        OnLevelCompleted?.Invoke(_currentLevel, isNewRecord);
        SaveProgress();

        if (IsLastLevel)
        {
            OnAllLevelsCompleted?.Invoke();
        }
        else
        {
            if (_autoLoadNextLevel)
            {
                StartCoroutine(LoadNextLevelAfterDelay());
            }
        }
    }

    public void NextLevel()
    {
        if (!IsLastLevel)
        {
            _currentLevel++;
            SaveProgress();

            if (SceneManager.GetActiveScene().name == _gameSceneName)
            {
                StartCurrentLevel();
            }
            else
            {
                LoadGameScene();
            }
        }
    }

    public void RestartLevel()
    {
        Debug.Log($"Restarting Level {_currentLevel}");
        StartCurrentLevel();
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

    private IEnumerator LoadNextLevelAfterDelay()
    {
        yield return new WaitForSeconds(_nextLevelDelay);
        NextLevel();
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene(_gameSceneName);
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadScene(_menuSceneName);
    }

    public int GetDifficultyTier(int levelNumber)
    {
        if (levelNumber <= 5) return 1;
        if (levelNumber <= 15) return 2;
        if (levelNumber <= 30) return 3;
        if (levelNumber <= 50) return 4;
        return 5;
    }

    public int CurrentDifficulty => GetDifficultyTier(_currentLevel);

    public string GetDifficultyName(int tier)
    {
        switch (tier)
        {
            case 1: return "Easy";
            case 2: return "Medium";
            case 3: return "Hard";
            case 4: return "Expert";
            case 5: return "Master";
            default: return "Unknown";
        }
    }
}