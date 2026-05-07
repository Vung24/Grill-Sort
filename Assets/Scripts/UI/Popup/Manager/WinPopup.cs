using UnityEngine;

public class WinPopup : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private ButtonEffectLogic _menuButton;
    [SerializeField] private ButtonEffectLogic _nextLevelButton;

    private void Awake()
    {
        BindButtons();
    }

    private void OnDestroy()
    {
        UnbindButtons();
    }

    private void BindButtons()
    {
        BindButton(_menuButton, OnClickMenu);
        BindButton(_nextLevelButton, OnClickNextLevel);
    }

    private void UnbindButtons()
    {
        UnbindButton(_menuButton, OnClickMenu);
        UnbindButton(_nextLevelButton, OnClickNextLevel);
    }

    private void OnClickMenu()
    {
        LinearLevelSystem.EnsureInstance().LoadMenuScene();
    }

    private void OnClickNextLevel()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNextLevelButtonClicked();
            return;
        }

        LinearLevelSystem.EnsureInstance().NextLevel();
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
