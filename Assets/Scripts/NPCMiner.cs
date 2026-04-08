using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class NPCMiner : BaseMiner
{
    [Header("NPC Movement")]
    public float moveSpeed = 3f;
    public float mineDistance = 1.5f;

    // 모든 NPC가 공유하는 예약된 광물 목록
    private static readonly HashSet<Mineral> reservedMinerals = new HashSet<Mineral>();

    private GridStacker mineralDrop;
    private GameObject mineralItemPrefab;
    private int miningDamage = 1;
    private float autoMiningInterval = 1f;
    private Coroutine miningRoutine;
    private Mineral currentTarget;
    private int pendingTweenCount;

    protected override void Start()
    {
        animator = GetComponent<Animator>();

        if (miningTool != null)
            miningTool.SetActive(false);
    }

    protected override void OnDestroy()
    {
        ReleaseTarget();
    }

    public void Setup(GridStacker drop, GameObject itemPrefab, int damage, float radius, float interval)
    {
        mineralDrop = drop;
        mineralItemPrefab = itemPrefab;
        miningDamage = damage;
        miningRadius = radius;
        autoMiningInterval = interval;
    }

    public override void StartAutoMining()
    {
        StopAutoMining();
        miningRoutine = StartCoroutine(AutoMineRoutine());
    }

    public override void StopAutoMining()
    {
        if (miningRoutine != null)
        {
            StopCoroutine(miningRoutine);
            miningRoutine = null;
        }
        ReleaseTarget();
        SetMining(false);
        if (animator != null)
            animator.SetBool("isWalking", false);
    }

    private void ReleaseTarget()
    {
        if (currentTarget != null)
        {
            reservedMinerals.Remove(currentTarget);
            currentTarget = null;
        }
    }

    /// <summary>
    /// Mining 애니메이션 이벤트에서 호출됩니다.
    /// </summary>
    public void OnMiningHit()
    {
        if (currentTarget == null || !currentTarget.gameObject.activeSelf) return;

        bool destroyed = currentTarget.Mine(miningDamage);
        if (destroyed)
            SpawnMineralItem();
    }

    IEnumerator AutoMineRoutine()
    {
        while (true)
        {
            // 다른 NPC가 예약하지 않은 가장 가까운 광물 탐색
            Mineral mineral = FindUnreservedMineral();

            if (mineral == null)
            {
                SetMining(false);
                if (animator != null)
                    animator.SetBool("isWalking", false);
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            // 광물 예약
            currentTarget = mineral;
            reservedMinerals.Add(mineral);

            // 광물까지 이동
            SetMining(false);
            if (animator != null)
                animator.SetBool("isWalking", true);

            while (mineral != null && mineral.gameObject.activeSelf)
            {
                float dist = Vector3.Distance(transform.position, mineral.transform.position);
                if (dist <= mineDistance)
                    break;

                Vector3 dir = mineral.transform.position - transform.position;
                dir.y = 0f;
                dir.Normalize();

                transform.position += dir * moveSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(dir),
                    10f * Time.deltaTime
                );

                yield return null;
            }

            if (animator != null)
                animator.SetBool("isWalking", false);

            // 광물이 사라졌으면 예약 해제 후 다시 탐색
            if (mineral == null || !mineral.gameObject.activeSelf)
            {
                ReleaseTarget();
                continue;
            }

            // 채광 시작 — 애니메이션 이벤트(OnMiningHit)가 실제 채광을 수행
            SetMining(true);

            // 광물이 파괴될 때까지 대기
            while (mineral != null && mineral.gameObject.activeSelf)
                yield return null;

            ReleaseTarget();
            SetMining(false);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private Mineral FindUnreservedMineral()
    {
        Vector3 origin = miningPos != null ? miningPos.position : transform.position;
        Collider[] hits = Physics.OverlapSphere(origin, miningRadius, mineralLayer);

        Mineral nearest = null;
        float minDist = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Mineral mineral = hits[i].GetComponent<Mineral>();
            if (mineral == null) continue;
            if (reservedMinerals.Contains(mineral)) continue;

            float dist = Vector3.Distance(origin, hits[i].transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = mineral;
            }
        }

        return nearest;
    }

    void SpawnMineralItem()
    {
        if (mineralDrop == null || mineralItemPrefab == null) return;

        Vector3 startPos = transform.position + Vector3.up;
        int targetIndex = mineralDrop.Count + pendingTweenCount;
        Vector3 targetPos = mineralDrop.transform.TransformPoint(
            mineralDrop.GetStackPosition(targetIndex)
        );

        GameObject item = Instantiate(mineralItemPrefab, startPos, Quaternion.identity);
        pendingTweenCount++;

        // 트윈이 완료된 후에만 GridStacker에 추가 — ConveyorLine이 도착한 아이템만 꺼내감
        ItemTweenHelper.MoveArc(item.transform, targetPos, 0.5f, 2f)
            .OnComplete(() =>
            {
                pendingTweenCount--;
                if (item != null && mineralDrop != null)
                    mineralDrop.AddItem(item);
            });
    }
}
