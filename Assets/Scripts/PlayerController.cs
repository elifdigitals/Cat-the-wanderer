using UnityEngine;
using UnityEngine.InputSystem; // новый ввод

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 8f;
    public float acceleration = 15f;
    public float deceleration = 20f;
    public float airControlMultiplier = 0.6f;

    [Header("Jump")]
    public float jumpForce = 13f;
    public float coyoteTime = 0.1f;
    public float jumpBuffer = 0.1f;
    public int extraAirJumps = 0;

    [Header("Ground Check")]
    public Transform feet;
    public float groundRadius = 0.08f;
    public LayerMask groundMask;

    [Header("Sprites")]
    public Sprite idleSprite;   // 1. стойка
    public Sprite walkASprite;  // 2. шаг левая
    public Sprite walkBSprite;  // 3. шаг правая
    public Sprite attackSprite; // 4. атака

    [Header("Sprite Animation")]
    [Tooltip("Скорость переключения кадров шага (кадров/сек).")]
    public float walkFps = 8f;
    [Tooltip("Длительность отображения спрайта атаки (сек).")]
    public float attackDuration = 0.18f;

    Rigidbody2D rb;
    SpriteRenderer sr;

    float xInput;
    float lastGroundedTime;
    float lastJumpPressedTime;
    int airJumpsLeft;

    // таймеры/флаги спрайтов
    float walkTimer;
    bool useWalkA = true;
    bool isAttacking;
    float attackTimer;

    // Input Actions
    InputAction moveAction;
    InputAction jumpAction;
    InputAction attackAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        // Горизонталь: A/D, стрелки, левый/правый стик
        moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick/x");
        moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/d")
            .With("Positive", "<Keyboard>/rightArrow");

        // Прыжок: Space / кнопка South (A)
        jumpAction = new InputAction("Jump", binding: "<Keyboard>/space");
        jumpAction.AddBinding("<Gamepad>/buttonSouth");

        // Атака: J / кнопка West (X) или правый бампер
        attackAction = new InputAction("Attack", binding: "<Keyboard>/j");
        attackAction.AddBinding("<Gamepad>/buttonWest");
        attackAction.AddBinding("<Gamepad>/rightShoulder");

        if (idleSprite != null) sr.sprite = idleSprite;
    }

    void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        attackAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        attackAction.Disable();
    }

    void Start()
    {
        airJumpsLeft = extraAirJumps;
    }

    void Update()
    {
        // === Ввод ===
        xInput = moveAction.ReadValue<float>();

        if (jumpAction.WasPressedThisFrame())
            lastJumpPressedTime = Time.time;

        bool attackPressed = attackAction.WasPressedThisFrame();

        // === Контакты с землёй ===
        bool grounded = Physics2D.OverlapCircle(feet.position, groundRadius, groundMask);
        if (grounded)
        {
            lastGroundedTime = Time.time;
            airJumpsLeft = extraAirJumps;
        }

        // === Прыжок (койот + буфер) ===
        bool coyoteOk = Time.time - lastGroundedTime <= coyoteTime;
        bool bufferOk = Time.time - lastJumpPressedTime <= jumpBuffer;
        if (bufferOk && (coyoteOk || airJumpsLeft > 0))
        {
            DoJump();
            lastJumpPressedTime = -999f;
        }

        // === Атака (приоритетная) ===
        if (attackPressed && !isAttacking)
        {
            isAttacking = true;
            attackTimer = attackDuration;
            if (attackSprite != null) sr.sprite = attackSprite;
        }

        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                isAttacking = false; // вернёмся к логике ходьбы/стояния ниже
            }
            else
            {
                return; // пока идёт атака — держим attackSprite
            }
        }

        // === Спрайты: ходьба/стойка ===
        float moveAbs = Mathf.Abs(xInput);

        // направление (как у тебя — через scale)
        if (moveAbs > 0.01f)
            transform.localScale = new Vector3(xInput > 0 ? 1 : -1, 1, 1);

        if (moveAbs > 0.01f)
        {
            // Переключаем кадры шага 2↔3 с частотой walkFps
            walkTimer += Time.deltaTime;
            float frameTime = 1f / Mathf.Max(1f, walkFps);
            if (walkTimer >= frameTime)
            {
                walkTimer -= frameTime;
                useWalkA = !useWalkA;
            }
            if (useWalkA && walkASprite != null) sr.sprite = walkASprite;
            else if (!useWalkA && walkBSprite != null) sr.sprite = walkBSprite;
        }
        else
        {
            // Стоим
            if (idleSprite != null) sr.sprite = idleSprite;
            walkTimer = 0f;
            useWalkA = true;
        }
    }

    void FixedUpdate()
    {
        bool grounded = Physics2D.OverlapCircle(feet.position, groundRadius, groundMask);

        float desired = xInput * moveSpeed;
        float accel = Mathf.Abs(desired) > 0.01f
            ? (grounded ? acceleration : acceleration * airControlMultiplier)
            : (grounded ? deceleration : deceleration * 0.5f);

        float newX = Mathf.MoveTowards(rb.velocity.x, desired, accel * Time.fixedDeltaTime);
        rb.velocity = new Vector2(newX, rb.velocity.y);
    }

    void DoJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        bool grounded = Physics2D.OverlapCircle(feet.position, groundRadius, groundMask);
        if (!grounded && airJumpsLeft > 0) airJumpsLeft--;
    }

    void OnDrawGizmosSelected()
    {
        if (feet == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(feet.position, groundRadius);
    }
}
