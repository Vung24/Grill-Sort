using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayConditionController : MonoBehaviour
{
    [Header("Hint Deadlock")]
    [SerializeField] private int _hintNearFullEmptySlotsThreshold = 2;

    private GameManager _gameManager;
    private List<GrillStation> _grillStations;

    public void Initialize(GameManager gameManager, List<GrillStation> grillStations)
    {
        _gameManager = gameManager;
        _grillStations = grillStations;
    }

    public void CheckWinCondition()
    {
        if (_gameManager == null || _gameManager.IsLevelComplete)
        {
            return;
        }

        if (IsBoardCleared())
        {
            _gameManager.TriggerLevelWin();
        }
    }

    public void TickLoseConditions()
    {
        if (_gameManager == null || _gameManager.IsLevelComplete)
        {
            return;
        }

        bool allGrillsFull = true;

        foreach (GrillStation grill in _grillStations)
        {
            if (grill == null || !grill.gameObject.activeInHierarchy) continue;
            if (grill.TrayContainer == null || !grill.TrayContainer.gameObject.activeInHierarchy) continue;

            if (grill.GetSlotNull() != null)
            {
                allGrillsFull = false;
                break;
            }
        }

        if (allGrillsFull && !CanMove())
        {
            _gameManager.TriggerLevelLose(EnumManager.LoseReason.OutOfSlot);
        }
    }

    public void HandleTimeUp()
    {
        if (_gameManager == null)
        {
            return;
        }

        if (_gameManager.CurrentLevelState != EnumManager.LevelState.Playing || _gameManager.IsLevelComplete)
        {
            return;
        }

        _gameManager.TriggerLevelLose(EnumManager.LoseReason.TimeUp);
    }

    public bool IsNearFullDeadlockedForHint()
    {
        return IsNearFullDeadlockedForHint(_hintNearFullEmptySlotsThreshold);
    }

    public bool IsNearFullDeadlockedForHint(int nearFullEmptySlotsThreshold)
    {
        if (_grillStations == null || _grillStations.Count == 0)
        {
            return false;
        }

        int totalEmptySlots = 0;
        bool hasAnyCompletelyEmptyGrill = false;
        Dictionary<string, int> visibleCountByType = new Dictionary<string, int>();

        foreach (GrillStation grill in _grillStations)
        {
            if (grill == null || !grill.gameObject.activeInHierarchy) continue;
            if (grill.TrayContainer == null || !grill.TrayContainer.gameObject.activeInHierarchy) continue;
            if (grill.TotalSlots == null || grill.TotalSlots.Count == 0) continue;

            int occupiedInThisGrill = 0;
            foreach (FoodSlot slot in grill.TotalSlots)
            {
                if (slot == null || !slot.HasFood() || slot.GetSpriteFood == null)
                {
                    totalEmptySlots++;
                    continue;
                }

                occupiedInThisGrill++;
                string foodType = slot.GetSpriteFood.name;
                if (!visibleCountByType.ContainsKey(foodType))
                {
                    visibleCountByType[foodType] = 0;
                }

                visibleCountByType[foodType]++;
            }

            if (occupiedInThisGrill == 0)
            {
                hasAnyCompletelyEmptyGrill = true;
            }
        }

        if (hasAnyCompletelyEmptyGrill)
        {
            return false;
        }

        if (totalEmptySlots > Mathf.Max(0, nearFullEmptySlotsThreshold))
        {
            return false;
        }

        foreach (KeyValuePair<string, int> pair in visibleCountByType)
        {
            if (pair.Value >= 3)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsBoardCleared()
    {
        if (_grillStations == null)
        {
            return true;
        }

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

    private bool CanMove()
    {
        if (_grillStations == null)
        {
            return false;
        }

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
                {
                    return true;
                }
            }
        }

        return false;
    }
}
