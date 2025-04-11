using UnityEngine;
using UnityEngine.AI;
using StarterAssets;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
public class AITarget : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private NavMeshAgent m_Agent;
    [SerializeField] private GameObject m_Escape;
    [SerializeField] private float m_spawnRadius = 10f;
    [SerializeField] private float offsetDistance = 4f;

    [Header("Stuck Detection")]
    [SerializeField] private float stuckTimeout = 2f;
    [SerializeField] private float positionCheckInterval = 1f;
    private float _lastMoveTime;
    private Vector3 _lastPosition;
    private float _currentStuckTime;

    [Header("Player Settings")]
    [SerializeField] private Transform Target;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float ColliderDistance;

    [Header("Cooldown Settings")]
    [SerializeField] private float HitCooldownDuration = 4f;
    [SerializeField] private float EscapeCooldownDuration = 7f;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRange = 20f;
    private ThirdPersonController targetController;

    [Header("Dash Settings")]
    [SerializeField] private DashController dashController;
    private float m_Distance;
    private bool isHitCooldown;
    private bool isEscapeCooldown;
    private bool isEscaping;

    private float targetMoveSpeed;
    private float targetSprintSpeed;
    private PlayerCooldown _playerCooldown;
    
    void Start()
    {
        targetController = Target.GetComponent<ThirdPersonController>(); // Access the targetController component on the Target
        m_Agent = GetComponent<NavMeshAgent>(); // Access the NavMeshAgent component on the AI
        _playerCooldown = Target.GetComponent<PlayerCooldown>();

        // Get the speed reference from the targetController
        targetMoveSpeed = targetController.MoveSpeed;
        targetSprintSpeed = targetController.SprintSpeed;
        m_Agent.speed = targetController.MoveSpeed * 1.2f; // Match the AI speed to the Target speed

        StartCoroutine(CheckMovementProgress());

        Vector3 spawnPoint;
        if (RandomPoint(transform.position, m_spawnRadius, out spawnPoint)) // Spawn the AI at a random point within the spawn radius
        {
            m_Agent.transform.position = spawnPoint;
        }
        else
        {
            Debug.LogWarning("Failed to find a random point for AI spawn.");
        }

    }

    void Update()
    {
        if (GameRule.Instance == null) return;

        // Mettre à jour l'état du jeu dans PlayerCooldown
        bool isGamePlaying = GameRule.Instance.IsGamePlaying();
        bool isGameOver = GameRule.Instance.IsGameOver();
        _playerCooldown.SetGameActiveState(isGamePlaying && !isGameOver);

        // Gestion de l'IA
        m_Agent.isStopped = !isGamePlaying || isGameOver;

        if (!GameRule.Instance.IsGamePlaying() && !GameRule.Instance.IsGameOver())
        {
            m_Agent.isStopped = true;
        }
        else if (GameRule.Instance.IsGameOver())
        {
            m_Agent.isStopped = false;
        }
        else if (GameRule.Instance.IsGamePlaying())
        {
            m_Agent.isStopped = false;            
        }

        m_Distance = Vector3.Distance(m_Agent.transform.position, Target.position);
    
        if (m_Distance < ColliderDistance && !isHitCooldown)
        {
            StartCoroutine(HandleHitCooldown());
        }

        if (IsVisible())
        {
            TrackingBehavior();
        }

        if ((!m_Agent.hasPath || (m_Agent.hasPath && Time.time - _lastMoveTime > stuckTimeout)) 
            && !isHitCooldown)
        {
            PassiveBehavior();
        }
    }

    private IEnumerator HandleHitCooldown()
    {
        isHitCooldown = true;
    
        if(dashController != null)
            dashController.ResetDashes();

        _playerCooldown.StartCooldown();
        FindObjectOfType<GameRule>().RemoveLvl();

        if(!isEscapeCooldown)
        {
            StartCoroutine(HandleEscapeCooldown());
        }

        PlayerStats.Instance.AddHit();

        yield return new WaitForSeconds(HitCooldownDuration);

        _playerCooldown.EndCooldown();
        isHitCooldown = false;
    }

    private IEnumerator HandleEscapeCooldown()
    {
        isEscapeCooldown = true;
        isEscaping = true;

        // Set destination (random point or fallback)
        Vector3 escapePoint;
        bool validPoint = RandomPoint(transform.position, patrolRange, out escapePoint);
        m_Agent.destination = validPoint ? escapePoint : m_Escape.transform.position;
        m_Agent.speed *= 2;

        float timer = 0;
        
        // Check arrival and timeout
        while (timer < EscapeCooldownDuration && 
            !(!m_Agent.pathPending && m_Agent.remainingDistance <= m_Agent.stoppingDistance))
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Reset speed and state
        m_Agent.speed /= 2;
        isEscapeCooldown = false;
        isEscaping = false;
    }

    private IEnumerator CheckMovementProgress()
{
    while (true)
    {
        if (m_Agent.hasPath && m_Agent.remainingDistance > m_Agent.stoppingDistance)
        {
            // Check if position has changed significantly
            if (Vector3.Distance(transform.position, _lastPosition) < 0.1f)
            {
                _currentStuckTime += positionCheckInterval;
                if (_currentStuckTime >= stuckTimeout)
                {
                    FindNewDestination();
                    _currentStuckTime = 0f;
                }
            }
            else
            {
                _currentStuckTime = 0f;
                _lastPosition = transform.position;
            }
        }
        yield return new WaitForSeconds(positionCheckInterval);
    }
}

    private void TrackingBehavior()
    {
        if(!isEscaping && !isHitCooldown)
        {
            m_Agent.destination = Target.position; // Set the destination to the target position
        }
    }

    private void PassiveBehavior()
    {
        if (!m_Agent.pathPending && 
        (m_Agent.remainingDistance <= m_Agent.stoppingDistance || 
         _currentStuckTime >= stuckTimeout))
        {
            FindNewDestination();
        }
    }

    private void FindNewDestination()
    {
        Vector3 point;
        if (RandomPoint(transform.position, patrolRange, out point))
        {
            m_Agent.SetDestination(point);
            _currentStuckTime = 0f;
            _lastMoveTime = Time.time;
        }
    }

    private bool RandomPoint(Vector3 center, float range, out Vector3 result, int maxAttempts = 30) // Thanks to him https://youtu.be/dYs0WRzzoRc
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }

    private bool IsVisible() // Check if AI is visible to the camera and is directly on sight of the player
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        bool IsVisibleToCamera = planes.All(plane => plane.GetDistanceToPoint(m_Agent.transform.position) >= -offsetDistance);
        if (!IsVisibleToCamera) return false;

        Collider targetCollider = Target.GetComponent<Collider>();
        if (targetCollider == null) return false;

        Vector3 closestPointOnTarget = targetCollider.ClosestPoint(transform.position);
        Vector3 directionToClosestPoint = closestPointOnTarget - transform.position;

        if (Physics.Raycast( // Shoot a ray from the AI to the closest point on the target
            transform.position, 
            directionToClosestPoint, 
            out RaycastHit hit, 
            directionToClosestPoint.magnitude)
        )
        {
            return hit.transform == Target; // True if the ray hits the target
        }
        return false;
    }

    private void OnDrawGizmos() // Debugging
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ColliderDistance);
        Gizmos.DrawWireSphere(m_Escape.transform.position, 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, Target.position);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, m_Escape.transform.position);
    }
}
