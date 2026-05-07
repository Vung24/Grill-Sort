using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private ShopController _shopController;
    [SerializeField] private ButtonEffectLogic _removeBoostButton;
    [SerializeField] private ButtonEffectLogic _swapBoostButton;
    [SerializeField] private ButtonEffectLogic _addTimeBoostButton;

    [Header("Collection Popup")]
    [SerializeField] private CollectionPopup _collectionPopup;
    [SerializeField] private Sprite _removeBoostSprite;
    [SerializeField] private Sprite _swapBoostSprite;
    [SerializeField] private Sprite _addTimeBoostSprite;

    void Start()
    {
        _removeBoostButton.onClick.AddListener(OnClickBuyRemoveBoost);
        _swapBoostButton.onClick.AddListener(OnClickBuySwapBoost);
        _addTimeBoostButton.onClick.AddListener(OnClickBuyAddTimeBoost);

        if (_collectionPopup != null)
        {
            _collectionPopup.gameObject.SetActive(false);
        }
    }
    public void OnClickBuyRemoveBoost()
    {
        if (_shopController != null)
        {
            if (_shopController.BuyRemoveBoost())
            {
                ShowCollectionPopup(_removeBoostSprite, BoostType.Remove);
            }
        }
        else
        {
            Debug.LogWarning("ShopController is not set in ShopManager.");
        }
    }

    public void OnClickBuySwapBoost()
    {
        if (_shopController != null)
        {
            if (_shopController.BuySwapBoost())
            {
                ShowCollectionPopup(_swapBoostSprite, BoostType.Swap);
            }
        }
        else
        {
            Debug.LogWarning("ShopController is not set in ShopManager.");
        }
    }

    public void OnClickBuyAddTimeBoost()
    {
        if (_shopController != null)
        {
            if (_shopController.BuyAddTimeBoost())
            {
                ShowCollectionPopup(_addTimeBoostSprite, BoostType.AddTime);
            }
        }
        else
        {
            Debug.LogWarning("ShopController is not set in ShopManager.");
        }
    }

    private void ShowCollectionPopup(Sprite boostSprite, BoostType boostType)
    {
        if (_collectionPopup != null)
        {
            _collectionPopup.ShowPopup(boostSprite, boostType);
        }
        else
        {
            Debug.LogWarning("CollectionPopup is not assigned in ShopManager!");
        }
    }
}