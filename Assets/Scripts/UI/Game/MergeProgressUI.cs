using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MergeProgressUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _progressFill;
    [SerializeField] private TextMeshProUGUI _percentText;

    private GameManager _boundManager;

    private void OnEnable()
    {
        TryBindManager();
        RefreshFromManager();
    }

    private void OnDisable()
    {
        UnbindManager();
    }

    private void Update()
    {
        if (_boundManager == null)
        {
            TryBindManager();
            RefreshFromManager();
        }
    }

    private void TryBindManager()
    {
        if (_boundManager != null || GameManager.Instance == null)
        {
            return;
        }

        _boundManager = GameManager.Instance;
        _boundManager.OnMergeProgressChanged += OnMergeProgressChanged;
    }

    private void UnbindManager()
    {
        if (_boundManager == null)
        {
            return;
        }

        _boundManager.OnMergeProgressChanged -= OnMergeProgressChanged;
        _boundManager = null;
    }

    private void OnMergeProgressChanged(float progress, int merged, int total)
    {
        ApplyProgress(progress);
    }

    private void RefreshFromManager()
    {
        if (_boundManager == null)
        {
            ApplyProgress(0f);
            return;
        }

        ApplyProgress(_boundManager.MergeProgress);
    }

    private void ApplyProgress(float progress)
    {
        float normalized = Mathf.Clamp01(progress);

        if (_progressFill != null)
        {
            _progressFill.fillAmount = normalized;
        }

        if (_percentText != null)
        {
            int percent = Mathf.RoundToInt(normalized * 100f);
            _percentText.text = $"{percent}%";
        }
    }
}
