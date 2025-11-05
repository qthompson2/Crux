using UnityEngine;

public class ChasingState : MonsterBaseState
{
    private float chaseTimer;
    private readonly float chaseDuration;
    private readonly float reachThreshold;
    private Transform player;
    private Transform target;

    public ChasingState(float chaseDuration, float reachThreshold)
    {
        this.chaseDuration = chaseDuration;
        this.reachThreshold = reachThreshold;
    }

    public override void EnterState(MonsterStateManager monster)
    {
        chaseTimer = 0f;
        player = monster.player;
        target = monster.target;
        monster.caughtPlayer = false;
    }

    public override void UpdateState(MonsterStateManager monster)
    {
        if (player == null)
        {
            monster.SwitchState(monster.fleeingState);
            return;
        }

        target.position = player.position;

        float dist = Vector3.Distance(monster.transform.position, player.position);
        if (dist <= reachThreshold)
        {
            monster.caughtPlayer = true;
            monster.SwitchState(monster.fleeingState);
            return;
        }

        chaseTimer += Time.deltaTime;
        if (chaseTimer >= chaseDuration)
        {
            monster.SwitchState(monster.fleeingState);
        }
    }

    public override void ExitState(MonsterStateManager monster)
    {
        monster.caughtPlayer = false;
    }
}
