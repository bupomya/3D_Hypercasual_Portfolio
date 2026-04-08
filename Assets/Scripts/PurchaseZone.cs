using UnityEngine;

public abstract class PurchaseZone : TransferZone
{
    [Header("비용")]
    public int cost = 5;

    private int itemsRemaining;
    private bool purchased;

    protected abstract bool CanPurchase();
    protected abstract void OnPurchaseComplete();

    protected override bool CanTransfer()
    {
        if (purchased) return false;
        if (itemsRemaining > 0) return true;

        if (!CanPurchase()) return false;

        ItemStacker moneyStacker = currentMiner.moneyStacker;
        if (moneyStacker == null) return false;

        int moneyPerItem = GetMoneyValue(moneyStacker);
        int itemsNeeded = moneyPerItem > 0 ? Mathf.CeilToInt((float)cost / moneyPerItem) : cost;

        if (moneyStacker.Count < itemsNeeded) return false;

        itemsRemaining = itemsNeeded;
        return true;
    }

    protected override void TransferOne()
    {
        ItemStacker moneyStacker = currentMiner.moneyStacker;
        if (moneyStacker == null) return;

        moneyStacker.RemoveItem();
        itemsRemaining--;

        if (itemsRemaining <= 0)
        {
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.Spend(cost);

            purchased = true;
            OnPurchaseComplete();
        }
    }

    protected int GetMoneyValue(ItemStacker stacker)
    {
        if (stacker.itemPrefab == null) return 1;

        MoneyItem moneyItem = stacker.itemPrefab.GetComponent<MoneyItem>();
        return moneyItem != null ? moneyItem.value : 1;
    }
}
