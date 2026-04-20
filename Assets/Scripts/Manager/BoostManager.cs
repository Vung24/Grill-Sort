using UnityEngine;

public class BoostManager : MonoBehaviour
{
    public static BoostManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public static BoostManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        BoostManager found = FindObjectOfType<BoostManager>();
        if (found != null)
        {
            Instance = found;
            return Instance;
        }

        GameObject host = new GameObject("BoostManager");
        Instance = host.AddComponent<BoostManager>();
        return Instance;
    }

    public bool UseRemoveThree()
    {
        return GameManager.Instance != null && GameManager.Instance.UseBoostRemoveThree();
    }

    public bool UseSwapForMerge()
    {
        return GameManager.Instance != null && GameManager.Instance.UseBoostSwapForMerge();
    }

    public bool CanUseAddThirtySeconds()
    {
        return GameManager.Instance != null && GameManager.Instance.CanUseBoostAddThirtySeconds();
    }

    public bool UseAddThirtySeconds()
    {
        return GameManager.Instance != null && GameManager.Instance.UseBoostAddThirtySeconds();
    }
}
