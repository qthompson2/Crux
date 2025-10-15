using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentController : MonoBehaviour
{
    [SerializeField] private Transform goal;
    [SerializeField] private float repathRate = 3f;
    [SerializeField] private float minSearchRadius = 10f;
    [SerializeField] private float destinationTolerance = 1f;
    [SerializeField] private float arrivalThreshold = 2f;
    [SerializeField] private float maxSearchRadius = 20f;

    private NavMeshAgent agent;
    private float effectiveSearchRadius;
    private float repathTimer;
    private Vector3 lastDestination;
    private int failedSamples = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoRepath = true;
        lastDestination = transform.position;
        effectiveSearchRadius = minSearchRadius;
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
            if (NavMesh.SamplePosition(goal.position, out NavMeshHit goalHit, minSearchRadius, NavMesh.AllAreas))
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
        Vector3 probePoint = transform.position + dirToGoal * effectiveSearchRadius;

        if (NavMesh.SamplePosition(probePoint, out NavMeshHit hit, effectiveSearchRadius, NavMesh.AllAreas))
        {
            failedSamples = 0;
            effectiveSearchRadius = minSearchRadius;

            if (Vector3.Distance(hit.position, lastDestination) > destinationTolerance)
            {
                agent.SetDestination(hit.position);
                lastDestination = hit.position;
                agent.isStopped = false;
            }
        }
        else
        {
            failedSamples++;
            effectiveSearchRadius = Mathf.Min(minSearchRadius + failedSamples, maxSearchRadius);

            NavMeshPath tempPath = new NavMeshPath();
            if (NavMesh.CalculatePath(transform.position, goal.position, NavMesh.AllAreas, tempPath)
                && tempPath.status != NavMeshPathStatus.PathInvalid
                && tempPath.corners.Length > 1)
            {
                Vector3 partialDest = tempPath.corners[1];
                Vector3 smoothDest = Vector3.Lerp(lastDestination, partialDest, 0.2f);

                if (Vector3.Distance(smoothDest, lastDestination) > destinationTolerance)
                {
                    agent.SetDestination(smoothDest);
                    lastDestination = smoothDest;
                    agent.isStopped = false;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (goal != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, goal.position);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(goal.position, arrivalThreshold);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, effectiveSearchRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(lastDestination, 0.5f);
    }
}
