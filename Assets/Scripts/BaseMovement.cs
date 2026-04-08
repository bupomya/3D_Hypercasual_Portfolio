using UnityEngine;

public abstract class BaseMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    protected CharacterController controller;
    protected Animator animator;

    protected virtual void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    protected void Move(Vector3 direction)
    {
        if (controller != null)
            controller.Move(direction * moveSpeed * Time.deltaTime);
        else
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
    }

    protected void Rotate(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public abstract Vector3 GetMoveDirection();
}
