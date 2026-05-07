using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DropDragController : MonoBehaviour
{
    [SerializeField] private Image _imgFoodDrag;
    [SerializeField] private float _timeCheckSuggest;
    private FoodSlot _currentFood, _cacheFood; 
    private bool _hasDrag; 
    private Vector3 _offset; 
    private float _countTime;
    void Update()
    {
        _countTime += Time.deltaTime;
        if (_countTime >= _timeCheckSuggest) // kiem tra goi y thuc an sau mot khoang thoi gian
        {
            _countTime = 0f;
            GameManager.Instance?.ShowHint();
        }

        if (Input.GetMouseButtonDown(0)) 
        {
            _currentFood = Utillities.GetRayCastUI<FoodSlot>(Input.mousePosition);
            if (_currentFood != null && _currentFood.HasFood())
            {
                _hasDrag = true;
                _cacheFood = _currentFood; 
                _imgFoodDrag.gameObject.SetActive(true);
                _imgFoodDrag.sprite = _currentFood.GetSpriteFood; 
                _imgFoodDrag.SetNativeSize();
                _imgFoodDrag.transform.position = _currentFood.transform.position; 

                Vector3 mouseWordPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                _offset = mouseWordPos - _currentFood.transform.position;
                _currentFood.OnActiveFood(false); 
            }
        }

        if (_hasDrag) 
        {
            Vector3 mouseWordPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 foodPos = mouseWordPos + _offset;
            foodPos.z = 0f;
            _imgFoodDrag.transform.position = foodPos; 
            _countTime = 0f;

            FoodSlot slot = Utillities.GetRayCastUI<FoodSlot>(Input.mousePosition);
            if (slot != null && slot.IsGrillActive)
            {
                if (!slot.HasFood()) 
                {
                    if (_cacheFood == null || _cacheFood.GetInstanceID() != slot.GetInstanceID()) // neu foodslot hien tai khac foodslot dang keo tha
                    {
                        _cacheFood?.OnHideFood();
                        _cacheFood = slot; 
                        _cacheFood.OnFadeFood(); 
                        _cacheFood.OnSetSlot(_currentFood.GetSpriteFood); 
                    }
                }
                else
                {
                    FoodSlot slotAvaiable = slot.GetSlotNull; 
                    if (slotAvaiable != null)
                    {
                        _cacheFood?.OnHideFood(); 
                        _cacheFood = slotAvaiable; 
                        _cacheFood.OnFadeFood(); 
                        _cacheFood.OnSetSlot(_currentFood.GetSpriteFood); 
                    }
                    else
                        this.OnClearCacheSlot();
                }
            }
            else 
            {
                if (_cacheFood != null) 
                {
                    _cacheFood.OnHideFood(); 
                    _cacheFood = null;
                }
            }
        }

        if (Input.GetMouseButtonUp(0) && _hasDrag)
        {
            if (_cacheFood != null) 
            {
                _cacheFood.transform.DOMove(_cacheFood.transform.position, 0.15f).OnComplete(() =>
                {
                    bool hasRealMove = _currentFood != null
                        && _cacheFood != null
                        && _currentFood.GetInstanceID() != _cacheFood.GetInstanceID();

                    _imgFoodDrag.gameObject.SetActive(false); 
                    _cacheFood.OnSetSlot(_currentFood.GetSpriteFood); 
                    _cacheFood.OnActiveFood(true); 
                    _cacheFood.OnCheckMerge(); 
                    _currentFood?.OnCheckPrepareTray();
                    if (hasRealMove)
                    {
                        GameManager.Instance?.MarkFoodMoved();
                    }

                    _cacheFood = null;
                    _currentFood = null;
                });
            }
            else
            {
                if (_imgFoodDrag != null && _currentFood != null)
                {
                    _imgFoodDrag.transform.DOMove(
                        _currentFood.transform.position,
                        0.3f
                    ).OnComplete(() =>
                    {
                        _imgFoodDrag.gameObject.SetActive(false);
                        _currentFood.OnActiveFood(true);
                    });
                }
            }
            _hasDrag = false;
        }
    }
    public void OnClearCacheSlot()
    {
        if (_cacheFood != null && _cacheFood.GetInstanceID() != _currentFood.GetInstanceID())
        {
            _cacheFood.OnHideFood();
            _cacheFood = null;
        }
    }
}
