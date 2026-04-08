using UnityEngine;
using System;
using System.Collections.Generic;

public class ArriveZone : MonoBehaviour
{
    public event Action OnNPCRegistered;
    [Header("수용 설정")]
    public int maxCapacity = 3;
    public int npcsPerRow = 3;
    public float npcSpacing = 1.5f;
    public float rowSpacing = 1.5f;

    private readonly List<GameObject> arrivedNPCs = new List<GameObject>();

    public int CurrentCount => arrivedNPCs.Count;
    public bool HasSpace => arrivedNPCs.Count < maxCapacity;

    public void RegisterNPC(GameObject npc)
    {
        if (npc == null) return;

        Vector3 pos = GetNPCPosition(arrivedNPCs.Count);
        npc.transform.position = pos;

        // ArriveZone 중심을 바라보게 회전
        Vector3 lookDir = transform.position - npc.transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.01f)
            npc.transform.rotation = Quaternion.LookRotation(lookDir);

        Animator anim = npc.GetComponent<Animator>();
        if (anim != null)
            anim.SetBool("isWalking", false);

        arrivedNPCs.Add(npc);
        OnNPCRegistered?.Invoke();
    }

    Vector3 GetNPCPosition(int index)
    {
        int row = index / npcsPerRow;
        int col = index % npcsPerRow;

        float offsetX = (col - (npcsPerRow - 1) * 0.5f) * npcSpacing;
        float offsetZ = -row * rowSpacing;

        return transform.position + transform.right * offsetX + transform.forward * offsetZ;
    }

    void OnDrawGizmos()
    {
        ItemTweenHelper.DrawZoneGizmos(GetComponent<BoxCollider>(), transform, new Color(1f, 0f, 1f));

        // NPC 배치 위치 미리보기
        Gizmos.color = Color.magenta;
        for (int i = 0; i < maxCapacity; i++)
        {
            Vector3 pos = GetNPCPosition(i);
            Gizmos.DrawWireSphere(pos, 0.3f);
        }
    }
}
