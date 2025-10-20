using UnityEngine;

public class BaseState
{
    public virtual void EnterState(PlayerStateManager player) { }
    public virtual void UpdateState(PlayerStateManager player) { }
    public virtual void FixedUpdateState(PlayerStateManager player) { }
    public virtual void ExitState(PlayerStateManager player) { }
}
