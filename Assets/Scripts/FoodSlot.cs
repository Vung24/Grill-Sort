using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FoodSlot : MonoBehaviour
{
    private Image _imgFood;
    private Color _normalColor = new Color(1f, 1f, 1f, 1f);
    private Color _fadeColor = new Color(1f, 1f, 1f, 0.7f);
    private GrillStation _grillCtrl;

    void Awake()
    {
        _imgFood = this.transform.GetChild(0).GetComponent<Image>();
        _imgFood.gameObject.SetActive(false);
        _grillCtrl = this.transform.parent.parent.GetComponent<GrillStation>(); // lay component GrillStation tu doi tuong cha. _grillCtrl = this.getComponentInParent<GrillStation>();

    }

    public void OnSetSlot(Sprite sprite)
    {
        _imgFood.gameObject.SetActive(true);
        _imgFood.sprite = sprite;
        _imgFood.SetNativeSize();
    }
    public void OnActiveFood(bool isActive)
    {
        _imgFood.gameObject.SetActive(isActive);
    }
    public void OnFadeFood()
    {
        this.OnActiveFood(true);
        _imgFood.color = _fadeColor;
    }
    public void OnHideFood()
    {
        this.OnActiveFood(false);
        _imgFood.color = _normalColor;
    }
    // public void OnCheckDrop(Sprite spr)
    // {
    //     _grillCtrl.OnCheckDrop(spr);
    // }
    public FoodSlot GetSlotNull => _grillCtrl.GetSlotNull();
    public bool HasFood() => _imgFood.gameObject.activeInHierarchy && _imgFood.color == _normalColor; 
    public Sprite GetSpriteFood => _imgFood.sprite;
}
