using TMPro;
using UnityEngine;

public class CoinController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _coinText;

    private void Awake()
    {
        if (_coinText == null)
        {
            _coinText = GetComponentInChildren<TextMeshProUGUI>(true);
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
}
