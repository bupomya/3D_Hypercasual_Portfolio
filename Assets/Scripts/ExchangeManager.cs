using UnityEngine;
using System.Collections;
using DG.Tweening;

public class ExchangeManager : MonoBehaviour
{
    [Header("NPC")]
    public GameObject npcPrefab;
    public Transform spawnPoint;
    public Transform waitingPoint;
    public float npcMoveSpeed = 3f;

    [Header("Exchange")]
    public GridStacker mineralRedDrop;
    public int requiredCount = 3;
    public float feedInterval = 0.3f;

    [Header("Money Output")]
    public GridStacker moneyDrop;
    public GameObject moneyPrefab;
    public int moneyPerExchange = 3;
    public float moneySpawnInterval = 0.2f;

    [Header("Travel")]
    public Transform[] travelWaypoints;
    public ArriveZone arriveZone;

    private GameObject currentNPC;
    private int currentFedCount;

    void Start()
    {
        StartCoroutine(NPCLifecycle());
    }

    IEnumerator NPCLifecycle()
    {
        while (true)
        {
            // ArriveZone에 공간이 생길 때까지 대기
            if (arriveZone != null)
            {
                while (!arriveZone.HasSpace)
                    yield return new WaitForSeconds(0.5f);
            }

            // NPC 생성 & 대기 지점으로 이동
            yield return StartCoroutine(SpawnAndMoveToWaiting());

            // MineralRed 먹이기 (NPC에 쌓기)
            yield return StartCoroutine(FeedUntilFull());

            // 돈 생성
            StartCoroutine(SpawnMoney());

            // 웨이포인트 경유 이동
            if (travelWaypoints != null)
            {
                for (int i = 0; i < travelWaypoints.Length; i++)
                {
                    if (travelWaypoints[i] == null) continue;
                    yield return StartCoroutine(MoveNPC(currentNPC, travelWaypoints[i].position));
                }
            }

            // ArriveZone에 등록 (사라지지 않고 서있음)
            if (arriveZone != null && currentNPC != null)
            {
                arriveZone.RegisterNPC(currentNPC);
            }
            else if (currentNPC != null)
            {
                Destroy(currentNPC);
            }

            currentNPC = null;
            currentFedCount = 0;
        }
    }

    IEnumerator SpawnAndMoveToWaiting()
    {
        if (npcPrefab == null || spawnPoint == null || waitingPoint == null) yield break;

        currentNPC = Instantiate(npcPrefab, spawnPoint.position, Quaternion.identity);
        currentFedCount = 0;

        yield return StartCoroutine(MoveNPC(currentNPC, waitingPoint.position));

        if (currentNPC != null)
        {
            Vector3 lookDir = waitingPoint.forward;
            if (travelWaypoints != null && travelWaypoints.Length > 0 && travelWaypoints[0] != null)
                lookDir = travelWaypoints[0].position - currentNPC.transform.position;

            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.01f)
                currentNPC.transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    IEnumerator FeedUntilFull()
    {
        ItemStacker npcStacker = currentNPC != null ? currentNPC.GetComponentInChildren<ItemStacker>() : null;

        while (currentFedCount < requiredCount)
        {
            if (mineralRedDrop != null && mineralRedDrop.Count > 0)
            {
                GameObject item = mineralRedDrop.PopItem();
                if (item != null)
                {
                    if (npcStacker != null)
                    {
                        // NPC의 ItemStacker에 쌓기
                        Vector3 startPos = item.transform.position;
                        int capturedIndex = npcStacker.Count;
                        Vector3 targetPos = npcStacker.transform.TransformPoint(
                            Vector3.up * (capturedIndex * npcStacker.stackInterval)
                        );

                        npcStacker.AddItem(item);
                        item.transform.position = startPos;

                        ItemTweenHelper.MoveArc(item.transform, targetPos, 0.3f, 1.5f)
                            .OnComplete(() =>
                            {
                                if (item != null)
                                    item.transform.localPosition = Vector3.up * (capturedIndex * npcStacker.stackInterval);
                            });
                    }
                    else
                    {
                        // ItemStacker가 없으면 기존 방식 (파괴)
                        Vector3 targetPos = currentNPC != null
                            ? currentNPC.transform.position + Vector3.up
                            : waitingPoint.position;

                        ItemTweenHelper.MoveArc(item.transform, targetPos, 0.3f, 1.5f)
                            .OnComplete(() => Destroy(item));
                    }

                    currentFedCount++;
                }
            }

            yield return new WaitForSeconds(feedInterval);
        }
    }

    IEnumerator SpawnMoney()
    {
        if (moneyDrop == null || moneyPrefab == null) yield break;

        for (int i = 0; i < moneyPerExchange; i++)
        {
            Vector3 localPos = moneyDrop.GetNextPosition();
            GameObject money = Instantiate(moneyPrefab, moneyDrop.transform.position + Vector3.up * 3f, Quaternion.Euler(-90f, 0f, 0f));

            Vector3 targetWorldPos = moneyDrop.transform.TransformPoint(localPos);
            int capturedIndex = moneyDrop.Count;
            moneyDrop.AddItem(money);

            money.transform.position = moneyDrop.transform.position + Vector3.up * 3f;
            ItemTweenHelper.MoveArc(money.transform, targetWorldPos, 0.3f, 1f)
                .OnComplete(() =>
                {
                    money.transform.localPosition = moneyDrop.GetStackPosition(capturedIndex);
                });

            yield return new WaitForSeconds(moneySpawnInterval);
        }
    }

    IEnumerator MoveNPC(GameObject npc, Vector3 target)
    {
        if (npc == null) yield break;

        Animator anim = npc.GetComponent<Animator>();
        if (anim != null)
            anim.SetBool("isWalking", true);

        while (npc != null && Vector3.Distance(npc.transform.position, target) > 0.1f)
        {
            Vector3 dir = target - npc.transform.position;
            dir.y = 0f;
            dir.Normalize();

            npc.transform.position = Vector3.MoveTowards(npc.transform.position, target, npcMoveSpeed * Time.deltaTime);

            if (dir.sqrMagnitude > 0.01f)
                npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);

            yield return null;
        }

        if (npc != null)
            npc.transform.position = target;

        if (anim != null)
            anim.SetBool("isWalking", false);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Transform prev = waitingPoint;
        if (travelWaypoints != null)
        {
            for (int i = 0; i < travelWaypoints.Length; i++)
            {
                if (travelWaypoints[i] == null) continue;
                if (prev != null)
                    Gizmos.DrawLine(prev.position, travelWaypoints[i].position);
                Gizmos.DrawWireSphere(travelWaypoints[i].position, 0.3f);
                prev = travelWaypoints[i];
            }
        }

        if (prev != null && arriveZone != null)
            Gizmos.DrawLine(prev.position, arriveZone.transform.position);
    }
}
