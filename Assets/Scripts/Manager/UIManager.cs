using UnityEngine;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Popups")]
    [SerializeField] private GameObject _winPopup;
    [SerializeField] private GameObject _losePopup;
    [SerializeField] private GameObject _restartPopup;
    [SerializeField] private GameObject _revivePopup;
    [SerializeField] private GameObject _rewardPopup;
    [SerializeField] private float _panelFadeDuration = 0.25f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        HideAllPopups();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void HideAllPopups()
    {
        HidePopup(_winPopup);
        HidePopup(_losePopup);
        HidePopup(_restartPopup);
        HidePopup(_revivePopup);
        HidePopup(_rewardPopup);
    }

    public void ResetGameplayPopups()
    {
        HidePopup(_winPopup);
        HidePopup(_losePopup);
        HidePopup(_revivePopup);
        HidePopup(_rewardPopup);
    }

    public void ShowWinPopup()
    {
        ShowPanel(_winPopup);
    }

    public void ShowLosePopup()
    {
        ShowPanel(_losePopup);
    }

    public void ShowRevivePopup()
    {
        ShowPanel(_revivePopup);
    }

    public void HideRevivePopup()
    {
        HidePopup(_revivePopup);
    }

    public void ShowRewardPopup()
    {
        ShowPanel(_rewardPopup);
    }

    public void HideRewardPopup()
    {
        HidePopup(_rewardPopup);
    }

    private void HidePopup(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        panel.SetActive(false);
    }

    private void ShowPanel(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = panel.AddComponent<CanvasGroup>();
        }

        panel.SetActive(true);
        group.alpha = 0f;
        group.interactable = true;
        group.blocksRaycasts = true;

        group.DOKill();
        group.DOFade(1f, _panelFadeDuration).SetEase(Ease.OutQuad);
    }
}
