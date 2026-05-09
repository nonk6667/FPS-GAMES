using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyStealthPatrolAI : MonoBehaviour
{
    private static readonly List<EnemyStealthPatrolAI> AllAgents = new List<EnemyStealthPatrolAI>();

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float speedRandomness = 0.4f;
    [SerializeField] private float reachDistance = 0.35f;

    [Header("Waypoint Wait")]
    [SerializeField] private float minWaitTime = 0.2f;
    [SerializeField] private float maxWaitTime = 1.0f;

    [Header("Separation")]
    [SerializeField] private float separationRadius = 2.0f;
    [SerializeField] private float separationWeight = 2.5f;

    [Header("Height Lock")]
    [SerializeField] private bool lockY = true;
    [SerializeField] private float fixedY = 1.39f;

    [Header("Proximity Alert")]
    [SerializeField] private Transform player;
    [SerializeField] private float alertRange = 4f;
    [SerializeField] private float alertTimeRequired = 5f;
    [SerializeField] private bool ignoreCrouchingForProximity = true;
    [SerializeField] private string failSceneName = "start";

    [Header("Optional Vision Detection")]
    [SerializeField] private bool enableVisionDetection = false;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float detectionAngle = 60f;
    [SerializeField] private float crouchHeightThreshold = 1.4f;
    [SerializeField] private LayerMask obstacleMask;

    private int currentPointIndex;
    private float actualSpeed;
    private float waitTimer;
    private float proximityTimer;

    private CharacterController playerController;
    private Rigidbody rb;
    private Collider[] ownColliders;

    private void OnEnable()
    {
        if (!AllAgents.Contains(this))
            AllAgents.Add(this);
    }

    private void OnDisable()
    {
        AllAgents.Remove(this);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ownColliders = GetComponentsInChildren<Collider>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.freezeRotation = true;
        }
    }

    private void Start()
    {
        if (player != null)
            playerController = player.GetComponent<CharacterController>();

        actualSpeed = patrolSpeed + Random.Range(-speedRandomness, speedRandomness);
        actualSpeed = Mathf.Max(0.5f, actualSpeed);

        if (patrolPoints != null && patrolPoints.Length > 0)
            currentPointIndex = Random.Range(0, patrolPoints.Length);

        waitTimer = Random.Range(minWaitTime, maxWaitTime);

        if (lockY)
        {
            Vector3 pos = transform.position;
            pos.y = fixedY;
            transform.position = pos;
        }

        IgnoreOtherEnemyCollisions();
    }

    private void Update()
    {
        Patrol();
        CheckProximityAlert();

        if (enableVisionDetection)
            DetectPlayerByVision();

        LockHeight();
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        if (patrolPoints[currentPointIndex] == null) return;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        Vector3 targetPosition = patrolPoints[currentPointIndex].position;

        if (lockY)
            targetPosition.y = fixedY;
        else
            targetPosition.y = transform.position.y;

        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude <= reachDistance)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            waitTimer = Random.Range(minWaitTime, maxWaitTime);
            return;
        }

        Vector3 targetDirection = toTarget.normalized;
        Vector3 separationDirection = GetSeparationDirection();

        Vector3 finalDirection = targetDirection + separationDirection * separationWeight;

        if (finalDirection.sqrMagnitude < 0.01f)
            finalDirection = targetDirection;

        finalDirection.y = 0f;
        finalDirection.Normalize();

        transform.position += finalDirection * actualSpeed * Time.deltaTime;

        if (finalDirection.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(finalDirection),
                8f * Time.deltaTime
            );
        }
    }

    private void CheckProximityAlert()
    {
        if (player == null) return;

        Vector3 enemyPos = transform.position;
        Vector3 playerPos = player.position;

        enemyPos.y = 0f;
        playerPos.y = 0f;

        float distance = Vector3.Distance(enemyPos, playerPos);

        if (distance <= alertRange)
        {
            if (!ignoreCrouchingForProximity && IsPlayerCrouching())
            {
                proximityTimer = 0f;
                return;
            }

            proximityTimer += Time.deltaTime;

            if (proximityTimer >= alertTimeRequired)
            {
                TriggerAlarm();
            }
        }
        else
        {
            proximityTimer = 0f;
        }
    }

    private void TriggerAlarm()
    {
        Debug.Log($"{name} triggered alarm. Player stayed too close for too long.");
        SceneManager.LoadScene(failSceneName);
    }

    private Vector3 GetSeparationDirection()
    {
        Vector3 separation = Vector3.zero;
        int nearbyCount = 0;

        foreach (EnemyStealthPatrolAI other in AllAgents)
        {
            if (other == null || other == this) continue;

            Vector3 difference = transform.position - other.transform.position;
            difference.y = 0f;

            float distance = difference.magnitude;

            if (distance > 0.01f && distance < separationRadius)
            {
                separation += difference.normalized / distance;
                nearbyCount++;
            }
        }

        if (nearbyCount > 0)
            separation /= nearbyCount;

        separation.y = 0f;
        return separation.normalized;
    }

    private void DetectPlayerByVision()
    {
        if (player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        float distanceToPlayer = toPlayer.magnitude;

        if (distanceToPlayer > detectionRange) return;

        float angleToPlayer = Vector3.Angle(transform.forward, toPlayer.normalized);

        if (angleToPlayer > detectionAngle * 0.5f) return;

        if (IsPlayerCrouching()) return;

        Vector3 eyePosition = transform.position + Vector3.up * 1.5f;
        Vector3 playerPosition = player.position + Vector3.up * 1.0f;
        Vector3 rayDirection = playerPosition - eyePosition;

        int rayMask = obstacleMask.value == 0 ? ~0 : obstacleMask.value;

        if (Physics.Raycast(eyePosition, rayDirection.normalized, out RaycastHit hit, detectionRange, rayMask))
        {
            if (hit.transform == player || hit.transform.IsChildOf(player))
            {
                TriggerAlarm();
            }
        }
    }

    private bool IsPlayerCrouching()
    {
        if (playerController == null) return false;
        return playerController.height <= crouchHeightThreshold;
    }

    private void LockHeight()
    {
        if (!lockY) return;

        Vector3 pos = transform.position;
        pos.y = fixedY;
        transform.position = pos;
    }

    private void IgnoreOtherEnemyCollisions()
    {
        foreach (EnemyStealthPatrolAI other in AllAgents)
        {
            if (other == null || other == this) continue;

            Collider[] otherColliders = other.GetComponentsInChildren<Collider>();

            foreach (Collider own in ownColliders)
            {
                if (own == null) continue;

                foreach (Collider otherCol in otherColliders)
                {
                    if (otherCol == null) continue;
                    Physics.IgnoreCollision(own, otherCol, true);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, alertRange);

        if (enableVisionDetection)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Vector3 left = Quaternion.Euler(0, -detectionAngle * 0.5f, 0) * transform.forward;
            Vector3 right = Quaternion.Euler(0, detectionAngle * 0.5f, 0) * transform.forward;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, left * detectionRange);
            Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, right * detectionRange);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}