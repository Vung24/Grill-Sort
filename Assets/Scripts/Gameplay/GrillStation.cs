using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GrillStation : MonoBehaviour
{
    [Header("transform")]
    [SerializeField] private Transform _trayContainer;
    [SerializeField] private Transform _slotContainer;
    [Header("list")]
    private List<TrayItem> _totalTrays;
    private List<FoodSlot> _totalSlots;
    private Stack<TrayItem> _stackTray = new Stack<TrayItem>();
    public List<FoodSlot> TotalSlots => _totalSlots;
    public Transform TrayContainer => _trayContainer;
    public Transform SlotContainer => _slotContainer;
    private void Awake()
    {
        _totalTrays = Utillities.GetListInChild<TrayItem>(_trayContainer);
        _totalSlots = Utillities.GetListInChild<FoodSlot>(_slotContainer);
    }
    public void OnInitGrill(int totalTray, List<Sprite> listFood, bool spawnMergeReady = false)
    {
        bool hasPreparedMergeReady = spawnMergeReady && TryAssignMergeReadySlots(listFood);
        if (!hasPreparedMergeReady)
        {
            int foodCount = Random.Range(1, _totalSlots.Count + 1);
            List<Sprite> listSlot = Utillities.TakeAndRemoveRandom<Sprite>(listFood, foodCount);
            for (int i = 0; i < listSlot.Count; i++)
            {
                FoodSlot slot = this.RandomSlot();
                slot.OnSetSlot(listSlot[i]);
            }
        }

        List<List<Sprite>> remainFood = new List<List<Sprite>>();

        for (int i = 0; i < totalTray; i++)
        {
            remainFood.Add(new List<Sprite>());
        }

        while (listFood.Count > 0 && remainFood.Count(t => t.Count == 0) > 0)
        {
            var emptyTrays = remainFood.FindAll(t => t.Count == 0);
            int trayIndex = Random.Range(0, emptyTrays.Count);
            int foodIndex = Random.Range(0, listFood.Count);

            emptyTrays[trayIndex].Add(listFood[foodIndex]);
            listFood.RemoveAt(foodIndex);
        }

        while (listFood.Count > 0)
        {
            var availableTrays = remainFood.Where(t => t.Count < 3).ToList();
            if (availableTrays.Count == 0) break;

            int trayIndex = Random.Range(0, availableTrays.Count);
            int foodIndex = Random.Range(0, listFood.Count);

            availableTrays[trayIndex].Add(listFood[foodIndex]);
            listFood.RemoveAt(foodIndex);
        }

        for (int i = 0; i < _totalTrays.Count; i++)
        {
            if (i < remainFood.Count)
            {
                if (remainFood[i].Count > 0)
                {
                    _totalTrays[i].gameObject.SetActive(true);
                    _totalTrays[i].OnSetFood(remainFood[i]);
                    _stackTray.Push(_totalTrays[i]);
                }
                else
                {
                    _totalTrays[i].gameObject.SetActive(false);
                    Debug.Log($"Tray {i}");
                }
            }
            else
            {
                _totalTrays[i].gameObject.SetActive(false);
            }
        }
    }

    private bool TryAssignMergeReadySlots(List<Sprite> listFood)
    {
        if (listFood == null || _totalSlots == null)
        {
            return false;
        }

        int requiredCount = _totalSlots.Count;
        if (requiredCount <= 0 || listFood.Count < requiredCount)
        {
            return false;
        }

        List<Sprite> candidates = listFood
            .GroupBy(s => s)
            .Where(g => g.Count() >= requiredCount)
            .Select(g => g.Key)
            .ToList();

        if (candidates.Count == 0)
        {
            return false;
        }

        Sprite chosenFood = candidates[Random.Range(0, candidates.Count)];
        for (int i = 0; i < requiredCount; i++)
        {
            listFood.Remove(chosenFood);
            _totalSlots[i].OnSetSlot(chosenFood);
        }

        return true;
    }

    private FoodSlot RandomSlot()
    {
    reRand: int n = Random.Range(0, _totalSlots.Count);
        if (_totalSlots[n].HasFood()) goto reRand;
        return _totalSlots[n];
    }

    public FoodSlot GetSlotNull()
    {
        for (int i = 0; i < _totalSlots.Count; i++)
        {
            if (!_totalSlots[i].HasFood())
            {
                return _totalSlots[i];
            }
        }
        return null;
    }

    private void OnPrepareTray()
    {
        while (_stackTray.Count > 0)
        {
            TrayItem item = _stackTray.Pop();
            if (item == null)
            {
                continue;
            }

            bool movedAny = false;

            for (int i = 0; i < item.FoodList.Count && i < _totalSlots.Count; i++)
            {
                Image img = item.FoodList[i];
                if (img != null && img.gameObject.activeInHierarchy)
                {
                    _totalSlots[i].OnPrepareItem(img);
                    img.gameObject.SetActive(false);
                    movedAny = true;
                }
            }

            item.gameObject.SetActive(false);

            if (movedAny)
            {
                return;
            }
        }
    }

    public void OnCheckPrepareTray()
    {
        if (this.HasGrillEmpty())
        {
            this.OnPrepareTray();
        }
    }

    public void OnCheckMerge()
    {
        if (this.GetSlotNull() == null)
        {
            if (this.CanMerge())
            {
                int itemsCleared = 0;

                for (int i = 0; i < _totalSlots.Count; i++)
                {
                    if (_totalSlots[i].HasFood())
                    {
                        itemsCleared++;
                    }
                    _totalSlots[i].OnActiveFood(false);
                }

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayCompleteMission();
                }

                this.OnCheckPrepareTray();
                GameManager.Instance.OnItemsMerged(itemsCleared);

            }
        }
    }

    private bool CanMerge()
    {
        string name = _totalSlots[0].GetSpriteFood.name;
        for (int i = 1; i < _totalSlots.Count; i++)
        {
            if (_totalSlots[i].GetSpriteFood.name != name)
            {
                return false;
            }
        }
        return true;
    }
    private bool HasGrillEmpty()
    {
        for (int i = 0; i < _totalSlots.Count; i++)
        {
            if (_totalSlots[i].HasFood())
            {
                return false;
            }
        }
        return true;
    }

    public List<Image> GetHiddenFoodImages()
    {
        List<Image> hiddenFoods = new List<Image>();

        foreach (TrayItem tray in _totalTrays)
        {
            if (tray == null || !tray.gameObject.activeInHierarchy) continue;

            foreach (Image img in tray.FoodList)
            {
                if (img == null || !img.gameObject.activeInHierarchy || img.sprite == null) continue;
                hiddenFoods.Add(img);
            }
        }

        return hiddenFoods;
    }

    public void ResolveAfterBoost()
    {
        HideEmptyTrays();

        if (HasGrillEmpty())
        {
            OnPrepareTray();
        }
    }

    private void HideEmptyTrays()
    {
        for (int i = 0; i < _totalTrays.Count; i++)
        {
            TrayItem tray = _totalTrays[i];
            if (tray == null || !tray.gameObject.activeInHierarchy) continue;

            if (!tray.HasAnyFood())
            {
                tray.gameObject.SetActive(false);
            }
        }
    }
}
