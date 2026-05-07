using UnityEngine;

public class ShopController : MonoBehaviour
{
    [Header("Boost Costs")]
    [SerializeField] private int _removeBoostCost = 300;
    [SerializeField] private int _swapBoostCost = 450;
    [SerializeField] private int _addTimeBoostCost = 350;

    public bool BuyRemoveBoost()
    {
        if (CoinManager.Instance == null)
        {
            Debug.LogWarning("CoinManager instance not found!");
            return false;
        }

        if (CoinManager.Instance.SpendCoins(_removeBoostCost))
        {
            Debug.Log("Bought Remove Boost. Remaining coins: " + CoinManager.Instance.CurrentCoins);
            return true;
        }
        else
        {
            Debug.LogWarning("Not enough coins for Remove Boost!");
            if (CoinController.Instance != null) CoinController.Instance.ShakeCoinText();
            return false;
        }
    }

    public bool BuySwapBoost()
    {
        if (CoinManager.Instance == null)
        {
            Debug.LogWarning("CoinManager instance not found!");
            return false;
        }

        if (CoinManager.Instance.SpendCoins(_swapBoostCost))
        {
            Debug.Log("Bought Swap Boost. Remaining coins: " + CoinManager.Instance.CurrentCoins);
            return true;
        }
        else
        {
            Debug.LogWarning("Not enough coins for Swap Boost!");
            if (CoinController.Instance != null) CoinController.Instance.ShakeCoinText();
            return false;
        }
    }

    public bool BuyAddTimeBoost()
    {
        if (CoinManager.Instance == null)
        {
            Debug.LogWarning("CoinManager instance not found!");
            return false;
        }

        if (CoinManager.Instance.SpendCoins(_addTimeBoostCost))
        {
            Debug.Log("Bought Add Time Boost. Remaining coins: " + CoinManager.Instance.CurrentCoins);
            return true;
        }
        else
        {
            Debug.LogWarning("Not enough coins for Add Time Boost!");
            if (CoinController.Instance != null) CoinController.Instance.ShakeCoinText();
            return false;
        }
    }
}