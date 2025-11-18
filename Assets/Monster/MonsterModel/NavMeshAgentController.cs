using UnityEngine;
using UnityEngine.AI;

public class NavMeshAgentController : MonoBehaviour
{
    [SerializeField] private Transform target; // Assign in Inspector
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }

    void Update()
    {
        // Optional: Continuously update destination if target moves
        if (target != null && agent.destination != target.position)
        {
            agent.SetDestination(target.position);
        }
    }
}