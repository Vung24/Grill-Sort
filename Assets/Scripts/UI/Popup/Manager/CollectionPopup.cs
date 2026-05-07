using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BoostType
{
    Remove,
    Swap,
    AddTime
}

public class CollectionPopup : MonoBehaviour
{
    [SerializeField] private Image _collectionImage;
    [SerializeField] private ButtonEffectLogic _claimButton;
    
    private BoostType _currentBoostType;

    private void Start()
    {
        if (_claimButton != null)
        {
            _claimButton.onClick.AddListener(OnClaimButtonClicked);
        }
    }

    public void ShowPopup(Sprite itemSprite, BoostType boostType)
    {
        _currentBoostType = boostType;
        if (_collectionImage != null)
        {
            _collectionImage.sprite = itemSprite;
        }
        gameObject.SetActive(true);
    }

    private void OnClaimButtonClicked()
    {
        switch (_currentBoostType)
        {
            case BoostType.Remove:
                int removeCount = PlayerPrefs.GetInt("Boost_RemoveThree_Count", 3);
                PlayerPrefs.SetInt("Boost_RemoveThree_Count", removeCount + 1);
                break;
            case BoostType.Swap:
                int swapCount = PlayerPrefs.GetInt("Boost_SwapForMerge_Count", 2);
                PlayerPrefs.SetInt("Boost_SwapForMerge_Count", swapCount + 1);
                break;
            case BoostType.AddTime:
                int timeCount = PlayerPrefs.GetInt("Boost_AddTime30_Count", 1);
                PlayerPrefs.SetInt("Boost_AddTime30_Count", timeCount + 1);
                break;
        }
        
        PlayerPrefs.Save();
        gameObject.SetActive(false);
    }
}
