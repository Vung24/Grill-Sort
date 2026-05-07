using TMPro;
using UnityEngine;
using DG.Tweening;

public class CoinController : MonoBehaviour
{
    public static CoinController Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _coinText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (_coinText == null)
        {
            _coinText = GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnEnable()
    {
        CoinManager.EnsureInstance();
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.OnCoinChanged += OnCoinChanged;
            Refresh();
        }
    }

    private void OnDisable()
    {
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.OnCoinChanged -= OnCoinChanged;
        }
    }

    private void OnCoinChanged(int _)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (_coinText == null || CoinManager.Instance == null)
        {
            return;
        }

        _coinText.text = CoinManager.Instance.CurrentCoins.ToString();
    }

    public void ShowCoin()
    {
        gameObject.SetActive(true);
    }

    public void HideCoin()
    {
        gameObject.SetActive(false);
    }

    public void ShakeCoinText()
    {
        if (_coinText != null)
        {
            _coinText.transform.DOComplete();
            _coinText.transform.DOShakePosition(0.4f, new Vector3(15f, 0, 0), 10, 90f, false, true);
        }
    }
}
