using UnityEngine;

public class RandomBotMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [SerializeField] private float changeDirectionTime = 2f;

    [SerializeField] private float movementRadius = 6f;

    [Header("Height Lock")]
    [SerializeField] private bool lockY = true;

    [SerializeField] private float fixedY = 1.39f;

    private Vector3 startPosition;
    private Vector3 moveDirection;

    private float timer;

    private void Start()
    {
        startPosition = transform.position;

        PickNewDirection();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= changeDirectionTime)
        {
            timer = 0f;
            PickNewDirection();
        }

        Move();

        LockHeight();
    }

    private void Move()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        float distance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(startPosition.x, 0, startPosition.z)
        );

        // 超出活动范围就返回
        if (distance >= movementRadius)
        {
            Vector3 backDirection =
                (startPosition - transform.position).normalized;

            backDirection.y = 0;

            moveDirection = backDirection;
        }
    }

    private void PickNewDirection()
    {
        Vector2 random = Random.insideUnitCircle.normalized;

        moveDirection = new Vector3(random.x, 0f, random.y);
    }

    private void LockHeight()
    {
        if (!lockY) return;

        Vector3 pos = transform.position;
        pos.y = fixedY;
        transform.position = pos;
    }

    private void OnCollisionEnter(Collision collision)
    {
        PickNewDirection();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        PickNewDirection();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(transform.position, movementRadius);
    }
}