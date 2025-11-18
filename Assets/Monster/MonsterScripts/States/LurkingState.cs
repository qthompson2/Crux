using UnityEngine;
using System.Collections;

public class LurkingState : MonsterBaseState
{
    private float visionDistance;
    private float horizontalSweepSpeed;
    private int verticalRayCount;
    private float verticalAngleRange;
    private float lurkAngleRange;
    private float lurkDuration;
    private float minLurkDistance;
    private float reachThreshold;
    private float maxLurkDistance;
    private float maxLurkTime;

    private float elapsed;
    private float totalLurkTime;
    private float currentAngle;
    private bool sweepingRight;

    private Vector3 directionToPlayer;

    public LurkingState(float visionDistance, float horizontalSweepSpeed, int verticalRayCount, float verticalAngleRange, float lurkAngleRange, float lurkDuration, float minLurkDistance, float maxLurkDistance, float reachThreshold, float maxLurkTime)
    {
        this.visionDistance = visionDistance;
        this.horizontalSweepSpeed = horizontalSweepSpeed;
        this.verticalRayCount = verticalRayCount;
        this.verticalAngleRange = verticalAngleRange;
        this.lurkAngleRange = lurkAngleRange;
        this.lurkDuration = lurkDuration;
        this.minLurkDistance = minLurkDistance;
        this.maxLurkDistance = maxLurkDistance;
        this.reachThreshold = reachThreshold;
        this.maxLurkTime = maxLurkTime;
    }

    public override void EnterState(MonsterStateManager monster)
    {
        Debug.Log("Entering Lurk State...");
        elapsed = 0f;
        totalLurkTime = 0f;
        currentAngle = -90f;
        sweepingRight = true;

        monster.StartCoroutine(InitializeLurk(monster)); // short setup delay
    }

    private IEnumerator InitializeLurk(MonsterStateManager monster)
    {
        yield return new WaitForSeconds(0.5f);

        // Pick a random direction opposite of the player to lurk in
        directionToPlayer = (monster.player.position - monster.transform.position).normalized;
        directionToPlayer = -directionToPlayer;
        directionToPlayer.y = 0f;

        monster.SetRandomTarget(
            monster.player.position,
            minLurkDistance,
            maxLurkDistance,
            directionToPlayer,
            lurkAngleRange
        );
    }

    public override void UpdateState(MonsterStateManager monster)
    {
        if (totalLurkTime >= maxLurkTime)
        {
            totalLurkTime = 0f;
            Debug.Log("Max Lurk Time exceeded — switching to Chase!");
            monster.SwitchState(monster.chasingState);
            return;
        }
        totalLurkTime += Time.deltaTime;
        elapsed += Time.deltaTime;

        // Horizontal sweep motion
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

        monster.gizmoHorizontalAngle = currentAngle;
        monster.playerInSight = false;

        // Perform vision sweep
        Vector3 baseDir = Quaternion.Euler(0f, currentAngle, 0f) * monster.transform.forward;
        Vector3 origin = monster.transform.position + Vector3.up * 1.5f;

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
                    monster.playerInSight = true;
                    monster.SwitchState(monster.chasingState);
                    return;
                }
            }
            else
            {
                Debug.DrawRay(origin, rayDir * visionDistance, Color.yellow, 0.05f);
            }
        }

        // Handle repositioning
        if (monster.target != null)
        {
            float distToTarget = Vector3.Distance(monster.transform.position, monster.target.position);
            bool timeExpired = elapsed >= lurkDuration;
            bool reachedTarget = distToTarget <= reachThreshold;

            if (timeExpired || reachedTarget)
            {
                Debug.Log("Repositioning target...");
                elapsed = 0f;

                float playerDist = Vector3.Distance(
                    new Vector3(monster.transform.position.x, 0f, monster.transform.position.z),
                    new Vector3(monster.player.position.x, 0f, monster.player.position.z)
                );
                float newMaxLurkDistance = Mathf.Min(100f, playerDist - 10f);

                monster.SetRandomTarget(
                    monster.player.position,
                    minLurkDistance,
                    newMaxLurkDistance,
                    directionToPlayer,
                    lurkAngleRange
                );
            }
        }
    }
}
