using UnityEngine;
using System.Collections;

public class MonsterStateManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Transform player;
    [SerializeField] public Transform target;
    [SerializeField] public AgentController agentController;

    [Header("Monster Chase Settings")]
    [SerializeField] public float chaseDuration = 10f;
    [SerializeField] public float reachThreshold = 2f;

    [Header("Monster Lurk Settings")]
    [SerializeField] public float visionDistance = 50f;
    [SerializeField] public float horizontalSweepSpeed = 60f;
    [SerializeField] public int verticalRayCount = 16;
    [SerializeField] public float verticalAngleRange = 60f;
    [SerializeField] public float lurkAngleRange = 30f;
    [SerializeField] public float lurkDuration = 6f;
    [SerializeField] public float minLurkDistance = 25f;
    [SerializeField] public float maxLurkDistance = 100f;

    [Header("Monster Flee Settings")]
    [SerializeField] public float minFleeDistance = 50f;
    [SerializeField] public float maxFleeDistance = 150f;
    [SerializeField] public float fleeDuration = 8f;

    public MonsterBaseState lurkingState;
    public MonsterBaseState chasingState;
    public MonsterBaseState fleeingState;

    [Header("Current State")]
    public MonsterBaseState currentState;

    public bool caughtPlayer = false;

    public float gizmoVisionDistance = 50f;
    public int gizmoVerticalRayCount = 6;
    public float gizmoVerticalAngleRange = 60f;
    public float gizmoHorizontalAngle = 0f;
    public bool gizmoShowVision = true;
    public bool playerInSight = false;

    void Awake()
    {
        agentController = GetComponent<AgentController>();
        lurkingState = new LurkingState(visionDistance, horizontalSweepSpeed, verticalRayCount, verticalAngleRange, lurkAngleRange, lurkDuration, minLurkDistance, maxLurkDistance, reachThreshold);
        chasingState = new ChasingState(chaseDuration,  reachThreshold);
        fleeingState = new FleeingState(minFleeDistance, maxFleeDistance, fleeDuration);
    }

    void Start()
    {
        SwitchState(lurkingState);
    }

    void Update()
    {
        if (currentState != null)
            currentState?.UpdateState(this);
    }

    public void SwitchState(MonsterBaseState newState)
    {
        if (currentState == newState)
            return;

        currentState?.ExitState(this);
        currentState = newState;
        Debug.Log("Monster State switched to: " + currentState.GetType().Name);
        currentState?.EnterState(this);
    }

    public void SetRandomTarget(Vector3 basePosition, float minDist, float maxDist, Vector3? direction = null, float angleRange = 30f)
    {
        int horizontalSlices = 8;
        int verticalSlices = 8;

        if (target == null) return;

        Vector3 randomDir;

        if (direction.HasValue)
        {
            float randomAngle = Random.Range(-angleRange, angleRange);
            randomDir = Quaternion.Euler(0f, randomAngle, 0f) * direction.Value.normalized;
        }
        else
        {
            randomDir = Random.insideUnitSphere;
            randomDir.y = 0f;
            randomDir.Normalize();
        }

        float distance = Random.Range(minDist, maxDist);
        Vector3 targetPos = basePosition + randomDir * distance;

        target.position = targetPos;

        float verticalAngle;
        Vector3 checkPos = targetPos;
        float checkDistance = -1;

        for (int i = 0; i < verticalSlices; i++)
        {
            verticalAngle = i * (360 / verticalSlices);
            for (int j = 0; j < horizontalSlices; j++)
            {
                Vector3 dir = Quaternion.Euler(verticalAngle, j * (360 / horizontalSlices), 0) * Vector3.one;
                dir.Normalize();
                if (Physics.Raycast(target.position, dir, out RaycastHit hit))
                {
                    if (checkDistance == -1)
                    {
                        checkPos = hit.point;
                        checkDistance = hit.distance;
                    }
                    else if (checkDistance > hit.distance)
                    {
                        checkPos = hit.point;
                        checkDistance = hit.distance;
                    }
                }
            }
        }

        target.position = checkPos;

        agentController.SetGoal(target);

        Debug.Log($"Random target set to {target.position}");
    }

    private void OnDrawGizmos()
    {
        if (!gizmoShowVision || currentState != lurkingState)
            return;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 baseDir = Quaternion.Euler(0f, gizmoHorizontalAngle, 0f) * transform.forward;

        Gizmos.color = playerInSight ? Color.red : Color.yellow;

        Gizmos.DrawLine(origin, origin + baseDir * gizmoVisionDistance);

        for (int i = 0; i < gizmoVerticalRayCount; i++)
        {
            float t = i / (float)(gizmoVerticalRayCount - 1);
            float verticalAngle = Mathf.Lerp(-gizmoVerticalAngleRange / 2f, gizmoVerticalAngleRange / 2f, t);
            Vector3 rayDir = Quaternion.Euler(verticalAngle, gizmoHorizontalAngle, 0f) * transform.forward;
            Gizmos.DrawLine(origin, origin + rayDir * gizmoVisionDistance);
        }
    }

}
