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

    private List<TrayItem> _totalTrays;
    private List<FoodSlot> _totalSlots;
    private Stack<TrayItem> _stackTray = new Stack<TrayItem>();
    public List<FoodSlot> TotalSlots => _totalSlots;
    public AudioManager audioManager;
    private void Awake()
    {
        _totalTrays = Utils.GetListInChild<TrayItem>(_trayContainer);
        _totalSlots = Utils.GetListInChild<FoodSlot>(_slotContainer);
        audioManager = FindAnyObjectByType<AudioManager>();
    }
    public void OnInitGrill(int totalTray, List<Sprite> listFood) 
    {
        int foodCount = Random.Range(1, _totalSlots.Count + 1); 
        List<Sprite> listSlot = Utils.TakeAndRemoveRandom<Sprite>(listFood, foodCount); 
        for (int i = 0; i < listSlot.Count; i++)
        {
            FoodSlot slot = this.RandomSlot();
            slot.OnSetSlot(listSlot[i]);
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
            bool active = i < remainFood.Count;
            _totalTrays[i].gameObject.SetActive(active);

            if (active)
            {
                _totalTrays[i].OnSetFood(remainFood[i]);
                TrayItem item = _totalTrays[i];
                _stackTray.Push(item);
            }
        }
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
        if (_stackTray.Count > 0)
        {
            TrayItem item = _stackTray.Pop(); // lay dia tiep theo de xu ly va xoa khoi ngan xep

            for (int i = 0; i < item.FoodList.Count; i++)
            {
                Image img = item.FoodList[i];
                if (img.gameObject.activeInHierarchy)
                {
                    _totalSlots[i].OnPrepareItem(img);
                    img.gameObject.SetActive(false);
                    //    yield return new WaitForSeconds(0.1f);
                }
            }
            item.gameObject.SetActive(false);
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
                Debug.Log("Merge Success!");

                int itemsCleared = 0;

                for (int i = 0; i < _totalSlots.Count; i++)
                {
                    if (_totalSlots[i].HasFood())
                    {
                        itemsCleared++;
                    }
                    _totalSlots[i].OnActiveFood(false);
                }

                if (audioManager != null)
                {
                    audioManager.PlayCompleteMission();
                }

                // Prepare next tray
                this.OnCheckPrepareTray();

                // ✨ Notify GameManager - Pass number of items merged (usually 3)
                if (SimpleGameManager.Instance != null)
                {
                    SimpleGameManager.Instance.OnItemsMerged(itemsCleared);
                }
                else
                {
                    Debug.LogError("⚠️ SimpleGameManager.Instance is NULL!");
                }
            }
        }
    }

    private bool CanMerge()
    {
        string name = _totalSlots[0].GetSpriteFood.name; // lay ten mon an de so sanh
        for (int i = 1; i < _totalSlots.Count; i++)
        {
            if (_totalSlots[i].GetSpriteFood.name != name) // neu co mon an khac ten
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
}
