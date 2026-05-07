using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Menu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _quitButton;

    [Header("Play Button Text")]
    [SerializeField] private TextMeshProUGUI _playButtonText;

    [Header("Animation")]
    [SerializeField] private float _animDuration = 0.3f;

    private int _currentLevel;

    private void Start()
    {
        UpdateUI();
        AnimateEntry();
    }
    private void OnEnable()
    {
        LinearLevelSystem.EnsureInstance();
        UpdateUI();
        EnergyManager.EnsureInstance();
        if (EnergyManager.Instance != null)
        {
            EnergyManager.Instance.OnEnergyChanged += UpdatePlayButtonState;
        }
        UpdatePlayButtonState();
    }

    private void OnDisable()
    {
        if (EnergyManager.Instance != null)
        {
            EnergyManager.Instance.OnEnergyChanged -= UpdatePlayButtonState;
        }
    }


    private void UpdateUI()
    {
        if (_playButtonText != null)
        {
            _currentLevel = LinearLevelSystem.EnsureInstance().CurrentLevel;
            if (_currentLevel == 1)
            {
                _playButtonText.text = "Play";
            }
            else
            {
                _playButtonText.text = $"<size=70>Play Level {_currentLevel}</size>";
            }
        }
    }
    public void OnPlayClicked()
    {
        AnimateButtonClick(_playButton, () =>
        {
            if (EnergyManager.Instance != null && !EnergyManager.Instance.CanPlay())
            {
                return;
            }
            LinearLevelSystem.EnsureInstance().ContinueGame();
        });
    }
    private void AnimateEntry()
    {
        if (_playButton != null)
        {
            _playButton.transform.localScale = Vector3.zero;
            _playButton.transform.DOScale(Vector3.one, _animDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(0.1f);
        }
        if (_quitButton != null)
        {
            _quitButton.transform.localScale = Vector3.zero;
            _quitButton.transform.DOScale(Vector3.one, _animDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(0.2f);
        }
    }
    private void AnimateButtonClick(Button button, System.Action callback)
    {
        if (button == null)
        {
            callback?.Invoke();
            return;
        }

        button.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 5, 0.5f)
            .OnComplete(() => callback?.Invoke());
    }
    public void PlayGame()
    {
        OnPlayClicked();
    }

    private void UpdatePlayButtonState()
    {
        if (_playButton == null)
        {
            return;
        }

        if (EnergyManager.Instance == null)
        {
            _playButton.interactable = true;
            return;
        }

        _playButton.interactable = EnergyManager.Instance.CanPlay();
    }
        public void OnQuitClicked()
    {
        AnimateButtonClick(_quitButton, () =>
        {
            QuitGame();
        });
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
