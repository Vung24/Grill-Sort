using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GameHintBoost : MonoBehaviour
{
    [SerializeField] private float _repeatHintCooldown = 2f;
    [SerializeField] private float _hintScale = 0.16f;

    private const string RemoveThreeCountKey = "Boost_RemoveThree_Count";
    private const string RemoveThreeUnlimitedKey = "Boost_RemoveThree_Unlimited";

    private GameplayConditionController _conditionController;
    private BoostRemove _boostRemove;
    private bool _wasDeadlocked;
    private float _lastHintTime = -999f;

    public void Initialize(List<GrillStation> grillStations)
    {
        if (GameManager.Instance != null)
        {
            _conditionController = GameManager.Instance.ConditionController;
        }

        if (_conditionController == null)
        {
            _conditionController = FindObjectOfType<GameplayConditionController>();
        }

        CacheBoostRemove();
        _wasDeadlocked = false;
        _lastHintTime = -999f;
    }

    public bool TryShowBoostHint()
    {
        if (!IsDeadlocked())
        {
            _wasDeadlocked = false;
            return false;
        }

        if (!CanUseRemoveBoost())
        {
            return false;
        }

        if (_boostRemove == null)
        {
            CacheBoostRemove();
        }

        if (_boostRemove == null || !_boostRemove.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (_wasDeadlocked && Time.time - _lastHintTime < _repeatHintCooldown)
        {
            return false;
        }

        PlayRemoveBoostHint();
        _wasDeadlocked = true;
        _lastHintTime = Time.time;
        return true;
    }

    private bool IsDeadlocked()
    {
        if (_conditionController == null)
        {
            if (GameManager.Instance != null)
            {
                _conditionController = GameManager.Instance.ConditionController;
            }

            if (_conditionController == null)
            {
                _conditionController = FindObjectOfType<GameplayConditionController>();
            }
        }

        return _conditionController != null && _conditionController.IsNearFullDeadlockedForHint();
    }

    private bool CanUseRemoveBoost()
    {
        bool isUnlimited = PlayerPrefs.GetInt(RemoveThreeUnlimitedKey, 0) == 1;
        if (isUnlimited)
        {
            return true;
        }

        return PlayerPrefs.GetInt(RemoveThreeCountKey, 0) > 0;
    }

    private void CacheBoostRemove()
    {
        if (BoostManager.Instance != null)
        {
            _boostRemove = BoostManager.Instance.RemoveBoostButton;
        }

        if (_boostRemove == null)
        {
            _boostRemove = FindObjectOfType<BoostRemove>(true);
        }
    }

    private void PlayRemoveBoostHint()
    {
        RectTransform target = _boostRemove.transform as RectTransform;
        if (target == null)
        {
            return;
        }

        target.DOKill();
        target.localScale = Vector3.one;

        Sequence seq = DOTween.Sequence();
        seq.Append(target.DOPunchScale(Vector3.one * _hintScale, 0.28f, 8, 0.8f));
        seq.Append(target.DOShakeAnchorPos(0.28f, new Vector2(12f, 0f), 12, 90f, false, true));
    }
}
