using UnityEngine;

public class NPCMovement : BaseMovement
{
    private Transform targetPosition;

    public void SetTarget(Transform target)
    {
        targetPosition = target;
    }

    public override Vector3 GetMoveDirection()
    {
        if (targetPosition == null) return Vector3.zero;

        Vector3 dir = (targetPosition.position - transform.position);
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.1f) return Vector3.zero;

        return dir.normalized;
    }

    void Update()
    {
        Vector3 moveDir = GetMoveDirection();
        bool isMoving = moveDir.sqrMagnitude >= 0.01f;

        if (animator != null)
            animator.SetBool("isWalking", isMoving);

        if (!isMoving) return;

        Move(moveDir);
        Rotate(moveDir);
    }
}
