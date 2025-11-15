using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CatControl : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float jumpAmount = 2;

    [Header("Dash Settings")]
    public KeyCode dashKey = KeyCode.LeftShift;
    public int maxDashes = 2;
    public float dashSpeed = 12f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 2f; // время восстановления одного слота

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("UI (dash indicators)")]
    public Image dashIcon1;
    public Image dashIcon2;
    public Color fullColor = Color.white;
    public Color emptyColor = new Color(1f,1f,1f,0.25f);

    [Header("Dash visual (no images)")]
    public TrailRenderer dashTrail;     // перетащите сюда TrailRenderer (опционально)
    public bool enableTrail = true;

    // приватные
    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveInput;
    private Animator anim;
    private float jumpsLeft;

    // слоты для дашей
    private bool[] slotUsed;
    private float[] slotEndTime;
    private int currentDashes;

    private bool isDashing = false;
    private Coroutine dashCoroutine;

    // параметры предотвращения одновременных рефиллов
    private float staggerOffset = 0.04f; // небольшой сдвиг времени (в секундах)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        jumpsLeft = jumpAmount;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (maxDashes <= 0) maxDashes = 1;
        slotUsed = new bool[maxDashes];
        slotEndTime = new float[maxDashes];
        for (int i = 0; i < maxDashes; i++)
        {
            slotUsed[i] = false;
            slotEndTime[i] = 0f;
        }
        currentDashes = maxDashes;
        UpdateDashUI();

        // выключаем трей по умолчанию
        if (dashTrail != null)
            dashTrail.emitting = false;
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.K) && jumpsLeft > 1)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpsLeft--;
        }

        if (Input.GetKeyDown(dashKey))
        {
            TryDash();
        }

        // проверка истечения кулдаунов — восстанавливаем только по одному за проход
        CheckRefills();
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        if (!isDashing)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

            if (moveInput > 0f)
                transform.localScale = new Vector3(-1f, 1f, 1f);
            else if (moveInput < 0f)
                transform.localScale = new Vector3(1f, 1f, 1f);
        }

        if (isGrounded)
        {
            jumpsLeft = jumpAmount;
        }

        if (!isGrounded)
            anim.Play("jumpV2");
        else if (Mathf.Abs(moveInput) > 0.01f && !isDashing)
            anim.Play("walkingCat");
        else
            anim.Play("stayV2");
    }

    private void TryDash()
    {
        if (isDashing) return;
        if (currentDashes <= 0) return;

        // найти первый свободный слот
        int useIndex = -1;
        for (int i = 0; i < slotUsed.Length; i++)
        {
            if (!slotUsed[i])
            {
                useIndex = i;
                break;
            }
        }

        if (useIndex == -1) return;

        // займём слот
        slotUsed[useIndex] = true;
        slotEndTime[useIndex] = Time.time + dashCooldown;
        currentDashes--;
        UpdateDashUI();

        // СТЕГГЕР: слегка сдвинуть остальные активные слоты, чтобы не совпали по времени.
        // Это предотвращает одновременное восстановление нескольких зарядов.
        for (int i = 0; i < slotUsed.Length; i++)
        {
            if (i == useIndex) continue;
            if (slotUsed[i])
            {
                // сдвигаем вперед небольшой интервал
                slotEndTime[i] += staggerOffset;
            }
        }

        // запускаем даш
        if (dashCoroutine != null) StopCoroutine(dashCoroutine);
        dashCoroutine = StartCoroutine(DoDash());
    }

    private IEnumerator DoDash()
    {
        isDashing = true;

        // делаем игрока неуязвимым (если есть PlayerHealth)
        var ph = GetComponent<PlayerHealth>();
        if (ph != null) ph.SetInvulnerable(true);

        // вкл. визуал трейла если есть
        if (dashTrail != null && enableTrail) dashTrail.emitting = true;

        // направление рывка
        float dir = 0f;
        if (Mathf.Abs(moveInput) > 0.01f) dir = Mathf.Sign(moveInput);
        else dir = transform.localScale.x < 0 ? 1f : -1f;

        float prevGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(0f, 0f);

        rb.linearVelocity = new Vector2(dir * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);
        
        rb.gravityScale = prevGravity;
        rb.linearVelocity = new Vector2(0f, 0f);
        // выключаем трейл и невосприимчивость
        if (dashTrail != null && enableTrail) dashTrail.emitting = false;
        isDashing = false;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.gravityScale = prevGravity;
        if (ph != null) ph.SetInvulnerable(false);
    }

    // Восстанавливаем не больше одного слота за вызов
    private void CheckRefills()
    {
        for (int i = 0; i < slotUsed.Length; i++)
        {
            if (slotUsed[i] && Time.time >= slotEndTime[i])
            {
                slotUsed[i] = false;
                slotEndTime[i] = 0f;
                currentDashes = Mathf.Min(currentDashes + 1, maxDashes);
                UpdateDashUI();

                // после восстановления одного — выходим, чтобы не восстанавливать второй в тот же кадр
                break;
            }
        }
    }

    private void UpdateDashUI()
    {
        if (dashIcon1 != null)
            dashIcon1.color = (currentDashes >= 1) ? fullColor : emptyColor;
        if (dashIcon2 != null)
            dashIcon2.color = (currentDashes >= 2) ? fullColor : emptyColor;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}
