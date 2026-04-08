using UnityEngine;
using Terresquall;

public class PlayerMovement : BaseMovement
{
    private BaseMiner miner;
    private float idleTimer;
    private bool hintShown;

    protected override void Start()
    {
        base.Start();
        miner = GetComponent<BaseMiner>();
    }

    public override Vector3 GetMoveDirection()
    {
        Vector2 input = VirtualJoystick.GetAxis();
        return new Vector3(input.x, 0f, input.y).normalized;
    }

    void Update()
    {
        Vector3 moveDir = GetMoveDirection();
        bool isMoving = moveDir.sqrMagnitude >= 0.01f;

        bool isMining = miner != null && miner.IsMining;

        if (animator != null)
            animator.SetBool("isWalking", isMoving && !isMining);

        // Idle 힌트 처리
        bool isInMiningZone = miner != null && miner.IsInMiningZone;
        if (isMoving || isMining || isInMiningZone)
        {
            if (hintShown)
            {
                IdleHintUI.Instance?.Hide();
                hintShown = false;
            }
            idleTimer = 0f;
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (!hintShown && IdleHintUI.Instance != null && idleTimer >= IdleHintUI.Instance.idleThreshold)
            {
                IdleHintUI.Instance.Show();
                hintShown = true;
            }
        }

        if (!isMoving) return;

        Move(moveDir);
        Rotate(moveDir);
    }
}
