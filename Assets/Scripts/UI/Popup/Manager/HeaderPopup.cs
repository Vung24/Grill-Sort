using TMPro;
using UnityEngine;

public class HeaderPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _difficultyText;
    void Start()
    {
        UpdateLevel();         
    }
    public void SetHeader(int level, string difficulty)
    {
        if (_levelText != null)
        {
            _levelText.text = $"<size=50>Level {level}</size>";
        }

        if (_difficultyText != null)
        {
            _difficultyText.text = difficulty;
        }
    }

    public void UpdateLevel()
    {
        LinearLevelSystem levelSystem = LinearLevelSystem.EnsureInstance();
        int currentLevel = levelSystem.CurrentLevel;
        int difficultyTier = GetDifficultyTierFromLevelData(currentLevel);
        string difficultyName = levelSystem.GetDifficultyName(difficultyTier);
        SetHeader(currentLevel, difficultyName);
    }

    private int GetDifficultyTierFromLevelData(int currentLevel)
    {
        LevelDataFromJSON levelData = null;

        if (GeneratorLevel.Instance != null)
        {
            levelData = GeneratorLevel.Instance.GetCurrentLevelData();
        }

        if (levelData == null)
        {
            levelData = LevelDataFromJSON.LoadFromResources($"level_{currentLevel}");
        }

        return levelData != null ? levelData.difficult : 0;
    }
}
