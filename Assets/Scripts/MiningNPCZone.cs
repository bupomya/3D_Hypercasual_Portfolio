using UnityEngine;

public class MiningNPCZone : PurchaseZone
{
    [Header("NPC 설정")]
    public GameObject npcPrefab;
    public int npcCount = 2;

    [Header("채광 레벨 설정")]
    public int miningDamage = 1;
    public float miningRadius = 5f;
    public float autoMiningInterval = 1f;

    [Header("출력")]
    public GridStacker mineralDrop;
    public GameObject mineralItemPrefab;

    private GameObject[] spawnedNPCs;

    protected override bool CanPurchase()
    {
        return spawnedNPCs == null;
    }

    protected override void OnPurchaseComplete()
    {
        SpawnNPCs();
    }

    void SpawnNPCs()
    {
        spawnedNPCs = new GameObject[npcCount];
        BoxCollider col = GetComponent<BoxCollider>();

        for (int i = 0; i < npcCount; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition(col);
            GameObject npc = Instantiate(npcPrefab, spawnPos, Quaternion.identity);

            NPCMiner miner = npc.GetComponent<NPCMiner>();
            if (miner != null)
            {
                miner.Setup(mineralDrop, mineralItemPrefab, miningDamage, miningRadius, autoMiningInterval);
                miner.StartAutoMining();
            }

            spawnedNPCs[i] = npc;
        }
    }

    Vector3 GetRandomSpawnPosition(BoxCollider col)
    {
        if (col == null) return transform.position;

        Vector3 center = transform.TransformPoint(col.center);
        Vector3 size = Vector3.Scale(col.size, transform.lossyScale);

        float x = Random.Range(-size.x * 0.4f, size.x * 0.4f);
        float z = Random.Range(-size.z * 0.4f, size.z * 0.4f);

        return new Vector3(center.x + x, transform.position.y, center.z + z);
    }
}
