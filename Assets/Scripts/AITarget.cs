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
    [SerializeField] private GameObject m_escape;

    [Header("Player Settings")]
    [SerializeField] private Transform Target;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float ColliderDistance;

    [Header("Cooldown Settings")]
    [SerializeField] private float HitCooldownDuration = 2f;
    [SerializeField] private float EscapeCooldownDuration = 5f;

    private ThirdPersonController targetController;
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
        m_Agent.speed = targetController.MoveSpeed * 1.2f; // match the AI speed to the Target speed
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
    }

    private IEnumerator HandleHitCooldown()
    {
        isHitCooldown = true;

        // Disable movement and sprint
        targetController.MoveSpeed = 0f;
        targetController.SprintSpeed = 0f;
        FindObjectOfType<GameRule>().RemoveLvl();

        if(!isEscapeCooldown)
        {
            StartCoroutine(HandleEscapeCooldown());
        }

        yield return new WaitForSeconds(HitCooldownDuration);
        
        isHitCooldown = false;

        // Re-enable movement and sprint
        targetController.MoveSpeed = targetMoveSpeed;
        targetController.SprintSpeed = targetSprintSpeed;
    }

    private IEnumerator HandleEscapeCooldown()
    {
        isEscapeCooldown = true;
        isEscaping = true;
        m_Agent.destination = m_escape.transform.position; // Set the destination to the escape point
        m_Agent.speed = m_Agent.speed * 2;

        yield return new WaitForSeconds(EscapeCooldownDuration);
        
        m_Agent.speed = m_Agent.speed / 2;
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

    private bool IsVisible() // Check if AI is visible to the camera
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        float offsetDistance = 5f;
        return planes.All(plane => plane.GetDistanceToPoint(m_Agent.transform.position) >= -offsetDistance);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ColliderDistance);
        Gizmos.DrawWireSphere(m_escape.transform.position, 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, Target.position);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, m_escape.transform.position);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 8f);


    }
}
