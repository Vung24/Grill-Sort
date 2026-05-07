#if DOTWEEN
using DG.Tweening;
#elif PRIME_TWEEN
using PrimeTween;
#endif
using UnityEngine;

public class UIShowEffect : MonoBehaviour
{
    #region ENUM
    
    public enum ShowEffectType
    {
        Left,
        Right,
        Down,
        Up,
        Scale,
    }
    
    /// <summary>
    /// Show Effect Type
    /// </summary>
    public enum ShowType
    {
        /// <summary>Automatic call show effect at Enable.</summary>
        Automatic,
        /// <summary>Callable only through functions show effect.</summary>
        Manual,
    }

    #endregion
    
    [SerializeField] private RectTransform _rect;
    [SerializeField] private ShowType showType;
    [SerializeField] private ShowEffectType showEffectTypeState;
    [SerializeField] private bool setUpdate = true;

    public float timeShow = .25f, effectMove = 15f; 
    private float timeAfterEffect = .1f;
    
    private Vector2 _prePos, _targetPos, _moveDelta;
    private bool _targetHadSet = false;
    private Vector3 _targetScale;

    private void Awake()
    {
        _prePos = _rect.anchoredPosition;
        _targetPos = _prePos;
        _moveDelta = Vector2.zero;
    }

    private void OnEnable()
    {
        if(CheckType(ShowType.Automatic)) Effect();
    }

    [Button]
    public void Effect()
    {
        if(!gameObject.activeSelf) gameObject.SetActive(true);
        
        if (!_targetHadSet)
        {
            _targetHadSet = true;
            switch (showEffectTypeState)
            {
                case ShowEffectType.Left:
                    _targetPos -= new Vector2(_rect.sizeDelta.x + 100f, 0);
                    _moveDelta = new Vector2(effectMove, 0);
                    
                    //Full Stretch
                    if (_rect.sizeDelta == Vector2.zero) _targetPos = new Vector2(-Screen.width / 2f - 100f, 0);
                    break;
                case ShowEffectType.Right:
                    _targetPos += new Vector2(_rect.sizeDelta.x + 100f, 0);
                    _moveDelta = new Vector2(-effectMove, 0);
                    
                    //Full Stretch
                    if (_rect.sizeDelta == Vector2.zero) _targetPos = new Vector2(Screen.width / 2f + 100f, 0);
                    break;
                case ShowEffectType.Up:
                    _targetPos -= new Vector2(0, _rect.sizeDelta.y + 100f);
                    
                    //Full Stretch
                    if (_rect.sizeDelta == Vector2.zero) _targetPos = new Vector2(0, -Screen.height / 2f - 100f);
                    break;
                case ShowEffectType.Down:
                    _targetPos += new Vector2(0, _rect.sizeDelta.y + 100f);
                    
                    //Full Stretch
                    if (_rect.sizeDelta == Vector2.zero) _targetPos = new Vector2(0, Screen.height / 2f + 100f);
                    break;
                case ShowEffectType.Scale:
                    _targetScale = transform.localScale;
                    break;
            }
        }

        if (showEffectTypeState == ShowEffectType.Scale)
        {
#if DOTWEEN
            transform.DOScale(_targetScale, timeShow).From(0).SetUpdate(setUpdate);
#elif PRIME_TWEEN
            Tween.Scale(transform, Vector3.zero, _targetScale, timeShow, useUnscaledTime: setUpdate);
#endif

        }
        else
        {
#if DOTWEEN
            _rect.DOAnchorPos(_prePos + _moveDelta, timeShow).From(_targetPos).SetUpdate(setUpdate).OnComplete(() =>
            {
                _rect.DOAnchorPos(_prePos, timeAfterEffect).SetUpdate(setUpdate);
            });
#elif PRIME_TWEEN
            Tween.UIAnchoredPosition(_rect, _targetPos, _prePos + _moveDelta, timeShow, useUnscaledTime: setUpdate).OnComplete(() =>
            {
                Tween.UIAnchoredPosition(_rect, _prePos, timeAfterEffect, useUnscaledTime: setUpdate);
            });
#endif

        }
        
      
    }
    
    public void Hide()
    {
        if(!gameObject.activeSelf) return;

        if (CheckEffectType(ShowEffectType.Scale))
        {
#if DOTWEEN
            transform.DOKill();
            transform.DOScale(0, timeShow).SetUpdate(setUpdate).OnComplete(() =>
            {
                if(CheckType(ShowType.Automatic)) gameObject.SetActive(false);
            });
#elif PRIME_TWEEN
            Tween.StopAll(transform);
            Tween.Scale(transform, 0, timeShow, useUnscaledTime: setUpdate).OnComplete(() =>
            {
                if (CheckType(ShowType.Automatic)) gameObject.SetActive(false);
            });
#endif

        }
        else
        {
#if DOTWEEN
            _rect.DOKill();
            _rect.DOAnchorPos(_prePos + _moveDelta, timeAfterEffect).SetUpdate(setUpdate).OnComplete(() =>
            {
                _rect.DOAnchorPos(_targetPos, timeShow).SetUpdate(setUpdate).OnComplete(() =>
                {
                    if(CheckType(ShowType.Automatic)) gameObject.SetActive(false);
                });
            });
#elif PRIME_TWEEN
            Tween.StopAll(_rect);
            Tween.UIAnchoredPosition(_rect, _prePos + _moveDelta, timeAfterEffect, useUnscaledTime: setUpdate).OnComplete(() =>
            {
                Tween.UIAnchoredPosition(_rect, _targetPos, timeShow, useUnscaledTime: setUpdate).OnComplete(() =>
                {
                    if(CheckType(ShowType.Automatic)) gameObject.SetActive(false);
                });
            });
#endif
           
        }
    }

    public bool CheckType(ShowType type)
    {
        return type == showType;
    }

    public bool CheckEffectType(ShowEffectType effectType)
    {
        return effectType == showEffectTypeState;
    }

    private void OnDisable()
    {
#if DOTWEEN
         _rect.DOKill();
        transform.DOKill();    
#elif PRIME_TWEEN
        Tween.StopAll(_rect);
        Tween.StopAll(transform);
#endif
       
    }
    
    #if UNITY_EDITOR

    [Button]
    private void DEV_INIT()
    {
        
    }
    
    
    #endif
}
