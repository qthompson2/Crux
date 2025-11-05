using UnityEngine;

public class FleeingState : MonsterBaseState
{
    private readonly float minFleeDistance;
    private readonly float maxFleeDistance;
    private readonly float fleeDuration;
    private readonly float fleeAngleRange = 45f;

    private float fleeTimer;
    private Vector3 fleeDirection;

    public FleeingState(float minFleeDistance, float maxFleeDistance, float fleeDuration)
    {
        this.minFleeDistance = minFleeDistance;
        this.maxFleeDistance = maxFleeDistance;
        this.fleeDuration = fleeDuration;
    }

    public override void EnterState(MonsterStateManager monster)
    {

        fleeTimer = 0f;

        // Pick a random flee direction
        float horizontalOffset = Random.Range(-45f, 45f);
        fleeDirection = Quaternion.Euler(0f, horizontalOffset, 0f) * Vector3.forward;

        // Set a random target behind or to the side of the monster
        monster.SetRandomTarget(
            monster.transform.position,
            minFleeDistance,
            maxFleeDistance,
            fleeDirection,
            fleeAngleRange
        );
    }

    public override void UpdateState(MonsterStateManager monster)
    {
        fleeTimer += Time.deltaTime;

        if (fleeTimer >= fleeDuration)
        {
            Debug.Log("Flee duration over — switching to Lurk State");
            monster.SwitchState(monster.lurkingState);
        }
    }
}
