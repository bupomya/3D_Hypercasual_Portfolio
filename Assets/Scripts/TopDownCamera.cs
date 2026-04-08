using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Settings")]
    public float height = 15f;
    public float angle = 45f;
    public float smoothSpeed = 5f;

    [Header("Rotation")]
    public float yRotation = 0f;

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        float rad = angle * Mathf.Deg2Rad;
        Vector3 localOffset = new Vector3(0f, height * Mathf.Sin(rad), -height * Mathf.Cos(rad));
        Vector3 rotatedOffset = Quaternion.Euler(0f, yRotation, 0f) * localOffset;

        Vector3 desiredPosition = target.position + rotatedOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(angle, yRotation, 0f);
    }
}
