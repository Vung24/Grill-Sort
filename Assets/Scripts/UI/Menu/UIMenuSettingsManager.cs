using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIMenuSettingsManager : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button settingButton, _btnClose;

    [Header("Gameobject")]
    [SerializeField] private GameObject settingPanel;
    private bool isOpen = false;

    void Start()
    {
        if (settingPanel != null)
        {
            settingPanel.transform.localScale = Vector3.zero;
            settingPanel.SetActive(false);
        }
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
        if (settingPanel == null) return;

        isOpen = true;
        settingPanel.SetActive(true);
        settingPanel.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
    }

    public void CloseSettings()
    {
        if (settingPanel == null) return;

        isOpen = false;
        settingPanel.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack)
            .OnComplete(() => settingPanel.SetActive(false));
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }

}
