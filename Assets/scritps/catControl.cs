using UnityEngine;

public class CatControl : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;    // скорость движения по X (5f — см. ниже про 'f')
    public float jumpForce = 10f;   // импульс прыжка по Y (10f — float literal)
    public float jumpAmount = 2;

    [Header("Ground Check")]
    public Transform groundCheck;      // пустой объект, расположенный у ног персонажа
    public float groundRadius = 0.2f;  // радиус окружности для проверки земли (0.2f)
    public LayerMask groundLayer;      // слой(маска) для "земли" — выбирается в инспекторе

    // Приватные поля (не видны в инспекторе)
    private Rigidbody2D rb;       // ссылка на Rigidbody2D — через rb мы управляем физикой
    private bool isGrounded;      // флаг — стоим ли на земле
    private float moveInput;      // ввод игрока по горизонтали (-1,0,1)
    private Animator anim;
    private float jumpsLeft;

    void Start()
    {
        // При старте сцены получаем компонент Rigidbody2D, который прикреплён к тому же объекту.
        // <> здесь — это синтаксис обобщённого метода (GetComponent<T>()), подробнее ниже.
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        jumpsLeft = jumpAmount;

        // Рекомендуется зафиксировать вращение, чтобы физика не "крутила" спрайт при столкновениях.
        // Это эквивалентно в инспекторе поставить Constraints -> Freeze Rotation Z.
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        // Читаем ввод игрока — GetAxisRaw возвращает -1, 0 или 1 (мгновенно, без сглаживания).
        // Подходит для платформера, где нужен "чёткий" отклик на нажатие клавиш.
        moveInput = Input.GetAxisRaw("Horizontal");

        // Прыжок: если игрок нажал кнопку "Jump" (обычно пробел) и мы стоим на земле — прыгаем.
        // Input.GetButtonDown срабатывает в кадр нажатия.
        // if (Input.GetButtonDown("Jump") && isGrounded)
        if (Input.GetKeyDown(KeyCode.K) && jumpsLeft > 1)
        {
            // Устанавливаем вертикальную скорость напрямую.
            // rb.velocity — это Vector2 (структура). Мы задаём новую Vector2 с прежней скоростью по X.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpsLeft--;
            // Debug.Log(jumpsLeft);
            // anim.Play("jump");
        }
    }

    void FixedUpdate()
    {
        // Проверка: находится ли под ногами земля?
        // OverlapCircle создаёт невидимую окружность в groundCheck.position с радиусом groundRadius
        // и проверяет, есть ли коллайдеры, попадающие в маску groundLayer.
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // Движение по X: устанавливаем скорость по X в соответствии с вводом.
        // rb.velocity = new Vector2(скоростьПоX, текущаяСкоростьПоY)
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Разворот спрайта: меняем масштаб по X на 1 или -1, чтобы отразить спрайт.
        if (moveInput > 0f)
            transform.localScale = new Vector3(-1f, 1f, 1f);    // смотрим вправо
        else if (moveInput < 0f)
            transform.localScale = new Vector3(1f, 1f, 1f);   // смотрим влево

        if (isGrounded)
        {
            jumpsLeft = jumpAmount;
            // Debug.Log(jumpsLeft);
        }

        if (!isGrounded)
        {
                // anim.Play("jump"); // если в воздухе → Jump
                anim.Play("jumpV2");
            }
            else if (Mathf.Abs(moveInput) > 0.01f)
            {
                anim.Play("walkingCat"); // если двигаемся → Run
            }
            else
        {
                // anim.Play("stay", 0, 0f);
                anim.Play("stayV2");
                // сбрасываем Idle в начало (0-й кадр)
            }
    }

    // В редакторе рисуем кружок проверки земли, чтобы было удобно настраивать groundRadius.
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}
