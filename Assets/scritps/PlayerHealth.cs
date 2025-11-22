using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int hp = 3;
    public int maxHp = 3;
    public float knockbackByHit = 50f;
    public Transform spawnPoint;
    public float invulnerabilityTime = 1.0f; // 1 секунда неуязвимости
    public float flashInterval = 0.1f;      // мигание каждые 0.1 сек

    // <-- Новая переменная: перетащите сюда коллайдер death zone (Is Trigger = true)
    public Collider2D deathZoneCollider;

    private bool isInvulnerable = false;
    private Rigidbody2D rb;
    private Animator anim;
    private float defaultGravityScale;
    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        defaultGravityScale = rb.gravityScale;
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
        }
    }

    public void ApplyDamage(int amount)
    {
        if (isInvulnerable) return;
        hp -= amount;
        rb.velocity = new Vector2(rb.velocity.x, knockbackByHit);
        if (hp <= 0)
        {
            GetComponent<CatControl>().enabled = false;
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityRoutine());
        }
    }

    // Сделано public — можно вызывать извне
    public void Die()
    {
        StartCoroutine(KnockbackRoutine());
    }

    IEnumerator KnockbackRoutine()
    {
        rb.velocity = new Vector2(0, 20f);
        yield return new WaitForSeconds(0.3f);
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
        if (anim != null) anim.Play("jumpV2");
        for (int i = 0; i < 6; i++)
        {
            rb.velocity = new Vector2(-30f, 0);
            yield return new WaitForSeconds(0.1f);
            rb.velocity = new Vector2(30f, 0);
            yield return new WaitForSeconds(0.1f);
        }
        rb.velocity = Vector2.zero;
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
        }
        hp = maxHp;
        rb.gravityScale = defaultGravityScale;
        GetComponent<CatControl>().enabled = true;
    }
    public void SetInvulnerable(bool value)
    {
        isInvulnerable = value;
    }
    public bool IsInvulnerable()
    {
        return isInvulnerable;
    }

    IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;

        float timer = 0f;
        while (timer < invulnerabilityTime)
        {
            // мигание спрайта
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }

        sr.enabled = true; // вернуть спрайт
        isInvulnerable = false;
    }

    // --- Новая логика: быстрый респон при столкновении с death zone ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        // если deathZoneCollider назначен и попали в него — мгновенный респаун
        if (deathZoneCollider != null && other == deathZoneCollider)
        {
            InstantRespawn();
        }
    }

    /// <summary>
    /// Мгновенно телепортирует игрока на spawnPoint, обнуляет скорость и восстанавливает hp.
    /// </summary>
    public void InstantRespawn()
    {
        // остановим все текущие корутины (например, мигание/анимации)
        StopAllCoroutines();

        // вернуть положение
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
        }

        // сброс физики
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = defaultGravityScale;

        // восстановление здоровья и состояний
        hp = maxHp;
        isInvulnerable = false;
        if (sr != null) sr.enabled = true;

        // включаем управление игроком (если было выключено)
        var control = GetComponent<CatControl>();
        if (control != null) control.enabled = true;
    }
}
