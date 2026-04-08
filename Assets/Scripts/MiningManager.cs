using UnityEngine;
using System;

public class MiningManager : MonoBehaviour
{
    public static MiningManager Instance { get; private set; }

    [Header("Level Data")]
    public MiningLevelData[] levels;

    [SerializeField] private int currentLevel = 0;

    public int CurrentLevel => currentLevel;
    public MiningLevelData CurrentData => levels[currentLevel];

    public event Action<int> OnLevelChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        OnLevelChanged?.Invoke(currentLevel);
    }

    public bool CanUpgrade()
    {
        return currentLevel < levels.Length - 1;
    }

    public bool TryUpgrade()
    {
        if (!CanUpgrade()) return false;
        if (CurrencyManager.Instance == null) return false;

        int cost = CurrentData.upgradeCost;
        if (!CurrencyManager.Instance.Spend(cost)) return false;

        currentLevel++;
        OnLevelChanged?.Invoke(currentLevel);
        return true;
    }
}
