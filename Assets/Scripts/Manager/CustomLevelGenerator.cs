// Save as: Assets/Scripts/Managers/CustomLevelGenerator.cs

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Generate levels ONLY from JSON data files
/// No procedural generation - Pure custom levels
/// </summary>
public class CustomLevelGenerator : MonoBehaviour
{
    public static CustomLevelGenerator Instance { get; private set; }

    [Header("JSON Settings")]
    [SerializeField] private string _levelFilePrefix = "level_";

    [Header("Resources")]
    [SerializeField] private string _foodSpritePath = "Items";

    [Header("References")]
    [SerializeField] private Transform _gridGrill;

    private List<GrillStation> _grillStations;
    private List<Sprite> _availableFoodSprites;
    private LevelDataFromJSON _currentLevelData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadResources();
        _grillStations = Utils.GetListInChild<GrillStation>(_gridGrill);
    }

    private void LoadResources()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(_foodSpritePath);
        _availableFoodSprites = sprites.ToList();

        if (_availableFoodSprites.Count == 0)
        {
            Debug.LogError($" No food sprites found in Resources/{_foodSpritePath}!");
        }
        else
        {
            Debug.Log($" Loaded {_availableFoodSprites.Count} food sprites");
        }
    }

    public void GenerateLevel(int levelNumber)
    {
        string fileName = $"{_levelFilePrefix}{levelNumber}";
        _currentLevelData = LevelDataFromJSON.LoadFromResources(fileName);

        if (_currentLevelData == null)
        {
            Debug.LogError($" Cannot load level {levelNumber}! File not found: Resources/Levels/{fileName}.json");
            Debug.LogError($" Create this file or check the file name.");
            return;
        }

        Debug.Log($"📄 Generating Level {levelNumber} from JSON: {fileName}.json");
        GenerateFromJSONData(_currentLevelData);
    }

    private void GenerateFromJSONData(LevelDataFromJSON data)
    {
        int totalWare = data.spawnWareData.totalWare;
        int totalTypes = data.spawnWareData.totalWarePattern;
        int totalGridPositions = data.boardData.listTrayData.Count;
        int activeGrills = 0;
        int totalTrays = 0; // Total waiting trays (stacked below)

        for (int i = 0; i < totalGridPositions; i++)
        {
            if (data.boardData.listTrayData[i] != null)
            {
                activeGrills++;
                totalTrays += data.boardData.listTrayData[i].size; // size = number of trays per grill
            }
        }

        // Validate data
        if (totalTypes > totalWare)
        {
            Debug.LogError($"❌ Invalid: totalWarePattern ({totalTypes}) > totalWare ({totalWare})");
            return;
        }

        if (totalTypes > _availableFoodSprites.Count)
        {
            Debug.LogError($"❌ Not enough food sprites! Need {totalTypes}, have {_availableFoodSprites.Count}");
            return;
        }

        if (activeGrills > _grillStations.Count)
        {
            Debug.LogError($"Not enough GrillStations! Need {activeGrills}, have {_grillStations.Count}");
            return;
        }

        int maxCapacity = totalTrays * 3;
        if (totalWare > maxCapacity)
        {
            Debug.LogWarning($"Items ({totalWare}) exceed max capacity ({maxCapacity})!");
            Debug.LogWarning($"Some trays may have 1-2 items instead of 3.");
        }

        List<Sprite> selectedFoods = SelectRandomFoods(totalTypes);
        List<Sprite> foodPool = CreateBalancedFoodPool(selectedFoods, totalWare);
        ShuffleList(foodPool);
        DistributeToGrillsFromJSON(data, foodPool, activeGrills);

        if (SimpleGameManager.Instance != null)
        {
            SimpleGameManager.Instance.OnLevelGenerated(totalWare);
        }

        Debug.Log($"Level generated! Merge all {totalWare} items to win.");
    }

    private void DistributeToGrillsFromJSON(LevelDataFromJSON data, List<Sprite> foodPool, int activeGrills)
    {
        List<int> traysPerGrill = new List<int>();

        for (int i = 0; i < data.boardData.listTrayData.Count; i++)
        {
            if (data.boardData.listTrayData[i] != null)
            {
                int numTrays = data.boardData.listTrayData[i].size;
                traysPerGrill.Add(numTrays);
            }
        }

        List<int> foodPerGrill = DistributeEvenly(activeGrills, foodPool.Count);

        int grillIndex = 0;
        for (int i = 0; i < _grillStations.Count && grillIndex < activeGrills; i++)
        {
            _grillStations[i].gameObject.SetActive(true);

            List<Sprite> grillFood = Utils.TakeAndRemoveRandom(foodPool, foodPerGrill[grillIndex]);
            _grillStations[i].OnInitGrill(traysPerGrill[grillIndex], grillFood);

            grillIndex++;
        }
        for (int i = activeGrills; i < _grillStations.Count; i++)
        {
            _grillStations[i].gameObject.SetActive(false);
        }
    }

    #region Helper Methods

    private List<Sprite> SelectRandomFoods(int count)
    {
        count = Mathf.Min(count, _availableFoodSprites.Count);
        return _availableFoodSprites.OrderBy(x => Random.value).Take(count).ToList();
    }

    private List<Sprite> CreateBalancedFoodPool(List<Sprite> foodTypes, int totalCount)
    {
        List<Sprite> pool = new List<Sprite>();
        foreach (var food in foodTypes)
        {
            for (int i = 0; i < 3; i++)
                pool.Add(food);
        }
        int remaining = totalCount - foodTypes.Count * 3;
        while (remaining >= 3)
        {
            var upgradable = foodTypes.Where(food => pool.Count(p => p.name == food.name) < 12).ToList();
            if (upgradable.Count == 0) break;
            ShuffleList(upgradable);
            remaining = upgradable.Count();

            int upgradableThisRound = 0;
            foreach (var food in upgradable)
            {
                if (remaining < 3) break;

                for (int i = 0; i < 3; i++)
                {
                    pool.Add(food);
                }
                remaining -= 3;
                upgradableThisRound++;
            }
            if (upgradableThisRound == 0) break;
        }
        return pool;
    }

    private List<int> DistributeEvenly(int bucketCount, int totalItems)
    {
        List<int> distribution = new List<int>();

        if (bucketCount <= 0 || totalItems <= 0)
        {
            Debug.LogWarning($"Invalid distribution: {bucketCount} buckets, {totalItems} items");
            return distribution;
        }

        float avg = (float)totalItems / bucketCount;
        int low = Mathf.FloorToInt(avg);
        int high = Mathf.CeilToInt(avg);
        int highCount = totalItems - (low * bucketCount);
        int lowCount = bucketCount - highCount;

        for (int i = 0; i < lowCount; i++)
            distribution.Add(low);

        for (int i = 0; i < highCount; i++)
            distribution.Add(high);

        ShuffleList(distribution);

        return distribution;
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    #endregion

    #region Public Methods
    public LevelDataFromJSON GetCurrentLevelData()
    {
        return _currentLevelData;
    }

    public bool HasLevel(int levelNumber)
    {
        string fileName = $"{_levelFilePrefix}{levelNumber}";
        TextAsset jsonFile = Resources.Load<TextAsset>($"Levels/{fileName}");
        return jsonFile != null;
    }

    public int GetTotalLevels()
    {
        int count = 0;
        int level = 1;

        while (HasLevel(level))
        {
            count++;
            level++;

            if (level > 1000) break;
        }

        return count;
    }

    #endregion

    #region Debug Tools

#if UNITY_EDITOR
    [ContextMenu("Test Load Level 1")]
    private void TestLevel1()
    {
        GenerateLevel(1);
    }

    [ContextMenu("Check Available Levels")]
    private void CheckAvailableLevels()
    {
        int total = GetTotalLevels();
        Debug.Log($"Total available levels: {total}");

        for (int i = 1; i <= Mathf.Min(total, 10); i++)
        {
            string fileName = $"{_levelFilePrefix}{i}";
            Debug.Log($"Level {i}: Resources/Levels/{fileName}.json");
        }
    }

    [ContextMenu("Validate Current Level Data")]
    private void ValidateCurrentLevel()
    {
        if (_currentLevelData == null)
        {
            Debug.LogWarning("No level loaded yet");
            return;
        }

        Debug.Log($"=== Level Validation ===");
        Debug.Log($"Total Ware: {_currentLevelData.spawnWareData.totalWare}");
        Debug.Log($"Ware Pattern: {_currentLevelData.spawnWareData.totalWarePattern}");
        Debug.Log($"Trays: {_currentLevelData.boardData.listTrayData.Count}");
        Debug.Log($"Difficulty: {_currentLevelData.difficult}");
        Debug.Log($"Time Limit: {_currentLevelData.levelSeconds}s");
    }
#endif

    #endregion
}