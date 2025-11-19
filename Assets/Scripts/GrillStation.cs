using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GrillStation : MonoBehaviour
{
    [SerializeField] private Transform _trayContainer;
    [SerializeField] private Transform _slotContainer;

    private List<TrayItem> _totalTrays;
    private List<FoodSlot> _totalSlots;
    private Stack<TrayItem> _stackTray = new Stack<TrayItem>();
    public List<FoodSlot> TotalSlots => _totalSlots;
    private void Awake()
    {
        _totalTrays = Utils.GetListInChild<TrayItem>(_trayContainer);
        _totalSlots = Utils.GetListInChild<FoodSlot>(_slotContainer);
    }
    public void OnInitGrill(int totalTray, List<Sprite> listFood) // khoi tao bep nuong
    {
        // xu ly set gia tri cho bep truoc
        int foodCount = Random.Range(1, _totalSlots.Count + 1); // so luong mon an tren bep
        List<Sprite> listSlot = Utils.TakeAndRemoveRandom<Sprite>(listFood, foodCount); // lay so luong mon an tren bep
        for (int i = 0; i < listSlot.Count; i++)
        {
            FoodSlot slot = this.RandomSlot();
            slot.OnSetSlot(listSlot[i]);
        }

        // xu ly set dia cho bep
        List<List<Sprite>> remainFood = new List<List<Sprite>>();
        for (int i = 0; i < totalTray - 1; i++)  //tru cho bep 1 dia da duoc xu ly o tren
        {
            remainFood.Add(new List<Sprite>());
            int n = Random.Range(0, listFood.Count);
            remainFood[i].Add(listFood[n]);
            listFood.RemoveAt(n);
        }
        while (listFood.Count > 0)
        {
            int rans = Random.Range(0, remainFood.Count);
            if (remainFood[rans].Count < 3) //gioi han toi da 3 mon/1 dia
            {
                int n = Random.Range(0, listFood.Count);
                remainFood[rans].Add(listFood[n]);
                listFood.RemoveAt(n);
            }
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

    // public void OnCheckDrop(Sprite spr)
    // {
    //     FoodSlot slotAvailable = this.GetSlotNull();
    //     if(slotAvailable != null)
    //     {
    //         slotAvailable.OnSetSlot(spr);
    //         slotAvailable.OnHideFood();
    //     }
    // }
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
               if(img.gameObject.activeInHierarchy)
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
                Debug.Log("Merge Success");
                for (int i = 0; i < _totalSlots.Count; i++)
                {
                    _totalSlots[i].OnActiveFood(false);
                }

                this.OnCheckPrepareTray();
                GameManagers.Instance?.OnMinusFood();
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
