using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public int hp = 15;
    public int maxHp = 15;
    public float knockbackByHit = 0f;
    public Transform spawnPoint;
    public float invulnerabilityTime = 0.5f; // 1 секунда неуязвимости
    public float flashInterval = 0.1f;      // мигание каждые 0.1 сек

    private bool isInvulnerable = false;
    private Rigidbody2D rb;
    private Animator anim;
    private float defaultGravityScale;
    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // anim = GetComponent<Animator>();
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
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, knockbackByHit);
        if (hp <= 0)
        {
            GetComponent<EnemyHit>().enabled = false;
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityRoutine());
        }
    }

    void Die()
    {
        StartCoroutine(KnockbackRoutine());
    }

    IEnumerator KnockbackRoutine()
    {

        rb.linearVelocity = new Vector2(0, 20f);
        // yield return new WaitForSeconds(0.3f);
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        // anim.Play("jumpV2");
        // for (int i = 0; i < 6; i++)
        // {
        //     rb.linearVelocity = new Vector2(-30f, 0);
        //     yield return new WaitForSeconds(0.1f);
        //     rb.linearVelocity = new Vector2(30f, 0);
        //     yield return new WaitForSeconds(0.1f);
        // }
        rb.linearVelocity = new Vector2(0, 0);
        yield return new WaitForSeconds(3f);
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
        }
        hp = maxHp;
        rb.gravityScale = defaultGravityScale;
        GetComponent<EnemyHit>().enabled = true;
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
}
