using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrayItem : MonoBehaviour
{
    private List<Image> _foodList;
    public List<Image> FoodList => _foodList;

    void Awake()
    {
        _foodList = Utillities.GetListInChild<Image>(this.transform);
        for(int i =0 ; i< _foodList.Count; i++)
        {
            _foodList[i].gameObject.SetActive(false);
        }
    }
    public void OnSetFood(List<Sprite> items)
    {
        if(items.Count <= _foodList.Count)
        {
            for(int i=0; i<items.Count; i++)
            {
                Image slot = this.RandomSlot();
                slot.gameObject.SetActive(true);
                slot.sprite = items[i];
                slot.SetNativeSize();
            }
        }
    }
    private Image RandomSlot()
    {
        rerand: int n = Random.Range(0, _foodList.Count);
        if(_foodList[n].gameObject.activeInHierarchy) goto rerand;
        return _foodList[n];
    }

    public bool HasAnyFood()
    {
        for (int i = 0; i < _foodList.Count; i++)
        {
            if (_foodList[i] != null && _foodList[i].gameObject.activeInHierarchy && _foodList[i].sprite != null)
            {
                return true;
            }
        }

        return false;
    }
}
