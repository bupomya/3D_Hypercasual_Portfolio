using UnityEngine;

public class ExchangeNPCZone : PurchaseZone
{
    [Header("NPC 설정")]
    public GameObject npcPrefab;

    [Header("운반 설정")]
    public int maxCarry = 5;
    public float pickupInterval = 0.3f;
    public float dropInterval = 0.3f;

    [Header("경유 지점")]
    public Transform pickupPoint;
    public Transform exchangePoint;

    [Header("스택커")]
    public GridStacker sourceStacker;
    public GridStacker targetStacker;

    private GameObject spawnedNPC;

    protected override bool CanPurchase()
    {
        return spawnedNPC == null;
    }

    protected override void OnPurchaseComplete()
    {
        SpawnNPC();
    }

    void SpawnNPC()
    {
        Vector3 spawnPos = exchangePoint != null ? exchangePoint.position : transform.position;
        spawnedNPC = Instantiate(npcPrefab, spawnPos, Quaternion.identity);

        ExchangeNPCWorker worker = spawnedNPC.GetComponent<ExchangeNPCWorker>();
        if (worker != null)
        {
            worker.Setup(sourceStacker, targetStacker, pickupPoint, exchangePoint,
                maxCarry, pickupInterval, dropInterval);
            worker.StartWorking();
        }
    }
}
