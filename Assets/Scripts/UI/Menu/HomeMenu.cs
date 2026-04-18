using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HomeMenu : MonoBehaviour
{
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject shopPanel;

    [Header("Tab Buttons")]
    [SerializeField] private Button homeButton;
    [SerializeField] private Button shopButton;

    [Header("Selection Fx")]
    [SerializeField] private float scaleDuration = 0.16f;
    [SerializeField] private float selectedTargetY = 100f;  // ← Absolute Y khi selected
    [SerializeField] private float selectedScale = 1.1f;
    [SerializeField] private float unselectedScale = 1f;
    [SerializeField] private float unselectedAlpha = 0.6f;

    private enum TabType { Home, Shop }

    private TabType _currentTab = TabType.Home;
    private RectTransform _homeRect;
    private RectTransform _shopRect;
    private Vector2 _homeBasePos;
    private Vector2 _shopBasePos;
    private Vector3 _homeBaseLocalPos;
    private Vector3 _shopBaseLocalPos;

    private CanvasGroup _homeCanvasGroup;
    private CanvasGroup _shopCanvasGroup;

    // --- FIX: flag tránh race condition OnEnable vs Start ---
    private bool _initialized;
    private TabType _pendingTab;

    private void Awake()
    {
        _homeRect = homeButton != null ? homeButton.GetComponent<RectTransform>() : null;
        _shopRect = shopButton != null ? shopButton.GetComponent<RectTransform>() : null;

        _homeCanvasGroup = GetOrCreateCanvasGroup(homeButton);
        _shopCanvasGroup = GetOrCreateCanvasGroup(shopButton);
    }

    private CanvasGroup GetOrCreateCanvasGroup(Button btn)
    {
        if (btn == null) return null;
        CanvasGroup cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
        return cg;
    }

    private IEnumerator Start()
    {
        // Chờ Canvas layout xong hoàn toàn mới đọc rect.size
        yield return new WaitForEndOfFrame();

        if (_homeRect != null)
        {
            _homeBasePos = _homeRect.anchoredPosition;
            _homeBaseLocalPos = _homeRect.localPosition;
        }

        if (_shopRect != null)
        {
            _shopBasePos = _shopRect.anchoredPosition;
            _shopBaseLocalPos = _shopRect.localPosition;
        }

        _initialized = true;
        SetTab(_pendingTab, false);
    }

    private void OnEnable()
    {
        _pendingTab = (shopPanel != null && shopPanel.activeSelf) ? TabType.Shop : TabType.Home;

        if (!_initialized) return; 

        SetTab(_pendingTab, false);
    }

    private void OnDisable()
    {
        KillTweens();
    }

    public void OnClickHome() => SetTab(TabType.Home, true);
    public void OnClickShop() => SetTab(TabType.Shop, true);

    private void SetTab(TabType tab, bool animate)
    {
        _currentTab = tab;
        bool isHome = tab == TabType.Home;

        if (homePanel != null) homePanel.SetActive(isHome);
        if (shopPanel != null) shopPanel.SetActive(!isHome);

        RectTransform selectedRect = isHome ? _homeRect : _shopRect;
        RectTransform unselectedRect = isHome ? _shopRect : _homeRect;
        CanvasGroup selectedCard = isHome ? _homeCanvasGroup : _shopCanvasGroup;
        CanvasGroup unselectedCard = isHome ? _shopCanvasGroup : _homeCanvasGroup;

        if (selectedRect == null) return;

        KillTweens();

        if (_homeRect != null) _homeRect.localPosition = _homeBaseLocalPos;
        if (_shopRect != null) _shopRect.localPosition = _shopBaseLocalPos;

        selectedRect.localScale = Vector3.one * unselectedScale;
        if (unselectedRect != null) unselectedRect.localScale = Vector3.one * unselectedScale;

        if (unselectedCard != null) unselectedCard.alpha = unselectedAlpha;

        Vector2 selectedBasePos = isHome ? _homeBasePos : _shopBasePos;
        Vector3 selectedBaseLocalPos = isHome ? _homeBaseLocalPos : _shopBaseLocalPos;
        Vector3 selectedLiftedLocalPos = new Vector3(
            selectedBaseLocalPos.x,
            selectedTargetY,      
            selectedBaseLocalPos.z
        );

        if (!animate)
        {
            selectedRect.localScale = Vector3.one * selectedScale;
            selectedRect.localPosition = selectedLiftedLocalPos;
            if (selectedCard != null) selectedCard.alpha = 1f;
            return;
        }

        Sequence selectedSeq = DOTween.Sequence();
        selectedSeq.Append(selectedRect.DOScale(Vector3.one * selectedScale, scaleDuration).SetEase(Ease.OutBack));
        selectedSeq.Join(selectedRect.DOLocalMove(selectedLiftedLocalPos, scaleDuration).SetEase(Ease.OutBack));
        if (selectedCard != null) selectedSeq.Join(selectedCard.DOFade(1f, scaleDuration).SetEase(Ease.InOutSine));
    }

    private void KillTweens()
    {
        _homeRect?.DOKill();
        _shopRect?.DOKill();
        _homeCanvasGroup?.DOKill();
        _shopCanvasGroup?.DOKill();
    }
}