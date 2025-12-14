// Save as: Assets/Scripts/Managers/SimpleGameManager.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SimpleGameManager : MonoBehaviour
{
    public static SimpleGameManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private Transform _gridGrill;
    
    [Header("Hint System")]
    [SerializeField] private float _hintCheckInterval = 5f;
    
    [Header("Win Panel (Optional)")]
    [SerializeField] private GameObject _winPanel;
    [SerializeField] private GameObject _losePanel;
    
    private List<GrillStation> _grillStations;
    private int _totalItemsToMerge;    
    private int _itemsMerged;      
    private float _hintTimer;
    private bool _levelComplete;
    
    public int RemainingItems => _totalItemsToMerge - _itemsMerged;
    public int ItemsMerged => _itemsMerged;
    public bool IsLevelComplete => _levelComplete;
    
    private void Awake()
    {
        Instance = this;
        _grillStations = Utils.GetListInChild<GrillStation>(_gridGrill);
    }
    
    private void Start()
    {
        _levelComplete = false;
        
        if (_winPanel != null) _winPanel.SetActive(false);
        if (_losePanel != null) _losePanel.SetActive(false);
    }
    
    private void Update()
    {
        if (!_levelComplete)
        {
            UpdateHintSystem();
        }
    }
    
    public void OnLevelGenerated(int totalItems)
    {
        _totalItemsToMerge = totalItems;
        _itemsMerged = 0;
        _levelComplete = false;
        _hintTimer = 0f;
    }
    
    public void OnItemsMerged(int count)
    {
        if (_levelComplete)
        {
            Debug.LogWarning("Level already complete, ignoring merge");
            return;
        }
        
        _itemsMerged += count;
        
        Debug.Log($"{count} items merged! Total: {_itemsMerged}/{_totalItemsToMerge}");
        
        if (_itemsMerged >= _totalItemsToMerge)
        {
            OnLevelWin();
        }
    }
    
    private void OnLevelWin()
    {
        if (_levelComplete) return;
        
        _levelComplete = true;
        
        Debug.Log("Level Complete");
        
        AudioManager.instance?.PlayCompleteMission();
        
        if (_winPanel != null)
        {
            _winPanel.SetActive(true);
        }
        
        if (LinearLevelSystem.Instance != null)
        {
            LinearLevelSystem.Instance.CompleteCurrentLevel();
        }
        else
        {
            StartCoroutine(AutoLoadNextLevel(2f));
        }
    }
    
    private IEnumerator AutoLoadNextLevel(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (LinearLevelSystem.Instance != null)
        {
            LinearLevelSystem.Instance.NextLevel();
        }
    }
    
    private void UpdateHintSystem()
    {
        _hintTimer += Time.deltaTime;
        
        if (_hintTimer >= _hintCheckInterval)
        {
            _hintTimer = 0f;
            CheckAndShowHint();
        }
    }
    
    private void CheckAndShowHint()
    {
        Dictionary<string, List<FoodSlot>> foodGroups = new Dictionary<string, List<FoodSlot>>();
        
        foreach (GrillStation grill in _grillStations)
        {
            if (!grill.gameObject.activeInHierarchy) continue;
            
            foreach (FoodSlot slot in grill.TotalSlots)
            {
                if (!slot.HasFood()) continue;
                
                string foodName = slot.GetSpriteFood.name;
                
                if (!foodGroups.ContainsKey(foodName))
                {
                    foodGroups[foodName] = new List<FoodSlot>();
                }
                
                foodGroups[foodName].Add(slot);
            }
        }
        
        foreach (var group in foodGroups)
        {
            if (group.Value.Count >= 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    group.Value[i].DoShake();
                }
                return;
            }
        }
    }
    
    public void ShowHint()
    {
        CheckAndShowHint();
    }
    
    public void OnRestartButtonClicked()
    {
        if (LinearLevelSystem.Instance != null)
        {
            LinearLevelSystem.Instance.RestartLevel();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
    }
    
    public void OnMenuButtonClicked()
    {
        if (LinearLevelSystem.Instance != null)
        {
            LinearLevelSystem.Instance.LoadMenuScene();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
    }
    
    public void OnNextLevelButtonClicked()
    {
        if (LinearLevelSystem.Instance != null)
        {
            LinearLevelSystem.Instance.NextLevel();
        }
    }
}