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

    [Header("Collision Avoidance")]
    [SerializeField] private LayerMask obstacleMask = ~0;
    [SerializeField] private float collisionRadius = 0.45f;
    [SerializeField] private float collisionCastHeight = 0.25f;
    [SerializeField] private float obstacleCheckDistance = 0.35f;
    [SerializeField] private int directionPickAttempts = 8;

    private Vector3 startPosition;
    private Vector3 moveDirection;

    private float timer;
    private Rigidbody rb;
    private Collider[] ownColliders;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ownColliders = GetComponentsInChildren<Collider>();

        if (rb == null) return;

        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void Start()
    {
        startPosition = transform.position;
        collisionRadius = EstimateCollisionRadius();

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
    }

    private void FixedUpdate()
    {
        LockHeight();

        Move();
    }

    private void Move()
    {
        Vector3 desiredDirection = moveDirection;
        desiredDirection.y = 0f;

        if (desiredDirection.sqrMagnitude < 0.001f)
        {
            PickNewDirection();
            return;
        }

        desiredDirection.Normalize();
        desiredDirection = KeepInsideMovementRadius(desiredDirection);

        if (desiredDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        if (IsDirectionBlocked(desiredDirection, out RaycastHit hit))
        {
            Vector3 slideDirection = Vector3.ProjectOnPlane(desiredDirection, hit.normal);
            slideDirection.y = 0f;

            if (slideDirection.sqrMagnitude > 0.01f &&
                !IsDirectionBlocked(slideDirection.normalized, out _))
            {
                desiredDirection = slideDirection.normalized;
                moveDirection = desiredDirection;
            }
            else
            {
                PickNewDirectionAwayFrom(hit.normal);
                return;
            }
        }

        Vector3 nextPosition = GetCurrentPosition() +
                               desiredDirection * moveSpeed * Time.fixedDeltaTime;

        if (lockY)
        {
            nextPosition.y = fixedY;
        }

        MoveTo(nextPosition);
    }

    private Vector3 KeepInsideMovementRadius(Vector3 desiredDirection)
    {
        Vector3 currentFlat = Flatten(GetCurrentPosition());
        Vector3 nextFlat = currentFlat + desiredDirection * moveSpeed * Time.fixedDeltaTime;
        Vector3 startFlat = Flatten(startPosition);

        if (Vector3.Distance(nextFlat, startFlat) <= movementRadius)
        {
            return desiredDirection;
        }

        Vector3 backDirection = startFlat - currentFlat;

        if (backDirection.sqrMagnitude < 0.001f)
        {
            return Vector3.zero;
        }

        moveDirection = backDirection.normalized;
        return moveDirection;
    }

    private void PickNewDirection()
    {
        for (int i = 0; i < directionPickAttempts; i++)
        {
            Vector3 randomDirection = GetRandomFlatDirection();

            if (!IsDirectionBlocked(randomDirection, out _))
            {
                moveDirection = randomDirection;
                return;
            }
        }

        moveDirection = -moveDirection.normalized;
    }

    private void LockHeight()
    {
        if (!lockY) return;

        Vector3 pos = GetCurrentPosition();
        pos.y = fixedY;
        MoveTo(pos);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 avoidDirection = Vector3.zero;

        for (int i = 0; i < collision.contactCount; i++)
        {
            avoidDirection += collision.GetContact(i).normal;
        }

        PickNewDirectionAwayFrom(avoidDirection);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        PickNewDirectionAwayFrom(hit.normal);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(transform.position, movementRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * collisionCastHeight, collisionRadius);
    }

    private void PickNewDirectionAwayFrom(Vector3 surfaceNormal)
    {
        surfaceNormal.y = 0f;

        if (surfaceNormal.sqrMagnitude > 0.001f)
        {
            surfaceNormal.Normalize();

            for (int i = 0; i < directionPickAttempts; i++)
            {
                Vector3 randomDirection = GetRandomFlatDirection();

                if (Vector3.Dot(randomDirection, surfaceNormal) > 0.2f &&
                    !IsDirectionBlocked(randomDirection, out _))
                {
                    moveDirection = randomDirection;
                    return;
                }
            }

            if (!IsDirectionBlocked(surfaceNormal, out _))
            {
                moveDirection = surfaceNormal;
                return;
            }
        }

        PickNewDirection();
    }

    private bool IsDirectionBlocked(Vector3 direction, out RaycastHit closestHit)
    {
        closestHit = default;

        if (direction.sqrMagnitude < 0.001f)
        {
            return false;
        }

        direction.Normalize();

        float castDistance = moveSpeed * Time.fixedDeltaTime + obstacleCheckDistance;
        Vector3 origin = GetCurrentPosition() + Vector3.up * collisionCastHeight;
        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            collisionRadius,
            direction,
            castDistance,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        float closestDistance = float.MaxValue;
        bool blocked = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null || IsOwnCollider(hit.collider))
            {
                continue;
            }

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestHit = hit;
                blocked = true;
            }
        }

        return blocked;
    }

    private bool IsOwnCollider(Collider other)
    {
        if (ownColliders == null) return false;

        foreach (Collider ownCollider in ownColliders)
        {
            if (ownCollider == other)
            {
                return true;
            }
        }

        return false;
    }

    private Vector3 GetRandomFlatDirection()
    {
        Vector2 random = Random.insideUnitCircle;

        if (random.sqrMagnitude < 0.001f)
        {
            random = Vector2.right;
        }

        random.Normalize();
        return new Vector3(random.x, 0f, random.y);
    }

    private Vector3 GetCurrentPosition()
    {
        return rb != null ? rb.position : transform.position;
    }

    private void MoveTo(Vector3 position)
    {
        if (rb != null)
        {
            rb.MovePosition(position);
        }
        else
        {
            transform.position = position;
        }
    }

    private float EstimateCollisionRadius()
    {
        if (ownColliders == null || ownColliders.Length == 0)
        {
            return collisionRadius;
        }

        float radius = collisionRadius;

        foreach (Collider ownCollider in ownColliders)
        {
            if (ownCollider == null || ownCollider.isTrigger)
            {
                continue;
            }

            Bounds bounds = ownCollider.bounds;
            float horizontalExtent = Mathf.Min(bounds.extents.x, bounds.extents.z);
            radius = Mathf.Max(radius, horizontalExtent * 0.9f);
        }

        return Mathf.Clamp(radius, 0.2f, 1f);
    }

    private static Vector3 Flatten(Vector3 value)
    {
        return new Vector3(value.x, 0f, value.z);
    }
}
