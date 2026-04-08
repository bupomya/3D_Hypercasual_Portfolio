using UnityEngine;
using DG.Tweening;

public class DropZone : TransferZone
{
    [Header("Drop Target")]
    public GridStacker targetStacker;
    public StackerType sourceStackerType;

    [Header("Feedback")]
    public FeedbackData dropFeedback;

    private int tweenCount;
    public bool HasPendingTweens => tweenCount > 0;

    protected override bool CanTransfer()
    {
        ItemStacker source = GetSourceStacker();
        if (source == null || source.Count == 0 || targetStacker == null) return false;

        if (targetStacker.IsFull)
        {
            if (WarningUI.Instance != null)
                WarningUI.Instance.Show("보관함이 가득 찼습니다! (" + targetStacker.maxCount + "/" + targetStacker.maxCount + ")");
            return false;
        }

        return true;
    }

    protected override void TransferOne()
    {
        ItemStacker source = GetSourceStacker();
        GameObject item = source.PopItem();
        if (item == null) return;

        Vector3 startPos = item.transform.position;
        int targetIndex = targetStacker.Count + tweenCount;
        Vector3 targetWorldPos = targetStacker.transform.TransformPoint(
            targetStacker.GetStackPosition(targetIndex)
        );

        if (FeedbackManager.Instance != null)
            FeedbackManager.Instance.Play(dropFeedback, startPos);

        tweenCount++;
        ItemTweenHelper.MoveArc(item.transform, targetWorldPos, tweenDuration, tweenHeight)
            .OnComplete(() =>
            {
                tweenCount--;
                if (item != null && targetStacker != null)
                    targetStacker.AddItem(item);
            });
    }

    ItemStacker GetSourceStacker()
    {
        if (currentMiner == null) return null;
        return currentMiner.GetStacker(sourceStackerType);
    }
}
