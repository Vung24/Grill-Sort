using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class CustomLevelGenerator : MonoBehaviour
{
    public static CustomLevelGenerator Instance { get; private set; }

    [Header("JSON Settings")]
    [SerializeField] private string _levelFilePrefix = "level_";

    [Header("Resources")]
    [SerializeField] private string _foodSpritePath = "Items";

    [Header("References")]
    [SerializeField] private Transform _gridGrill;
    [SerializeField] private Sprite _disAbleSprite;
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
    }

    public void GenerateLevel(int levelNumber)
    {
        string fileName = $"{_levelFilePrefix}{levelNumber}";
        _currentLevelData = LevelDataFromJSON.LoadFromResources(fileName);

        if (_currentLevelData == null)
        {
            return;
        }
        GenerateFromJSONData(_currentLevelData);
    }

    private void GenerateFromJSONData(LevelDataFromJSON data)
    {
        int totalWare = data.spawnWareData.totalWare;
        int totalTypes = data.spawnWareData.totalWarePattern;
        int totalGridPositions = data.boardData.listTrayData.Count;

        int activeGrills = 0;
        int totalTrays = 0;
        List<int> activeGrillIndices = new List<int>();
        for (int i = 0; i < totalGridPositions; i++)
        {
            TrayDataInfo trayInfo = data.boardData.listTrayData[i];

            if (trayInfo == null || string.IsNullOrEmpty(trayInfo.id) || trayInfo.size == 0)
            {
            }
            else
            {
                int traySize = trayInfo.size;
                activeGrills++;
                totalTrays += traySize;
                activeGrillIndices.Add(i);
            }
        }

        if (totalWare % 3 != 0)
        {
            Debug.LogWarning($"totalWare ({totalWare}) NOT divisible by 3! Leftover: {totalWare % 3}");
        }

        List<Sprite> selectedFoods = SelectRandomFoods(totalTypes);
        if (selectedFoods.Count == 0)
        {
            Debug.LogWarning("No food sprites found in Resources/Items.");
            return;
        }

        if (selectedFoods.Count < totalTypes)
        {
            Debug.LogWarning($"Requested {totalTypes} food types but only {selectedFoods.Count} are available.");
        }

        List<Sprite> foodPool = CreateBalancedFoodPool(selectedFoods, totalWare);
        ShuffleList(foodPool);
        int generatedItemCount = foodPool.Count;

        if (generatedItemCount != totalWare)
        {
            Debug.LogWarning($"Level food pool mismatch. Requested: {totalWare}, Generated: {generatedItemCount}");
        }

        DistributeToGrillsFromJSON(data, foodPool, activeGrillIndices);
        DisableGrill(activeGrillIndices);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelGenerated(generatedItemCount);
        }

    }

    private void DistributeToGrillsFromJSON(LevelDataFromJSON data, List<Sprite> foodPool, List<int> activeGrillIndices)
    {
        List<int> traysPerGrill = new List<int>();

        foreach (int grillIndex in activeGrillIndices)
        {
            TrayDataInfo trayInfo = data.boardData.listTrayData[grillIndex];

            if (trayInfo != null && !string.IsNullOrEmpty(trayInfo.id))
            {
                int numTrays = trayInfo.size;
                traysPerGrill.Add(numTrays);
            }
        }

        List<int> foodPerGrill = DistributeEvenly(activeGrillIndices.Count, foodPool.Count);

        for (int i = 0; i < activeGrillIndices.Count; i++)
        {
            int jsonIndex = activeGrillIndices[i];
            if (jsonIndex >= _grillStations.Count) continue;

            GrillStation grill = _grillStations[jsonIndex];
            grill.gameObject.SetActive(true);

            List<Sprite> grillFood = Utils.TakeAndRemoveRandom(foodPool, foodPerGrill[i]);
            grill.OnInitGrill(traysPerGrill[i], grillFood);
        }
    }
    public void DisableGrill(List<int> activeGrillIndices)
    {
        for (int i = 0; i < _grillStations.Count; i++)
        {
            if (!activeGrillIndices.Contains(i))
            {
                Image img = _grillStations[i].GetComponentInChildren<Image>();
                if (img != null)
                {
                    img.sprite = _disAbleSprite;
                }
                _grillStations[i].TrayContainer.gameObject.SetActive(false);
            }
        }
    }
    private List<Sprite> SelectRandomFoods(int count)
    {
        if (_availableFoodSprites == null || _availableFoodSprites.Count == 0 || count <= 0)
        {
            return new List<Sprite>();
        }

        int safeCount = Mathf.Min(count, _availableFoodSprites.Count);
        return _availableFoodSprites.OrderBy(x => Random.value).Take(safeCount).ToList();
    }

    private List<Sprite> CreateBalancedFoodPool(List<Sprite> foodTypes, int totalCount)
    {
        List<Sprite> pool = new List<Sprite>();

        if (foodTypes == null || foodTypes.Count == 0 || totalCount <= 0)
        {
            return pool;
        }

        int normalizedTotal = totalCount - (totalCount % 3);
        if (normalizedTotal != totalCount)
        {
            Debug.LogWarning($"totalWare ({totalCount}) is not divisible by 3. Pool will use {normalizedTotal}.");
        }

        int totalTriples = normalizedTotal / 3;
        int baseTriplesPerType = totalTriples / foodTypes.Count;
        int extraTriples = totalTriples % foodTypes.Count;

        List<Sprite> shuffledTypes = new List<Sprite>(foodTypes);
        ShuffleList(shuffledTypes);

        foreach (Sprite food in shuffledTypes)
        {
            for (int i = 0; i < baseTriplesPerType; i++)
            {
                pool.Add(food);
                pool.Add(food);
                pool.Add(food);
            }
        }

        for (int i = 0; i < extraTriples; i++)
        {
            Sprite food = shuffledTypes[i];
            pool.Add(food);
            pool.Add(food);
            pool.Add(food);
        }

        return pool;
    }

    private List<int> DistributeEvenly(int bucketCount, int totalItems)
    {
        List<int> distribution = new List<int>();

        if (bucketCount <= 0 || totalItems <= 0)
        {
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

        for (int i = 1; i <= Mathf.Min(total, 20); i++)
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
    }
#endif

    #endregion
}
