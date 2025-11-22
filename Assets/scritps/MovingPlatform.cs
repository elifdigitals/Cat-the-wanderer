using UnityEngine;

public class MovingPlatformVelocity : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;

    public Vector2 currentVelocity { get; private set; }

    Rigidbody2D rb;
    Vector3 lastPos;
    Vector3 target;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        target = pointB.position;
        lastPos = transform.position;
    }

    void FixedUpdate()
    {
        Vector3 newPos = Vector3.MoveTowards(transform.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector3.Distance(newPos, target) < 0.05f)
        {
            target = (target == pointA.position) ? pointB.position : pointA.position;
        }

        currentVelocity = (newPos - lastPos) / Time.fixedDeltaTime;
        lastPos = newPos;
    }
}
