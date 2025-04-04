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

    [Header("Player Settings")]
    [SerializeField] private Transform Target;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float ColliderDistance;

    [Header("Cooldown Settings")]
    [SerializeField] private float HitCooldownDuration = 3f;
    [SerializeField] private float EscapeCooldownDuration = 5f;

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
    
    void Start()
    {
        targetController = Target.GetComponent<ThirdPersonController>(); // Access the targetController component on the Target
        m_Agent = GetComponent<NavMeshAgent>(); // Access the NavMeshAgent component on the AI

        // Get the speed reference from the targetController
        targetMoveSpeed = targetController.MoveSpeed;
        targetSprintSpeed = targetController.SprintSpeed;
        m_Agent.speed = targetController.MoveSpeed * 1.2f; // Match the AI speed to the Target speed

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
        m_Distance = Vector3.Distance(m_Agent.transform.position, Target.position); // Calculate the distance to the target
        
        if (m_Distance < ColliderDistance)
        {
            if(!isHitCooldown)
            {
                StartCoroutine(HandleHitCooldown());
            }
        }

        if (IsVisible() == true)
        {
            TrackingBehavior();
        }

        if (!m_Agent.hasPath && !isHitCooldown)
        {
            PassiveBehavior();
        }
    }

    private IEnumerator HandleHitCooldown()
    {
        isHitCooldown = true;
        if(dashController != null)
        dashController.SetCanDash(false);

        // Disable movement and sprint
        targetController.MoveSpeed = 0f;
        targetController.SprintSpeed = 0f;
        FindObjectOfType<GameRule>().RemoveLvl();

        if(!isEscapeCooldown)
        {
            StartCoroutine(HandleEscapeCooldown());
        }

        yield return new WaitForSeconds(HitCooldownDuration);

        if(dashController != null)
        dashController.SetCanDash(true);
        
        isHitCooldown = false;

        // Re-enable movement and sprint
        targetController.MoveSpeed = targetMoveSpeed;
        targetController.SprintSpeed = targetSprintSpeed;
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

    private void TrackingBehavior()
    {
        if(!isEscaping && !isHitCooldown)
        {
            m_Agent.destination = Target.position; // Set the destination to the target position
        }
    }

    private void PassiveBehavior()
    {
        Vector3 point;
        if (RandomPoint(transform.position, patrolRange, out point))
        {
            m_Agent.SetDestination(point);
        }
    }

    private bool RandomPoint(Vector3 center, float range, out Vector3 result, int maxAttempts = 150) // Thanks to him https://youtu.be/dYs0WRzzoRc
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
        float offsetDistance = 5f; // Offset of the camera detection
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
