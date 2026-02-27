using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILoseGame : MonoBehaviour
{
    [SerializeField] private Button _menuButton, _playAgainButton;
    void Start()
    {
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

}
