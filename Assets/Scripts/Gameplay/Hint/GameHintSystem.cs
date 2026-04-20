using System.Collections.Generic;
using UnityEngine;

public class GameHintSystem : MonoBehaviour
{
    [SerializeField] private float _hintCheckInterval = 10f;

    private float _hintTimer;
    private List<GrillStation> _grillStations;

    public void Initialize(List<GrillStation> grillStations)
    {
        _grillStations = grillStations;
        _hintTimer = 0f;
    }

    public void ResetHintTimer()
    {
        _hintTimer = 0f;
    }

    public void TickHint()
    {
        if (_grillStations == null || _grillStations.Count == 0)
        {
            return;
        }

        _hintTimer += Time.deltaTime;
        if (_hintTimer < _hintCheckInterval)
        {
            return;
        }

        _hintTimer = 0f;
        TryShowHint();
    }

    public void ShowHintNow()
    {
        TryShowHint();
    }

    private void TryShowHint()
    {
        if (_grillStations == null || _grillStations.Count == 0)
        {
            return;
        }

        Dictionary<string, List<FoodSlot>> foodGroups = new Dictionary<string, List<FoodSlot>>();
        foreach (GrillStation grill in _grillStations)
        {
            if (!IsGrillValid(grill))
            {
                continue;
            }

            foreach (FoodSlot slot in grill.TotalSlots)
            {
                if (slot == null || !slot.HasFood() || slot.GetSpriteFood == null)
                {
                    continue;
                }

                string foodName = slot.GetSpriteFood.name;
                if (!foodGroups.ContainsKey(foodName))
                {
                    foodGroups[foodName] = new List<FoodSlot>();
                }

                foodGroups[foodName].Add(slot);
            }
        }

        foreach (KeyValuePair<string, List<FoodSlot>> group in foodGroups)
        {
            if (group.Value.Count < 3)
            {
                continue;
            }

            for (int i = 0; i < 3; i++)
            {
                group.Value[i].DoShake();
            }
            return;
        }
    }

    private static bool IsGrillValid(GrillStation grill)
    {
        if (grill == null || !grill.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (grill.TrayContainer == null || !grill.TrayContainer.gameObject.activeInHierarchy)
        {
            return false;
        }

        return true;
    }
}
