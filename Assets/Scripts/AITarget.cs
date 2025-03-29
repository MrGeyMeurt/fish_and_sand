using UnityEngine;
using UnityEngine.AI;
using StarterAssets;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class AITarget : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private NavMeshAgent m_Agent;
    [SerializeField] private Animator m_Animator;
    [Header("Player Settings")]
    [SerializeField] private Transform Target;
    [SerializeField] private float ColliderDistance;

    [Header("Settings")]
    [SerializeField] private float CooldownDuration = 3f;

    private ThirdPersonController targetController;
    private float m_Distance;
    private bool isCooldown;
    private bool hasTriggered;

    private float targetMoveSpeed;
    private float targetSprintSpeed;
    void Start()
    {
        m_Animator = GetComponent<Animator>();
        targetController = Target.GetComponent<ThirdPersonController>(); // Access the targetController component on the Target
        m_Agent = GetComponent<NavMeshAgent>(); // Access the NavMeshAgent component on the AI

        // Get the speed reference from the targetController
        targetMoveSpeed = targetController.MoveSpeed;
        targetSprintSpeed = targetController.SprintSpeed;

        m_Agent.speed = targetController.MoveSpeed; // match the AI speed to the Target speed

        GetComponent<Animator>().Play("Moving");
    }

    void Update()
    {
        m_Distance = Vector3.Distance(m_Agent.transform.position, Target.position); // Calculate the distance to the target
        
        if (m_Distance < ColliderDistance)
        {
            if(!isCooldown && !hasTriggered)
            {
                hasTriggered = true;
                targetController.MoveSpeed = 0f;
                targetController.SprintSpeed = 0f;
                FindObjectOfType<GameRule>().RemoveLvl();
                StartCoroutine(Cooldown());
            }
        }
        else
        {
            hasTriggered = false;
            m_Agent.destination = Target.position; // Set the destination to the target position
            m_Animator.SetBool("Moving", true);
            
            if(!isCooldown)
            {
                targetController.MoveSpeed = targetMoveSpeed;
                targetController.SprintSpeed = targetSprintSpeed;
            }
        }
    }

    private IEnumerator Cooldown()
    {
        isCooldown = true;
        
        yield return new WaitForSeconds(CooldownDuration);
        
        isCooldown = false;

        if(m_Distance < ColliderDistance) // If the player is still stuck then reset the speed to the original values
        {
            targetController.MoveSpeed = targetMoveSpeed;
            targetController.SprintSpeed = targetSprintSpeed;
        }
    }
}
