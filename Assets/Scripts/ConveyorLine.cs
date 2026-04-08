using UnityEngine;
using System.Collections;
using DG.Tweening;

public class ConveyorLine : MonoBehaviour
{
    [Header("Source")]
    public DropZone sourceDropZone;

    [Header("Movement")]
    public Transform moveTarget;
    public Transform[] waypoints;
    public float moveSpeed = 3f;
    public float sendInterval = 0.5f;

    [Header("Conversion")]
    public GameObject convertedPrefab;

    [Header("Output")]
    public GridStacker outputStacker;
    public float dropTweenDuration = 0.4f;
    public float dropTweenHeight = 1f;

    private bool isSending;

    void Update()
    {
        if (isSending) return;
        if (sourceDropZone == null || sourceDropZone.targetStacker == null) return;
        if (sourceDropZone.HasPendingTweens) return;
        if (sourceDropZone.targetStacker.Count == 0) return;

        StartCoroutine(SendRoutine());
    }

    IEnumerator SendRoutine()
    {
        isSending = true;
        GridStacker source = sourceDropZone.targetStacker;

        while (source.Count > 0)
        {
            GameObject item = source.PopItem();
            if (item == null) break;

            yield return StartCoroutine(MoveAndConvert(item));
            yield return new WaitForSeconds(sendInterval);
        }

        isSending = false;
    }

    IEnumerator MoveAndConvert(GameObject item)
    {
        // moveTarget까지 이동
        yield return StartCoroutine(MoveTo(item, moveTarget != null ? moveTarget.position : transform.position));
        if (item == null) yield break;

        // 변환
        if (convertedPrefab != null)
        {
            Vector3 pos = item.transform.position;
            Destroy(item);
            item = Instantiate(convertedPrefab, pos, Quaternion.identity);
        }

        // Waypoints 경유 이동
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            yield return StartCoroutine(MoveTo(item, waypoints[i].position));
            if (item == null) yield break;
        }

        if (outputStacker == null) yield break;

        // 마지막: GridStacker로 DOTween 드롭
        Vector3 targetLocalPos = outputStacker.GetNextPosition();
        Vector3 targetWorldPos = outputStacker.transform.TransformPoint(targetLocalPos);

        int capturedIndex = outputStacker.Count;
        outputStacker.AddItem(item);

        Vector3 startPos = item.transform.position;
        item.transform.position = startPos;

        ItemTweenHelper.MoveArc(item.transform, targetWorldPos, dropTweenDuration, dropTweenHeight)
            .OnComplete(() =>
            {
                item.transform.localPosition = outputStacker.GetStackPosition(capturedIndex);
            });
    }

    IEnumerator MoveTo(GameObject item, Vector3 target)
    {
        while (item != null && Vector3.Distance(item.transform.position, target) > 0.1f)
        {
            item.transform.position = Vector3.MoveTowards(item.transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        if (item != null)
            item.transform.position = target;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Transform prev = moveTarget;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            if (prev != null)
                Gizmos.DrawLine(prev.position, waypoints[i].position);
            Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);
            prev = waypoints[i];
        }

        if (prev != null && outputStacker != null)
            Gizmos.DrawLine(prev.position, outputStacker.transform.position);
    }
}
