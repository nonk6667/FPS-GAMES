using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TargetBounceAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float directionChangeInterval = 1.5f;
    [SerializeField] private float randomTurnStrength = 0.5f;

    [Header("Height Lock")]
    [SerializeField] private bool lockY = true;
    [SerializeField] private float fixedY = 1.4f;

    [Header("Bounce")]
    [SerializeField] private float wallBounceBoost = 1.1f;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private float directionTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
    }

    private void Start()
    {
        PickRandomDirection();
        directionTimer = directionChangeInterval;
    }

    private void FixedUpdate()
    {
        directionTimer -= Time.fixedDeltaTime;

        if (directionTimer <= 0f)
        {
            AddRandomTurn();
            directionTimer = directionChangeInterval;
        }

        Vector3 velocity = moveDirection * moveSpeed;

        if (lockY)
            velocity.y = 0f;

        rb.linearVelocity = velocity;

        if (lockY)
        {
            Vector3 pos = transform.position;
            pos.y = fixedY;
            transform.position = pos;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.contactCount <= 0) return;

        Vector3 normal = collision.GetContact(0).normal;

        moveDirection = Vector3.Reflect(moveDirection, normal).normalized;

        AddRandomTurn();

        rb.linearVelocity = moveDirection * moveSpeed * wallBounceBoost;
    }

    private void PickRandomDirection()
    {
        Vector3 dir = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        );

        if (dir.sqrMagnitude < 0.01f)
            dir = Vector3.forward;

        moveDirection = dir.normalized;
    }

    private void AddRandomTurn()
    {
        Vector3 offset = new Vector3(
            Random.Range(-randomTurnStrength, randomTurnStrength),
            0f,
            Random.Range(-randomTurnStrength, randomTurnStrength)
        );

        moveDirection = (moveDirection + offset).normalized;

        if (moveDirection.sqrMagnitude < 0.01f)
            moveDirection = Vector3.forward;
    }
}