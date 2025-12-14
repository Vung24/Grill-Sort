using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class UISettingsManager : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button settingButton, Menu, Continue, playAgain, _btnClose;
    [Header("Gameobject")]
    [SerializeField] private GameObject settingPanel;
    private bool isOpen = false;

    void Start()
    {
        settingPanel.transform.localScale = Vector3.zero;
        settingPanel.SetActive(false);
        settingButton.onClick.AddListener(ToggleSettings);
        Menu.onClick.AddListener(LoadMenu);
        Continue.onClick.AddListener(CloseSettings);
        playAgain.onClick.AddListener(LoadPlayAgainGame);
        _btnClose.onClick.AddListener(CloseSettings);
    }

    void ToggleSettings()
    {
        if (!isOpen)
            OpenSettings();
        else
            CloseSettings();
    }

    void OpenSettings()
    {
        isOpen = true;
        settingPanel.SetActive(true);
        settingPanel.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);

    }

    void CloseSettings()
    {
        isOpen = false;
        settingPanel.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack)
            .OnComplete(() => settingPanel.SetActive(false));
    }
    void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    void LoadPlayAgainGame()
    {
        if (LinearLevelSystem.Instance != null)
        {
            Destroy(LinearLevelSystem.Instance.gameObject);
        }

        SceneManager.LoadScene("MainScene");
    }
}
