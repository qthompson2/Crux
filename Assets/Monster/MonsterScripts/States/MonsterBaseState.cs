using UnityEngine;

public class MonsterBaseState
{
    public virtual void EnterState(MonsterStateManager monster) { }
    public virtual void UpdateState(MonsterStateManager monster) { }
    public virtual void FixedUpdateState(MonsterStateManager monster) { }
    public virtual void ExitState(MonsterStateManager monster) { }
}
