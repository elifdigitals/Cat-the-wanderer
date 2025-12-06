using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatformVelocity : MonoBehaviour
{
    [Header("Path")]
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;

    [Header("Interaction")]
    public Transform playerTransform;      // drag player сюда, если не указан — будет попытка найти по тегу "Player"
    public float interactRadius = 2f;      // радиус, в котором можно нажать E
    public bool requirePlayerInRadius = true; // если false — E работает из любой точки
    public bool toggleWithE = false;       // если true — E переключает движение (вкл/выкл). Если false — одноразовый старт

    [Header("Behavior")]
    public bool startOnAwake = false;      // начать движение сразу при старте (для теста)

    // Текущее вычисленное ускорение/скорость платформы (как было)
    public Vector2 currentVelocity { get; private set; }

    Rigidbody2D rb;
    Vector3 lastPos;
    Vector3 target;
    bool isMoving = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (pointA == null || pointB == null)
            Debug.LogWarning("MovingPlatformVelocity: назначьте pointA и pointB в инспекторе.");

        target = (pointB != null) ? pointB.position : transform.position;
        lastPos = transform.position;

        if (playerTransform == null)
        {
            var found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) playerTransform = found.transform;
        }

        if (startOnAwake)
            isMoving = true;
    }

    void Update()
    {
        // Проверяем нажатие E в Update (ввод должен обрабатываться тут)
        if (Input.GetKeyDown(KeyCode.E))
        {
            bool inRange = true;
            if (requirePlayerInRadius && playerTransform != null)
            {
                float d = Vector2.Distance(playerTransform.position, transform.position);
                inRange = d <= interactRadius;
            }
            else if (requirePlayerInRadius && playerTransform == null)
            {
                // если требуется радиус, но игрок не назначен — не разрешаем
                inRange = false;
            }

            if (inRange)
            {
                if (toggleWithE)
                {
                    isMoving = !isMoving;
                }
                else
                {
                    // одноразовый старт — включаем движение (если уже включено, не выключаем)
                    if (!isMoving) isMoving = true;
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (pointA == null || pointB == null)
        {
            // Если нет точек — ничего не двигаем, но обнуляем velocity
            currentVelocity = Vector2.zero;
            lastPos = transform.position;
            return;
        }

        if (isMoving)
        {
            Vector3 newPos = Vector3.MoveTowards(transform.position, target, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            if (Vector3.Distance(newPos, target) < 0.05f)
            {
                // меняем цель на противоположную
                target = (target == pointA.position) ? pointB.position : pointA.position;
            }

            currentVelocity = (newPos - lastPos) / Time.fixedDeltaTime;
            lastPos = newPos;
        }
        else
        {
            // Когда стоим на месте — нулевая скорость и синхронизация lastPos
            currentVelocity = Vector2.zero;
            lastPos = transform.position;
        }
    }

    void OnDrawGizmosSelected()
    {
        // рисуем радиус взаимодействия и точки A/B
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);

        Gizmos.color = Color.yellow;
        if (pointA != null) Gizmos.DrawWireSphere(pointA.position, 0.1f);
        if (pointB != null) Gizmos.DrawWireSphere(pointB.position, 0.1f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, (pointA != null) ? pointA.position : transform.position);
        Gizmos.DrawLine(transform.position, (pointB != null) ? pointB.position : transform.position);
    }
}
