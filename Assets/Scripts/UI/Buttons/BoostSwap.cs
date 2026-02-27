using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoostSwap : MonoBehaviour
{
    [Header("Boost Data")]
    [SerializeField] private int _defaultAmount = 2;
    [SerializeField] private TextMeshProUGUI _countText;

    private const string SwapForMergeCountKey = "Boost_SwapForMerge_Count";
    private const string SwapForMergeUnlimitedKey = "Boost_SwapForMerge_Unlimited";

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_countText == null)
        {
            _countText = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        EnsureCountInitialized();
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    public void OnUseBoost()
    {
        bool isUnlimited = IsUnlimited();
        int currentCount = GetCount();
        if (!isUnlimited && currentCount <= 0)
        {
            RefreshUI();
            return;
        }

        bool usedSuccessfully = GameManager.Instance != null && GameManager.Instance.UseBoostSwapForMerge();
        if (usedSuccessfully && !isUnlimited)
        {
            SetCount(currentCount - 1);
        }

        RefreshUI();
    }

    public void ResetBoostToDefault()
    {
        SetUnlimited(false);
        SetCount(_defaultAmount);
        RefreshUI();
    }

    public void EnableUnlimitedUse()
    {
        SetUnlimited(true);
        RefreshUI();
    }

    private void EnsureCountInitialized()
    {
        if (!PlayerPrefs.HasKey(SwapForMergeCountKey))
        {
            int initialCount = Mathf.Max(0, _defaultAmount);
            PlayerPrefs.SetInt(SwapForMergeCountKey, initialCount);
            PlayerPrefs.Save();
        }
    }

    private int GetCount()
    {
        EnsureCountInitialized();
        return Mathf.Max(0, PlayerPrefs.GetInt(SwapForMergeCountKey, _defaultAmount));
    }

    private void SetCount(int value)
    {
        PlayerPrefs.SetInt(SwapForMergeCountKey, Mathf.Max(0, value));
        PlayerPrefs.Save();
    }

    private bool IsUnlimited()
    {
        return PlayerPrefs.GetInt(SwapForMergeUnlimitedKey, 0) == 1;
    }

    private void SetUnlimited(bool value)
    {
        PlayerPrefs.SetInt(SwapForMergeUnlimitedKey, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void RefreshUI()
    {
        bool isUnlimited = IsUnlimited();
        int count = GetCount();

        if (_countText != null)
        {
            _countText.text = isUnlimited ? "N" : count.ToString();
        }

        if (_button != null)
        {
            _button.interactable = isUnlimited || count > 0;
        }
    }
}
