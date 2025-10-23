using UnityEngine;
using UnityEngine.InputSystem; // новый ввод

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Move (instant)")]
    public float moveSpeed = 8f;           // мгновенная скорость по X, без ускорения

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
    public Sprite stand;     // стойка
    public Sprite moving1;   // кадр 1
    public Sprite moving2;   // кадр 2
    public Sprite moving3;   // кадр 3
    public Sprite moving4;   // кадр 4
    public Sprite attackSprite; // атака (приоритетно)

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
    bool isAttacking;
    float attackTimer;

    // управление входом
    InputAction moveAction;
    InputAction jumpAction;
    InputAction attackAction;

    // последовательность кадров ходьбы (см. задание)
    // 1,2,3,2,1,4,3,2,1 ... (зациклено)
    readonly int[] walkSeq = new int[] { 1, 2, 3, 2, 1, 4, 3, 2, 1 };
    int walkIndex = 0;          // текущая позиция в последовательности
    bool wasMovingLastFrame;    // чтобы правильно стартовать с moving1

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

        if (stand != null) sr.sprite = stand;
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

        // направление (разворот через scale)
        if (moveAbs > 0.01f)
            transform.localScale = new Vector3(xInput > 0 ? 1 : -1, 1, 1);

        // если начали движение в этом кадре — стартуем строго с moving1
        bool isMovingNow = moveAbs > 0.01f;
        if (isMovingNow && !wasMovingLastFrame)
        {
            walkIndex = 0;       // индекс указывает на 'moving1' в последовательности (значение 1)
            walkTimer = 0f;
            SetWalkSpriteBySeqIndex(walkIndex);
        }
        wasMovingLastFrame = isMovingNow;

        if (isMovingNow)
        {
            // Переключаем кадры шага по кастомной последовательности
            walkTimer += Time.deltaTime;
            float frameTime = 1f / Mathf.Max(1f, walkFps);
            if (walkTimer >= frameTime)
            {
                walkTimer -= frameTime;
                walkIndex = (walkIndex + 1) % walkSeq.Length;
                SetWalkSpriteBySeqIndex(walkIndex);
            }
        }
        else
        {
            // Мгновенная остановка — стойка
            if (stand != null) sr.sprite = stand;
            walkTimer = 0f;
            walkIndex = 0;
        }
    }

    void FixedUpdate()
    {
        // === Мгновенное управление скоростью по X ===
        float moveAbs = Mathf.Abs(xInput);

        if (moveAbs > 0.01f)
        {
            rb.velocity = new Vector2(moveSpeed * Mathf.Sign(xInput), rb.velocity.y);
        }
        else
        {
            // сразу «встань на месте»: обнуляем X-скорость
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }

    void DoJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        bool grounded = Physics2D.OverlapCircle(feet.position, groundRadius, groundMask);
        if (!grounded && airJumpsLeft > 0) airJumpsLeft--;
    }

    void SetWalkSpriteBySeqIndex(int idx)
    {
        int frame = walkSeq[idx];
        switch (frame)
        {
            case 1: if (moving1 != null) sr.sprite = moving1; break;
            case 2: if (moving2 != null) sr.sprite = moving2; break;
            case 3: if (moving3 != null) sr.sprite = moving3; break;
            case 4: if (moving4 != null) sr.sprite = moving4; break;
            default: if (moving1 != null) sr.sprite = moving1; break;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (feet == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(feet.position, groundRadius);
    }
}
