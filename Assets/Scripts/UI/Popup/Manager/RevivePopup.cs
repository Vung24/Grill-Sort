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
    [SerializeField] private int _timeUpShuffleCoinCost = 350;
    [SerializeField] private TextMeshProUGUI _coinCostText;

    [Header("Header UI")]
    [SerializeField] private TextMeshProUGUI _headerText;
    [SerializeField] private string _outOfSlotHeaderText = "Out Of Slot!";
    [SerializeField] private string _timeUpHeaderText = "Time's up!";
    [SerializeField] private Image _headerIcon;
    [SerializeField] private Sprite _outOfSlotHeaderIcon;
    [SerializeField] private Sprite _clockHeaderIcon;
    [SerializeField] private Vector3 _outOfSlotIconScale = Vector3.one;

    [Header("Boost Text UI")]
    [SerializeField] private TextMeshProUGUI _coinBoostText;
    [SerializeField] private string _outOfSlotCoinBoostText = "Swap";
    [SerializeField] private string _timeUpCoinBoostText = "Time";

    [Header("Coin UI")]
    [SerializeField] private GameObject _coinAmountRoot;

    private Vector3 _timeUpIconScale = new Vector3(0.45f, 0.35f, 1f);
    private int _currentShuffleCoinCost;

    private void Awake()
    {
        _currentShuffleCoinCost = _shuffleCoinCost;
        RefreshCoinCostText();
    }
    void Start()
    {
        _coinShuffleButton.onClick.AddListener(OnClickBuyShuffleByCoin);
        _freeShuffleButton.onClick.AddListener(OnClickFreeShuffle);
        _closeButton.onClick.AddListener(OnClickClose);
    }
    private void OnEnable()
    {
        RefreshHeaderAndCostByLoseReason();
        if (_coinAmountRoot != null)
        {
            _coinAmountRoot.SetActive(true);
        }
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

        bool revived = GameManager.Instance.TryReviveWithSwapByCoin(_currentShuffleCoinCost);
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
            && CoinManager.Instance.CanAfford(_currentShuffleCoinCost);

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

    private void RefreshHeaderAndCostByLoseReason()
    {
        bool isTimeUp = GameManager.Instance != null
            && GameManager.Instance.CurrentLoseReason == EnumManager.LoseReason.TimeUp;

        _currentShuffleCoinCost = isTimeUp ? _timeUpShuffleCoinCost : _shuffleCoinCost;
        RefreshCoinCostText();

        if (_headerText != null)
        {
            _headerText.text = isTimeUp ? _timeUpHeaderText : _outOfSlotHeaderText;
        }

        if (_headerIcon != null)
        {
            if (isTimeUp)
            {
                _headerIcon.sprite = _clockHeaderIcon;
                _headerIcon.rectTransform.localScale = _timeUpIconScale;
            }
            else
            {
                _headerIcon.sprite = _outOfSlotHeaderIcon;
                // _headerIcon.rectTransform.localScale = _outOfSlotIconScale;
            }
        }

        if (_coinBoostText != null)
        {
            _coinBoostText.text = isTimeUp ? _timeUpCoinBoostText : _outOfSlotCoinBoostText;
        }
    }

    private void RefreshCoinCostText()
    {
        if (_coinCostText != null)
        {
            _coinCostText.text = _currentShuffleCoinCost.ToString();
        }
    }
}
