using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWinGame : MonoBehaviour
{
    [SerializeField] private Button _menu, _nextLevel;
    void Start()
    {
    }
    public void LoadMenu()
    {
        LinearLevelSystem.EnsureInstance().LoadMenuScene();
    }
    public void NextLevel()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNextLevelButtonClicked();
        }
    }
}
