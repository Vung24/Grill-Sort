using UnityEngine;
using UnityEngine.UI;

public class RestartPopup : MonoBehaviour
{
    [SerializeField] private ButtonEffectLogic _btnRestart;
    [SerializeField] private ButtonEffectLogic _btnClose;
    [SerializeField] private GameObject _popupPanel;
    [SerializeField] private SettingsPopup settingPopup;
    private bool isOpen;
    private bool _isInitialized;

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        _isInitialized = true;

        if (_popupPanel != null)
        {
            if (!isOpen)
            {
                _popupPanel.SetActive(false);
            }
        }
        if (_btnClose != null)
        {
            _btnClose.onClick.RemoveListener(OnClickCancel);
            _btnClose.onClick.AddListener(OnClickCancel);
        }
        if (_btnRestart != null)
        {
            _btnRestart.onClick.RemoveListener(OnClickRestart);
            _btnRestart.onClick.AddListener(OnClickRestart);
        }
    }

    private void OnDestroy()
    {
        if (_btnClose != null)
        {
            _btnClose.onClick.RemoveListener(OnClickCancel);
        }
        if (_btnRestart != null)
        {
            _btnRestart.onClick.RemoveListener(OnClickRestart);
        }
    }

    private void OnClickRestart()
    {
        ConfirmRestartFromPopup();
    }
    private void OnClickCancel()
    {
        ClosePopup();
    }
    public void OpenPopup()
    {
        isOpen = true;
        if (_popupPanel != null)
        {
            _popupPanel.SetActive(true);
        }
    }
    public void ClosePopup()
    {
        isOpen = false;
        if (_popupPanel != null)
        {
            _popupPanel.SetActive(false);
            settingPopup.gameObject.SetActive(false);
        }
    }

    public void ConfirmRestartFromPopup()
    {
        if (!isOpen)
        {
            return;
        }

        if (EnergyManager.Instance != null && !EnergyManager.Instance.CanPlay())
        {
            return;
        }

        ClosePopup();
        settingPopup?.OnRestartConfirmed();

        GameManager.Instance?.ConsumeEnergyIfAbandon();
        LinearLevelSystem.EnsureInstance().RestartLevel();
    }
}
