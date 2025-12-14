using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class SimpleMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _quitButton;

    [Header("Play Button Text")]
    [SerializeField] private TextMeshProUGUI _playButtonText;

    [Header("Animation")]
    [SerializeField] private float _animDuration = 0.3f;

    private int _currentLevel;
    private int _highestLevel;

    private void Start()
    {
        LoadProgress();
        SetupButtons();
        UpdateUI();
        AnimateEntry();
    }
    private void OnEnable()
    {
        LoadProgress();
        UpdateUI();
    }
    private void LoadProgress()
    {
        _currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        _highestLevel = PlayerPrefs.GetInt("HighestLevel", 1);
    }

    private void SetupButtons()
    {
        if (_playButton != null)
        {
            _playButton.onClick.RemoveAllListeners();
            _playButton.onClick.AddListener(OnPlayClicked);
        }
        if (_quitButton != null)
        {
            _quitButton.onClick.RemoveAllListeners();
            _quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    private void UpdateUI()
    {
        if (_playButtonText != null)
        {
            if (_currentLevel == 1)
            {
                _playButtonText.text = "PLAY";
            }
            else
            {
                _playButtonText.text = $"PLAY LEVEL {_currentLevel}";
            }
        }
    }
    private void OnPlayClicked()
    {
        AnimateButtonClick(_playButton, () =>
        {
            if (LinearLevelSystem.Instance != null)
            {
                LinearLevelSystem.Instance.ContinueGame();
            }
            else
            {
                SceneManager.LoadScene("MainScene");
            }
        });
    }
    private void OnQuitClicked()
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
}