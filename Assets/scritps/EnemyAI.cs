using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Idle, Chase }

    [Header("Скорости")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 4f;

    [Header("Агрессия")]
    public float chaseRange = 5f;
    public Transform player;

    [Header("Проверки")]
    public LayerMask groundLayer;
    public float groundCheckForward = 0.6f;  // насколько вперёд от центра смотрим на землю
    public float groundCheckDown = 1.0f;     // как далеко вниз смотрим
    public float wallCheckDistance = 0.35f;  // на какой дистанции считаем стену

    [Header("Защита от дрожания")]
    public float flipCooldown = 0.18f;       // минимальное время между разворотами
    public float minTravelBeforeFlip = 0.15f; // минимальное перемещение (в метрах) перед тем как разрешить новый flip

    [Header("Idle (передышка)")]
    public bool enableIdle = true;
    public float idleIntervalMin = 1.5f;    // минимальное время между паузами
    public float idleIntervalMax = 4f;      // максимальное время между паузами
    public float idleDurationMin = 0.6f;    // минимальная длительность паузы
    public float idleDurationMax = 1.8f;    // макс.длительность паузы

    Rigidbody2D rb;
    Collider2D col;
    EnemyHealth health;

    int direction = 1; // 1 вправо, -1 влево
    float lastFlipTime = -10f;
    Vector2 lastFlipPos;
    State currentState = State.Patrol;
    Coroutine idleCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        health = GetComponent<EnemyHealth>();

        // установить начальное направление — по игроку если есть, иначе вправо
        if (player != null)
            direction = (player.position.x >= transform.position.x) ? 1 : -1;
        else
            direction = 1;

        ApplyFlip();
        lastFlipPos = transform.position;

        if (enableIdle)
            StartCoroutine(IdleScheduler());
    }

    void Update()
    {
        if (health != null && health.hp <= 0) return;

        // переключение на Chase происходит в FixedUpdate (физика), но можно коротко проверить тут
        if (player != null)
        {
            float d = Vector2.Distance(transform.position, player.position);
            if (d <= chaseRange)
            {
                // отменяем запланированные паузы в момент агра
                if (idleCoroutine != null)
                {
                    StopCoroutine(idleCoroutine);
                    idleCoroutine = null;
                }
                currentState = State.Chase;
                return;
            }
            else
            {
                if (currentState == State.Chase)
                {
                    // вернуться в патруль
                    currentState = State.Patrol;
                    if (enableIdle && idleCoroutine == null)
                        idleCoroutine = StartCoroutine(IdleScheduler());
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (health != null && health.hp <= 0)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        switch (currentState)
        {
            case State.Patrol:
                PatrolStep();
                break;
            case State.Idle:
                rb.velocity = new Vector2(0f, rb.velocity.y);
                break;
            case State.Chase:
                ChaseStep();
                break;
        }
    }

    void PatrolStep()
    {
        // движение
        rb.velocity = new Vector2(direction * patrolSpeed, rb.velocity.y);

        // проверки
        bool noGroundAhead = !CheckGroundInDirection(direction);
        bool hitWall = CheckWallInDirection(direction);

        // минимальное смещение от последнего флипа, чтобы избежать дрожания
        float distSinceFlip = Mathf.Abs(transform.position.x - lastFlipPos.x);
        bool traveledEnough = distSinceFlip >= minTravelBeforeFlip;

        // разворот если нет земли впереди или есть стена, и прошло время cooldown и немного прошло расстояния
        if ((noGroundAhead || hitWall) && Time.time - lastFlipTime >= flipCooldown && traveledEnough)
        {
            FlipDirection();
        }
    }

    void ChaseStep()
    {
        if (player == null)
        {
            currentState = State.Patrol;
            return;
        }

        float dirToPlayer = player.position.x - transform.position.x;
        int chaseDir = dirToPlayer >= 0f ? 1 : -1;

        rb.velocity = new Vector2(chaseDir * chaseSpeed, rb.velocity.y);

        // если нужно сменить визуальное направление
        if (chaseDir != direction && Time.time - lastFlipTime >= flipCooldown)
        {
            direction = chaseDir;
            ApplyFlip();
            lastFlipTime = Time.time;
            lastFlipPos = transform.position;
        }
    }

    bool CheckGroundInDirection(int dir)
    {
        Vector2 origin = new Vector2(transform.position.x + dir * groundCheckForward, transform.position.y);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDown, groundLayer);
#if UNITY_EDITOR
        Debug.DrawRay(origin, Vector2.down * groundCheckDown, hit ? Color.green : Color.red);
#endif
        return hit.collider != null;
    }

    bool CheckWallInDirection(int dir)
    {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y);
        Vector2 dirVec = new Vector2(dir, 0);
        RaycastHit2D hit = Physics2D.Raycast(origin, dirVec, wallCheckDistance, groundLayer);
#if UNITY_EDITOR
        Debug.DrawRay(origin, dirVec * wallCheckDistance, hit ? Color.blue : Color.gray);
#endif
        return hit.collider != null;
    }

    void FlipDirection()
    {
        direction *= -1;
        ApplyFlip();
        lastFlipTime = Time.time;
        lastFlipPos = transform.position;

        // после флипа можно дать небольшой отступ назад, чтобы не стоять прямо на краю
        // сдвинуть позицию плавно не будем — просто немного подвинем transform (опционально)
        // transform.position = transform.position + new Vector3(direction * 0.02f, 0f, 0f);
    }

    void ApplyFlip()
    {
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (direction >= 0 ? 1 : -1);
        transform.localScale = s;
    }

    IEnumerator IdleScheduler()
    {
        // планируем случайные паузы пока враг в патруле (не в chase)
        while (true)
        {
            // ждём случайный интервал
            float wait = Random.Range(idleIntervalMin, idleIntervalMax);
            yield return new WaitForSeconds(wait);

            // не садимся в idle если сейчас в chase или здоровье ноль
            if (currentState == State.Chase || (health != null && health.hp <= 0))
                continue;

            // запускаем паузу
            currentState = State.Idle;
            float idleDur = Random.Range(idleDurationMin, idleDurationMax);
            yield return new WaitForSeconds(idleDur);

            // вернуть в патруль
            currentState = State.Patrol;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // если врезались в объект из groundLayer — разворот (контакт по бокам)
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            foreach (ContactPoint2D cp in collision.contacts)
            {
                if (Mathf.Abs(cp.normal.x) > 0.5f)
                {
                    if (Time.time - lastFlipTime >= flipCooldown && Mathf.Abs(transform.position.x - lastFlipPos.x) >= minTravelBeforeFlip)
                        FlipDirection();
                    break;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.yellow;
        Vector2 rightOrigin = new Vector2(transform.position.x + groundCheckForward, transform.position.y);
        Vector2 leftOrigin = new Vector2(transform.position.x - groundCheckForward, transform.position.y);
        Gizmos.DrawLine(rightOrigin, rightOrigin + Vector2.down * groundCheckDown);
        Gizmos.DrawLine(leftOrigin, leftOrigin + Vector2.down * groundCheckDown);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * wallCheckDistance);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * wallCheckDistance);
    }
}
