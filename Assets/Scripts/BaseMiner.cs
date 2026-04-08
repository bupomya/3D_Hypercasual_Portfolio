using UnityEngine;

public abstract class BaseMiner : MonoBehaviour
{
    [Header("Visual")]
    public GameObject[] humanVisuals;

    [Header("Mining Tool")]
    public GameObject miningTool;

    [Header("Mining Detection")]
    public Transform miningPos;
    public float miningRadius = 2f;
    public LayerMask mineralLayer;

    protected Animator animator;
    protected bool isMining;
    private bool inMiningZone;

    public bool IsMining => isMining;
    public bool IsInMiningZone => inMiningZone;

    public void SetInMiningZone(bool value) { inMiningZone = value; }

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();

        if (miningTool != null)
            miningTool.SetActive(false);

        if (MiningManager.Instance != null)
        {
            MiningManager.Instance.OnLevelChanged += ApplyLevelData;
            ApplyLevelData(MiningManager.Instance.CurrentLevel);
        }
    }

    protected virtual void OnDestroy()
    {
        if (MiningManager.Instance != null)
            MiningManager.Instance.OnLevelChanged -= ApplyLevelData;
    }

    public virtual void ApplyLevelData(int level)
    {
        MiningLevelData data = MiningManager.Instance.CurrentData;
        if (data == null) return;

        miningRadius = data.miningRadius;
    }

    public void ShowHuman(bool show)
    {
        for (int i = 0; i < humanVisuals.Length; i++)
        {
            if (humanVisuals[i] != null)
                humanVisuals[i].SetActive(show);
        }
    }

    public void SetMining(bool mining)
    {
        isMining = mining;

        if (animator != null)
            animator.SetBool("isMining", isMining);
        if (miningTool != null)
            miningTool.SetActive(isMining);
    }

    public virtual void StartAutoMining() { }
    public virtual void StopAutoMining() { }

    protected Mineral FindNearestMineral()
    {
        Vector3 origin = miningPos != null ? miningPos.position : transform.position;
        Collider[] hits = Physics.OverlapSphere(origin, miningRadius, mineralLayer);

        Mineral nearest = null;
        float minDist = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Mineral mineral = hits[i].GetComponent<Mineral>();
            if (mineral == null) continue;

            float dist = Vector3.Distance(origin, hits[i].transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = mineral;
            }
        }

        return nearest;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 origin = miningPos != null ? miningPos.position : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, miningRadius);
    }
}
