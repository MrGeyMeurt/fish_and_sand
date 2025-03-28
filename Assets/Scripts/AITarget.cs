using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AITarget : MonoBehaviour
{
    [SerializeField] private NavMeshAgent m_Agent;
    [SerializeField] private Transform Target;
    [SerializeField] private float AttackDistance;
    private float m_Distance;

    void Start()
    {
        Debug.Log(Target.position);
        Debug.Log(m_Agent.transform.position);
    }

    void Update()
    {
        m_Distance = Vector3.Distance(m_Agent.transform.position, Target.position);
        
        if (m_Distance < AttackDistance)
        {
            Debug.Log("Target reached!");
            m_Agent.isStopped = true;
        }
        else
        {
            m_Agent.isStopped = false;
            m_Agent.destination = Target.position;
        }
    }
}
