using TMPro;
using UnityEngine;

public class RewardPopup : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private ButtonEffectLogic _claimButton;
    [SerializeField] private ButtonEffectLogic _claimX2Button;
    [Header("Reward UI")]
    [SerializeField] private int _baseRewardCoins = 50;
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;

    void Awake()
    {
        // RefreshRewardTexts();
    }
    void Start()
    {
       _claimButton.onClick.AddListener(OnClickClaim);
       _claimX2Button.onClick.AddListener(OnClickClaimX2); 
    }
    private void OnEnable()
    {
        // RefreshRewardTexts();
        RefreshInteractable();
    }

    public void OnClickClaim()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        bool claimed = GameManager.Instance.TryClaimWinReward();
        if (!claimed)
        {
            RefreshInteractable();
        }
    }

    public void OnClickClaimX2()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        bool claimed = GameManager.Instance.TryClaimWinRewardX2WithAd();
        if (!claimed)
        {
            RefreshInteractable();
        }
    }

    private void RefreshInteractable()
    {
        bool canClaim = GameManager.Instance != null
            && GameManager.Instance.CurrentLevelState == EnumManager.LevelState.RewardPanel;

        if (_claimButton != null)
        {
            _claimButton.interactable = canClaim;
        }

        if (_claimX2Button != null)
        {
            _claimX2Button.interactable = canClaim;
        }
    }
}
