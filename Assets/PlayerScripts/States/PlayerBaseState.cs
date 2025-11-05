using UnityEngine;

public class PlayerBaseState
{
    public virtual void EnterState(PlayerStateManager player) { }
    public virtual void UpdateState(PlayerStateManager player) { }
    public virtual void ExitState(PlayerStateManager player) { }
}
