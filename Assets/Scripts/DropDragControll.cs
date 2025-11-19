using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DropDragControll : MonoBehaviour
{
    [SerializeField] private Image _imgFoodDrag;
    [SerializeField] private float _timeCheckSuggest;
    private FoodSlot _currentFood, _cacheFood; // bien luu tru foodslot hien tai dang duoc keo tha
    private bool _hasDrag; // bien kiem tra co dang keo tha hay khong
    private Vector3 _offset; // bien luu tru khoang cach giua diem chon va vi tri foodslot
    private float _countTime;
    void Update()
    {
        _countTime += Time.deltaTime;
        if(_countTime >= _timeCheckSuggest) // kiem tra goi y thuc an sau mot khoang thoi gian
        {
            _countTime = 0f;
            GameManagers.Instance?.OnCheckAndShake();
        }

        if (Input.GetMouseButtonDown(0)) // check diem chuot
        {
            _currentFood = Utils.GetRayCastUI<FoodSlot>(Input.mousePosition); // lay foodslot hien tai dang keo tha
            if (_currentFood != null && _currentFood.HasFood())
            {
                _hasDrag = true;
                _cacheFood = _currentFood; // luu foodslot hien tai vao bien tam
                _imgFoodDrag.gameObject.SetActive(true);
                _imgFoodDrag.sprite = _currentFood.GetSpriteFood; // gan sprite cho hinh anh keo tha
                _imgFoodDrag.SetNativeSize();
                _imgFoodDrag.transform.position = _currentFood.transform.position; // dat hinh anh keo tha o vi tri foodslot

                //tinh offset
                Vector3 mouseWordPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                _offset = mouseWordPos - _currentFood.transform.position;
                _currentFood.OnActiveFood(false); // an foodslot dang keo tha
            }
        }

        if (_hasDrag)  // neu dang keo tha
        {
            Vector3 mouseWordPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 foodPos = mouseWordPos + _offset;
            foodPos.z = 0f;
            _imgFoodDrag.transform.position = foodPos; // cap nhat vi tri hinh anh keo tha theo chuot
            _countTime = 0f;

            FoodSlot slot = Utils.GetRayCastUI<FoodSlot>(Input.mousePosition); // lay foodslot dang o vi tri chuot hien tai
            if (slot != null) // neu foodslot khac null
            {
                if (!slot.HasFood()) // neu foodslot dang o vi tri chuot khong co food
                {
                    if ( _cacheFood == null || _cacheFood.GetInstanceID() != slot.GetInstanceID()) // neu foodslot hien tai khac foodslot dang keo tha
                    {
                        _cacheFood?.OnHideFood(); // an foodslot tam thoi
                        _cacheFood = slot; // cap nhat foodslot tam thoi
                        _cacheFood.OnFadeFood(); // lam mo foodslot dang o vi tri chuot
                        _cacheFood.OnSetSlot(_currentFood.GetSpriteFood); // hien food trong foodslot dang o vi tri chuot

                    }
                }
                else // neu foodslot dang o vi tri chuot co food
                {
                    FoodSlot slotAvaiable = slot.GetSlotNull; // lay foodslot trong nhung foodslot con trong
                    if (slotAvaiable != null) // neu con foodslot trong
                    {

                        _cacheFood?.OnHideFood(); // an foodslot tam thoi
                        _cacheFood = slotAvaiable; // cap nhat foodslot tam thoi
                        _cacheFood.OnFadeFood(); // lam mo foodslot dang o vi tri chuot
                        _cacheFood.OnSetSlot(_currentFood.GetSpriteFood); // hien food trong foodslot dang o vi tri chuot

                    }
                    else
                    {
                        this.OnClearCacheSlot();

                    }
                }

            }
            else // neu foodslot dang o vi tri chuot la null
            {
                if(_cacheFood != null) // neu foodslot tam thoi khac null
                {
                    _cacheFood.OnHideFood(); // an foodslot tam thoi
                    _cacheFood = null;
                }
            }
        }

        if (Input.GetMouseButtonUp(0) && _hasDrag) // khi tha chuot
        {
            if( _cacheFood != null) // neu foodslot tam thoi khac null
            {
                _cacheFood.transform.DOMove(_cacheFood.transform.position, 0.15f).OnComplete(()=>
                {
                    _imgFoodDrag.gameObject.SetActive(false); // an hinh anh keo tha
                    _cacheFood.OnSetSlot(_currentFood.GetSpriteFood); // hien food trong foodslot tam thoi
                    _cacheFood.OnActiveFood(true); // hien foodslot dang o vi tri chuot
                    _cacheFood.OnCheckMerge(); // kiem tra hop nhat mon an
                    _currentFood?.OnCheckPrepareTray();

                    _cacheFood = null; // xoa foodslot tam thoi
                    _currentFood = null; 
                });
            }
            else
            {
                 _cacheFood.transform.DOMove(_cacheFood.transform.position, 0.3f).OnComplete(()=>
                {
                    _imgFoodDrag.gameObject.SetActive(false); // an hinh anh keo tha
                    _currentFood.OnActiveFood(true); // hien foodslot dang keo tha
                }); 
            }
            _hasDrag = false; // dat lai trang thai khong keo tha
        }
    }
    public void OnClearCacheSlot() // an foodslot tam thoi
    {
        if (_cacheFood != null && _cacheFood.GetInstanceID() != _currentFood.GetInstanceID())
        {
            _cacheFood.OnHideFood(); // an foodslot tam thoi
            _cacheFood = null;
        }
    }
}
