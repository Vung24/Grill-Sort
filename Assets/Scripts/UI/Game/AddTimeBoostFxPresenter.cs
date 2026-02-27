using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class AddTimeBoostFxPresenter : MonoBehaviour
{
    [Header("FX")]
    [SerializeField] private RectTransform _fxRoot;
    [SerializeField] private RectTransform _startPoint;
    [SerializeField] private RectTransform _middlePoint;
    [SerializeField] private RectTransform _endPoint;
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private string _text = "+30s";
    [SerializeField] private float _popupDuration = 0.25f;
    [SerializeField] private float _flyDuration = 1.5f;
    [SerializeField] private float _firstFlyDuration = 0.45f;
    [SerializeField] private float _middleHoldDuration = 0.5f;
    [SerializeField] private float _secondFlyDuration = 0.45f;
    [SerializeField] private float _startScale = 0.65f;
    [SerializeField] private float _middleScale = 1.8f;
    [SerializeField] private float _endScale = 0.65f;
    [SerializeField] private Ease _moveEase = Ease.InOutCubic;
    [SerializeField] private bool _pauseTimerWhilePlaying = true;

    private bool _isPlaying;
    private bool _canToggleFxRootActive;
    private GameManager _pausedGameManager;
    private bool _restoreTimerPause;
    private bool _oldTimerPause;
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
        RestoreTimerPauseIfNeeded();

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

            GameManager gm = GameManager.Instance;
            if (_pauseTimerWhilePlaying && gm != null)
            {
                _oldTimerPause = gm.IsTimerPaused;
                gm.SetTimerPaused(true);
                _pausedGameManager = gm;
                _restoreTimerPause = true;
            }

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

            if (_label != null)
            {
                _label.text = _text;
            }
            else
            {
                Debug.LogWarning("AddTimeBoostFxPresenter: _label is not assigned and no TMP child was found.");
            }

            Vector2 startPos = ResolveAnchoredPosition(_startPoint, Vector2.zero);
            Vector2 middleFallback = Vector2.Lerp(startPos, ResolveAnchoredPosition(_endPoint, startPos), 0.5f);
            Vector2 middlePos = ResolveAnchoredPosition(_middlePoint, middleFallback);
            Vector2 endPos = ResolveAnchoredPosition(_endPoint, startPos);
            _fxRoot.anchoredPosition = startPos;

            if (_popupDuration > 0f)
            {
                yield return new WaitForSeconds(_popupDuration);
            }

            float totalFly = Mathf.Max(0.01f, _flyDuration);
            float firstFly = _firstFlyDuration > 0f ? _firstFlyDuration : totalFly * 0.5f;
            float secondFly = _secondFlyDuration > 0f ? _secondFlyDuration : totalFly * 0.5f;
            firstFly = Mathf.Max(0.01f, firstFly);
            secondFly = Mathf.Max(0.01f, secondFly);

            Sequence firstLeg = DOTween.Sequence();
            firstLeg.Join(_fxRoot.DOAnchorPos(middlePos, firstFly).SetEase(_moveEase));
            firstLeg.Join(_fxRoot.DOScale(Vector3.one * Mathf.Max(0.01f, _middleScale), firstFly).SetEase(Ease.OutBack));
            yield return firstLeg.WaitForCompletion();

            if (_middleHoldDuration > 0f)
            {
                yield return new WaitForSeconds(_middleHoldDuration);
            }

            Sequence secondLeg = DOTween.Sequence();
            secondLeg.Join(_fxRoot.DOAnchorPos(endPos, secondFly).SetEase(_moveEase));
            secondLeg.Join(_fxRoot.DOScale(Vector3.one * Mathf.Max(0.01f, _endScale), secondFly).SetEase(Ease.InQuad));
            secondLeg.Join(canvasGroup.DOFade(0f, secondFly * 0.75f).SetDelay(secondFly * 0.25f));
            yield return secondLeg.WaitForCompletion();

            SetFxVisible(false);

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
        finally
        {
            RestoreTimerPauseIfNeeded();
            _isPlaying = false;
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

        if (_label == null && _fxRoot != null)
        {
            _label = _fxRoot.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        _canToggleFxRootActive = _fxRoot != null && _fxRoot != transform;
    }

    private void RestoreTimerPauseIfNeeded()
    {
        if (_restoreTimerPause && _pausedGameManager != null)
        {
            _pausedGameManager.SetTimerPaused(_oldTimerPause);
        }

        _restoreTimerPause = false;
        _pausedGameManager = null;
        _oldTimerPause = false;
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
