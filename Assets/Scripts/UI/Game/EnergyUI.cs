using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnergyUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _countEnergyText;
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private Image _panelTime;
    [SerializeField] private Image _energyImage;

    private void OnEnable()
    {
        EnergyManager.EnsureInstance();
        if (EnergyManager.Instance != null)
        {
            EnergyManager.Instance.OnEnergyChanged += Refresh;
        }
        Refresh();
    }

    private void OnDisable()
    {
        if (EnergyManager.Instance != null)
        {
            EnergyManager.Instance.OnEnergyChanged -= Refresh;
        }
    }

    private void Update()
    {
        if (EnergyManager.Instance == null)
        {
            return;
        }

        if (!EnergyManager.Instance.IsFull && _timeText != null)
        {
            UpdateTimeText();
        }
    }

    private void Refresh()
    {
        if (EnergyManager.Instance == null)
        {
            return;
        }

        if (_countEnergyText != null)
        {
            _countEnergyText.text = EnergyManager.Instance.CurrentHearts.ToString();
        }
        UpdateTimeText();
    }

    private void UpdateTimeText()
    {
        if (_timeText == null || EnergyManager.Instance == null)
        {
            return;
        }

        if (!_timeText.gameObject.activeInHierarchy)
        {
            return;
        }

        if (EnergyManager.Instance.IsFull)
        {
            _timeText.text = "FULL";
            return;
        }

        System.TimeSpan remaining = EnergyManager.Instance.TimeToNext;
        int minutes = Mathf.FloorToInt((float)remaining.TotalMinutes);
        int seconds = Mathf.FloorToInt((float)remaining.TotalSeconds % 60f);
        _timeText.text = $"{minutes:00}:{seconds:00}";
    }
}
