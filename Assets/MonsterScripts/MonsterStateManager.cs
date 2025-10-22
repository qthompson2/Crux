using UnityEngine;
using System.Collections;

public class MonsterStateManager : MonoBehaviour
{
    public enum MonsterState
    {
        Lurking,
        Chasing,
        Fleeing
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform target;
    [SerializeField] private AgentController agentController;

    [Header("Monster Behavior Settings")]
    [SerializeField] private float chaseDuration = 10f;
    [SerializeField] private float lurkDuration = 6f;
    [SerializeField] private float fleeDuration = 8f;
    [SerializeField] private float minLurkDistance = 25f;
    [SerializeField] private float maxLurkDistance = 100f;
    [SerializeField] private float minFleeDistance = 50f;
    [SerializeField] private float maxFleeDistance = 150f;

    [SerializeField] private float reachThreshold = 2f;

    private float gizmoVisionDistance = 50f;
    private int gizmoVerticalRayCount = 6;
    private float gizmoVerticalAngleRange = 60f;
    private float gizmoHorizontalAngle = 0f;
    private bool gizmoShowVision = true;
    private bool playerInSight = false;

    private MonsterState currentState = MonsterState.Lurking;
    private Coroutine stateRoutine;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (target == null)
            Debug.LogWarning("Target Transform not assigned to MonsterStateManager!");

        if (currentState == MonsterState.Lurking)
            LogStateChange(MonsterState.Lurking);
            StartCoroutine(LurkRoutine());
    }

    public void SetState(MonsterState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;
        LogStateChange(newState);

        if (stateRoutine != null)
            StopCoroutine(stateRoutine);

        switch (newState)
        {
            case MonsterState.Lurking:
                stateRoutine = StartCoroutine(LurkRoutine());
                break;

            case MonsterState.Chasing:
                stateRoutine = StartCoroutine(ChaseRoutine());
                break;

            case MonsterState.Fleeing:
                stateRoutine = StartCoroutine(FleeRoutine());
                break;
        }
    }

    private void LogStateChange(MonsterState state)
    {
        Debug.Log($"Monster entered state: {state}");
    }

    // === STATE ROUTINES ===
    private IEnumerator LurkRoutine()
    {
        Debug.Log("Monster is lurking...");
        float visionDistance = 50f;
        float horizontalSweepSpeed = 60f;
        int verticalRayCount = 16;
        float verticalAngleRange = 60f;
        float lurkAngleRange = 30f; 

        float elapsed = 0f;
        float currentAngle = -90f;
        bool sweepingRight = true;

        yield return new WaitForSeconds(0.5f);

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer = -directionToPlayer;
        directionToPlayer.y = 0f;
        SetRandomTarget(player.position, minLurkDistance, maxLurkDistance, directionToPlayer, lurkAngleRange);

        while (currentState == MonsterState.Lurking)
        {
            elapsed += Time.deltaTime;

            float delta = horizontalSweepSpeed * Time.deltaTime * (sweepingRight ? 1 : -1);
            currentAngle += delta;

            if (currentAngle >= 90f)
            {
                currentAngle = 90f;
                sweepingRight = false;
            }
            else if (currentAngle <= -90f)
            {
                currentAngle = -90f;
                sweepingRight = true;
            }

            gizmoHorizontalAngle = currentAngle;
            playerInSight = false;

            Vector3 baseDir = Quaternion.Euler(0f, currentAngle, 0f) * transform.forward;
            Vector3 origin = transform.position + Vector3.up * 1.5f;

            for (int i = 0; i < verticalRayCount; i++)
            {
                float t = i / (float)(verticalRayCount - 1);
                float verticalAngle = Mathf.Lerp(-verticalAngleRange / 2f, verticalAngleRange / 2f, t);
                Vector3 rayDir = Quaternion.Euler(verticalAngle, 0f, 0f) * baseDir;

                if (Physics.Raycast(origin, rayDir, out RaycastHit hit, visionDistance))
                {
                    Debug.DrawRay(origin, rayDir * hit.distance, Color.red, 0.05f);

                    if (hit.collider.CompareTag("Player"))
                    {
                        Debug.Log($"Player detected during Lurking — switching to Chase! ({hit.collider.name})");
                        playerInSight = true;
                        SetState(MonsterState.Chasing);
                        yield break;
                    }
                }
                else
                {
                    Debug.DrawRay(origin, rayDir * visionDistance, Color.yellow, 0.05f);
                }
            }

            if (target != null)
            {
                float distToTarget = Vector3.Distance(transform.position, target.position);
                bool timeExpired = elapsed >= lurkDuration;
                bool reachedTarget = distToTarget <= reachThreshold;

                if (timeExpired || reachedTarget)
                {
                    Debug.Log("Repositioning target...");
                    elapsed = 0f;

                    float playerDist = Vector3.Distance(
                        new Vector3(transform.position.x, 0f, transform.position.z),
                        new Vector3(player.position.x, 0f, player.position.z)
                    );
                    float newMaxLurkDistance = Mathf.Min(100f, playerDist - 10f);
                    SetRandomTarget(player.position, minLurkDistance, newMaxLurkDistance, directionToPlayer, lurkAngleRange);
                }
            }

            yield return null;
        }
    }

    private void SetRandomTarget(Vector3 basePosition, float minDist, float maxDist, Vector3? direction = null, float angleRange = 30f)
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


    private IEnumerator ChaseRoutine()
    {
        float timer = 0f;

        while (timer < chaseDuration)
        {
            if (player == null)
                yield break;

            target.position = player.position;

            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= reachThreshold)
            {
                Debug.Log("Player reached!");
                SetState(MonsterState.Fleeing);
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Chase expired!");
        SetState(MonsterState.Fleeing);
    }

    private IEnumerator FleeRoutine()
    {
        Vector3 fleeBaseDir = Vector3.forward;
        float horizontalOffset = Random.Range(-45f, 45f);
        Vector3 fleeDir = Quaternion.Euler(0, Random.Range(-45f, 45f), 0) * Vector3.forward;
        SetRandomTarget(transform.position, minFleeDistance, maxFleeDistance, fleeDir, 45f);

        yield return new WaitForSeconds(fleeDuration);
        SetState(MonsterState.Lurking);
    }

    private void OnDrawGizmos()
    {
        if (!gizmoShowVision || currentState != MonsterState.Lurking)
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

    public void Lurk() => SetState(MonsterState.Lurking);
    public void Chase() => SetState(MonsterState.Chasing);
    public void Flee() => SetState(MonsterState.Fleeing);
}
