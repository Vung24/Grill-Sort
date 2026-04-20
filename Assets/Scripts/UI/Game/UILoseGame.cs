using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILoseGame : MonoBehaviour
{
    [SerializeField] private Button _menuButton, _playAgainButton;
    [Header("Lose Message")]
    [SerializeField] private TextMeshProUGUI _loseReasonText;
    [SerializeField] private string _timeUpMessage = "Time's up!";
    [SerializeField] private string _outOfSlotMessage = "Out of slot";

    void Start()
    {
    }

    private void OnEnable()
    {
        RefreshLoseReasonText();
    }

    public void LoadMenu()
    {
        LinearLevelSystem.EnsureInstance().LoadMenuScene();
    }
    public void LoadPlayAgainGame()
    {
        if (EnergyManager.Instance != null && !EnergyManager.Instance.CanPlay())
        {
            return;
        }
        GameManager.Instance?.ConsumeEnergyIfAbandon();
        LinearLevelSystem.EnsureInstance().RestartLevel();
    }

    private void RefreshLoseReasonText()
    {
        if (_loseReasonText == null)
        {
            return;
        }

        bool isTimeUp = GameManager.Instance != null
            && GameManager.Instance.CurrentLoseReason == EnumManager.LoseReason.TimeUp;
        _loseReasonText.text = isTimeUp ? _timeUpMessage : _outOfSlotMessage;
    }
}
