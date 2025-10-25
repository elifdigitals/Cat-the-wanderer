using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int hp = 3;
    public int maxHp = 3;
    public float knockbackByHit = 50f;
    public Transform spawnPoint;

    private Rigidbody2D rb;
    private Animator anim;
    private float defaultGravityScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        defaultGravityScale = rb.gravityScale;
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
        }
    }
    public void ApplyDamage(int amount)
    {
        hp -= amount;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, knockbackByHit);
        if (hp <= 0)
        {
            GetComponent<CatControl>().enabled = false;
            Die();
        }
    }

    void Die()
    {
        StartCoroutine(KnockbackRoutine());
    }

    IEnumerator KnockbackRoutine()
    {

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 30f);
        yield return new WaitForSeconds(0.5f);
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        anim.Play("jumpV2");
        for (int i = 0; i < 6; i++)
        {
            rb.linearVelocity = new Vector2(-30f, 0);
            yield return new WaitForSeconds(0.1f);
            rb.linearVelocity = new Vector2(30f, 0);
            yield return new WaitForSeconds(0.1f);
        }
        rb.linearVelocity = new Vector2(0, 0);
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
        }
        hp = maxHp;
        rb.gravityScale = defaultGravityScale;
        GetComponent<CatControl>().enabled = true;
    }
}
