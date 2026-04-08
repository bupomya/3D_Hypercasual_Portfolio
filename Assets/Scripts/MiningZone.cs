using UnityEngine;

public class MiningZone : MonoBehaviour
{
    [Header("Mineral Spawning")]
    public GameObject mineralPrefab;
    public Transform mineralParent;
    public float mineralSpacing = 3f;
    public float mineralYOffset = 0f;

    private BaseMiner currentMiner;
    private GameObject spawnedWorker;
    private BoxCollider boxCollider;

    void Start()
    {
        if (MiningManager.Instance != null)
            MiningManager.Instance.OnLevelChanged += OnMiningLevelChanged;

        boxCollider = GetComponent<BoxCollider>();
        SpawnMinerals();
    }

    void OnDisable()
    {
        if (MiningManager.Instance != null)
            MiningManager.Instance.OnLevelChanged -= OnMiningLevelChanged;
    }

    void SpawnMinerals()
    {
        if (mineralPrefab == null || boxCollider == null) return;

        Vector3 center = boxCollider.center;
        Vector3 size = boxCollider.size;

        // 월드 스케일 적용된 실제 크기
        Vector3 worldSize = Vector3.Scale(size, transform.lossyScale);
        Vector3 worldCenter = transform.TransformPoint(center);

        float halfX = worldSize.x * 0.5f;
        float halfZ = worldSize.z * 0.5f;

        float startX = worldCenter.x - halfX + mineralSpacing * 0.5f;
        float endX = worldCenter.x + halfX;
        float startZ = worldCenter.z - halfZ + mineralSpacing * 0.5f;
        float endZ = worldCenter.z + halfZ;

        for (float x = startX; x < endX; x += mineralSpacing)
        {
            for (float z = startZ; z < endZ; z += mineralSpacing)
            {
                Vector3 pos = new Vector3(x, mineralYOffset, z);
                Instantiate(mineralPrefab, pos, Quaternion.identity, mineralParent);
            }
        }
    }

    void OnMiningLevelChanged(int newLevel)
    {
        if (currentMiner != null)
            SwapToWorker(currentMiner);
    }

    void DestroyWorker()
    {
        if (spawnedWorker != null)
        {
            Destroy(spawnedWorker);
            spawnedWorker = null;
        }
    }

    void SwapToWorker(BaseMiner miner)
    {
        DestroyWorker();
        miner.StopAutoMining();

        MiningLevelData data = MiningManager.Instance != null ? MiningManager.Instance.CurrentData : null;

        // 레벨 데이터 즉시 적용 (maxCarryCount, miningRadius 등)
        miner.ApplyLevelData(MiningManager.Instance.CurrentLevel);

        if (data == null || data.workerPrefab == null)
        {
            // Level 0: 애니메이션 기반 채광
            miner.ShowHuman(true);
            miner.SetMining(true);
            return;
        }

        // Level 1+: Worker 프리팹 + 자동 채광
        miner.ShowHuman(false);
        miner.SetMining(false);

        spawnedWorker = Instantiate(data.workerPrefab, miner.transform.position, miner.transform.rotation, miner.transform);
        miner.StartAutoMining();
    }

    void SwapToHuman(BaseMiner miner)
    {
        DestroyWorker();
        miner.StopAutoMining();
        miner.ShowHuman(true);
        miner.SetMining(false);
    }

    void OnTriggerEnter(Collider other)
    {
        BaseMiner miner = other.GetComponent<BaseMiner>();
        if (miner == null) return;

        currentMiner = miner;
        currentMiner.SetInMiningZone(true);
        SwapToWorker(currentMiner);
    }

    void OnTriggerExit(Collider other)
    {
        if (currentMiner == null) return;

        // spawnedWorker가 파괴될 때 발생하는 OnTriggerExit 무시
        if (other.gameObject == spawnedWorker) return;

        // BaseMiner 컴포넌트가 있는 오브젝트만 처리
        BaseMiner miner = other.GetComponent<BaseMiner>();
        if (miner == null) return;

        currentMiner.SetInMiningZone(false);
        SwapToHuman(currentMiner);
        currentMiner = null;
    }

    void OnDrawGizmos()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(col.center, col.size);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(col.center, col.size);
        Gizmos.matrix = oldMatrix;
    }
}
