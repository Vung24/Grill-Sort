using System;
using UnityEngine;

public class EnergyManager : MonoBehaviour
{
    public static EnergyManager Instance { get; private set; }

    public event Action OnEnergyChanged;

    public static EnergyManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        EnergyManager existing = FindObjectOfType<EnergyManager>();
        if (existing != null)
        {
            Instance = existing;
            return existing;
        }

        GameObject go = new GameObject("EnergyManager");
        return go.AddComponent<EnergyManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BindEnergy();
    }

    private void OnDestroy()
    {
        UnbindEnergy();
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool CanPlay() => Energy.EnsureInstance().CanPlay();
    public void OnLose() => Energy.EnsureInstance().OnLose();
    public void ResetHearts() => Energy.EnsureInstance().ResetHearts();
    public int CurrentHearts => Energy.EnsureInstance().CurrentHearts;
    public int MaxHearts => Energy.EnsureInstance().MaxHearts;
    public bool IsFull => Energy.EnsureInstance().IsFull;
    public TimeSpan TimeToNext => Energy.EnsureInstance().TimeToNext;

    private void BindEnergy()
    {
        Energy energy = Energy.EnsureInstance();
        if (energy != null)
        {
            energy.OnEnergyChanged -= ForwardEnergyChanged;
            energy.OnEnergyChanged += ForwardEnergyChanged;
        }
    }

    private void UnbindEnergy()
    {
        if (Energy.Instance != null)
        {
            Energy.Instance.OnEnergyChanged -= ForwardEnergyChanged;
        }
    }

    private void ForwardEnergyChanged()
    {
        OnEnergyChanged?.Invoke();
    }
}
