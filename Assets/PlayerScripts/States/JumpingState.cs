using UnityEngine;

public class JumpingState : BaseState
{

    public override void ExitState(PlayerStateManager player)
    {
        player.UpdateAnimator(this.GetType().Name, false);
    }

    public override void EnterState(PlayerStateManager player)
    {
        player.UpdateAnimator(this.GetType().Name, true);
        if (player.controller.IsGrounded)
        {
            player.controller.Jump();
            
        }
    }

    public override void UpdateState(PlayerStateManager player)
    {
        Vector2 input = player.inputHandler.MoveInput;
        Vector3 move = player.transform.right * input.x + player.transform.forward * input.y;
        player.controller.Move(move);
        player.controller.Jump();

        // ---- Handle Transitions ----
        player.SwitchState(player.fallingState);
    }
}
