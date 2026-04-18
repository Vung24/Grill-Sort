using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class GeneratorLevel : MonoBehaviour
{
    public static GeneratorLevel Instance { get; private set; }

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
        GenFromJson(_currentLevelData);
    }

    private void GenFromJson(LevelDataFromJSON data)
    {
        int wareCount = data.spawnWareData.totalWare;
        int typeCount = data.spawnWareData.totalWarePattern;
        int gridCount = data.boardData.listTrayData.Count;

        List<int> activeIds = new List<int>();
        for (int i = 0; i < gridCount; i++)
        {
            TrayDataInfo trayInfo = data.boardData.listTrayData[i];

            if (trayInfo == null || string.IsNullOrEmpty(trayInfo.id) || trayInfo.size == 0)
            {
            }
            else
            {
                activeIds.Add(i);
            }
        }

        if (wareCount % 3 != 0)
        {
            Debug.LogWarning($"totalWare ({wareCount}) NOT divisible by 3! Leftover: {wareCount % 3}");
        }

        List<Sprite> pickedFoods = PickFoods(typeCount);
        if (pickedFoods.Count == 0)
        {
            Debug.LogWarning("No food sprites found in Resources/Items.");
            return;
        }

        if (pickedFoods.Count < typeCount)
        {
            Debug.LogWarning($"Requested {typeCount} food types but only {pickedFoods.Count} are available.");
        }

        List<Sprite> foodPool = MakeFoodPool(pickedFoods, wareCount);
        ShuffleList(foodPool);
        int itemCount = foodPool.Count;

        if (itemCount != wareCount)
        {
            Debug.LogWarning($"Level food pool mismatch. Requested: {wareCount}, Generated: {itemCount}");
        }

        DistToGrills(data, foodPool, activeIds);
        DisableGrill(activeIds);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelGenerated(itemCount);
        }

    }

    private void DistToGrills(LevelDataFromJSON data, List<Sprite> foodPool, List<int> activeIds)
    {
        List<int> traysPerGrill = new List<int>();

        foreach (int grillIndex in activeIds)
        {
            TrayDataInfo trayInfo = data.boardData.listTrayData[grillIndex];

            if (trayInfo != null && !string.IsNullOrEmpty(trayInfo.id))
            {
                int numTrays = trayInfo.size;
                traysPerGrill.Add(numTrays);
            }
        }

        List<int> perGrill = SplitEvenly(activeIds.Count, foodPool.Count);
        List<List<Sprite>> foodsByGrill = Build2Plus1(foodPool, perGrill);

        for (int i = 0; i < activeIds.Count; i++)
        {
            int jsonIndex = activeIds[i];
            if (jsonIndex >= _grillStations.Count) continue;

            GrillStation grill = _grillStations[jsonIndex];
            grill.gameObject.SetActive(true);

            List<Sprite> grillFood = i < foodsByGrill.Count ? foodsByGrill[i] : new List<Sprite>();
            grill.OnInitGrill(traysPerGrill[i], grillFood, false);
        }
    }

    private List<List<Sprite>> Build2Plus1(List<Sprite> foodPool, List<int> foodPerGrill)
    {
        int gCount = foodPerGrill != null ? foodPerGrill.Count : 0;
        List<List<Sprite>> result = new List<List<Sprite>>();

        if (gCount <= 0)
        {
            return result;
        }

        List<int> capLeft = new List<int>(foodPerGrill);
        for (int i = 0; i < gCount; i++)
        {
            result.Add(new List<Sprite>());
        }

        if (foodPool == null || foodPool.Count == 0)
        {
            return result;
        }

        Dictionary<Sprite, int> typeMap = new Dictionary<Sprite, int>();
        foreach (Sprite food in foodPool)
        {
            if (food == null) continue;

            if (!typeMap.ContainsKey(food))
            {
                typeMap[food] = 0;
            }
            typeMap[food]++;
        }

        List<Sprite> triples = new List<Sprite>();
        List<Sprite> leftovers = new List<Sprite>();

        foreach (KeyValuePair<Sprite, int> kv in typeMap)
        {
            int tripleCount = kv.Value / 3;
            int remain = kv.Value % 3;

            for (int i = 0; i < tripleCount; i++)
            {
                triples.Add(kv.Key);
            }

            for (int i = 0; i < remain; i++)
            {
                leftovers.Add(kv.Key);
            }
        }

        ShuffleList(triples);
        ShuffleList(leftovers);

        if (gCount == 1)
        {
            foreach (Sprite food in foodPool)
            {
                if (capLeft[0] <= 0) break;
                result[0].Add(food);
                capLeft[0]--;
            }

            return result;
        }

        foreach (Sprite tripleFood in triples)
        {
            List<int> pairOpts = new List<int>();
            for (int i = 0; i < gCount; i++)
            {
                if (capLeft[i] >= 2)
                {
                    pairOpts.Add(i);
                }
            }

            if (pairOpts.Count == 0)
            {
                leftovers.Add(tripleFood);
                leftovers.Add(tripleFood);
                leftovers.Add(tripleFood);
                continue;
            }

            int pairGrill = pairOpts[Random.Range(0, pairOpts.Count)];

            List<int> singleOpts = new List<int>();
            for (int i = 0; i < gCount; i++)
            {
                if (i != pairGrill && capLeft[i] >= 1)
                {
                    singleOpts.Add(i);
                }
            }

            if (singleOpts.Count == 0)
            {
                leftovers.Add(tripleFood);
                leftovers.Add(tripleFood);
                leftovers.Add(tripleFood);
                continue;
            }

            int singleGrill = singleOpts[Random.Range(0, singleOpts.Count)];

            result[pairGrill].Add(tripleFood);
            result[pairGrill].Add(tripleFood);
            capLeft[pairGrill] -= 2;

            result[singleGrill].Add(tripleFood);
            capLeft[singleGrill] -= 1;
        }

        foreach (Sprite food in leftovers)
        {
            List<int> available = new List<int>();
            for (int i = 0; i < gCount; i++)
            {
                if (capLeft[i] > 0)
                {
                    available.Add(i);
                }
            }

            if (available.Count == 0)
            {
                break;
            }

            int target = available[Random.Range(0, available.Count)];
            result[target].Add(food);
            capLeft[target]--;
        }

        return result;
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
    private List<Sprite> PickFoods(int count)
    {
        if (_availableFoodSprites == null || _availableFoodSprites.Count == 0 || count <= 0)
        {
            return new List<Sprite>();
        }

        int safeCount = Mathf.Min(count, _availableFoodSprites.Count);
        return _availableFoodSprites.OrderBy(x => Random.value).Take(safeCount).ToList();
    }

    private List<Sprite> MakeFoodPool(List<Sprite> foodTypes, int totalCount)
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

    private List<int> SplitEvenly(int bucketCount, int totalItems)
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


    #region Test

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
