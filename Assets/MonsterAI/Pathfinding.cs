using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentController : MonoBehaviour
{
    [SerializeField] private Transform goal;
    [SerializeField] private float repathRate = 3f;
    [SerializeField] private float searchRadius = 10f;
    [SerializeField] private float destinationTolerance = 1f;
    [SerializeField] private float arrivalThreshold = 2f;

    private NavMeshAgent agent;
    private float repathTimer;
    private Vector3 lastDestination;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoRepath = true;
        lastDestination = transform.position;
    }

    void Update()
    {
        if (goal == null || !agent.isOnNavMesh)
            return;

        float distToGoal = Vector3.Distance(transform.position, goal.position);

        NavMeshPath tempPath = new NavMeshPath();
        bool goalReachable = NavMesh.CalculatePath(transform.position, goal.position, NavMesh.AllAreas, tempPath)
                            && tempPath.status == NavMeshPathStatus.PathComplete;

        if (distToGoal <= arrivalThreshold && goalReachable)
        {
            if (NavMesh.SamplePosition(goal.position, out NavMeshHit goalHit, searchRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(goalHit.position);
            }

            if (!agent.pathPending && agent.remainingDistance <= arrivalThreshold)
                agent.isStopped = true;

            return;
        }

        repathTimer += Time.deltaTime;
        if (repathTimer >= repathRate)
        {
            repathTimer = 0f;
            UpdateDestination();
        }
    }

    void UpdateDestination()
    {
        Vector3 dirToGoal = (goal.position - transform.position).normalized;
        Vector3 probePoint = transform.position + dirToGoal * searchRadius;

        if (NavMesh.SamplePosition(probePoint, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
        {
            if (Vector3.Distance(hit.position, lastDestination) > destinationTolerance)
            {
                agent.SetDestination(hit.position);
                lastDestination = hit.position;
                agent.isStopped = false;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (goal != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, goal.position);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}
