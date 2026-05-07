using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoostManager : MonoBehaviour
{
    public static BoostManager Instance { get; private set; }

    [Header("Boost Buttons")]
    [SerializeField] private BoostRemove _boostRemove;
    [SerializeField] private BoostSwap _boostSwap;
    [SerializeField] private BoostAddTime _boostAddTime;

    [Header("Boost Gameplay")]
    [SerializeField] private BoostFxController _boostFxController;
    [SerializeField] private BoostGameplayController _boostGameplayController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CacheBoostReferences();
        CacheBoostGameplayReferences();
        BindBoostButtons();
    }
    private void Start()
    {
        _boostRemove.ResetBoostToDefault();
        _boostSwap.ResetBoostToDefault();
        _boostAddTime.ResetBoostToDefault();
        // // _boostRemove.EnableUnlimitedUse();
        // // _boostSwap.EnableUnlimitedUse();
        // _boostAddTime?.EnableUnlimitedUse();
    }
    private void OnDestroy()
    {
        UnbindBoostButtons();
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static BoostManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        BoostManager boostManager = GameManager.Instance._boostManager;
        if (boostManager != null)
        {
            Instance = boostManager;
            return Instance;
        }

        GameObject host = new GameObject("BoostManager");
        Instance = host.AddComponent<BoostManager>();
        return Instance;
    }

    public bool UseRemoveThree()
    {
        return _boostGameplayController != null && _boostGameplayController.UseBoostRemoveThree();
    }

    public bool UseSwapForMerge()
    {
        return _boostGameplayController != null && _boostGameplayController.UseBoostSwapForMerge();
    }

    public bool CanUseAddThirtySeconds()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsLevelComplete)
        {
            return false;
        }

        return TimeManager.Instance != null && TimeManager.Instance.CanAddTime();
    }

    public bool UseAddThirtySeconds()
    {
        if (!CanUseAddThirtySeconds())
        {
            return false;
        }

        TimeManager.Instance.AddTime(30f);
        GameManager.Instance.ShowHint();
        return true;
    }

    public void InitializeGameplay(
        GameManager gameManager,
        List<GrillStation> grillStations,
        GameHintSystem hintSystem)
    {
        if (_boostGameplayController == null)
        {
            CacheBoostGameplayReferences();
        }

        if (_boostGameplayController != null)
        {
            _boostGameplayController.Initialize(gameManager, grillStations, hintSystem, _boostFxController);
        }
    }

    private void CacheBoostReferences()
    {
        if (_boostRemove == null)
        {
            _boostRemove = GetComponentInChildren<BoostRemove>(true);
        }

        if (_boostSwap == null)
        {
            _boostSwap = GetComponentInChildren<BoostSwap>(true);
        }

        if (_boostAddTime == null)
        {
            _boostAddTime = GetComponentInChildren<BoostAddTime>(true);
        }
    }

    private void CacheBoostGameplayReferences()
    {
        if (_boostFxController == null)
        {
            _boostFxController = GetComponent<BoostFxController>();
        }

        if (_boostFxController == null)
        {
            _boostFxController = gameObject.AddComponent<BoostFxController>();
        }

        if (_boostGameplayController == null)
        {
            _boostGameplayController = GetComponent<BoostGameplayController>();
        }

        if (_boostGameplayController == null)
        {
            _boostGameplayController = gameObject.AddComponent<BoostGameplayController>();
        }
    }

    private void BindBoostButtons()
    {
        BindButton(_boostRemove, _boostRemove != null ? _boostRemove.OnUseBoost : null);
        BindButton(_boostSwap, _boostSwap != null ? _boostSwap.OnUseBoost : null);
        BindButton(_boostAddTime, _boostAddTime != null ? _boostAddTime.OnUseBoost : null);
    }

    private void UnbindBoostButtons()
    {
        UnbindButton(_boostRemove, _boostRemove != null ? _boostRemove.OnUseBoost : null);
        UnbindButton(_boostSwap, _boostSwap != null ? _boostSwap.OnUseBoost : null);
        UnbindButton(_boostAddTime, _boostAddTime != null ? _boostAddTime.OnUseBoost : null);
    }

    private static void BindButton(MonoBehaviour boost, UnityEngine.Events.UnityAction handler)
    {
        if (boost == null || handler == null)
        {
            return;
        }

        Button button = boost.GetComponent<Button>();
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(handler);
        button.onClick.AddListener(handler);
    }

    private static void UnbindButton(MonoBehaviour boost, UnityEngine.Events.UnityAction handler)
    {
        if (boost == null || handler == null)
        {
            return;
        }

        Button button = boost.GetComponent<Button>();
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(handler);
    }
}
