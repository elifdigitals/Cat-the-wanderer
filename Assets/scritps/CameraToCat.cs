using UnityEngine;

public class CameraToCat : MonoBehaviour
{
    public Transform target;       // 🎯 цель — наш игрок
    public float smoothSpeed = 0.125f; // скорость сглаживания (чем меньше, тем больше задержка)
    public Vector3 offset;         // смещение камеры относительно игрока (например, (0,1,-10))

    private void LateUpdate()
    {
        if (target == null) return;

        // желаемая позиция камеры = позиция игрока + смещение
        Vector3 desiredPosition = target.position + offset;

        // плавное движение камеры от текущей позиции к желаемой
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // задаём позицию
        transform.position = smoothedPosition;
    }
}
