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

    private ThirdPersonController thirdPersonController;
    private float m_Distance;
    private bool isCooldown;

    private float targetMoveSpeed;
    private float targetSprintSpeed;
    void Start()
    {
        m_Animator = GetComponent<Animator>();
        thirdPersonController = Target.GetComponent<ThirdPersonController>(); // Access the ThirdPersonController component on the Target

        targetMoveSpeed = thirdPersonController.MoveSpeed;
        targetSprintSpeed = thirdPersonController.SprintSpeed;
    }

    void Update()
    {
        m_Distance = Vector3.Distance(m_Agent.transform.position, Target.position);
        
        if (m_Distance < ColliderDistance)
        {
            thirdPersonController.MoveSpeed = 0f;
            thirdPersonController.SprintSpeed = 0f;

            if(!isCooldown)
            {
                thirdPersonController.MoveSpeed = targetMoveSpeed;
                thirdPersonController.SprintSpeed = targetSprintSpeed;
            }
            
            FindObjectOfType<GameRule>().RemoveLvl();
            StartCoroutine(Cooldown());
        }
        else
        {
            m_Agent.destination = Target.position;
        }
    }

    private IEnumerator Cooldown()
    {
        isCooldown = true;
        Debug.Log("Cooldown started");
        
        yield return new WaitForSeconds(CooldownDuration);
        
        Debug.Log("Cooldown finished");
        isCooldown = false;
    }
}
