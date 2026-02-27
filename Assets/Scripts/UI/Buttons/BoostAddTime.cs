using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoostAddTime : MonoBehaviour
{
    [Header("Boost Data")]
    [SerializeField] private int _defaultAmount = 3;
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private AddTimeBoostFxPresenter _fxPresenter;

    private const string AddTimeCountKey = "Boost_AddTime30_Count";
    private const string AddTimeUnlimitedKey = "Boost_AddTime30_Unlimited";

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
        bool isUnlimitedNow = IsUnlimited();
        int currentCount = GetCount();
        if (!isUnlimitedNow && currentCount <= 0)
        {
            RefreshUI();
            return;
        }

        if (GameManager.Instance == null || !GameManager.Instance.CanUseBoostAddThirtySeconds())
        {
            RefreshUI();
            return;
        }

        if (_fxPresenter != null)
        {
            bool fxStarted = _fxPresenter.Play(() =>
            {
                bool usedWithFx = GameManager.Instance != null && GameManager.Instance.UseBoostAddThirtySeconds();
                if (usedWithFx && !IsUnlimited())
                {
                    SetCount(GetCount() - 1);
                }

                RefreshUI();
            });

            RefreshUI();
            if (!fxStarted)
            {
                return;
            }

            return;
        }

        bool usedSuccessfully = GameManager.Instance.UseBoostAddThirtySeconds();
        if (usedSuccessfully && !IsUnlimited())
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
        if (!PlayerPrefs.HasKey(AddTimeCountKey))
        {
            int initialCount = Mathf.Max(0, _defaultAmount);
            PlayerPrefs.SetInt(AddTimeCountKey, initialCount);
            PlayerPrefs.Save();
        }
    }

    private int GetCount()
    {
        EnsureCountInitialized();
        return Mathf.Max(0, PlayerPrefs.GetInt(AddTimeCountKey, _defaultAmount));
    }

    private void SetCount(int value)
    {
        PlayerPrefs.SetInt(AddTimeCountKey, Mathf.Max(0, value));
        PlayerPrefs.Save();
    }

    private bool IsUnlimited()
    {
        return PlayerPrefs.GetInt(AddTimeUnlimitedKey, 0) == 1;
    }

    private void SetUnlimited(bool value)
    {
        PlayerPrefs.SetInt(AddTimeUnlimitedKey, value ? 1 : 0);
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
            bool fxBusy = _fxPresenter != null && _fxPresenter.IsPlaying;
            _button.interactable = (isUnlimited || count > 0) && !fxBusy;
        }
    }
}
