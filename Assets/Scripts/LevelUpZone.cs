using UnityEngine;

public class LevelUpZone : MonoBehaviour
{
    private PlayerMiner currentMiner;
    private int levelOnEnter;

    void OnTriggerEnter(Collider other)
    {
        PlayerMiner miner = other.GetComponent<PlayerMiner>();
        if (miner == null) return;

        currentMiner = miner;
        levelOnEnter = MiningManager.Instance != null ? MiningManager.Instance.CurrentLevel : -1;
    }

    void OnTriggerStay(Collider other)
    {
        if (currentMiner == null) return;
        if (other.GetComponent<PlayerMiner>() == null) return;
        if (MiningManager.Instance == null || !MiningManager.Instance.CanUpgrade()) return;
        if (MiningManager.Instance.CurrentLevel != levelOnEnter) return;

        int cost = MiningManager.Instance.CurrentData.upgradeCost;
        ItemStacker moneyStacker = currentMiner.moneyStacker;

        if (moneyStacker == null) return;

        // MoneyItem의 value를 기준으로 필요한 개수 계산
        int moneyPerItem = GetMoneyValue(moneyStacker);
        int itemsNeeded = moneyPerItem > 0 ? Mathf.CeilToInt((float)cost / moneyPerItem) : cost;

        if (moneyStacker.Count < itemsNeeded) return;

        if (MiningManager.Instance.TryUpgrade())
        {
            for (int i = 0; i < itemsNeeded; i++)
                moneyStacker.RemoveItem();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerMiner>() == null) return;
        currentMiner = null;
    }

    int GetMoneyValue(ItemStacker stacker)
    {
        if (stacker.itemPrefab == null) return 1;

        MoneyItem moneyItem = stacker.itemPrefab.GetComponent<MoneyItem>();
        return moneyItem != null ? moneyItem.value : 1;
    }

    void OnDrawGizmos()
    {
        ItemTweenHelper.DrawZoneGizmos(GetComponent<BoxCollider>(), transform, Color.cyan);
    }
}
