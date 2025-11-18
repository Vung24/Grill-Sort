using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrillStation : MonoBehaviour
{
    [SerializeField] private Transform _trayContainer;
    [SerializeField] private Transform _slotContainer;

    private List<TrayItem> _totalTrays;
    private List<FoodSlot> _totalSlots;
    private void Awake()
    {
        _totalTrays = Utils.GetListInChild<TrayItem>(_trayContainer);
        _totalSlots = Utils.GetListInChild<FoodSlot>(_slotContainer);
    }
    public void OnInitGrill(int totalTray, List<Sprite> listFood) // khoi tao bep nuong
    {
        // xu ly set gia tri cho bep truoc
        int foodCount = Random.Range(1,_totalSlots.Count +1); // so luong mon an tren bep
        List<Sprite> listSlot = Utils.TakeAndRemoveRandom<Sprite>(listFood, foodCount); // lay so luong mon an tren bep
        for(int i =0; i< listSlot.Count; i++)
        {
            FoodSlot slot = this.RandomSlot();
            slot.OnSetSlot(listSlot[i]);
        }

        // xu ly set dia cho bep
        List<List<Sprite>> remainFood = new List<List<Sprite>>();
        for(int i =0; i< totalTray-1; i++)  //tru cho bep 1 dia da duoc xu ly o tren
        {
            remainFood.Add(new List<Sprite>());
            int n= Random.Range(0, listFood.Count);
            remainFood[i].Add(listFood[n]);
            listFood.RemoveAt(n);
        }
        while (listFood.Count > 0)
        {
            int rans = Random.Range(0, remainFood.Count);
            if(remainFood[rans].Count < 3) //gioi han toi da 3 mon/1 dia
            {
                int n = Random.Range(0, listFood.Count);
                remainFood[rans].Add(listFood[n]);
                listFood.RemoveAt(n);
            }
        }
        for(int i=0 ; i< _totalTrays.Count; i++)
        {
            bool active = i < remainFood.Count;
            _totalTrays[i].gameObject.SetActive(active);

            if(active)
            {
                _totalTrays[i].OnSetFood(remainFood[i]);
            }

        }
    }
    private FoodSlot RandomSlot()
    {
        reRand : int n = Random.Range(0, _totalSlots.Count);
        if(_totalSlots[n].HasFood()) goto reRand;
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
        for(int i=0;i< _totalSlots.Count; i++)
        {
            if(!_totalSlots[i].HasFood())
            {
                return _totalSlots[i];
            }
        }

        return null;
    }

}
