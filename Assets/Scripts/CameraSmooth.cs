using UnityEngine;

public class SmoothCamera2D : MonoBehaviour
{
    public Transform target;          // наш Player
    public float smoothSpeed = 0.125f; // чем меньше, тем плавнее (0.05–0.2 оптимально)
    public Vector3 offset;             // смещение камеры

    void LateUpdate()
    {
        if (target == null) return;

        // текущая и желаемая позиции
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
    }
}
