using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum LoseReason
    {
        None = 0,
        TimeUp = 1,
        OutOfSlot = 2
    }

    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform _gridGrill;

    [Header("Hint System")]
    [SerializeField] private float _hintCheckInterval = 10f;

    [Header("Timer UI")]
    [SerializeField] private TextMeshProUGUI _timerText;

    [Header("Win/Lose Panels")]
    [SerializeField] private GameObject _winPanel;
    [SerializeField] private GameObject _losePanel;
    [SerializeField] private float _panelFadeDuration = 0.25f;

    private List<GrillStation> _grillStations;
    private int _totalItemsToMerge;
    private int _itemsMerged;
    private float _hintTimer;
    private float _currentTime;
    private float _levelTimeLimit;
    private bool _timerStarted;
    private bool _isTimerPaused;
    private bool _levelComplete;
    private bool _levelWon;
    private bool _hasMovedFood;
    private bool _energyConsumedThisAttempt;
    private LoseReason _currentLoseReason;

    public event System.Action<float, int, int> OnMergeProgressChanged;

    private void Awake()
    {
        Instance = this;
        _grillStations = Utils.GetListInChild<GrillStation>(_gridGrill);
        EnergyManager.EnsureInstance();
    }

    private void Start()
    {
        _levelComplete = false;
        _currentLoseReason = LoseReason.None;

        if (_winPanel != null) _winPanel.SetActive(false);
        if (_losePanel != null) _losePanel.SetActive(false);
    }

    private void Update()
    {
        if (!_levelComplete)
        {
            UpdateHintSystem();
            UpdateTimer();
            CheckLoseCondition();
        }
    }

    public void OnLevelGenerated(int totalItems)
    {
        _totalItemsToMerge = totalItems;
        _itemsMerged = 0;
        _levelComplete = false;
        _levelWon = false;
        _currentLoseReason = LoseReason.None;
        _hintTimer = 0f;
        _hasMovedFood = false;
        _energyConsumedThisAttempt = false;
        _timerStarted = false;
        _isTimerPaused = false;
        SetTimerVisible(true);

        LevelDataFromJSON levelData = CustomLevelGenerator.Instance?.GetCurrentLevelData();

        _levelTimeLimit = levelData.levelSeconds;
        _currentTime = _levelTimeLimit;
        Debug.Log($"Level time limit: {_levelTimeLimit} seconds");

        UpdateTimerDisplay();
        NotifyMergeProgressChanged();
    }

    public void OnItemsMerged(int count)
    {
        if (_levelComplete)
        {
            return;
        }

        _itemsMerged += count;
        _hintTimer = 0f;
        NotifyMergeProgressChanged();
        if (IsBoardCleared())
        {
            OnLevelWin();
        }
    }

    private bool IsBoardCleared()
    {
        foreach (GrillStation grill in _grillStations)
        {
            if (grill == null || !grill.gameObject.activeInHierarchy) continue;
            if (grill.TrayContainer == null || !grill.TrayContainer.gameObject.activeInHierarchy) continue;

            foreach (FoodSlot slot in grill.TotalSlots)
            {
                if (slot != null && slot.HasFood())
                {
                    return false;
                }
            }

            List<Image> hiddenFoods = grill.GetHiddenFoodImages();
            if (hiddenFoods.Count > 0)
            {
                return false;
            }
        }

        return true;
    }

    private void OnLevelWin()
    {
        if (_levelComplete) return;

        _levelComplete = true;
        _levelWon = true;
        SetTimerVisible(false);
        AudioManager.Instance?.PlayCompleteMission();

        if (_winPanel != null)
        {
            ShowPanel(_winPanel);
        }
        LinearLevelSystem.Instance?.CompleteCurrentLevel();
    }

    private void OnLevelLose(LoseReason reason)
    {
        if (_levelComplete) return;

        _levelComplete = true;
        _levelWon = false;
        _currentLoseReason = reason;
        SetTimerVisible(false);
        //audio 
        if (_losePanel != null)
        {
            ShowPanel(_losePanel);
        }

        ConsumeEnergy();
        Debug.Log("Level Failed!");
    }

    private void CheckLoseCondition()
    {
        if (_levelComplete) return;

        bool allGrillsFull = true;

        foreach (GrillStation grill in _grillStations)
        {
            if (!grill.gameObject.activeInHierarchy) continue;
            if (grill.TrayContainer == null || !grill.TrayContainer.gameObject.activeInHierarchy) continue;

            if (grill.GetSlotNull() != null)
            {
                allGrillsFull = false;
                break;
            }
        }

        if (allGrillsFull && !CanMakeAnyMove())
        {
            OnLevelLose(LoseReason.OutOfSlot);
        }
    }

    private bool CanMakeAnyMove()
    {
        foreach (GrillStation grill in _grillStations)
        {
            if (!grill.gameObject.activeInHierarchy) continue;
            if (grill.TrayContainer == null || !grill.TrayContainer.gameObject.activeInHierarchy) continue;

            bool canMerge = true;
            if (grill.TotalSlots.Count > 0)
            {
                string firstFoodName = null;

                foreach (FoodSlot slot in grill.TotalSlots)
                {
                    if (!slot.HasFood())
                    {
                        canMerge = false;
                        break;
                    }

                    if (firstFoodName == null)
                    {
                        firstFoodName = slot.GetSpriteFood.name;
                    }
                    else if (slot.GetSpriteFood.name != firstFoodName)
                    {
                        canMerge = false;
                        break;
                    }
                }

                if (canMerge && firstFoodName != null)
                    return true;
            }
        }

        return false;
    }

    private void UpdateTimer()
    {
        if (_levelTimeLimit <= 0) return; 
        if (!_timerStarted || _isTimerPaused) return;

        _currentTime -= Time.deltaTime;

        if (_currentTime <= 0f)
        {
            _currentTime = 0f;
            OnLevelLose(LoseReason.TimeUp); 
        }

        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (_timerText == null || _levelTimeLimit <= 0) return;

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
            if (grill.TrayContainer == null || !grill.TrayContainer.gameObject.activeInHierarchy) continue;

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

    public void OnNextLevelButtonClicked()
    {
        LinearLevelSystem.Instance?.NextLevel();
    }

    public float CurrentTime => _currentTime;
    public float LevelTimeLimit => _levelTimeLimit;
    public float MergeProgress => _totalItemsToMerge > 0 ? Mathf.Clamp01((float)_itemsMerged / _totalItemsToMerge) : 0f;
    public int ItemsMerged => _itemsMerged;
    public int TotalItemsToMerge => _totalItemsToMerge;
    public bool HasMovedFood => _hasMovedFood;
    public bool IsLevelComplete => _levelComplete;
    public bool IsLevelWon => _levelWon;
    public LoseReason CurrentLoseReason => _currentLoseReason;
    public bool IsTimerPaused => _isTimerPaused;

    public void MarkFoodMoved()
    {
        _hasMovedFood = true;
        _timerStarted = true;
    }

    public void SetTimerPaused(bool isPaused)
    {
        _isTimerPaused = isPaused;
    }

    public void ConsumeEnergyIfAbandon()
    {
        if (_levelComplete)
        {
            return;
        }

        if (!_hasMovedFood)
        {
            return;
        }

        ConsumeEnergy();
    }

    private void ConsumeEnergy()
    {
        if (_energyConsumedThisAttempt)
        {
            return;
        }

        EnergyManager.Instance?.OnLose();
        _energyConsumedThisAttempt = true;
    }

    public bool UseBoostRemoveThree()
    {
        if (_levelComplete)
        {
            return false;
        }

        Dictionary<string, List<FoodSlot>> visibleByType = new Dictionary<string, List<FoodSlot>>();
        Dictionary<string, List<Image>> hiddenByType = new Dictionary<string, List<Image>>();

        foreach (GrillStation grill in _grillStations)
        {
            if (!grill.gameObject.activeInHierarchy) continue;
            if (grill.TrayContainer == null || !grill.TrayContainer.gameObject.activeInHierarchy) continue;

            foreach (FoodSlot slot in grill.TotalSlots)
            {
                if (slot.HasFood())
                {
                    string foodType = slot.GetSpriteFood.name;
                    if (!visibleByType.ContainsKey(foodType))
                    {
                        visibleByType[foodType] = new List<FoodSlot>();
                    }

                    visibleByType[foodType].Add(slot);
                }
            }

            List<Image> hiddenFoods = grill.GetHiddenFoodImages();
            foreach (Image hidden in hiddenFoods)
            {
                string foodType = hidden.sprite.name;
                if (!hiddenByType.ContainsKey(foodType))
                {
                    hiddenByType[foodType] = new List<Image>();
                }

                hiddenByType[foodType].Add(hidden);
            }
        }

        HashSet<string> allFoodTypes = new HashSet<string>();
        foreach (var pair in visibleByType)
        {
            allFoodTypes.Add(pair.Key);
        }

        foreach (var pair in hiddenByType)
        {
            allFoodTypes.Add(pair.Key);
        }

        List<string> validFoodTypes = new List<string>();
        foreach (string foodType in allFoodTypes)
        {
            int visibleCount = visibleByType.ContainsKey(foodType) ? visibleByType[foodType].Count : 0;
            int hiddenCount = hiddenByType.ContainsKey(foodType) ? hiddenByType[foodType].Count : 0;
            if (visibleCount + hiddenCount >= 3)
            {
                validFoodTypes.Add(foodType);
            }
        }

        if (validFoodTypes.Count == 0)
        {
            Debug.Log("Boost RemoveThree: no valid food type with total >= 3.");
            return false;
        }

        int maxVisible = -1;
        List<string> preferredTypes = new List<string>();
        foreach (string foodType in validFoodTypes)
        {
            int visibleCount = visibleByType.ContainsKey(foodType) ? visibleByType[foodType].Count : 0;
            if (visibleCount > maxVisible)
            {
                maxVisible = visibleCount;
                preferredTypes.Clear();
                preferredTypes.Add(foodType);
            }
            else if (visibleCount == maxVisible)
            {
                preferredTypes.Add(foodType);
            }
        }

        string selectedType = preferredTypes[Random.Range(0, preferredTypes.Count)];

        int removedCount = 0;
        bool playedRemovalAnimation = false;
        List<FoodSlot> visibleSlots = visibleByType.ContainsKey(selectedType)
            ? visibleByType[selectedType]
            : new List<FoodSlot>();

        while (visibleSlots.Count > 0 && removedCount < 3)
        {
            int index = Random.Range(0, visibleSlots.Count);
            FoodSlot slot = visibleSlots[index];
            visibleSlots.RemoveAt(index);
            slot.RemoveByBoost();
            playedRemovalAnimation = true;
            removedCount++;
        }

        if (removedCount < 3 && hiddenByType.ContainsKey(selectedType))
        {
            List<Image> hiddenSlots = hiddenByType[selectedType];
            while (hiddenSlots.Count > 0 && removedCount < 3)
            {
                int index = Random.Range(0, hiddenSlots.Count);
                Image hidden = hiddenSlots[index];
                hiddenSlots.RemoveAt(index);
                PlayHiddenRemoveByBoostEffect(hidden);
                playedRemovalAnimation = true;
                removedCount++;
            }
        }

        if (removedCount > 0)
        {
            OnItemsMerged(removedCount);
            StartCoroutine(ResolveBoostBoardState(playedRemovalAnimation));
            Debug.Log($"Boost RemoveThree: removed {removedCount} item(s) of type {selectedType}.");
            return true;
        }

        return false;
    }

    public bool UseBoostSwapForMerge()
    {
        if (_levelComplete)
        {
            return false;
        }

        List<FoodSlot> visibleSlots = new List<FoodSlot>();
        List<Image> hiddenImages = new List<Image>();
        Dictionary<FoodSlot, GrillStation> slotOwnerByVisibleSlot = new Dictionary<FoodSlot, GrillStation>();
        CollectVisibleAndHiddenItems(visibleSlots, hiddenImages, slotOwnerByVisibleSlot);

        if (visibleSlots.Count == 0 || hiddenImages.Count == 0)
        {
            Debug.Log("Boost SwapForMerge: no active board item found.");
            return false;
        }

        // Rule: swap all currently visible foods 1-1 with hidden foods.
        if (hiddenImages.Count < visibleSlots.Count)
        {
            Debug.Log($"Boost SwapForMerge: not enough hidden foods ({hiddenImages.Count}) for visible foods ({visibleSlots.Count}).");
            return false;
        }

        Dictionary<FoodSlot, Image> assignment = new Dictionary<FoodSlot, Image>(visibleSlots.Count);
        List<Image> availableHidden = new List<Image>(hiddenImages);

        bool hasPreparedMerge = ReservePotentialMergeAssignment(visibleSlots, assignment, availableHidden, slotOwnerByVisibleSlot);

        if (!hasPreparedMerge)
        {
            Debug.Log("Boost SwapForMerge: cannot create split merge-ready setup across different grills.");
            return false;
        }

        foreach (FoodSlot slot in visibleSlots)
        {
            if (assignment.ContainsKey(slot))
            {
                continue;
            }

            int randomIndex = Random.Range(0, availableHidden.Count);
            assignment[slot] = availableHidden[randomIndex];
            availableHidden.RemoveAt(randomIndex);
        }

        ApplyVisibleHiddenSwap(assignment);

        Debug.Log("Boost SwapForMerge: swapped all visible items and prepared merge setup.");

        _hintTimer = 0f;
        return true;
    }

    public bool UseBoostAddThirtySeconds()
    {
        if (!CanUseBoostAddThirtySeconds())
        {
            return false;
        }

        ApplyAddTimeSeconds(30f);
        return true;
    }

    public bool CanUseBoostAddThirtySeconds()
    {
        if (_levelComplete)
        {
            return false;
        }

        if (_levelTimeLimit <= 0f)
        {
            return false;
        }

        return true;
    }

    private void CollectVisibleAndHiddenItems(
        List<FoodSlot> visibleSlots,
        List<Image> hiddenImages,
        Dictionary<FoodSlot, GrillStation> slotOwnerByVisibleSlot)
    {
        foreach (GrillStation grill in _grillStations)
        {
            if (grill == null || !grill.gameObject.activeInHierarchy) continue;
            if (grill.TrayContainer == null || !grill.TrayContainer.gameObject.activeInHierarchy) continue;

            foreach (FoodSlot slot in grill.TotalSlots)
            {
                if (slot == null || !slot.HasFood() || slot.GetSpriteFood == null) continue;
                visibleSlots.Add(slot);
                slotOwnerByVisibleSlot[slot] = grill;
            }

            List<Image> hiddenFoods = grill.GetHiddenFoodImages();
            foreach (Image hidden in hiddenFoods)
            {
                if (hidden == null || !hidden.gameObject.activeInHierarchy || hidden.sprite == null) continue;
                hiddenImages.Add(hidden);
            }
        }
    }

    private bool ReservePotentialMergeAssignment(
        List<FoodSlot> visibleSlots,
        Dictionary<FoodSlot, Image> assignment,
        List<Image> availableHidden,
        Dictionary<FoodSlot, GrillStation> slotOwnerByVisibleSlot)
    {
        if (visibleSlots.Count < 3 || availableHidden.Count < 3)
        {
            return false;
        }

        Dictionary<string, int> hiddenCountByType = BuildHiddenCountByType(availableHidden);
        List<string> typeCandidates = new List<string>();
        foreach (KeyValuePair<string, int> pair in hiddenCountByType)
        {
            if (pair.Value >= 3)
            {
                typeCandidates.Add(pair.Key);
            }
        }

        if (typeCandidates.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < typeCandidates.Count; i++)
        {
            int swapIndex = Random.Range(i, typeCandidates.Count);
            string temp = typeCandidates[i];
            typeCandidates[i] = typeCandidates[swapIndex];
            typeCandidates[swapIndex] = temp;
        }

        foreach (string type in typeCandidates)
        {
            List<FoodSlot> targetSlots = SelectThreeSlotsAcrossDifferentGrills(visibleSlots, assignment, slotOwnerByVisibleSlot);
            if (targetSlots == null || targetSlots.Count < 3)
            {
                continue;
            }

            List<Image> selectedHidden = TakeHiddenByType(type, 3, availableHidden);
            if (selectedHidden == null || selectedHidden.Count < 3)
            {
                continue;
            }

            for (int i = 0; i < 3; i++)
            {
                assignment[targetSlots[i]] = selectedHidden[i];
            }

            return true;
        }

        return false;
    }

    private List<FoodSlot> SelectThreeSlotsAcrossDifferentGrills(
        List<FoodSlot> visibleSlots,
        Dictionary<FoodSlot, Image> assignment,
        Dictionary<FoodSlot, GrillStation> slotOwnerByVisibleSlot)
    {
        Dictionary<GrillStation, List<FoodSlot>> slotsByGrill = new Dictionary<GrillStation, List<FoodSlot>>();
        foreach (FoodSlot slot in visibleSlots)
        {
            if (slot == null || assignment.ContainsKey(slot))
            {
                continue;
            }

            GrillStation grill;
            if (!slotOwnerByVisibleSlot.TryGetValue(slot, out grill) || grill == null)
            {
                continue;
            }

            if (!slotsByGrill.ContainsKey(grill))
            {
                slotsByGrill[grill] = new List<FoodSlot>();
            }

            slotsByGrill[grill].Add(slot);
        }

        if (slotsByGrill.Count < 2)
        {
            return null;
        }

        foreach (KeyValuePair<GrillStation, List<FoodSlot>> primary in slotsByGrill)
        {
            if (primary.Value.Count < 2)
            {
                continue;
            }

            foreach (KeyValuePair<GrillStation, List<FoodSlot>> secondary in slotsByGrill)
            {
                if (secondary.Key == primary.Key || secondary.Value.Count < 1)
                {
                    continue;
                }

                return new List<FoodSlot>
                {
                    primary.Value[0],
                    primary.Value[1],
                    secondary.Value[0]
                };
            }
        }

        return null;
    }

    private void ApplyVisibleHiddenSwap(Dictionary<FoodSlot, Image> assignment)
    {
        Dictionary<FoodSlot, Sprite> oldVisibleSprite = new Dictionary<FoodSlot, Sprite>();
        Dictionary<Image, Sprite> oldHiddenSprite = new Dictionary<Image, Sprite>();

        foreach (KeyValuePair<FoodSlot, Image> pair in assignment)
        {
            if (pair.Key == null || pair.Value == null) continue;
            if (pair.Key.GetSpriteFood == null || pair.Value.sprite == null) continue;

            oldVisibleSprite[pair.Key] = pair.Key.GetSpriteFood;
            oldHiddenSprite[pair.Value] = pair.Value.sprite;
        }

        foreach (KeyValuePair<FoodSlot, Image> pair in assignment)
        {
            if (!oldVisibleSprite.ContainsKey(pair.Key) || !oldHiddenSprite.ContainsKey(pair.Value))
            {
                continue;
            }

            pair.Key.OnSetSlot(oldHiddenSprite[pair.Value]);
            pair.Key.OnActiveFood(true);
            pair.Key.PlaySwapByBoostEffect();
        }

        foreach (KeyValuePair<FoodSlot, Image> pair in assignment)
        {
            if (!oldVisibleSprite.ContainsKey(pair.Key) || !oldHiddenSprite.ContainsKey(pair.Value))
            {
                continue;
            }

            pair.Value.sprite = oldVisibleSprite[pair.Key];
            pair.Value.SetNativeSize();
            pair.Value.gameObject.SetActive(true);
            PlayHiddenSwapByBoostEffect(pair.Value);
        }
    }

    private void PlayHiddenSwapByBoostEffect(Image hiddenImage)
    {
        if (hiddenImage == null || !hiddenImage.gameObject.activeInHierarchy)
        {
            return;
        }

        Vector3 baseScale = hiddenImage.transform.localScale;
        float scaleFactor = Mathf.Max(Mathf.Abs(baseScale.x), Mathf.Abs(baseScale.y), Mathf.Abs(baseScale.z));
        if (scaleFactor <= 0f)
        {
            scaleFactor = 1f;
        }

        hiddenImage.transform.DOKill();
        hiddenImage.DOKill();
        hiddenImage.color = Color.white;

        Sequence seq = DOTween.Sequence();
        seq.Append(hiddenImage.transform.DOPunchScale(new Vector3(0.14f, 0.14f, 0f) * scaleFactor, 0.18f, 8, 0.8f));
        seq.Join(hiddenImage.DOColor(new Color(1f, 0.95f, 0.75f, 1f), 0.09f));
        seq.Append(hiddenImage.DOColor(Color.white, 0.12f));
        seq.OnComplete(() =>
        {
            hiddenImage.transform.localScale = baseScale;
        });
    }

    private void PlayHiddenRemoveByBoostEffect(Image hiddenImage)
    {
        if (hiddenImage == null || !hiddenImage.gameObject.activeInHierarchy)
        {
            return;
        }

        Vector3 baseScale = hiddenImage.transform.localScale;
        float scaleFactor = Mathf.Max(Mathf.Abs(baseScale.x), Mathf.Abs(baseScale.y), Mathf.Abs(baseScale.z));
        if (scaleFactor <= 0f)
        {
            scaleFactor = 1f;
        }

        hiddenImage.transform.DOKill();
        hiddenImage.DOKill();
        hiddenImage.color = Color.white;

        Sequence seq = DOTween.Sequence();
        seq.Append(hiddenImage.transform.DOPunchScale(new Vector3(0.18f, 0.18f, 0f) * scaleFactor, 0.12f, 8, 0.75f));
        seq.Append(hiddenImage.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));
        seq.Join(hiddenImage.DOFade(0f, 0.2f));
        seq.OnComplete(() =>
        {
            hiddenImage.gameObject.SetActive(false);
            hiddenImage.transform.localScale = baseScale;
            hiddenImage.color = Color.white;
        });
    }

    private List<Image> TakeHiddenByType(string targetType, int count, List<Image> availableHidden)
    {
        List<Image> selected = new List<Image>(count);
        for (int i = availableHidden.Count - 1; i >= 0 && selected.Count < count; i--)
        {
            Image hidden = availableHidden[i];
            if (hidden == null || hidden.sprite == null || hidden.sprite.name != targetType)
            {
                continue;
            }

            selected.Add(hidden);
            availableHidden.RemoveAt(i);
        }

        return selected;
    }

    private Dictionary<string, int> BuildHiddenCountByType(List<Image> hiddenImages)
    {
        Dictionary<string, int> countByType = new Dictionary<string, int>();
        foreach (Image hidden in hiddenImages)
        {
            if (hidden == null || hidden.sprite == null)
            {
                continue;
            }

            string type = hidden.sprite.name;
            if (!countByType.ContainsKey(type))
            {
                countByType[type] = 0;
            }
            countByType[type]++;
        }

        return countByType;
    }

    private IEnumerator ResolveBoostBoardState(bool waitForVisibleAnimation)
    {
        if (waitForVisibleAnimation)
        {
            yield return new WaitForSeconds(0.22f);
        }

        if (_levelComplete)
        {
            yield break;
        }

        foreach (GrillStation grill in _grillStations)
        {
            if (grill == null || !grill.gameObject.activeInHierarchy) continue;
            if (grill.TrayContainer == null || !grill.TrayContainer.gameObject.activeInHierarchy) continue;

            grill.ResolveAfterBoost();
        }
    }

    private void ShowPanel(GameObject panel)
    {
        if (panel == null) return;

        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = panel.AddComponent<CanvasGroup>();
        }

        panel.SetActive(true);
        group.alpha = 0f;
        group.interactable = true;
        group.blocksRaycasts = true;

        group.DOKill();
        group.DOFade(1f, _panelFadeDuration).SetEase(Ease.OutQuad);
    }

    private void SetTimerVisible(bool isVisible)
    {
        if (_timerText == null) return;
        _timerText.gameObject.SetActive(isVisible);
    }

    private void ApplyAddTimeSeconds(float addSeconds)
    {
        _currentTime += Mathf.Max(0f, addSeconds);
        _hintTimer = 0f;
        UpdateTimerDisplay();
        Debug.Log($"Boost AddTime: +{addSeconds:0}s. Current time: {_currentTime:0.00}s");
    }

    private void NotifyMergeProgressChanged()
    {
        OnMergeProgressChanged?.Invoke(MergeProgress, _itemsMerged, _totalItemsToMerge);
    }
}
