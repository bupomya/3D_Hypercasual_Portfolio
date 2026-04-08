using UnityEngine;
using DG.Tweening;

public class PickupZone : TransferZone
{
    [Header("Pickup Source")]
    public GridStacker sourceStacker;
    public StackerType targetStackerType;

    [Header("Feedback")]
    public FeedbackData pickupFeedback;

    protected override bool CanTransfer()
    {
        ItemStacker target = GetTargetStacker();
        return sourceStacker != null && sourceStacker.Count > 0 && target != null && !target.IsFull;
    }

    protected override void TransferOne()
    {
        ItemStacker target = GetTargetStacker();
        GameObject item = sourceStacker.PopItem();
        if (item == null) return;

        Vector3 startPos = item.transform.position;
        Vector3 targetLocalPos = Vector3.up * (target.Count * target.stackInterval);
        Vector3 targetWorldPos = target.transform.TransformPoint(targetLocalPos);

        MoneyItem moneyItem = item.GetComponent<MoneyItem>();
        if (moneyItem != null && CurrencyManager.Instance != null)
            CurrencyManager.Instance.Add(moneyItem.value);

        int capturedIndex = target.Count;
        target.AddItem(item);

        if (FeedbackManager.Instance != null)
            FeedbackManager.Instance.Play(pickupFeedback, startPos);

        item.transform.position = startPos;
        ItemTweenHelper.MoveArc(item.transform, targetWorldPos, tweenDuration, tweenHeight)
            .OnComplete(() =>
            {
                item.transform.localPosition = Vector3.up * (capturedIndex * target.stackInterval);
            });
    }

    ItemStacker GetTargetStacker()
    {
        if (currentMiner == null) return null;
        return currentMiner.GetStacker(targetStackerType);
    }
}
