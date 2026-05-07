#if DOTWEEN
using DG.Tweening;
#elif PRIME_TWEEN
using PrimeTween;
#endif

using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ApplyEffectType
{
    Child,
    Parent
}

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class ButtonAttribute : PropertyAttribute {
}

public class ButtonEffectLogic : Button
{
    [SerializeField] private Transform renderTransform;
    [SerializeField] private ApplyEffectType applyEffectType = ApplyEffectType.Child;
    
    
    public bool hasEffect = true;
    public bool hasSound = true;
    public UnityEvent onEnter = new UnityEvent(), 
        onDown = new UnityEvent(),
        onExit = new UnityEvent(),
        onUp = new UnityEvent();
    Vector3 initScale;
    Transform effectTarget;
    bool isPressing;
#if PRIME_TWEEN
    Tween tween;
#endif
    
    

    protected override void Awake()
    {
        base.Awake();
        EnsureClickableGraphic();

        if (renderTransform == null && applyEffectType == ApplyEffectType.Child && transform.childCount > 0)
        {
            renderTransform = transform.GetChild(0);
        }
        effectTarget = applyEffectType == ApplyEffectType.Child && renderTransform != null
            ? renderTransform
            : transform;
        initScale = effectTarget.localScale;
    }

    private void EnsureClickableGraphic()
    {
        if (targetGraphic == null)
        {
            var img = GetComponent<Image>();
            if (img != null)
            {
                targetGraphic = img;
            }
        }

        if (targetGraphic is Graphic g && !g.raycastTarget)
        {
            g.raycastTarget = true;
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        // if(hasSound) AudioManager.Instance.PlayButtonSound();
        base.OnPointerDown(eventData);
        isPressing = true;
        onDown.Invoke();
        EffectDown();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        onEnter.Invoke();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        isPressing = false;
        onUp.Invoke();
        EffectUp();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        onExit.Invoke();
        if (isPressing)
        {
            isPressing = false;
            EffectUp();
        }
    }


    void EffectDown()
    {
        if (hasEffect)
        {
            if (effectTarget == null)
            {
                return;
            }
            effectTarget.localScale = initScale;
#if DOTWEEN
            effectTarget.DOScale(initScale * 1.1f, 0.2f).SetEase(Ease.Linear);
#elif PRIME_TWEEN
            tween.Stop();
            tween = Tween.Scale(effectTarget, initScale * 1.1f, .2f, Ease.Linear);
#else
            effectTarget.localScale = initScale * 1.1f;
#endif
        }
    }

    void EffectUp()
    {
        if (hasEffect)
        {
            if (effectTarget == null)
            {
                return;
            }
            effectTarget.localScale = initScale * 1.1f;
#if DOTWEEN
             effectTarget.DOScale(initScale, 0.2f).SetEase(Ease.Linear);
#elif PRIME_TWEEN
            tween.Stop();
            tween = Tween.Scale(effectTarget, initScale, .2f, Ease.Linear);
#else
            effectTarget.localScale = initScale;
#endif

        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        isPressing = false;
    #if DOTWEEN
        if (effectTarget != null)
        {
            effectTarget.DOKill();
            effectTarget.localScale = initScale;
        }
    #elif PRIME_TWEEN
        tween.Stop();
        if (effectTarget != null)
        {
            effectTarget.localScale = initScale;
        }
    #endif
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    #if DOTWEEN
        if (effectTarget != null)
        {
            effectTarget.DOKill();
        }
    #elif PRIME_TWEEN
        tween.Stop();
    #endif
    }
    
    #if UNITY_EDITOR

    [Header("DEV")]
    public bool IS_REFRESH = true;

    public void DEV_SET_RENDER_TRANSFORM()
    {
        if (applyEffectType == ApplyEffectType.Child)
        {
            renderTransform = transform.GetChild(0);
        }else if (applyEffectType == ApplyEffectType.Parent)
        {
            renderTransform = transform;
        }
        this.SetDirty();
        gameObject.SetDirty();
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        if (IS_REFRESH)
        {
            IS_REFRESH = false;
            if (applyEffectType == ApplyEffectType.Child && transform.childCount > 0)
            {
                renderTransform = transform.GetChild(0);
            }
            else
            {
                renderTransform = transform;
            }
        }
        
    }

    #endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(ButtonEffectLogic))]
public class ButtonEffectLogicEditor : Editor {
    ButtonEffectLogic mtarget;
    private void OnEnable()
    {
        mtarget = target as ButtonEffectLogic;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

    }
}
#endif
