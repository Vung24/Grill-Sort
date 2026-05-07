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

public class ButtonHoldLogic : Button, IUpdate
{
	[SerializeField] private Transform renderTransform;
	[SerializeField] private ApplyEffectType applyEffectType = ApplyEffectType.Child;
	[SerializeField] private Image fillerImage;
	
	public float holdTime = 2f;
	private float holdTimer;
	
	public bool hasEffect = true;
	public bool hasSound = true;
	public UnityEvent onEnter = new UnityEvent(),
		onDown = new UnityEvent(),
		onExit = new UnityEvent(),
		onUp = new UnityEvent();
		
	
	public UnityEvent onHoldCompleted = new UnityEvent(); 
	
	Vector3 initScale;



	protected override void Awake()
	{
		initScale = Vector3.one;
		if (renderTransform == null)
		{
			renderTransform = applyEffectType == ApplyEffectType.Child ? transform.GetChild(0) : transform;
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		// if (hasSound) AudioManager.Instance.PlayButtonSound();
		base.OnPointerDown(eventData);
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
		onUp.Invoke();
		EffectUp();
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		onExit.Invoke();
	}
	
	public void OnUpdate()
	{
		holdTimer += Time.deltaTime;
		fillerImage.fillAmount = holdTimer / holdTime;
		if (holdTimer >= holdTime)
		{
			onHoldCompleted.Invoke();
			EffectUp();
		}
	}


	void EffectDown()
	{
		holdTimer = 0;
		fillerImage.fillAmount = 0;
		UpdateManager.AddUpdate(this);
		if (hasEffect)
		{
			renderTransform.localScale = initScale;
#if DOTWEEN
            renderTransform.DOScale(initScale * 1.1f, 0.2f).SetEase(Ease.Linear);
#elif PRIME_TWEEN
			Tween.Scale(renderTransform, initScale * 1.1f, .2f, Ease.Linear);
#endif
		}
	}

	void EffectUp()
	{
		holdTimer = 0;
		fillerImage.fillAmount = 0;
		UpdateManager.RemoveUpdate(this);
		if (hasEffect)
		{
			renderTransform.localScale = initScale * 1.1f;
#if DOTWEEN
             renderTransform.DOScale(initScale, 0.2f).SetEase(Ease.Linear);
#elif PRIME_TWEEN
			Tween.Scale(renderTransform, initScale, .2f, Ease.Linear);
#endif

		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		UpdateManager.RemoveUpdate(this);
#if DOTWEEN
        renderTransform.DOKill();
#elif PRIME_TWEEN
		Tween.StopAll(renderTransform);
#endif
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (applyEffectType == ApplyEffectType.Child)
		{
#if DOTWEEN
            renderTransform.DOKill();
#elif PRIME_TWEEN

#endif

		}
		else
		{
#if DOTWEEN
            renderTransform.DOKill();
#elif PRIME_TWEEN

#endif

		}
	}

    #if UNITY_EDITOR

	[Header("DEV")]
	public bool IS_REFRESH = true;

	public void DEV_SET_RENDER_TRANSFORM()
	{
		if (applyEffectType == ApplyEffectType.Child)
		{
			renderTransform = transform.GetChild(0);
		}
		else if (applyEffectType == ApplyEffectType.Parent)
		{
			renderTransform = transform;
		}
		fillerImage = transform.GetChild(0).GetChild(1).GetComponent<Image>();
		this.SetDirty();
		gameObject.SetDirty();
	}

	protected override void OnValidate()
	{
		base.OnValidate();
		if (IS_REFRESH)
		{
			IS_REFRESH = false;
			renderTransform = transform.GetChild(0);
		}

	}

    #endif
}


#if UNITY_EDITOR
[CustomEditor(typeof(ButtonHoldLogic))]
public class ButtonHoldLogicEditor : Editor {
	
	ButtonHoldLogic mtarget;
	private void OnEnable()
	{
		mtarget = target as ButtonHoldLogic;
	}
	
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

	}
}
#endif
