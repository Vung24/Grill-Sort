using System;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [SerializeField] private int _defaultCoins = 0;

    private const string CoinKey = "Currency_Coin";

    private int _coins;

    public event Action<int> OnCoinChanged;

    public static CoinManager EnsureInstance()
    {
        CoinManager existing = FindObjectOfType<CoinManager>();
        if (existing != null)
        {
            Instance = existing;
            return existing;
        }

        GameObject go = new GameObject("CoinManager");
        return go.AddComponent<CoinManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public int CurrentCoins => _coins;

    public bool CanAfford(int amount)
    {
        return amount <= 0 || _coins >= amount;
    }

    public void SetCoins(int amount)
    {
        _coins = Mathf.Max(0, amount);
        Save();
        OnCoinChanged?.Invoke(_coins);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _coins += amount;
        Save();
        OnCoinChanged?.Invoke(_coins);
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (_coins < amount)
        {
            return false;
        }

        _coins -= amount;
        Save();
        OnCoinChanged?.Invoke(_coins);
        return true;
    }

    private void Load()
    {
        _coins = Mathf.Max(0, PlayerPrefs.GetInt(CoinKey, _defaultCoins));
        OnCoinChanged?.Invoke(_coins);
    }

    private void Save()
    {
        PlayerPrefs.SetInt(CoinKey, Mathf.Max(0, _coins));
        PlayerPrefs.Save();
    }
}
