using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RevivePopup : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private ButtonEffectLogic _coinShuffleButton;
    [SerializeField] private ButtonEffectLogic _freeShuffleButton;
    [SerializeField] private ButtonEffectLogic _closeButton;

    [Header("Coin Price")]
    [SerializeField] private int _shuffleCoinCost = 450;
    [SerializeField] private TextMeshProUGUI _coinCostText;

    private void Awake()
    {
        if (_coinCostText != null)
        {
            _coinCostText.text = _shuffleCoinCost.ToString();
        }
    }
    void Start()
    {
        _coinShuffleButton.onClick.AddListener(OnClickBuyShuffleByCoin);
        _freeShuffleButton.onClick.AddListener(OnClickFreeShuffle);
        _closeButton.onClick.AddListener(OnClickClose);
    }
    private void OnEnable()
    {
        CoinManager.EnsureInstance();
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.OnCoinChanged += OnCoinChanged;
        }

        RefreshInteractable();
    }

    private void OnDisable()
    {
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.OnCoinChanged -= OnCoinChanged;
        }
    }

    public void OnClickBuyShuffleByCoin()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        bool revived = GameManager.Instance.TryReviveWithSwapByCoin(_shuffleCoinCost);
        if (!revived)
        {
            RefreshInteractable();
        }
    }

    public void OnClickFreeShuffle()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        bool revived = GameManager.Instance.TryReviveWithSwapFree();
        if (!revived)
        {
            RefreshInteractable();
        }
    }

    public void OnClickClose()
    {
        GameManager.Instance?.CloseRevivePanelAndShowLose();
    }

    private void OnCoinChanged(int _)
    {
        RefreshInteractable();
    }

    private void RefreshInteractable()
    {
        bool canRevive = GameManager.Instance != null
            && GameManager.Instance.CurrentLevelState == EnumManager.LevelState.RevivePanel;
        bool canAfford = CoinManager.Instance != null
            && CoinManager.Instance.CanAfford(_shuffleCoinCost);

        if (_coinShuffleButton != null)
        {
            _coinShuffleButton.interactable = canRevive && canAfford;
        }

        if (_freeShuffleButton != null)
        {
            _freeShuffleButton.interactable = canRevive;
        }

        if (_closeButton != null)
        {
            _closeButton.interactable = canRevive;
        }
    }
}
