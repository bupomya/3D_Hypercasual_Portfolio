using UnityEngine;
using System.Collections;
using DG.Tweening;

public class ExchangeNPCWorker : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Carry")]
    public ItemStacker itemStacker;

    private GridStacker sourceStacker;
    private GridStacker targetStacker;
    private Transform pickupPoint;
    private Transform exchangePoint;
    private Animator animator;
    private int maxCarry = 5;
    private float pickupInterval = 0.3f;
    private float dropInterval = 0.3f;

    public void Setup(GridStacker source, GridStacker target, Transform pickup, Transform exchange,
        int maxCarry, float pickupInterval, float dropInterval)
    {
        sourceStacker = source;
        targetStacker = target;
        pickupPoint = pickup;
        exchangePoint = exchange;
        this.maxCarry = maxCarry;
        this.pickupInterval = pickupInterval;
        this.dropInterval = dropInterval;

        if (itemStacker != null)
            itemStacker.maxCount = maxCarry;
    }

    public void StartWorking()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(WorkRoutine());
    }

    IEnumerator WorkRoutine()
    {
        while (true)
        {
            // 소스에 아이템이 없으면 ExchangeZone에서 대기
            if (sourceStacker.Count == 0)
            {
                yield return StartCoroutine(MoveTo(exchangePoint.position));

                while (sourceStacker.Count == 0)
                    yield return new WaitForSeconds(0.5f);
            }

            // MineralRedPickupZone으로 이동
            yield return StartCoroutine(MoveTo(pickupPoint.position));

            // 아이템 픽업 (ItemStacker에 쌓기)
            yield return StartCoroutine(PickupItems());

            // ExchangeZone으로 이동
            yield return StartCoroutine(MoveTo(exchangePoint.position));

            // 아이템 드롭 (GridStacker에 내려놓기)
            yield return StartCoroutine(DropItems());
        }
    }

    IEnumerator PickupItems()
    {
        while (itemStacker.Count < maxCarry && sourceStacker.Count > 0)
        {
            GameObject item = sourceStacker.PopItem();
            if (item == null) break;

            Vector3 startPos = item.transform.position;
            int capturedIndex = itemStacker.Count;
            Vector3 targetPos = itemStacker.transform.TransformPoint(
                Vector3.up * (capturedIndex * itemStacker.stackInterval)
            );

            itemStacker.AddItem(item);
            item.transform.position = startPos;

            ItemTweenHelper.MoveArc(item.transform, targetPos, 0.3f, 1.5f)
                .OnComplete(() =>
                {
                    if (item != null)
                        item.transform.localPosition = Vector3.up * (capturedIndex * itemStacker.stackInterval);
                });

            yield return new WaitForSeconds(pickupInterval);
        }
    }

    IEnumerator DropItems()
    {
        while (itemStacker.Count > 0)
        {
            GameObject item = itemStacker.PopItem();
            if (item == null) break;

            Vector3 startPos = item.transform.position;
            int capturedIndex = targetStacker.Count;
            Vector3 targetPos = targetStacker.transform.TransformPoint(
                targetStacker.GetStackPosition(capturedIndex)
            );

            targetStacker.AddItem(item);
            item.transform.position = startPos;

            ItemTweenHelper.MoveArc(item.transform, targetPos, 0.3f, 1.5f)
                .OnComplete(() =>
                {
                    if (item != null)
                        item.transform.localPosition = targetStacker.GetStackPosition(capturedIndex);
                });

            yield return new WaitForSeconds(dropInterval);
        }
    }

    IEnumerator MoveTo(Vector3 target)
    {
        if (animator != null)
            animator.SetBool("isWalking", true);

        Vector3 flatTarget = new Vector3(target.x, transform.position.y, target.z);

        while (Vector3.Distance(transform.position, flatTarget) > 0.1f)
        {
            flatTarget.y = transform.position.y;
            Vector3 dir = flatTarget - transform.position;
            dir.Normalize();

            transform.position += dir * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                10f * Time.deltaTime
            );

            yield return null;
        }

        transform.position = flatTarget;

        if (animator != null)
            animator.SetBool("isWalking", false);
    }
}
