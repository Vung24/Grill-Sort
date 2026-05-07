using System.Collections;
using DG.Tweening;
using UnityEngine;

public class RemoveFx : MonoBehaviour
{
    [Header("FX")]
    [SerializeField] private RectTransform _fxRoot;
    [SerializeField] private RectTransform _startPoint;
    [SerializeField] private RectTransform _middlePoint;
    [SerializeField] private RectTransform _endPoint;
    [SerializeField] private float _popupDuration = 0.15f;
    [SerializeField] private float _flyDuration = 0.6f;
    [SerializeField] private float _firstFlyDuration = 0.35f;
    [SerializeField] private float _shakeDuration = 0.3f;
    [SerializeField] private float _shakeStrength = 18f;
    [SerializeField] private int _shakeVibrato = 18;
    [SerializeField] private float _shakeRandomness = 90f;
    [SerializeField] private float _secondFlyDuration = 0.25f;
    [SerializeField] private float _startScale = 0.7f;
    [SerializeField] private float _middleScale = 1.8f;
    [SerializeField] private float _endScale = 0.2f;
    [SerializeField] private Ease _moveEase = Ease.InOutCubic;

    private bool _isPlaying;
    private bool _canToggleFxRootActive;
    private Tween _watchdogTween;
    public bool IsPlaying => _isPlaying;

    private void OnValidate()
    {
        TryResolveReferences();
    }

    private void Awake()
    {
        TryResolveReferences();
        SetFxVisible(false);
    }

    private void OnDisable()
    {
        if (_fxRoot != null)
        {
            _fxRoot.DOKill();
            SetFxVisible(false);
        }
        _isPlaying = false;
    }

    public bool Play(System.Action onCompleted)
    {
        if (_isPlaying)
        {
            return false;
        }

        if (!isActiveAndEnabled)
        {
            onCompleted?.Invoke();
            return true;
        }

        if (_fxRoot == null)
        {
            onCompleted?.Invoke();
            return true;
        }

        StartCoroutine(PlayRoutine(onCompleted));
        return true;
    }

    private IEnumerator PlayRoutine(System.Action onCompleted)
    {
        _isPlaying = true;
        try
        {
            TryResolveReferences();

            SetFxVisible(true);
            _fxRoot.DOKill();
            _fxRoot.localScale = Vector3.one * Mathf.Max(0.01f, _startScale);

            CanvasGroup canvasGroup = _fxRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = _fxRoot.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.DOKill();
            canvasGroup.alpha = 1f;

            Vector2 startPos = ResolveAnchoredPosition(_startPoint, Vector2.zero);
            Vector2 middleFallback = Vector2.Lerp(startPos, ResolveAnchoredPosition(_endPoint, startPos), 0.5f);
            Vector2 middlePos = ResolveAnchoredPosition(_middlePoint, middleFallback);
            Vector2 endPos = ResolveAnchoredPosition(_endPoint, middlePos);
            _fxRoot.anchoredPosition = startPos;

            if (_popupDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(_popupDuration);
            }

            float totalFly = Mathf.Max(0.01f, _flyDuration);
            float firstFly = _firstFlyDuration > 0f ? _firstFlyDuration : totalFly * 0.6f;
            float secondFly = _secondFlyDuration > 0f ? _secondFlyDuration : totalFly * 0.4f;
            firstFly = Mathf.Max(0.01f, firstFly);
            secondFly = Mathf.Max(0.01f, secondFly);

            float watchdogDuration = _popupDuration + firstFly + _shakeDuration + secondFly + 0.2f;
            _watchdogTween?.Kill();
            _watchdogTween = DOVirtual.DelayedCall(watchdogDuration, () =>
            {
                if (_isPlaying)
                {
                    SetFxVisible(false);
                    _isPlaying = false;
                }
            }).SetUpdate(true);

            Sequence firstLeg = DOTween.Sequence().SetUpdate(true);
            firstLeg.Join(_fxRoot.DOAnchorPos(middlePos, firstFly).SetEase(_moveEase));
            firstLeg.Join(_fxRoot.DOScale(Vector3.one * Mathf.Max(0.01f, _middleScale), firstFly).SetEase(Ease.OutBack));
            yield return firstLeg.WaitForCompletion();

            if (_shakeDuration > 0f)
            {
                yield return _fxRoot.DOShakeAnchorPos(
                        _shakeDuration,
                        _shakeStrength,
                        _shakeVibrato,
                        _shakeRandomness,
                        false,
                        true)
                    .SetUpdate(true)
                    .WaitForCompletion();
            }

            Sequence secondLeg = DOTween.Sequence().SetUpdate(true);
            secondLeg.Join(_fxRoot.DOAnchorPos(endPos, secondFly).SetEase(_moveEase));
            secondLeg.Join(_fxRoot.DOScale(Vector3.one * Mathf.Max(0.01f, _endScale), secondFly).SetEase(Ease.InQuad));
            secondLeg.Join(canvasGroup.DOFade(0f, secondFly * 0.75f).SetDelay(secondFly * 0.25f));
            yield return secondLeg.WaitForCompletion();

            SetFxVisible(false);
        }
        finally
        {
            _watchdogTween?.Kill();
            _isPlaying = false;

            if (onCompleted != null)
            {
                try
                {
                    onCompleted.Invoke();
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }

    private void SetFxVisible(bool isVisible)
    {
        if (_fxRoot == null)
        {
            return;
        }

        if (_canToggleFxRootActive)
        {
            _fxRoot.gameObject.SetActive(isVisible);
            return;
        }

        CanvasGroup canvasGroup = _fxRoot.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = _fxRoot.gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = isVisible ? 1f : 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void TryResolveReferences()
    {
        if (_fxRoot == null)
        {
            _fxRoot = transform as RectTransform;
        }

        _canToggleFxRootActive = _fxRoot != null && _fxRoot != transform;
    }

    private Vector2 ResolveAnchoredPosition(RectTransform target, Vector2 fallback)
    {
        RectTransform parentRect = _fxRoot != null ? _fxRoot.parent as RectTransform : null;
        if (target == null || parentRect == null)
        {
            return fallback;
        }

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(GetCanvasCamera(target), target.position);
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, GetCanvasCamera(parentRect), out localPoint))
        {
            return localPoint;
        }

        return fallback;
    }

    private Camera GetCanvasCamera(Component component)
    {
        Canvas canvas = component != null ? component.GetComponentInParent<Canvas>() : null;
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return canvas.worldCamera;
    }
}
