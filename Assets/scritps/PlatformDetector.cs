using UnityEngine;

public class PlatformDetector : MonoBehaviour
{
    public Vector2 platformVelocity { get; private set; } = Vector2.zero;

    private MovingPlatformVelocity currentPlatform; // компонент платформы, если стоим на ней

    void FixedUpdate()
    {
        // если стоим на платформе — обновляем скорость
        if (currentPlatform != null)
        {
            platformVelocity = currentPlatform.currentVelocity;
        }
        else
        {
            platformVelocity = Vector2.zero;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckPlatform(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckPlatform(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // если ушли с платформы
        if (currentPlatform != null &&
            collision.gameObject == currentPlatform.gameObject)
        {
            currentPlatform = null;
        }
    }

    void CheckPlatform(Collision2D collision)
    {
        // проверяем все точки контакта
        foreach (var contact in collision.contacts)
        {
            // Нормаль направлена от платформы к игроку.
            // Если normal.y > 0.5 — игрок стоит сверху платформы.
            if (contact.normal.y > 0.5f)
            {
                var platform = collision.collider.GetComponentInParent<MovingPlatformVelocity>();
                if (platform != null)
                {
                    currentPlatform = platform;
                    return;
                }
            }
        }
    }
}
