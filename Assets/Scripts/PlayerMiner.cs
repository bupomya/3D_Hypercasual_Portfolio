using UnityEngine;
using System.Collections;

public class PlayerMiner : BaseMiner
{
    [Header("Stacker")]
    public ItemStacker mineralStacker;
    public ItemStacker mineralRedStacker;
    public ItemStacker moneyStacker;

    private Coroutine autoMiningCoroutine;

    public ItemStacker GetStacker(StackerType type)
    {
        switch (type)
        {
            case StackerType.Mineral: return mineralStacker;
            case StackerType.MineralRed: return mineralRedStacker;
            case StackerType.Money: return moneyStacker;
            default: return null;
        }
    }

    public override void ApplyLevelData(int level)
    {
        base.ApplyLevelData(level);

        MiningLevelData data = MiningManager.Instance.CurrentData;
        if (data == null || mineralStacker == null) return;

        mineralStacker.maxCount = data.maxCarryCount;
    }

    [Header("Feedback")]
    public FeedbackData miningHitFeedback;

    public void OnMiningHit()
    {
        if (!isMining) return;

        if (FeedbackManager.Instance != null)
            FeedbackManager.Instance.Play(miningHitFeedback, transform.position);

        DoMine();
    }

    public override void StartAutoMining()
    {
        StopAutoMining();

        MiningLevelData data = MiningManager.Instance != null ? MiningManager.Instance.CurrentData : null;
        if (data == null || data.autoMiningInterval <= 0f) return;

        autoMiningCoroutine = StartCoroutine(AutoMiningRoutine(data.autoMiningInterval));
    }

    public override void StopAutoMining()
    {
        if (autoMiningCoroutine != null)
        {
            StopCoroutine(autoMiningCoroutine);
            autoMiningCoroutine = null;
        }
    }

    IEnumerator AutoMiningRoutine(float interval)
    {
        while (true)
        {
            DoMine();
            yield return new WaitForSeconds(interval);
        }
    }

    void DoMine()
    {
        Mineral mineral = FindNearestMineral();
        if (mineral == null) return;

        int damage = MiningManager.Instance != null ? MiningManager.Instance.CurrentData.miningDamage : 1;
        bool destroyed = mineral.Mine(damage);

        if (!destroyed) return;

        if (mineralStacker != null && !mineralStacker.IsFull)
            mineralStacker.AddItem();
        else if (mineralStacker != null && WarningUI.Instance != null)
            WarningUI.Instance.Show("보관함이 가득 찼습니다! (" + mineralStacker.maxCount + "/" + mineralStacker.maxCount + ")");
    }
}
