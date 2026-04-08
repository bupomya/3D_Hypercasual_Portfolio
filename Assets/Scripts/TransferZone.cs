using UnityEngine;
using System.Collections;

public abstract class TransferZone : MonoBehaviour
{
    [Header("Transfer Settings")]
    public float delay = 1f;
    public float interval = 0.3f;

    [Header("Animation")]
    public float tweenDuration = 0.4f;
    public float tweenHeight = 2f;

    [Header("Gizmos")]
    public Color gizmoColor = Color.yellow;

    protected PlayerMiner currentMiner;
    private float stayTimer;
    private bool isTransferring;

    void OnTriggerEnter(Collider other)
    {
        PlayerMiner miner = other.GetComponent<PlayerMiner>();
        if (miner == null) return;

        currentMiner = miner;
        stayTimer = 0f;
    }

    void OnTriggerStay(Collider other)
    {
        if (currentMiner == null || isTransferring) return;
        if (other.GetComponent<PlayerMiner>() == null) return;
        if (!CanTransfer()) return;

        stayTimer += Time.deltaTime;

        if (stayTimer >= delay)
        {
            stayTimer = 0f;
            StartCoroutine(TransferRoutine());
        }
    }

    void OnTriggerExit(Collider other)
    {
        PlayerMiner miner = other.GetComponent<PlayerMiner>();
        if (miner == null) return;

        currentMiner = null;
        stayTimer = 0f;
    }

    IEnumerator TransferRoutine()
    {
        isTransferring = true;

        while (currentMiner != null && CanTransfer())
        {
            TransferOne();
            yield return new WaitForSeconds(interval);
        }

        isTransferring = false;
    }

    protected abstract bool CanTransfer();
    protected abstract void TransferOne();

    void OnDrawGizmos()
    {
        ItemTweenHelper.DrawZoneGizmos(GetComponent<BoxCollider>(), transform, gizmoColor);
    }
}
