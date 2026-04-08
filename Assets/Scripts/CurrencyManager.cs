using UnityEngine;
using UnityEngine.UI;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("UI")]
    public Text currencyText;

    private int currency;
    public int Currency => currency;

    public event Action<int> OnCurrencyChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        UpdateUI();
    }

    public void Add(int amount)
    {
        currency += amount;
        OnCurrencyChanged?.Invoke(currency);
        UpdateUI();
    }

    public bool Spend(int amount)
    {
        if (currency < amount) return false;

        currency -= amount;
        OnCurrencyChanged?.Invoke(currency);
        UpdateUI();
        return true;
    }

    void UpdateUI()
    {
        if (currencyText != null)
            currencyText.text = "Money : " + currency.ToString();
    }
}
