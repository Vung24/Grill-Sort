using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FoodSlot : MonoBehaviour
{
    private Image _imgFood;
    private Color _normalColor = new Color(1f, 1f, 1f, 1f);
    private Color _fadeColor = new Color(1f, 1f, 1f, 0.7f);
    private Color _swapFlashColor = new Color(1f, 0.95f, 0.75f, 1f);
    private GrillStation _grillCtrl;

    void Awake()
    {
        _imgFood = this.transform.GetChild(0).GetComponent<Image>();
        _imgFood.gameObject.SetActive(false);
        _grillCtrl = this.transform.parent.parent.GetComponent<GrillStation>(); 
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
        _imgFood.color = _normalColor;
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
    public void OnCheckMerge()
    {
        _grillCtrl?.OnCheckMerge();
    }

    public void OnPrepareItem(Image img)
    {
        this.OnSetSlot(img.sprite);
        _imgFood.color = _normalColor;
        _imgFood.transform.position = img.transform.position;
        _imgFood.transform.localScale = img.transform.localScale;
        _imgFood.transform.localEulerAngles = img.transform.localEulerAngles; 
        _imgFood.transform.DOLocalMove(Vector3.zero, 0.3f);         
        _imgFood.transform.DOScale(Vector3.one, 0.3f);              
        _imgFood.transform.DORotate(Vector3.zero, 0.3f);           
    }

    public void OnCheckPrepareTray()
    {
        _grillCtrl?.OnCheckPrepareTray();
    }
    public void DoShake()
    {
        _imgFood.transform.DOShakePosition(0.5f, 10f, 20, 90f, false, true);
    }

    public void RemoveByBoost()
    {
        if (!_imgFood.gameObject.activeInHierarchy)
        {
            return;
        }

        _imgFood.transform.DOKill();
        _imgFood.DOKill();
        _imgFood.color = _normalColor;
        _imgFood.transform.localScale = Vector3.one;

        Sequence seq = DOTween.Sequence();
        seq.Append(_imgFood.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.12f, 8, 0.75f));
        seq.Append(_imgFood.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));
        seq.Join(_imgFood.DOFade(0f, 0.2f));
        seq.OnComplete(() =>
        {
            _imgFood.gameObject.SetActive(false);
            _imgFood.transform.localScale = Vector3.one;
            _imgFood.transform.localPosition = Vector3.zero;
            _imgFood.color = _normalColor;
        });
    }

    public void PlaySwapByBoostEffect()
    {
        if (!_imgFood.gameObject.activeInHierarchy)
        {
            return;
        }

        _imgFood.transform.DOKill();
        _imgFood.DOKill();
        _imgFood.transform.localScale = Vector3.one;
        _imgFood.color = _normalColor;

        Sequence seq = DOTween.Sequence();
        seq.Append(_imgFood.transform.DOPunchScale(new Vector3(0.16f, 0.16f, 0f), 0.18f, 8, 0.8f));
        seq.Join(_imgFood.DOColor(_swapFlashColor, 0.09f));
        seq.Append(_imgFood.DOColor(_normalColor, 0.12f));
    }

    public bool IsGrillActive => _grillCtrl != null && _grillCtrl.TrayContainer.gameObject.activeInHierarchy;
    public FoodSlot GetSlotNull => IsGrillActive ? _grillCtrl.GetSlotNull() : null;
    public bool HasFood() => _imgFood.gameObject.activeInHierarchy && _imgFood.color == _normalColor; 
    public Sprite GetSpriteFood => _imgFood.sprite;
}
