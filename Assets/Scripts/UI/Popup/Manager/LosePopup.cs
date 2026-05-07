using TMPro;
using UnityEngine;

public class LosePopup : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private ButtonEffectLogic _menuButton;
    [SerializeField] private ButtonEffectLogic _playAgainButton;

    [Header("Lose Message")]
    [SerializeField] private TextMeshProUGUI _loseReasonText;
    [SerializeField] private string _timeUpMessage = "Time's up!";
    [SerializeField] private string _outOfSlotMessage = "Out of slot";

    private void Awake()
    {
        BindButtons();
    }

    private void OnEnable()
    {
        RefreshLoseReasonText();
    }

    private void OnDestroy()
    {
        UnbindButtons();
    }

    private void BindButtons()
    {
        BindButton(_menuButton, OnClickMenu);
        BindButton(_playAgainButton, OnClickPlayAgain);
    }

    private void UnbindButtons()
    {
        UnbindButton(_menuButton, OnClickMenu);
        UnbindButton(_playAgainButton, OnClickPlayAgain);
    }

    private void OnClickMenu()
    {
        LinearLevelSystem.EnsureInstance().LoadMenuScene();
    }

    private void OnClickPlayAgain()
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

    private static void BindButton(ButtonEffectLogic button, UnityEngine.Events.UnityAction handler)
    {
        if (button == null || handler == null)
        {
            return;
        }

        button.onClick.RemoveListener(handler);
        button.onClick.AddListener(handler);
    }

    private static void UnbindButton(ButtonEffectLogic button, UnityEngine.Events.UnityAction handler)
    {
        if (button == null || handler == null)
        {
            return;
        }

        button.onClick.RemoveListener(handler);
    }
}
