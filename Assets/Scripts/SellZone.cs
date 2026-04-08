using UnityEngine;

public class SellZone : TransferZone
{
    [Header("Sell")]
    public StackerType sourceStackerType;
    public StackerType targetStackerType;

    protected override bool CanTransfer()
    {
        ItemStacker source = currentMiner != null ? currentMiner.GetStacker(sourceStackerType) : null;
        ItemStacker target = currentMiner != null ? currentMiner.GetStacker(targetStackerType) : null;
        return source != null && source.Count > 0 && target != null && !target.IsFull;
    }

    protected override void TransferOne()
    {
        ItemStacker source = currentMiner.GetStacker(sourceStackerType);
        ItemStacker target = currentMiner.GetStacker(targetStackerType);

        source.RemoveItem();
        target.AddItem();
    }
}
