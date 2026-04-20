using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class UISettingsManager : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button settingButton, Menu, playAgain, _btnClose;
    [Header("Panel")]
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject _restartPanel;
    [SerializeField] private TextMeshProUGUI _levelText;
    private bool isOpen = false;

    void Start()
    {
        settingPanel.transform.localScale = Vector3.zero;
        settingPanel.SetActive(false);
        if (_restartPanel != null)
        {
            _restartPanel.SetActive(false);
        }
        UpdateLevel();
    }

    public void ToggleSettings()
    {
        if (!isOpen)
            OpenSettings();
        else
            CloseSettings();
    }

    public void OpenSettings()
    {
        isOpen = true;
        GameManager.Instance?.SetTimerPaused(true);
        settingPanel.SetActive(true);
        settingPanel.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);

    }

    public void CloseSettings()
    {
        isOpen = false;
        GameManager.Instance?.SetTimerPaused(false);
        settingPanel.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack)
            .OnComplete(() => settingPanel.SetActive(false));
        if (_restartPanel != null)
        {
            _restartPanel.SetActive(false);
        }
    }

    private void OnDisable()
    {
        isOpen = false;
        GameManager.Instance?.SetTimerPaused(false);
    }
    public void LoadMenu()
    {
        GameManager.Instance?.ConsumeEnergyIfAbandon();
        LinearLevelSystem.EnsureInstance().LoadMenuScene();
    }

    public void LoadPlayAgainGame()
    {
        // First tap from settings only opens restart confirmation panel.
        if (_restartPanel == null)
        {
            ConfirmPlayAgainFromRestartPanel();
            return;
        }

        settingPanel.SetActive(false);
        _restartPanel.SetActive(true);
    }

    public void ConfirmPlayAgainFromRestartPanel()
    {
        if (EnergyManager.Instance != null && !EnergyManager.Instance.CanPlay())
        {
            return;
        }

        if (_restartPanel != null)
        {
            _restartPanel.SetActive(false);
        }

        GameManager.Instance?.ConsumeEnergyIfAbandon();
        LinearLevelSystem.EnsureInstance().RestartLevel();
    }

    public void CloseRestartPanel()
    {
        if (_restartPanel != null)
        {
            _restartPanel.SetActive(false);
        }

        settingPanel.SetActive(true);
    }
    public void UpdateLevel()
    {
        if (_levelText == null)
        {
            return;
        }

        int currentLevel = LinearLevelSystem.EnsureInstance().CurrentLevel;
        _levelText.text = $"<size=50>Level {currentLevel}</size>";
    }
}
