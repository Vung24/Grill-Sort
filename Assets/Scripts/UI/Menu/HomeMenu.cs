using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HomeMenu : MonoBehaviour
{
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject coin;

    [Header("Tab Buttons")]
    [SerializeField] private ButtonEffectLogic homeButton;
    [SerializeField] private ButtonEffectLogic shopButton;

    [Header("Slider")]
    [SerializeField] private RectTransform slide;
    [SerializeField] private int defaultIndex = 0;
    [SerializeField] private bool resizeWithButton = true;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float moveDuration = 0.25f;
    [SerializeField] private float scaleDuration = 0.2f;
    [SerializeField] private string visualRootName = "Render";

    private ButtonEffectLogic[] _buttons;
    private GameObject[] _panels;
    private UnityAction[] _buttonCallbacks;
    private Transform[] _scaleTargets;
    private Vector3[] _baseScales;
    private int _currentIndex;
    private Tween _moveTween;

    private void Awake()
    {
        _buttons = new[] { homeButton, shopButton };
        _panels = new[] { homePanel, shopPanel };

        _buttonCallbacks = new UnityAction[_buttons.Length];
        _scaleTargets = new Transform[_buttons.Length];
        _baseScales = new Vector3[_buttons.Length];

        for (int i = 0; i < _buttons.Length; i++)
        {
            _scaleTargets[i] = ResolveScaleTarget(_buttons[i]);
            _baseScales[i] = _scaleTargets[i] != null ? _scaleTargets[i].localScale : Vector3.one;
        }
    }

    private void OnEnable()
    {
        BindButtons();
        Select(GetInitialIndex(), true);
    }

    private void OnDisable()
    {
        UnbindButtons();
        _moveTween?.Kill();

        for (int i = 0; i < _buttons.Length; i++)
        {
            Transform scaleTarget = GetScaleTarget(i);
            if (scaleTarget == null) continue;

            scaleTarget.DOKill();
            if (_baseScales != null && i < _baseScales.Length)
            {
                scaleTarget.localScale = _baseScales[i];
            }
        }
    }

    public void OnClickHome() => Select(0);
    public void OnClickShop() => Select(1);

    public void Select(int index, bool instant = false)
    {
        if (slide == null || _buttons == null || _buttons.Length == 0)
        {
            return;
        }

        _currentIndex = Mathf.Clamp(index, 0, _buttons.Length - 1);
        RectTransform targetRect = GetTargetRect(_currentIndex);
        if (targetRect == null)
        {
            return;
        }

        for (int i = 0; i < _panels.Length; i++)
        {
            if (_panels[i] == null) continue;
            _panels[i].SetActive(i == _currentIndex);
        }

        // Active Gameobject Coin thông qua hàm ShowCoin()
        if (CoinController.Instance != null)
        {
            CoinController.Instance.ShowCoin();
        }
        else if (coin != null)
        {
            coin.SetActive(true);
        }

        if (resizeWithButton)
        {
            slide.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetRect.rect.width);
        }

        float targetX = targetRect.anchoredPosition.x;
        _moveTween?.Kill();

        if (instant)
        {
            slide.anchoredPosition = new Vector2(targetX, slide.anchoredPosition.y);
        }
        else
        {
            _moveTween = slide.DOAnchorPosX(targetX, moveDuration).SetEase(Ease.OutCubic);
        }

        UpdateButtonScales(instant);
    }

    private void UpdateButtonScales(bool instant)
    {
        if (_buttons == null || _baseScales == null) return;

        for (int i = 0; i < _buttons.Length; i++)
        {
            Transform scaleTarget = GetScaleTarget(i);
            if (scaleTarget == null || i >= _baseScales.Length) continue;

            scaleTarget.DOKill();

            bool isSelected = i == _currentIndex;
            Vector3 targetScale = isSelected ? _baseScales[i] * selectedScale : _baseScales[i];

            if (instant)
            {
                scaleTarget.localScale = targetScale;
            }
            else
            {
                scaleTarget.DOScale(targetScale, scaleDuration).SetEase(Ease.OutCubic);
            }
        }
    }

    private int GetInitialIndex()
    {
        if (shopPanel != null && shopPanel.activeSelf)
        {
            return 1;
        }

        return Mathf.Clamp(defaultIndex, 0, _buttons.Length - 1);
    }

    private Transform GetScaleTarget(int index)
    {
        if (_scaleTargets == null || index < 0 || index >= _scaleTargets.Length)
        {
            return null;
        }

        return _scaleTargets[index];
    }

    private Transform ResolveScaleTarget(Button button)
    {
        if (button == null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(visualRootName))
        {
            Transform namedChild = button.transform.Find(visualRootName);
            if (namedChild != null)
            {
                return namedChild;
            }
        }

        if (button.transform.childCount > 0)
        {
            return button.transform.GetChild(0);
        }

        return button.transform;
    }

    private RectTransform GetTargetRect(int index)
    {
        if (_buttons == null || index < 0 || index >= _buttons.Length)
        {
            return null;
        }

        Button button = _buttons[index];
        if (button == null)
        {
            return null;
        }

        return button.transform as RectTransform;
    }

    private void BindButtons()
    {
        if (_buttons == null) return;

        for (int i = 0; i < _buttons.Length; i++)
        {
            if (_buttons[i] == null) continue;

            int index = i;
            _buttonCallbacks[i] = () => Select(index);
            _buttons[i].onClick.AddListener(_buttonCallbacks[i]);
        }
    }

    private void UnbindButtons()
    {
        if (_buttons == null || _buttonCallbacks == null) return;

        for (int i = 0; i < _buttons.Length; i++)
        {
            if (_buttons[i] == null || _buttonCallbacks[i] == null) continue;
            _buttons[i].onClick.RemoveListener(_buttonCallbacks[i]);
        }
    }
}