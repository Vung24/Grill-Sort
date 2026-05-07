using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoostGameplayController : MonoBehaviour
{
    private GameManager _gameManager;
    private BoostFxController _boostFxController;
    private GameHintSystem _hintSystem;
    private List<GrillStation> _grillStations;

    public void Initialize(
        GameManager gameManager,
        List<GrillStation> grillStations,
        GameHintSystem hintSystem,
        BoostFxController boostFxController)
    {
        _gameManager = gameManager;
        _grillStations = grillStations;
        _hintSystem = hintSystem;
        _boostFxController = boostFxController;
    }

    public bool UseBoostRemoveThree()
    {
        if (_gameManager == null || _gameManager.IsLevelComplete)
        {
            return false;
        }

        if (_grillStations == null || _grillStations.Count == 0)
        {
            return false;
        }

        Dictionary<string, List<FoodSlot>> visibleByType = new Dictionary<string, List<FoodSlot>>();
        Dictionary<string, List<Image>> hiddenByType = new Dictionary<string, List<Image>>();

        foreach (GrillStation grill in _grillStations)
        {
            if (grill == null || !grill.gameObject.activeInHierarchy) continue;
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
                _boostFxController?.PlayHiddenRemoveFx(hidden);
                playedRemovalAnimation = true;
                removedCount++;
            }
        }

        if (removedCount > 0)
        {
            _gameManager.OnItemsMerged(removedCount);
            if (_boostFxController != null)
            {
                StartCoroutine(_boostFxController.ResolveBoost(
                    playedRemovalAnimation,
                    _gameManager.IsLevelComplete,
                    _grillStations));
            }
            Debug.Log($"Boost RemoveThree: removed {removedCount} item(s) of type {selectedType}.");
            return true;
        }

        return false;
    }

    public bool UseBoostSwapForMerge()
    {
        if (_gameManager == null || _gameManager.IsLevelComplete)
        {
            return false;
        }

        if (_grillStations == null || _grillStations.Count == 0)
        {
            return false;
        }

        List<FoodSlot> visibleSlots = new List<FoodSlot>();
        List<Image> hiddenImages = new List<Image>();
        Dictionary<FoodSlot, GrillStation> slotOwnerByVisibleSlot = new Dictionary<FoodSlot, GrillStation>();
        CollectBoardItems(visibleSlots, hiddenImages, slotOwnerByVisibleSlot);

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

        bool hasPreparedMerge = ReserveMergeSwap(visibleSlots, assignment, availableHidden, slotOwnerByVisibleSlot);

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

        ApplySwap(assignment);

        _hintSystem?.ResetHintTimer();
        return true;
    }

    private void CollectBoardItems(
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

    private bool ReserveMergeSwap(
        List<FoodSlot> visibleSlots,
        Dictionary<FoodSlot, Image> assignment,
        List<Image> availableHidden,
        Dictionary<FoodSlot, GrillStation> slotOwnerByVisibleSlot)
    {
        if (visibleSlots.Count < 3 || availableHidden.Count < 3)
        {
            return false;
        }

        Dictionary<string, int> hiddenCountByType = CountHidden(availableHidden);
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
            List<FoodSlot> targetSlots = PickSplitSlots(visibleSlots, assignment, slotOwnerByVisibleSlot);
            if (targetSlots == null || targetSlots.Count < 3)
            {
                continue;
            }

            List<Image> selectedHidden = TakeHidden(type, 3, availableHidden);
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

    private List<FoodSlot> PickSplitSlots(
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

    private void ApplySwap(Dictionary<FoodSlot, Image> assignment)
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
            _boostFxController?.PlayHiddenSwapFx(pair.Value);
        }
    }

    private List<Image> TakeHidden(string targetType, int count, List<Image> availableHidden)
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

    private Dictionary<string, int> CountHidden(List<Image> hiddenImages)
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
}
